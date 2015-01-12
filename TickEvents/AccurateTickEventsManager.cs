using LostParticles.TicksEngine.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;



namespace LostParticles.TicksEngine
{
    /// <summary>
    /// Accurate Ticks Manager use the hardware stopwatch and adjusted algorithm to ensure that events are on time. 
    /// </summary>
    public sealed class AccurateTickEventsManager: ITicksManager
    {

        public event EventHandler FinishedEvent;

        #region TickGenerator


        /// <summary>
        /// Ticks (based on StopWatch.Resolution TicksPerSecond) Per Beat
        /// </summary>
        private long _TicksPerBeat;


        private long CurrentTick;
        private long PreviousTick;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bpm">Beats per minute</param>
        public AccurateTickEventsManager(double beatsPerMinute)
        {
            
            //converting bpm into frequency of hardware timer 
            // so I can know how many ticks the beat will consume
            _TicksPerBeat = (long)((60 * Stopwatch.Frequency) / beatsPerMinute);

        }




        /// <summary>
        /// Beats Per Minute based on Stop Hardware stop watch.
        /// </summary>
        public double Tempo
        {
            get
            {
                return (double)((60 * Stopwatch.Frequency) / _TicksPerBeat);
            }
            set
            {
                _TicksPerBeat = (long)((60 * Stopwatch.Frequency) / value);
            }

        }

        /// <summary>
        /// Ticks per beat.
        /// Hardware Timer.Resolution as Ticks per beat.
        /// </summary>
        public long TicksPerBeat
        {
            get
            {
               return _TicksPerBeat;  //based on the frequency of high performance counter hardware found in the pc
            }
        }


        #region Accurate Methods


        IAsyncResult ar;
        Action<bool> PlayProc;

        /// <summary>
        /// using Another Thread with high resolution performance counter
        /// </summary>
        public void Start()
        {


            PlayProc = RunningPlayThread;

            ar = PlayProc.BeginInvoke(true, null, null);

            

            
        }




        /// <summary>
        /// The thread that plays the events.
        /// </summary>
        private void RunningPlayThread(bool canStop)
        {

            Stopwatch sw = Stopwatch.StartNew();

            while (!IsFinished)
            {
                CalculateAndSendStopWatchTicks(sw);
                
                //Thread.Sleep(0); //make time for other threads must in uniprocessor environment

                if (canStop)
                {
                    //check if I {the current running proc} should stop
                }

            }

            sw.Stop();

            FinishedEvent(this, null);

        }

        public void Stop()
        {
            if (!ar.IsCompleted)
            {
                EndRunningEvents();
                PlayProc.EndInvoke(ar);
            }
        }



        bool SendingTicks;

        /// <summary>
        /// Calculate the elapsed ticks of the Stop watch timer.
        /// </summary>
        /// <param name="sw"></param>
        public void CalculateAndSendStopWatchTicks(Stopwatch sw)
        {
            if (!SendingTicks)
            {
                CurrentTick = sw.ElapsedTicks;

                //specify the delta ticks that were consumed till now.
                long dTicks = CurrentTick - PreviousTick;

                PreviousTick = CurrentTick;

                SendingTicks = true;      //to prevent sending multiple ticks when calling exceed of the function increase 
                if (dTicks > 0) SendAccurateTicks(dTicks);
                SendingTicks = false;
            }
        }

        #endregion





        #endregion

        public void SendBeat()
        {
            SendAccurateTicks(TicksPerBeat);
        }

        public int RemainingBeatTicks
        {
            get
            {
                if (_ElapsedTicks < _TicksPerBeat) return (int)(_TicksPerBeat - _ElapsedTicks);
                else
                {
                    int remaining_beat_ticks = (int)(_TicksPerBeat - (_ElapsedTicks % _TicksPerBeat));
                    return remaining_beat_ticks;
                }
            }
        }

        public void SendRemainingBeat()
        {
            SendAccurateTicks(RemainingBeatTicks);
        }

        #region TickEvent Management

        private Queue<TickEvent> WaitingEvents = new Queue<TickEvent>();

        private List<TickEvent> RunningEvents = new List<TickEvent>();

        private long _ElapsedTicks;

        /// <summary>
        /// Elapsed Ticks since the first event.
        /// </summary>
        public long ElapsedTicks
        {
            get
            {
                return _ElapsedTicks;
            }
        }

        public double ElapsedBeats
        {
            get
            {
                return _ElapsedTicks / _TicksPerBeat;
            }
        }


        private long _TicksFromLastRunningEvent;

        /// <summary>
        /// Elapsed ticks from the last started event
        /// </summary>
        public long TicksFromLastRunningEvent
        {
            get
            {
                return _TicksFromLastRunningEvent;
            }
        }

        /// <summary>
        /// This method will be called any time you need to add event
        /// </summary>
        /// <param name="tickEvent">TickEvent object.</param>
        public void AddEvent(TickEvent tickEvent)
        {
            tickEvent.TicksManager = this;
            WaitingEvents.Enqueue(tickEvent);
        }



        /// <summary>
        /// This function send one tick to the Event Manager
        /// </summary>
        public void SendTick()
        {
            
            ProcessRunningEvents(1);


            bool ProcessAgain = true;


            _TicksFromLastRunningEvent++;

            while (ProcessAgain)
            {
                ProcessAgain = ProcessWaitingEvents();
            }


            _ElapsedTicks++;
        }

        /// <summary>
        /// this function send multiple ticks to the event manager
        /// </summary>
        /// <param name="Ticks"></param>
        public void SendAccurateTicks(long ticks)
        {
            //divide the ticks to be less than the ticks per beat by 2


            //  this is important because I had an error when sending all ticks in one shot that ..
            //   there were skipped ticks 
            //    the problem was if you have an event that will start after 3 ticks 
            //       and it duration is 4 ticks
            //       imagine that you sent 10 ticks 
            //         the event will start and then for the second iteration of sending ticks to stop
            //    which means in the first 10 ticks event started
            //    if I sent another 10 ticks the event will be ended
            //    this means the actual event ticks were 10-3 =7 at start + 10 in the end = 17  tick duration
            //      and a late starting as well.
            //      in one tick manager this was hidden due to the fact the queue of events is consumed one by one
            //    but in multi queue algorithm the queues weren't synchronized and a big flaw was happeninging in hearing the midi file.
            //
            // so I have to make sure that sent ticks is less than the tempo of the queue event manager
            //  and I chosed it to be half the tempo ticks per beat (which more than accurate )


            //the mximum ticks to be sended based on my assumption 
            // however in reality the maximum will equal the TicksPerBeat because in my events I deal with beats
            // but fot accuracy I decided to be half this value.
            double MaximumTicksPerBeat = (TicksPerBeat / 2);

            //how many times I'll call the internal sending ticks.
            double times = ticks / MaximumTicksPerBeat;

            //for example times = 3.6
            while (times >= 0)
            {
                if (times < 1)
                {
                    //this is fraction  part which is == 0.6 :)
                    InternalSendTicks((long)(times * MaximumTicksPerBeat));
                }
                else
                {
                    //this is the integer whole part  which  is == 3
                    InternalSendTicks((long)MaximumTicksPerBeat);
                }

                times--;
            }
            
        }


        private void InternalSendTicks(long ticks)
        {
            ProcessRunningEvents(ticks);

            bool ProcessAgain = true;

            _TicksFromLastRunningEvent += ticks;

            while (ProcessAgain)
            {
                ProcessAgain = ProcessWaitingEvents();
            }

            _ElapsedTicks += ticks;

        }

        

        /// <summary>
        /// the method should process all next events that its hold time
        /// equal ZERO in the same time
        /// which means its check the next event and
        /// if its hold is zero then it must start it
        /// </summary>
        /// <returns>
        /// true if the next event is zero holding time
        /// and should be processed
        /// else false
        /// </returns>
        private bool ProcessWaitingEvents()
        {
            if (WaitingEvents.Count > 0)
            {
                TickEvent tev = WaitingEvents.Peek();

                if (_TicksFromLastRunningEvent >= tev.HoldTicks)
                {
                    tev = WaitingEvents.Dequeue();
                    
                    RunningEvents.Add(tev);

                    tev.Begin();
                    tev.AfterBegin();

                    _TicksFromLastRunningEvent = 0;

                    //important if the next event is having 0 events
                    //then we should tell the control method to call this
                    //code again
                    if (WaitingEvents.Count > 0)
                    {
                        if (WaitingEvents.Peek().HoldTicks == 0)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region Running Events




        /// <summary>
        /// responsible of deciding if the running events
        /// should contiue or ended.
        /// after event consumes its duration ticks it will be removed 
        /// and disposed.
        /// </summary>
        private void ProcessRunningEvents(long ticks)
        {
            //starting from last item to the first item
            //so when I remove the item I want I know for sure
            //its behind me not in what I am going to read
            // and also i keep the right index

            int index = 0;
            while (index < RunningEvents.Count)
            {
                TickEvent tev = RunningEvents[index];

                //tell the event how many tiks passed on it.
                TickEvent.NotifyTickWithElapsedTicks(tev, ticks);

                //specify if the event is already passed the reaquired ticks
                if (tev.ElapsedTicksSinceStart >= tev.DurationTicks)
                {
                    tev.End();
                    tev.AfterEnd();

                    //remove the item
                    RunningEvents.RemoveAt(index);

                    //get back one step behind because we will increase the index again.
                    index--;

                }

                index++;

            }
        }

        #endregion


        /// <summary>
        /// Tells if the TickManager run out from waiting and running ticks or not.
        /// </summary>
        public bool IsFinished
        {
            get
            {

                return WaitingEvents.Count == 0 & RunningEvents.Count == 0;
            }
        }



        /// <summary>
        /// Force running events to end their life.
        /// </summary>
        private void EndRunningEvents()
        {

            int index = RunningEvents.Count - 1;
            while (index >= 0)
            {
                TickEvent tev = RunningEvents[index];

                tev.End();

                RunningEvents.RemoveAt(index);
            }

        }

        #endregion



    }

}

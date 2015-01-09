using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace LostParticles.TickEvents
{

    /// <summary>
    /// Responsible on managing stored TickEvents
    /// contains Two Lists of EventTicks
    /// WaitingEvents  List is push first int first out List
    /// RunningEvents  List is a list which its events all are processed
    /// </summary>
    public sealed class TickEventsManager : ITickEventsManager, IDisposable
    {

        public event EventHandler FinishedEvent;

        #region TickGenerator

        System.Timers.Timer MyTimer;

        BackgroundWorker PlayWorker;

        //what about making all timings in MilliSecond
        //Second = 1000 MilliSecond

        /// <summary>
        /// Milli Seconds Per Beat based on that second have 1000 milli second/ second
        /// I mean 1000 Hertz  frequency.
        /// </summary>
        private  int MilliSecondsPerBeat;


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
        public TickEventsManager(double beatsPerMinute)
        {

            //converting bpm into milliseconds per beat
            MilliSecondsPerBeat = (int)(60000 / beatsPerMinute);

            //converting bpm into frequency of hardware timer 
            _TicksPerBeat = (long)((60 * Stopwatch.Frequency)/beatsPerMinute);


            //making windows timere call the event two times the required beat
            MyTimer = new System.Timers.Timer(MilliSecondsPerBeat/2);
            MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(MyTimer_Elapsed);


            
            //worker mechanism to play ticks and monitoring the running thread to close it
            PlayWorker = new BackgroundWorker();

            

            PlayWorker.WorkerSupportsCancellation = true;

            PlayWorker.DoWork += new DoWorkEventHandler(PlayWorker_DoWork);

            PlayWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PlayWorker_RunWorkerCompleted);



            
        }


        /// <summary>
        /// Beats Per Minute Based on Windows Timer.
        /// </summary>
        public double Tempo
        {
            get
            {
                return (double)(60000 / MilliSecondsPerBeat); 
            }
            set
            {
                MilliSecondsPerBeat = (int)(60000 / value);
                //call the event two times the required beat
                 
                MyTimer.Interval = MilliSecondsPerBeat / 2;
            }
        }


        /// <summary>
        /// Beats Per Minute based on Stop Hardware stop watch.
        /// </summary>
        public double AccurateTempo
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
        /// If using windows timer it will use 1000 tick per second as resolution.
        /// If using Hardware timer it will use Timer.Resolution of the hardware.
        /// </summary>
        public long TicksPerBeat
        {
            get
            {
                if (UseAccurateTiming)
                    return _TicksPerBeat;  //based on the frequency of high performance counter hardware found in the pc
                else
                    return MilliSecondsPerBeat;
            }
        }

 


        #region Accurate Methods


        /// <summary>
        /// setting this property to true 
        /// makes the tick events use the property
        /// TickPerBeat which corresponds to the high perfromance counter
        /// frequency
        /// </summary>
        private bool UseAccurateTiming;


        /// <summary>
        /// Start the the ticks generation in Background worker object.
        /// Use High Resolution timer.
        /// </summary>
        public void StartAccurate()
        {

            PlayWorker.RunWorkerAsync();

            //original code with threads but I don't have 
            //notification when thread ends.
            //or when the thread updates the UI the exception occure.
            //so instead of this I made it Backgroundworker thread which is not
            //accurate.
            //Thread th = new Thread(RunningPlayThread);
            //th.Start();
            

        }

        void PlayWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RunningPlayThread();
        }

        void PlayWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (FinishedEvent != null)
            {
                FinishedEvent(this, null);
            }
        }


        /// <summary>
        /// The thread that plays the events. When StartAccurate() called.
        /// </summary>
        private void RunningPlayThread()
        {
            UseAccurateTiming = true;

            Stopwatch sw = Stopwatch.StartNew();

            while (WaitingEvents.Count > 0 || RunningEvents.Count > 0)
            {
                CalcAndSendStopWatchTicks(sw);
                Thread.Sleep(0); //make time for other threads must in uniprocessor environment

                if (PlayWorker.CancellationPending)
                {
                    EndRunningEvents();
                    break;
                }
            }

            sw.Stop();
            
            

            UseAccurateTiming = false;
        }

        public void StopAccurate()
        {
            PlayWorker.CancelAsync();
        }

        public bool IsFinished
        {
            get
            {
                return WaitingEvents.Count == 0;
            }
        }


        bool SendingTicks;

        /// <summary>
        /// Calculate the elapsed ticks of the Stop watch timer.
        /// </summary>
        /// <param name="sw"></param>
        private void CalcAndSendStopWatchTicks(Stopwatch sw)
        {
            if (!SendingTicks)
            {
                CurrentTick = sw.ElapsedTicks;
                
                long dTicks = CurrentTick - PreviousTick;
                
                PreviousTick = CurrentTick;

                SendingTicks = true;
                if (dTicks > 0) SendTicks(dTicks);
                SendingTicks = false;
            }
        }

        #endregion



        #region Non Accurate Methods based on windows timer.
        public void Start()
        {
            PreviousTick = (uint)Environment.TickCount;
            MyTimer.Start();
        }


        /// <summary>
        /// The function calculate how many ticks elapsed since 
        /// the last Environment Tick and send inform it to
        /// due to Environment Ticks are based on milli seoncnds also.
        /// <see cref="SendAccurateTicks"/>
        /// </summary>
        private void SendTicks()
        {
            if (!SendingTicks)
            {
                CurrentTick = (long)Environment.TickCount;
                long dTicks;

                if (CurrentTick >= PreviousTick)
                {
                    dTicks = CurrentTick - PreviousTick;
                }
                else
                {
                    //like 3 - 100 it was  99 100 0 1 2 3 
                    dTicks = CurrentTick - 0;
                    dTicks += (Int32.MaxValue - PreviousTick);
                }
                PreviousTick = CurrentTick;

                SendingTicks = true;
                if (dTicks > 0) SendTicks(dTicks);
                SendingTicks = false;
            }
        }

        /// <summary>
        /// when using Internal Timer to control sending ticks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendTicks();
            //check if there are more events to process
            if (WaitingEvents.Count == 0 && RunningEvents.Count == 0)
            {
                //to ensure that after notes timer is stopped
                MyTimer.Stop();
            }
        }

        public void Stop()
        {
            EndRunningEvents();
            MyTimer.Stop();
        }

        #endregion


        #endregion

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
            NotifyRunningEvents(1);

            ProcessRunningEvents();


            bool ProcessAgain = true;


            _TicksFromLastRunningEvent++;

            while (ProcessAgain)
            {
                ProcessAgain = ProcessWaitingEvents();
            }


            _ElapsedTicks++;
        }


        /// <summary>
        /// Send Accurate Ticks instruct the object to use the high resolution timer to calculate its events.
        /// </summary>
        /// <param name="ticks"></param>
        public void SendAccurateTicks(long ticks)
        {
            UseAccurateTiming = true;
            SendTicks(ticks);
            UseAccurateTiming = false;
        }

        /// <summary>
        /// Send ticks but with the resolution of the 1000 ms per second.
        /// Divide the ticks to be lower than the ticks per beat so that no lagging occur in starting and ending ticks.
        /// </summary>
        /// <param name="ticks"></param>
        private void SendTicks(long ticks)
        {
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

        /// <summary>
        /// this function send multiple ticks to the event manager
        /// </summary>
        /// <param name="Ticks"></param>
        private  void InternalSendTicks(long ticks)
        {
            NotifyRunningEvents(ticks);

            ProcessRunningEvents();

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
                    RunningEvents.Insert(0,tev);

                    tev.Begin();
                    tev.AfterBegin();

                    _TicksFromLastRunningEvent = 0;

                    //important if the next event is having 0 events
                    //then we should tell the control method to call this
                    //code again
                    // if the sum of events that after this event is less than the _TecksFromLastRunningEvent then we must process them also
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

        /// <summary>
        /// responsible of deciding if the running events
        /// should contiue or ended.
        /// after event consumes its duration ticks it will be removed 
        /// and disposed.
        /// </summary>
        private void ProcessRunningEvents()
        {
            //starting from last item to the first item
            //so when I remove the item I want I know for sure
            //its behind me not in what I am going to read
            // and also i keep the right index

            int index = RunningEvents.Count-1 ;
            while (index >= 0)
            {
                TickEvent tev = RunningEvents[index];

                if (tev.ElapsedTicksSinceStart >= tev.DurationTicks)
                {
                    tev.End();
                    tev.AfterEnd();

                    RunningEvents.RemoveAt(index);
                }

                index--;

                
            }
        }


        /// <summary>
        /// mainly to inform running events about current ticks
        /// </summary>
        private void NotifyRunningEvents(long ticks)
        {
            foreach (TickEvent tev in RunningEvents)
            {
                TickEvent.NotifyTickWithElapsedTicks(tev, ticks);
                
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

                index--;
            }

        }

        #endregion


        #region IDisposable Members

        ~TickEventsManager()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                MyTimer.Dispose();
                PlayWorker.Dispose();
            }
        }

        #endregion
    }
}

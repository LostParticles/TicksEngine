using LostParticles.TicksEngine.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostParticles.TicksEngine
{
    public class HardwareTicksPlayer : TicksManager, ITicksPlayer
    {


        public HardwareTicksPlayer(double beatsPerMinute)
            : base(beatsPerMinute)
        {
            
            //converting bpm into frequency of hardware timer 
            // so I can know how many ticks the beat will consume
            _TicksPerBeat = (long)((60 * Stopwatch.Frequency) / beatsPerMinute);

        }


        /// <summary>
        /// Beats Per Minute based on Stop Hardware stop watch.
        /// </summary>
        public override double Tempo
        {
            get
            {
                //return (double)((60 * Stopwatch.Frequency) / _TicksPerBeat);
                return base._BeatsPerMinute;

            }
            set
            {
                base._BeatsPerMinute = value;

                _TicksPerBeat = (long)((60 * Stopwatch.Frequency) / value);
            }

        }


        IAsyncResult ar;
        Action<bool> PlayProc;

        bool IsPaused = false;

        /// <summary>
        /// using Another Thread with high resolution performance counter
        /// </summary>
        public void Play()
        {
            PlayProc = RunningPlayThread;

            IsPaused = false;

            ar = PlayProc.BeginInvoke(true, null, null);
        }



        public void Pause()
        {
            IsPaused = true;
        }


        /// <summary>
        /// The thread that plays the events.
        /// </summary>
        private void RunningPlayThread(bool canStop)
        {

            Stopwatch sw = Stopwatch.StartNew();

            
            while (IsFinished == false && IsPaused == false)
            {
                CalculateAndSendStopWatchTicks(sw);
                    
                //Thread.Sleep(0); //make time for other threads must in uniprocessor environment

                if (canStop)
                {
                    //check if I {the current running proc} should stop
                }
            }

            sw.Stop();
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

    }
}

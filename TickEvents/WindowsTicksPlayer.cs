using LostParticles.TicksEngine.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LostParticles.TicksEngine
{
    /// <summary>
    /// Ticks generator using the windows timer of 1000 milli second resolution
    /// </summary>
    public sealed class WindowsTicksPlayer: TicksManager, ITicksPlayer
    {

        Timer MyTimer;


        public WindowsTicksPlayer(double beatsPerMinute):base(beatsPerMinute)
        {
            TimerCallback ElapsedTimer = MyTimer_Elapsed;


            //making windows timere call the event two times the required beat
            MyTimer = new Timer(ElapsedTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Beats Per Minute Based on Windows Timer.
        /// </summary>
        public override double Tempo
        {
            get
            {
                return base._BeatsPerMinute;
            }
            set
            {
                base._BeatsPerMinute = value;

                _TicksPerBeat = (long)(60000 / value);


                //call the event two times the required beat

                MyTimer.Change(0, (int)_TicksPerBeat / 2);
            }
        }



        bool SendingTicks;

        /// <summary>
        /// The function calculate how many ticks elapsed since 
        /// the last Environment Tick and send inform it to
        /// due to Environment Ticks are based on milli seoncnds also.
        /// <see cref="SendAccurateTicks"/>
        /// </summary>
        private void SendElapsedTicks()
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
                if (dTicks > 0) SendAccurateTicks(dTicks);
                SendingTicks = false;
            }
        }

        public void Play()
        {
            PreviousTick = (uint)Environment.TickCount;
            MyTimer.Change(0, (int)_TicksPerBeat / 2);
        }

        public void Pause()
        {

            MyTimer.Change(Timeout.Infinite, Timeout.Infinite);

        }

        /// <summary>
        /// when using Internal Timer to control sending ticks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyTimer_Elapsed(object state)
        {
            SendElapsedTicks();
            //check if there are more events to process
            if (WaitingEvents.Count == 0 && RunningEvents.Count == 0)
            {
                //to ensure that after notes timer is stopped
                MyTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Stop()
        {
            EndRunningEvents();
            MyTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}

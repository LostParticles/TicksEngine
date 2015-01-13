using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using System;
using System.Threading.Tasks;
using LostParticles.TicksEngine.Manager;

namespace LostParticles.TicksEngine
{
    /// <summary>
    /// This class will deal with more than one TickManager instance to synchronize between them all.
    /// </summary>
    public class MultiPlayer : ITicksPlayer
    {


        private List<ITicksPlayer> TicksPlayers = new List<ITicksPlayer>();

        public void AddTicksPlayer(ITicksPlayer ticksPlayer)
        {
            TicksPlayers.Add(ticksPlayer);
        }



        public event EventHandler<EventArgs> PlayFinished;


        public void Play()
        {

            RunningPlayThread();

        }

        public async void PlayAsynchronus()
        {

            Action PlayThread = RunningPlayThread;

            Task tsk = Task.Factory.StartNew (PlayThread);

            await tsk;
            
            /*   original code
            Thread th = new Thread(RunningPlayThread);
            th.Start();
             */

            
        }


        private long CurrentTick;
        private long PreviousTick;

        bool SendingTicks;


        /// <summary>
        /// The thread that plays the events.
        /// </summary>
        private void RunningPlayThread()
        {

            Stopwatch sw = new Stopwatch();

            //lowering the ticks per beat by the number of tracks of the midi
            // actually it can be divided by two but I made it like this for more ensuring that no tick 
            //  will be skipped.

            
            sw.Reset();

            sw.Start();


            while (!IsFinished)
            {
                CurrentTick = sw.ElapsedTicks;

                //specify the delta ticks that were consumed till now.
                long dTicks = CurrentTick - PreviousTick;

                PreviousTick = CurrentTick;

                SendingTicks = true;      //to prevent sending multiple ticks when calling exceed of the function increase 

                SendTicks(dTicks);

                SendingTicks = false;

                //Thread.Sleep(0); //make time for other threads must in uniprocessor environment
            }
                

            sw.Stop();

            if (PlayFinished != null) PlayFinished(this, new EventArgs());
        }

        public void SendTick()
        {
            foreach (ITicksManager atm in TicksPlayers)
            {
                atm.SendTick();
            }
        }

        public void SendTicks(long ticks)
        {
            foreach (ITicksManager atm in TicksPlayers)
            {
                lock (atm)
                {
                    atm.SendAccurateTicks(ticks);
                }
            }
        
        }


        public bool IsFinished
        {
            get
            {
                bool fin = true;

                foreach (ITicksManager tm in TicksPlayers)
                {
                    fin &= tm.IsFinished;
                }
                return fin;
            }
        }


        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}

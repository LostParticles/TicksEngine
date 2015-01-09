using System;

namespace LostParticles.TickEvents
{
    /// <summary>
    /// TickEvent class is expressing Amount of Beats
    /// the class is base class for any tick type you want in future.
    /// </summary>
    public class TickEvent
    {

        #region Tick Events
        public event EventHandler TickStarted;
        public event EventHandler TickStopped;
        public event EventHandler TickSuspended;
        public event EventHandler TickResumed;
        public event EventHandler TickRestarted;
        public event EventHandler TickNotified;
        #endregion

        #region Tick Instantiation.
        private double HoldBeats = 0;
        private double DurationBeats = 0;


        /// <summary>
        /// The TickEvent Constructor.
        /// </summary>
        /// <param name="holdBeats">Number Of Beats TickEvent will wait before starting.</param>
        /// <param name="durationBeats">Number of beats TickEvent will consume before end.</param>
        public TickEvent(double holdBeats, double durationBeats)
        {
            HoldBeats = holdBeats;
            DurationBeats = durationBeats;
        }

        private ITickEventsManager _TicksManager;
        public ITickEventsManager TicksManager
        {
            get
            {
                return _TicksManager;
            }
            set
            {
                _TicksManager = value;
            }
        }


        #endregion


        #region Ticks Information.
        /// <summary>
        /// Number of Ticks in every beat and it depends on the Associated TicksManager.
        /// </summary>
        private long TicksPerBeat
        {
            get 
            {
                return (long)_TicksManager.TicksPerBeat;
            }

        }

        /// <summary>
        /// Number of Holding Ticks based on Holding Beats.
        /// </summary>
        public long HoldTicks
        {
            get 
            {
                return (long)(HoldBeats * TicksPerBeat); 
            }
        }

        /// <summary>
        /// Number of duration ticks based on Duration Beats
        /// </summary>
        public long DurationTicks
        {
            get 
            { 
                return (long)(DurationBeats * TicksPerBeat); 
            }
        }

        #endregion

        #region Ticks consumption

        private long _ElapsedTicksSinceStart;

        /// <summary>
        /// Number of elapsed ticks since the tick event started.
        /// </summary>
        public long ElapsedTicksSinceStart
        {
            get
            {
                return _ElapsedTicksSinceStart;
            }
        }


        /// <summary>
        /// Tell the tick event that there are more ticks
        /// have been elapsed since you start.
        /// Have no effect if the event didn't start yet
        /// or the event is suspended
        /// </summary>
        /// <param name="Ticks">Number of ticks elapsed since the TickEvent Started.</param>
        private void UpdateElapsedTicks(long ticksCount)
        {
            _ElapsedTicksSinceStart += ticksCount;
            if (TickNotified != null) TickNotified(this, null);
        }


        /// <summary>
        /// Notify the tick instance with the elapsed ticks.
        /// </summary>
        /// <param name="tick">Tick instance.</param>
        /// <param name="elapsedTicks">ticks count to tell the tick instance.</param>
        public static void NotifyTickWithElapsedTicks(TickEvent tick, long elapsedTicks)
        {
            if (tick != null) tick.UpdateElapsedTicks(elapsedTicks);
        }

        #endregion

        #region TickEvent Control Methods
        private EventState _CurrentState = EventState.NotStarted;

        public EventState CurrentState
        {
            get
            {
                return _CurrentState;
            }
        }

        private EventArgs CurrentTickArgs;
        public EventArgs TickArgs
        {

            set
            {
                CurrentTickArgs = value;
            }
            get
            {
                return CurrentTickArgs;
            }
        }


        internal void AfterBegin()
        {
            _CurrentState = EventState.Started;

            if (TickStarted != null) TickStarted(this, TickArgs);

        }

        /// <summary>
        /// Begin the Tick operaion.
        /// </summary>
        public virtual void Begin()
        {

        }

        /// <summary>
        /// End the current Tick operation.
        /// </summary>
        public virtual void End()
        {
        }


        /// <summary>
        /// called after end to make any further tasks
        /// </summary>
        internal void AfterEnd()
        {
            _CurrentState = EventState.Ended;
            if (TickStopped != null) TickStopped(this, TickArgs);
        }

        /// <summary>
        /// Hang the current Tick 
        /// and making it not process any more ticks notified to it
        /// </summary>
        public virtual void Suspend()
        {
            _CurrentState = EventState.Suspended;
            if (TickSuspended != null) TickSuspended(this, TickArgs);
        }

        /// <summary>
        /// Resume suspended Tick to process notified ticks
        /// </summary>
        public virtual void Resume()
        {
            _CurrentState = EventState.Started;
            if (TickResumed != null) TickResumed(this, TickArgs);
        }


        /// <summary>
        /// Reset the Tick like its been never started.
        /// </summary>
        public virtual void Reset()
        {
            _CurrentState = EventState.NotStarted;
            _ElapsedTicksSinceStart = 0;
            if (TickRestarted != null) TickRestarted(this, TickArgs);
        }

        #endregion

    }
}

using LostParticles.TicksEngine.Manager;
using System;

namespace LostParticles.TicksEngine
{
    /// <summary>
    /// TickEvent class is expressing Amount of Beats
    /// the class is base class for any tick type you want in future.
    /// </summary>
    public class TickEvent
    {

        #region Tick Events
        public event EventHandler TickEventStarted;
        public event EventHandler TickEventStopped;
        public event EventHandler TickEventSuspended;
        public event EventHandler TickEventResumed;
        public event EventHandler TickEventRestarted;
        public event EventHandler TickEventNotified;
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

        private ITicksManager _TicksManager;
        public ITicksManager TicksManager
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


        long? _HoldTicks;

        /// <summary>
        /// Number of Holding Ticks based on Holding Beats.
        /// </summary>
        public long HoldTicks
        {
            get 
            {
                if (_HoldTicks == null)
                {
                    _HoldTicks  = (long)(HoldBeats * TicksPerBeat); 
                }
                return _HoldTicks.Value;
            }
        }


        long? _DurationTicks;

        /// <summary>
        /// Number of duration ticks based on Duration Beats
        /// </summary>
        public long DurationTicks
        {
            get 
            { 
                if(_DurationTicks == null)
                {
                    _DurationTicks = (long)(DurationBeats * TicksPerBeat);
                }
                return _DurationTicks.Value; 
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


        private long _LateFinishTicks;

        /// <summary>
        /// Indicates that the ending signal happened after the actual duration of the tick event.
        /// </summary>
        public bool OverflowFinishingTicks
        {
            get 
            {
                return _LateFinishTicks > 0;
            }
        }

        public long LateFinishTicks
        {
            get
            {
                return _LateFinishTicks;
            }
        }

        /// <summary>
        /// Tell the tick event that there are more ticks
        /// have been elapsed since you start.
        /// Have no effect if the event didn't start yet
        /// or the event is suspended
        /// </summary>
        /// <param name="Ticks">Number of ticks elapsed since the TickEvent Started.</param>
        private void UpdateElapsedTicks(long deltaTicksCount)
        {
            _ElapsedTicksSinceStart += deltaTicksCount;

            _LateFinishTicks = _ElapsedTicksSinceStart - DurationTicks;

            if (_LateFinishTicks > 0)
            {
                TicksUpdated(deltaTicksCount - _LateFinishTicks);  // in case the notification of ticks exceeding actual duration ticks for this event
            }
            else
                TicksUpdated(deltaTicksCount);


            if (TickEventNotified != null) TickEventNotified(this, null);
        }

        /// <summary>
        /// Called whenever the event has its ticks updated.
        /// </summary>
        /// <param name="deltaTicksCount"></param>
        public virtual void TicksUpdated(long deltaTicksCount)
        {
            
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
        private TickEventState _CurrentState = TickEventState.NotStarted;

        public TickEventState CurrentState
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


        private long _LateStartTicks;

        /// <summary>
        /// Ticks count on the start of the event. 
        /// </summary>
        public long LateStartTicks { get { return _LateStartTicks; } }
        

        internal void BeforeBegin(long lateStartTicks)
        {
            _LateStartTicks = lateStartTicks;
        }

        /// <summary>
        /// Begin the Tick operaion.
        /// </summary>
        public virtual void Begin()
        {

        }


        internal void AfterBegin()
        {
            _CurrentState = TickEventState.Started;

            if (TickEventStarted != null) TickEventStarted(this, TickArgs);

            this.UpdateElapsedTicks(_LateStartTicks);

        }

        internal void BeforeEnd()
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
            _CurrentState = TickEventState.Ended;
            if (TickEventStopped != null) TickEventStopped(this, TickArgs);
        }

        /// <summary>
        /// Hang the current Tick 
        /// and making it not process any more ticks notified to it
        /// </summary>
        public virtual void Suspend()
        {
            _CurrentState = TickEventState.Suspended;
            if (TickEventSuspended != null) TickEventSuspended(this, TickArgs);
        }

        /// <summary>
        /// Resume suspended Tick to process notified ticks
        /// </summary>
        public virtual void Resume()
        {
            _CurrentState = TickEventState.Started;
            if (TickEventResumed != null) TickEventResumed(this, TickArgs);
        }


        /// <summary>
        /// Reset the Tick like its been never started.
        /// </summary>
        public virtual void Reset()
        {
            _CurrentState = TickEventState.NotStarted;
            _ElapsedTicksSinceStart = 0;
        }

        public virtual void Restart()
        {
            
            if (TickEventRestarted != null) TickEventRestarted(this, TickArgs);
        }

        #endregion

    }
}

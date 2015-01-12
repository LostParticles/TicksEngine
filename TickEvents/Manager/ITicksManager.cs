
namespace LostParticles.TicksEngine.Manager
{
    public interface ITicksManager
    {
        /// <summary>
        /// Beat is consumed after how many ticks per beat.
        /// </summary>
        long TicksPerBeat { get; }

        void AddEvent(TickEvent tickEvent);

        /// <summary>
        /// Send a single tick to loaded events.
        /// </summary>
        void SendTick();

        /// <summary>
        /// Send a number of ticks
        /// </summary>
        /// <param name="ticks"></param>
        void SendAccurateTicks(long ticks);

        /// <summary>
        /// Send beat ticks 
        /// </summary>
        void SendBeat();

        /// <summary>
        /// Number of remining ticks of this beat.
        /// </summary>
        int RemainingBeatTicks { get; }

        /// <summary>
        /// Sends the remaining beat ticks to close current beat
        /// </summary>
        void SendRemainingBeat();
        
        bool IsFinished
        {
            get;
        }

        /// <summary>
        /// Beats per Minute
        /// </summary>
        double Tempo { get; set; }

        /// <summary>
        /// Elapsed Generated Ticks since start.
        /// </summary>
        long ElapsedTicks { get; }

        /// <summary>
        /// Elapsed Generated Beats since start.
        /// </summary>
        double ElapsedBeats { get; }

    }
}


namespace LostParticles.TickEvents
{
    public interface ITickEventsManager
    {
        long TicksPerBeat { get; }

        void AddEvent(TickEvent tickEvent);
        void SendTick();
        void SendAccurateTicks(long ticks);
        
        void Start();

        bool IsFinished
        {
            get;
        }

        double Tempo { get; set; }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostParticles.TicksEngine.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITicksPlayer
    {
        /// <summary>
        /// Starts the tick generator based on tempo (beats per minute)
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses the ticks generation
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the ticks generator.
        /// </summary>
        void Stop();

    }
}

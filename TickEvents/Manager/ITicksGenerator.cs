using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostParticles.TickEvents.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITicksGenerator : ITicksManager
    {
        /// <summary>
        /// Starts the tick generator based on tempo (beats per minute)
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the ticks generator.
        /// </summary>
        void Stop();

    }
}

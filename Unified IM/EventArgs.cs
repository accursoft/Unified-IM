using System;

namespace UnifiedIM
{
    /// <summary>Generic <see cref="EventArgs"/>.</summary>
    /// <typeparam name="T">Type of the event data.</typeparam>
    public class EventArgs<T> : EventArgs
    {
        internal EventArgs(T arg)
        {
            Arg = arg;
        }

        /// <summary>The event data</summary>
        public readonly T Arg;
    }
}
using System;

namespace UsbEject.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLogger : TextLogger
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="level">Minimum log level.</param>
        public ConsoleLogger(LogLevel level)
            : base(level, Console.Out, false)
        {
        }

        #endregion
    }
}

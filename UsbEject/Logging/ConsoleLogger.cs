using System;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Console"/>.
    /// </summary>
    public sealed class ConsoleLogger : TextLogger
    {
        #region Singleton

        /// <summary>
        /// Console logger instance.
        /// </summary>
        public static readonly ConsoleLogger Instance = new ConsoleLogger();

        private ConsoleLogger()
            : base(Console.Out, false)
        {
        }

        #endregion
    }
}

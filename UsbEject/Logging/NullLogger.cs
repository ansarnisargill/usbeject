using System;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// No-op logger implementation.
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        #region Singleton

        /// <summary>
        /// No-op logger instance.
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger()
        {
        }

        #endregion

        #region ILogger Members

        void IDisposable.Dispose()
        {
            //Do nothing
        }

        void ILogger.Write(LogLevel level, object obj)
        {
            //Do nothing
        }

        void ILogger.Write(LogLevel level, string str)
        {
            //Do nothing
        }

        void ILogger.Write(LogLevel level, string format, object arg0)
        {
            //Do nothing
        }

        void ILogger.Write(LogLevel level, string format, object[] args)
        {
            //Do nothing
        }

        #endregion
    }
}

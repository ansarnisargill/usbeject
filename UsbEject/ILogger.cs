using System;

namespace UsbEject.Library
{
    /// <summary>
    /// Logger interface.
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Writes an object to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="obj">Object to write.</param>
        void Write(LogLevel level, object obj);

        /// <summary>
        /// Writes a string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="str">String to write.</param>
        void Write(LogLevel level, string str);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="format">Format string.</param>
        /// <param name="arg0">Argument.</param>
        void Write(LogLevel level, string format, object arg0);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="format">Format string.</param>
        /// <param name="args">Arguments.</param>
        void Write(LogLevel level, string format, params object[] args);
    }
}

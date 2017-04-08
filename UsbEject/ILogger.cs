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
        /// <param name="obj">Object to write.</param>
        void Write(object obj);

        /// <summary>
        /// Writes a string to the log.
        /// </summary>
        /// <param name="str">String to write.</param>
        void Write(string str);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="arg0">Argument.</param>
        void Write(string format, object arg0);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Arguments.</param>
        void Write(string format, params object[] args);
    }
}

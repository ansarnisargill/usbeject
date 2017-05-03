// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using System;

namespace UsbEject
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
        void Log(LogLevel level, object obj);

        /// <summary>
        /// Writes a string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="str">String to write.</param>
        void Log(LogLevel level, string str);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="format">Format string.</param>
        /// <param name="arg0">Argument.</param>
        void Log(LogLevel level, string format, object arg0);

        /// <summary>
        /// Writes a formatted string to the log.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <param name="format">Format string.</param>
        /// <param name="args">Arguments.</param>
        void Log(LogLevel level, string format, params object[] args);
    }
}

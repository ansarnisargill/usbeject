using System;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A base class for trace logger implementations.
    /// </summary>
    public abstract class TraceLoggerBase : ILogger
    {
        #region Fields

        private readonly LogLevel _level;
        private readonly string _category;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLoggerBase"/> class.
        /// </summary>
        /// <param name="level">Minimum log level.</param>
        /// <param name="category">Trace category.</param>
        protected TraceLoggerBase(LogLevel level, string category)
        {
            _level = level;
            _category = category;
        }

        #endregion

        #region ILogger Members

        void IDisposable.Dispose()
        {
            //Do nothing
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, object obj)
        {
            if (_category != null)
                WriteLineIf(level >= _level, obj, _category);
            else
                WriteLineIf(level >= _level, obj);
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string str)
        {
            if (_category != null)
                WriteLineIf(level >= _level, str, _category);
            else
                WriteLineIf(level >= _level, str);
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string format, object arg0)
        {
            Write(level, string.Format(format, arg0));
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string format, object[] args)
        {
            Write(level, string.Format(format, args));
        }

        #endregion

        #region Abstract Members

        /// <summary>
        /// Writes <paramref name="category"/> and <paramref name="value"/> to trace listeners if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">true to cause a message to be written; otherwise, false.</param>
        /// <param name="value">An object whose name is sent to the trace listeners.</param>
        /// <param name="category">A category name used to organize the output.</param>
        protected abstract void WriteLineIf(bool condition, object value, string category);

        /// <summary>
        /// Writes <paramref name="value"/> to trace listeners if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">true to cause a message to be written; otherwise, false.</param>
        /// <param name="value">An object whose name is sent to the trace listeners.</param>
        protected abstract void WriteLineIf(bool condition, object value);

        /// <summary>
        /// Writes <paramref name="category"/> and <paramref name="message"/> to trace listeners if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">true to cause a message to be written; otherwise, false.</param>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        protected abstract void WriteLineIf(bool condition, string message, string category);

        /// <summary>
        /// Writes <paramref name="message"/> to the trace listeners if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">true to cause a message to be written; otherwise, false.</param>
        /// <param name="message">A message to write.</param>
        protected abstract void WriteLineIf(bool condition, string message);

        #endregion
    }
}

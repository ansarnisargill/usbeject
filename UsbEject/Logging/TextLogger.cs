using System;
using System.IO;

namespace UsbEject.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="TextWriter"/>.
    /// </summary>
    public class TextLogger : ILogger
    {
        #region Fields

        private readonly LogLevel _level;
        private TextWriter _writer;
        private readonly bool _writerOwner;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLogger"/> class.
        /// </summary>
        /// <param name="level">Minimum log level.</param>
        /// <param name="writer">Text writer.</param>
        /// <param name="writerOwner">Indicates whether the logger instance owns <paramref name="writer"/>.</param>
        public TextLogger(LogLevel level, TextWriter writer, bool writerOwner)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _writer = writer;
            _writerOwner = writerOwner;
            _level = level;
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (_writer != null)
            {
                if (_writerOwner)
                    _writer.Dispose();
                _writer = null;
            }
        }

        #endregion

        #region ILogger Members

        /// <inheritdoc/>
        public void Log(LogLevel level, object obj)
        {
            if (level >= _level)
                _writer.WriteLine(obj);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string str)
        {
            if (level >= _level)
                _writer.WriteLine(str);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string format, object arg0)
        {
            if (level >= _level)
                _writer.WriteLine(format, arg0);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string format, object[] args)
        {
            if (level >= _level)
                _writer.WriteLine(format, args);
        }

        #endregion
    }
}

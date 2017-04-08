using System;
using System.IO;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="TextWriter"/>.
    /// </summary>
    public class TextLogger : ILogger
    {
        #region Fields

        private TextWriter _writer;
        private readonly bool _writerOwner;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLogger"/> class.
        /// </summary>
        /// <param name="writer">Text writer.</param>
        /// <param name="writerOwner">Indicates whether the logger instance owns <paramref name="writer"/>.</param>
        public TextLogger(TextWriter writer, bool writerOwner)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _writer = writer;
            _writerOwner = writerOwner;
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
        public void Write(object obj)
        {
            _writer.WriteLine(obj);
        }

        /// <inheritdoc/>
        public void Write(string str)
        {
            _writer.WriteLine(str);
        }

        /// <inheritdoc/>
        public void Write(string format, object arg0)
        {
            _writer.Write(format, arg0);
        }

        /// <inheritdoc/>
        public void Write(string format, object[] args)
        {
            _writer.Write(format, args);
        }

        #endregion
    }
}

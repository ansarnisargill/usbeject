using System;
using System.Diagnostics;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Trace"/>.
    /// </summary>
    public sealed class TraceLogger : ILogger
    {
        #region Fields

        private readonly string _category;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogger"/> class.
        /// </summary>
        /// <param name="category">Trace category.</param>
        public TraceLogger(string category = null)
        {
            _category = category;
        }

        #endregion

        #region ILogger Members

        void IDisposable.Dispose()
        {
            //Do nothing
        }

        /// <inheritdoc/>
        public void Write(object obj)
        {
            if (_category != null)
                Trace.WriteLine(obj, _category);
            else
                Trace.WriteLine(obj);
        }

        /// <inheritdoc/>
        public void Write(string str)
        {
            if (_category != null)
                Trace.WriteLine(str, _category);
            else
                Trace.WriteLine(str);
        }

        /// <inheritdoc/>
        public void Write(string format, object arg0)
        {
            Write(string.Format(format, arg0));
        }

        /// <inheritdoc/>
        public void Write(string format, object[] args)
        {
            Write(string.Format(format, args));
        }

        #endregion
    }
}

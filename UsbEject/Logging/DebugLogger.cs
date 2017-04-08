using System;
using System.Diagnostics;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Debug"/>.
    /// </summary>
    public sealed class DebugLogger : ILogger
    {
        #region Fields

        private readonly string _category;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogger"/> class.
        /// </summary>
        /// <param name="category">Debug category.</param>
        public DebugLogger(string category = null)
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
                Debug.WriteLine(obj, _category);
            else
                Debug.WriteLine(obj);
        }

        /// <inheritdoc/>
        public void Write(string str)
        {
            if (_category != null)
                Debug.WriteLine(str, _category);
            else
                Debug.WriteLine(str);
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

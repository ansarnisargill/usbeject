using System.Diagnostics;

namespace UsbEject.Library.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="TraceSource"/>.
    /// </summary>
    public sealed class TraceSourceLogger : ILogger
    {
        private TraceSource _traceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceLogger"/> class.
        /// </summary>
        /// <param name="name">The name of the source.</param>
        public TraceSourceLogger(string name)
        {
            _traceSource = new TraceSource(name);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_traceSource != null)
            {
                _traceSource.Close();
                _traceSource = null;
            }
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, object obj)
        {
            TraceEventType eventType = GetEventType(level);
            _traceSource.TraceData(eventType, 0, obj);
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string str)
        {
            TraceEventType eventType = GetEventType(level);
            _traceSource.TraceEvent(eventType, 0, str);
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string format, object arg0)
        {
            TraceEventType eventType = GetEventType(level);
            _traceSource.TraceEvent(eventType, 0, format, arg0);
        }

        /// <inheritdoc/>
        public void Write(LogLevel level, string format, object[] args)
        {
            TraceEventType eventType = GetEventType(level);
            _traceSource.TraceEvent(eventType, 0, format, args);
        }

        private static TraceEventType GetEventType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return TraceEventType.Verbose;
                case LogLevel.Information:
                    return TraceEventType.Information;
                case LogLevel.Warning:
                    return TraceEventType.Warning;
                case LogLevel.Error:
                    return TraceEventType.Error;
                case LogLevel.Critical:
                    return TraceEventType.Critical;
            }

            return 0;
        }
    }
}

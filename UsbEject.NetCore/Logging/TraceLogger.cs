﻿using Chimp.Logging;
using System.Diagnostics;

namespace UsbEject.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Trace"/>.
    /// </summary>
    public sealed class TraceLogger : TraceLoggerBase
    {
        /// <summary>
        /// Default <see cref="TraceLogger"/> instance.
        /// </summary>
        public static readonly TraceLogger Default = new TraceLogger(LogLevel.Trace, "UsbEject");

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceLogger"/> class.
        /// </summary>
        /// <param name="level">Minimum log level.</param>
        /// <param name="category">Trace category.</param>
        public TraceLogger(LogLevel level, string category)
            : base(level, category)
        {
        }

        #endregion

        #region Member Overrides

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, object value, string category)
        {
            Trace.WriteLineIf(condition, value, category);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, object value)
        {
            Trace.WriteLineIf(condition, value);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, string message, string category)
        {
            Trace.WriteLineIf(condition, message, category);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, string message)
        {
            Trace.WriteLineIf(condition, message);
        }

        #endregion
    }
}

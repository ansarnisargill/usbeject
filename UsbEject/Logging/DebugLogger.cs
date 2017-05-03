using System.Diagnostics;

namespace UsbEject.Logging
{
    /// <summary>
    /// A logger implementation using <see cref="Debug"/>.
    /// </summary>
    public sealed class DebugLogger : TraceLoggerBase
    {
        /// <summary>
        /// Default <see cref="DebugLogger"/> instance.
        /// </summary>
        public static readonly DebugLogger Default = new DebugLogger(LogLevel.Debug, "UsbEject");

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogger"/> class.
        /// </summary>
        /// <param name="level">Minimum log level.</param>
        /// <param name="category">Debug category.</param>
        public DebugLogger(LogLevel level, string category)
            : base(level, category)
        {
        }

        #endregion

        #region Member Overrides

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, object value, string category)
        {
            Debug.WriteLineIf(condition, value, category);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, object value)
        {
            Debug.WriteLineIf(condition, value);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, string message, string category)
        {
            Debug.WriteLineIf(condition, message, category);
        }

        /// <inheritdoc/>
        protected override void WriteLineIf(bool condition, string message)
        {
            Debug.WriteLineIf(condition, message);
        }

        #endregion
    }
}

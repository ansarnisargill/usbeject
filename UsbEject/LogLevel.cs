// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

namespace UsbEject
{
    /// <summary>
    /// Log level.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Trace.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Debug.
        /// </summary>
        Debug,

        /// <summary>
        /// Information.
        /// </summary>
        Information,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Error.
        /// </summary>
        Error,

        /// <summary>
        /// Critical.
        /// </summary>
        Critical,

        /// <summary>
        /// None.
        /// </summary>
        None = int.MaxValue
    }
}

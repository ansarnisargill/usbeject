namespace UsbEject.Library
{
    /// <summary>
    /// Log level.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Verbose.
        /// </summary>
        Verbose,

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

namespace UsbEject.Library
{
    /// <summary>
    /// A disk device.
    /// </summary>
    public class Disk : Device
    {
        #region Constructor

        internal Disk(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum, ILogger logger)
            : base(deviceClass, deviceInfoData, path, index, logger)
        {
            DiskNumber = disknum;
        }

        #endregion

        #region DiskNumber

        /// <summary>
        /// Gets the disk number.
        /// </summary>
        public int DiskNumber
        {
            get;
        }

        #endregion
    }
}
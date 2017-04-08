// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections;
using System.Collections.Generic;

namespace UsbEject.Library
{
    /// <summary>
    /// The device class for disk devices.
    /// </summary>
    public class DiskDeviceClass : DeviceClass, IEnumerable<Disk>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DiskDeviceClass class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="loggerOwner">Indicates whether the device class instance owns <paramref name="logger"/>.</param>
        public DiskDeviceClass(ILogger logger = null, bool loggerOwner = false)
            : base(new Guid(Native.GUID_DEVINTERFACE_DISK), logger, loggerOwner)
        {
            _disks = new Lazy<IEnumerable<Disk>>(GetDisks);
        }

        #endregion

        #region Member Overrides

        internal override Device CreateDevice(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum)
        {
            return new Disk(deviceClass, deviceInfoData, path, index: index, disknum: disknum, logger: Logger);
        }

        #endregion

        #region Disks

        private readonly Lazy<IEnumerable<Disk>> _disks;

        /// <summary>
        /// Gets the list of disks.
        /// </summary>
        public IEnumerable<Disk> Disks
        {
            get
            {
                return _disks.Value;
            }
        }

        private IEnumerable<Disk> GetDisks()
        {
            List<Disk> disks = new List<Disk>();

            foreach (Disk disk in Devices)
            {
                disks.Add(disk);
            }

            return disks;
        }

        #endregion

        #region IEnumerable
        
        /// <inheritdoc/>
        public IEnumerator<Disk> GetEnumerator()
        {
            return Disks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Disks.GetEnumerator();
        }

        #endregion
    }
}

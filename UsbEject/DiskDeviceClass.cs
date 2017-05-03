// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections;
using System.Collections.Generic;

#if NET45
using DiskCollection = System.Collections.Generic.IReadOnlyCollection<UsbEject.Disk>;
#else
using DiskCollection = System.Collections.Generic.IEnumerable<UsbEject.Disk>;
#endif

namespace UsbEject
{
    /// <summary>
    /// The device class for disk devices.
    /// </summary>
    public class DiskDeviceClass : DeviceClass, DiskCollection
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
            _disks = new Lazy<DiskCollection>(GetDisks);
        }

        #endregion

        #region CreateDevice

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
        {
            return new Disk(this, deviceInfoData, path, index, Logger);
        }

        #endregion

        #region Disks

        private readonly Lazy<DiskCollection> _disks;

        /// <summary>
        /// Gets the list of disks.
        /// </summary>
        public DiskCollection Disks
        {
            get
            {
                return _disks.Value;
            }
        }

        private DiskCollection GetDisks()
        {
            List<Disk> disks = new List<Disk>();

            foreach (Disk disk in Devices)
            {
                disks.Add(disk);
            }

            return disks;
        }

        #endregion

        #region DiskCollection

        /// <inheritdoc/>
        public IEnumerator<Disk> GetEnumerator()
        {
            return Disks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Disks.GetEnumerator();
        }

#if NET45
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return Disks.Count;
            }
        }
#endif

        #endregion
    }
}

// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using Chimp.Logging;
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
    public sealed class DiskDeviceClass : DeviceClass, DiskCollection
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DiskDeviceClass class.
        /// </summary>
        public DiskDeviceClass()
            : this(NoOpLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DiskDeviceClass class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        public DiskDeviceClass(ILoggerFactory loggerFactory)
            : this(loggerFactory.CreateLogger<DiskDeviceClass>())
        {
        }

        internal DiskDeviceClass(ILogger logger)
            : base(Native.GUID_DEVINTERFACE_DISK, logger)
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

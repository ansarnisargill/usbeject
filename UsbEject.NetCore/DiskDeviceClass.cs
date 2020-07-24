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

        internal DiskDeviceClass()
            : base(Native.GUID_DEVINTERFACE_DISK)
        {
            _disks = new Lazy<DiskCollection>(GetDisks);
        }

        #endregion

        #region CreateDevice

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
        {
            return new Disk(this, deviceInfoData, path, index);
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

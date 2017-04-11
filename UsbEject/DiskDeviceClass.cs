// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

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

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum)
        {
            return new Disk(this, deviceInfoData, path, index: index, disknum: disknum, logger: Logger);
        }

        internal override Native.STORAGE_DEVICE_NUMBER GetDiskNumber(string devicePath)
        {
            // Find disks
            IntPtr hFile = Native.CreateFile(devicePath, 0, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
            if (hFile == (IntPtr)Native.INVALID_HANDLE_VALUE)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            int size = 0x400; // some big size
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                int bytesReturned = 0;
                try
                {
                    if (!Native.DeviceIoControl(hFile, Native.IOCTL_STORAGE_GET_DEVICE_NUMBER, IntPtr.Zero, 0, buffer, size, out bytesReturned, IntPtr.Zero))
                    {
                        Logger.Write("IOCTL failed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write("Exception calling IOCTL: {0}", ex);
                }
                finally
                {
                    Native.CloseHandle(hFile);
                }

                if (bytesReturned > 0)
                {
                    return (Native.STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(buffer, typeof(Native.STORAGE_DEVICE_NUMBER));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return base.GetDiskNumber(devicePath);
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

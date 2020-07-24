﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace UsbEject
{
    /// <summary>
    /// A disk device.
    /// </summary>
    public sealed class Disk : Device
    {
        #region Constructor

        internal Disk(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
            : base(deviceClass, deviceInfoData, path, index)
        {
            _diskNumber = new Lazy<int>(GetDiskNumber);
        }

        #endregion

        #region DiskNumber

        private readonly Lazy<int> _diskNumber;

        /// <summary>
        /// Gets the disk number.
        /// </summary>
        public int DiskNumber
        {
            get
            {
                return _diskNumber.Value;
            }
        }

        private int GetDiskNumber()
        {
            // Find disks
            SafeFileHandle hFile = Native.CreateFile(Path, 0, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
            if (hFile.IsInvalid)
            {
                Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                
                throw ex;
            }

            using (hFile)
            {
                Native.STORAGE_DEVICE_NUMBER diskNumber = GetDiskNumber(hFile);
                return diskNumber.DeviceNumber;
            }
        }

        private Native.STORAGE_DEVICE_NUMBER GetDiskNumber(SafeFileHandle hFile)
        {
            int size = Native.IOCTL_BUFFER_SIZE;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                int bytesReturned = 0;
                try
                {
                    if (!Native.DeviceIoControl(hFile, Native.IOCTL_STORAGE_GET_DEVICE_NUMBER, IntPtr.Zero, 0, buffer, size, out bytesReturned, IntPtr.Zero))
                    {
                        // TODO
                    }
                }
                catch (Exception ex)
                {
                    // TODO
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

            return new Native.STORAGE_DEVICE_NUMBER { DeviceNumber = -1, DeviceType = -1, PartitionNumber = -1 };
        }

        #endregion
    }
}

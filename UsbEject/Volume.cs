// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using Chimp.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if NET45
using DeviceCollection = System.Collections.Generic.IReadOnlyCollection<UsbEject.Device>;
using DiskCollection = System.Collections.Generic.IReadOnlyCollection<UsbEject.Disk>;
#else
using DeviceCollection = System.Collections.Generic.IEnumerable<UsbEject.Device>;
using DiskCollection = System.Collections.Generic.IEnumerable<UsbEject.Disk>;
#endif

namespace UsbEject
{
    /// <summary>
    /// A volume device.
    /// </summary>
    public sealed class Volume : Device, IComparable
    {
        #region Constructors

        internal Volume(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, ILogger logger)
            : base(deviceClass, deviceInfoData, path, index, logger)
        {
            _volumeName = new Lazy<string>(GetVolumeName);
            _logicalDrive = new Lazy<string>(GetLogicalDrive);
            _disks = new Lazy<DiskCollection>(GetDisks);
            _diskNumbers = new Lazy<int[]>(GetDiskNumbers);
        }

        #endregion

        #region VolumeName
        private readonly Lazy<string> _volumeName;

        /// <summary>
        /// Gets the volume's name.
        /// </summary>
        public string VolumeName
        {
            get
            {
                return _volumeName.Value;
            }
        }

        private string GetVolumeName()
        {
            StringBuilder sb = new StringBuilder(Native.CM_BUFFER_SIZE);
            if (!Native.GetVolumeNameForVolumeMountPoint(Path + "\\", sb, sb.Capacity))
            {
                Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                Logger.Log(LogLevel.Error, ex);
                //throw ex;
            }

            if (sb.Length > 0)
            {
                return sb.ToString();
            }

            return null;
        }
        #endregion

        #region LogicalDrive
        private readonly Lazy<string> _logicalDrive;

        /// <summary>
        /// Gets the volume's logical drive in the form [letter]:\
        /// </summary>
        public string LogicalDrive
        {
            get
            {
                return _logicalDrive.Value;
            }
        }

        private string GetLogicalDrive()
        {
            if (VolumeName != null)
            {
                string logicalDrive;
                ((VolumeDeviceClass)DeviceClass).LogicalDrives.TryGetValue(VolumeName, out logicalDrive);
                return logicalDrive;
            }

            return null;
        }
        #endregion

        #region Disks
        private readonly Lazy<DiskCollection> _disks;

        /// <summary>
        /// Gets a list of underlying disks for this volume.
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

            if (DiskNumbers != null)
            {
                DiskDeviceClass diskClass = new DiskDeviceClass(Logger);
                foreach (int index in DiskNumbers)
                {
                    foreach (Disk disk in diskClass)
                    {
                        if (disk.DiskNumber == index)
                        {
                            disks.Add(disk);
                        }
                    }
                }
            }

            return disks;
        }
        #endregion

        #region DiskNumbers
        private readonly Lazy<int[]> _diskNumbers;

        /// <summary>
        /// Gets the volume's disk numbers
        /// </summary>
        public int[] DiskNumbers
        {
            get
            {
                return _diskNumbers.Value;
            }
        }

        private int[] GetDiskNumbers()
        {
            List<int> numbers = new List<int>();
            if (LogicalDrive != null)
            {
                Logger.Log(LogLevel.Trace, "Finding disk extents for volume: {0}", LogicalDrive);
                SafeFileHandle hFile = Native.CreateFile(@"\\.\" + LogicalDrive, 0, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
                if (hFile.IsInvalid)
                {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    Logger.Log(LogLevel.Error, ex);
                    throw ex;
                }

                using (hFile)
                {
                    int size = Native.IOCTL_BUFFER_SIZE;
                    IntPtr buffer = Marshal.AllocHGlobal(size);
                    try
                    {
                        int bytesReturned = 0;
                        try
                        {
                            if (!Native.DeviceIoControl(hFile, Native.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, buffer, size, out bytesReturned, IntPtr.Zero))
                            {
                                Logger.Log(LogLevel.Warning, "IOCTL failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, "Exception calling IOCTL: {0}", ex);
                        }

                        if (bytesReturned > 0)
                        {
                            int numberOfDiskExtents = (int)Marshal.PtrToStructure(buffer, typeof(int));
                            for (int i = 0; i < numberOfDiskExtents; i++)
                            {
                                IntPtr extentPtr = new IntPtr(buffer.ToInt32() + Marshal.SizeOf(typeof(long)) + i * Marshal.SizeOf(typeof(Native.DISK_EXTENT)));
                                Native.DISK_EXTENT extent = (Native.DISK_EXTENT)Marshal.PtrToStructure(extentPtr, typeof(Native.DISK_EXTENT));
                                numbers.Add(extent.DiskNumber);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }

            return numbers.ToArray();
        }
        #endregion

        #region Member Overrides

        internal override bool GetIsUsb()
        {
            if (Disks != null)
            {
                foreach (Device disk in Disks)
                {
                    if (disk.IsUsb)
                        return true;
                }
            }
            return false;
        }

        internal override DeviceCollection GetRemovableDevices()
        {
            if (Disks == null)
            {
                return base.GetRemovableDevices();
            }

            List<Device> removableDevices = new List<Device>();
            foreach (Device disk in Disks)
            {
                removableDevices.AddRange(disk.RemovableDevices);
            }

            return removableDevices;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
        public override int CompareTo(object obj)
        {
            Volume device = obj as Volume;
            if (device == null)
                throw new ArgumentException();

            if (LogicalDrive == null)
                return 1;

            if (device.LogicalDrive == null)
                return -1;

            return LogicalDrive.CompareTo(device.LogicalDrive);
        }

        #endregion
    }
}

// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbEject.Library
{
    /// <summary>
    /// A generic base class for physical device classes.
    /// </summary>
    public abstract class DeviceClass : IDisposable
    {
        #region Fields

        private IntPtr _deviceInfoSet;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DeviceClass class.
        /// </summary>
        /// <param name="classGuid">A device class Guid.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="loggerOwner">Indicates whether the device class instance owns <paramref name="logger"/>.</param>
        protected DeviceClass(Guid classGuid, ILogger logger, bool loggerOwner)
            : this(classGuid, IntPtr.Zero, logger, loggerOwner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DeviceClass class.
        /// </summary>
        /// <param name="classGuid">A device class Guid.</param>
        /// <param name="hwndParent">The handle of the top-level window to be used for any user interface or IntPtr.Zero for no handle.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="loggerOwner">Indicates whether the device class instance owns <paramref name="logger"/>.</param>
		protected DeviceClass(Guid classGuid, IntPtr hwndParent, ILogger logger, bool loggerOwner)
        {
            _classGuid = classGuid;

            _deviceInfoSet = Native.SetupDiGetClassDevs(ref _classGuid, 0, hwndParent, Native.DIGCF_DEVICEINTERFACE | Native.DIGCF_PRESENT);
            if (_deviceInfoSet == (IntPtr)Native.INVALID_HANDLE_VALUE)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            Logger = logger ?? Logging.NullLogger.Instance;
            LoggerOwner = loggerOwner;

            _devices = new Lazy<List<Device>>(GetDevices);
        }

        #endregion

        #region CreateDevice

        internal abstract Device CreateDevice(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum);

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_deviceInfoSet != IntPtr.Zero)
            {
                Native.SetupDiDestroyDeviceInfoList(_deviceInfoSet);
                _deviceInfoSet = IntPtr.Zero;
            }

            if (Logger != null)
            {
                if (LoggerOwner)
                    Logger.Dispose();
                Logger = null;
            }
        }

        /// <inheritdoc/>
        ~DeviceClass()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ClassGuid
        private Guid _classGuid;

        /// <summary>
        /// Gets the device class's guid.
        /// </summary>
		public Guid ClassGuid
        {
            get
            {
                return _classGuid;
            }
        }
        #endregion

        #region Logger
        internal ILogger Logger
        {
            get;
            private set;
        }
        #endregion

        #region LoggerOwner
        private bool LoggerOwner
        {
            get;
        }
        #endregion

        #region Devices
        private readonly Lazy<List<Device>> _devices;

        /// <summary>
        /// Gets the list of devices of this device class.
        /// </summary>
        public List<Device> Devices
        {
            get
            {
                return _devices.Value;
            }
        }

        private List<Device> GetDevices()
        {
            List<Device> devices = new List<Device>();
            int index = 0;
            while (true)
            {
                Native.SP_DEVICE_INTERFACE_DATA interfaceData = new Native.SP_DEVICE_INTERFACE_DATA();
                interfaceData.cbSize = (uint)Marshal.SizeOf(interfaceData);

                if (!Native.SetupDiEnumDeviceInterfaces(_deviceInfoSet, null, ref _classGuid, index, interfaceData))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != Native.ERROR_NO_MORE_ITEMS)
                        throw new Win32Exception(error);
                    break;
                }

                Native.SP_DEVINFO_DATA devData = new Native.SP_DEVINFO_DATA();
                devData.cbSize = (uint)Marshal.SizeOf(devData);
                int size = 0;
                if (!Native.SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, IntPtr.Zero, 0, ref size, devData))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != Native.ERROR_INSUFFICIENT_BUFFER)
                        throw new Win32Exception(error);
                }

                string devicePath;
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    Native.SP_DEVICE_INTERFACE_DETAIL_DATA detailData = new Native.SP_DEVICE_INTERFACE_DETAIL_DATA();
                    detailData.cbSize = (uint)Marshal.SizeOf(detailData);
                    Marshal.StructureToPtr(detailData, buffer, false);

                    if (!Native.SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, buffer, size, ref size, devData))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    IntPtr pDevicePath = (IntPtr)((int)buffer + Marshal.SizeOf(typeof(int)));
                    devicePath = Marshal.PtrToStringAuto(pDevicePath);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }

                if (_classGuid.Equals(new Guid(Native.GUID_DEVINTERFACE_DISK)))
                {
                    // Find disks
                    IntPtr hFile = Native.CreateFile(devicePath, 0, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
                    if (hFile == (IntPtr)Native.INVALID_HANDLE_VALUE)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    int numBufSize = 0x400; // some big size
                    IntPtr numBuffer = Marshal.AllocHGlobal(numBufSize);
                    try
                    {
                        Native.STORAGE_DEVICE_NUMBER disknum;

                        int bytesReturned = 0;
                        try
                        {
                            if (!Native.DeviceIoControl(hFile, Native.IOCTL_STORAGE_GET_DEVICE_NUMBER, IntPtr.Zero, 0, numBuffer, numBufSize, out bytesReturned, IntPtr.Zero))
                            {
                                Logger.Write("IOCTL failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Write("Exception calling ioctl: {0}", ex);
                        }
                        finally
                        {
                            Native.CloseHandle(hFile);
                        }

                        if (bytesReturned > 0)
                            disknum = (Native.STORAGE_DEVICE_NUMBER)Marshal.PtrToStructure(numBuffer, typeof(Native.STORAGE_DEVICE_NUMBER));
                        else
                            disknum = new Native.STORAGE_DEVICE_NUMBER { DeviceNumber = -1, DeviceType = -1, PartitionNumber = -1 };

                        Device device = CreateDevice(this, devData, devicePath, index: index, disknum: disknum.DeviceNumber);
                        devices.Add(device);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(numBuffer);
                    }
                }
                else
                {
                    Device device = CreateDevice(this, devData, devicePath, index: index, disknum: -1);
                    devices.Add(device);
                }

                index++;
            }

            devices.Sort();
            return devices;
        }
        #endregion

        #region Helper Methods

        internal Native.SP_DEVINFO_DATA GetInfo(uint dnDevInst)
        {
            StringBuilder sb = new StringBuilder(1024);
            int hr = Native.CM_Get_Device_ID(dnDevInst, sb, sb.Capacity, 0);
            if (hr != 0)
                throw new Win32Exception(hr);

            Native.SP_DEVINFO_DATA devData = new Native.SP_DEVINFO_DATA();
            devData.cbSize = (uint)Marshal.SizeOf(devData);
            if (!Native.SetupDiOpenDeviceInfo(_deviceInfoSet, sb.ToString(), IntPtr.Zero, 0, devData))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return devData;
        }

        internal string GetProperty(Native.SP_DEVINFO_DATA devData, int property, string defaultValue)
        {
            if (devData == null)
                throw new ArgumentNullException(nameof(devData));

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = 1024;

            IntPtr propertyBuffer = Marshal.AllocHGlobal(propertyBufferSize);
            try
            {
                if (!Native.SetupDiGetDeviceRegistryProperty(_deviceInfoSet,
                    devData,
                    property,
                    out propertyRegDataType,
                    propertyBuffer,
                    propertyBufferSize,
                    out requiredSize))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != Native.ERROR_INVALID_DATA)
                        throw new Win32Exception(error);
                    return defaultValue;
                }

                return Marshal.PtrToStringAuto(propertyBuffer);
            }
            finally
            {
                Marshal.FreeHGlobal(propertyBuffer);
            }
        }

        internal int GetProperty(Native.SP_DEVINFO_DATA devData, int property, int defaultValue)
        {
            if (devData == null)
                throw new ArgumentNullException(nameof(devData));

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = Marshal.SizeOf(typeof(int));

            IntPtr propertyBuffer = Marshal.AllocHGlobal(propertyBufferSize);
            try
            {
                if (!Native.SetupDiGetDeviceRegistryProperty(_deviceInfoSet,
                    devData,
                    property,
                    out propertyRegDataType,
                    propertyBuffer,
                    propertyBufferSize,
                    out requiredSize))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != Native.ERROR_INVALID_DATA)
                        throw new Win32Exception(error);
                    return defaultValue;
                }

                return (int)Marshal.PtrToStructure(propertyBuffer, typeof(int));
            }
            finally
            {
                Marshal.FreeHGlobal(propertyBuffer);
            }
        }

        #endregion
    }
}

// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using Chimp.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#if NET45
using DeviceCollection = System.Collections.Generic.IReadOnlyCollection<UsbEject.Device>;
#else
using DeviceCollection = System.Collections.Generic.IEnumerable<UsbEject.Device>;
#endif

namespace UsbEject
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
        protected DeviceClass(Guid classGuid, ILogger logger)
            : this(classGuid, IntPtr.Zero, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DeviceClass class.
        /// </summary>
        /// <param name="classGuid">A device class Guid.</param>
        /// <param name="hwndParent">The handle of the top-level window to be used for any user interface or IntPtr.Zero for no handle.</param>
        /// <param name="logger">Logger.</param>
		protected DeviceClass(Guid classGuid, IntPtr hwndParent, ILogger logger)
        {
            _classGuid = classGuid;

            Logger = logger;

            _deviceInfoSet = Native.SetupDiGetClassDevs(ref _classGuid, 0, hwndParent, Native.DIGCF_DEVICEINTERFACE | Native.DIGCF_PRESENT);
            if (_deviceInfoSet == (IntPtr)Native.INVALID_HANDLE_VALUE)
            {
                Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                Logger.Log(LogLevel.Error, ex);
                throw ex;
            }

            _devices = new Lazy<DeviceCollection>(GetDevices);
        }

        #endregion

        #region CreateDevice

        internal abstract Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index);

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
        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger Logger
        {
            get;
            private set;
        }
        #endregion

        #region Devices

        private readonly Lazy<DeviceCollection> _devices;

        /// <summary>
        /// Gets the list of devices of this device class.
        /// </summary>
        public DeviceCollection Devices
        {
            get
            {
                return _devices.Value;
            }
        }

        private DeviceCollection GetDevices()
        {
            List<Device> devices = new List<Device>();
            Native.SP_DEVICE_INTERFACE_DATA interfaceData;
            for (int index = 0; (interfaceData = GetInterfaceData(index)) != null; index++)
            {
                int size;
                Native.SP_DEVINFO_DATA devData = GetDeviceData(interfaceData, out size);
                string devicePath = GetDevicePath(interfaceData, devData, size);
                Device device = CreateDevice(devData, devicePath, index);
                devices.Add(device);
            }

            devices.Sort();
            return devices;
        }

        private Native.SP_DEVICE_INTERFACE_DATA GetInterfaceData(int index)
        {
            Native.SP_DEVICE_INTERFACE_DATA interfaceData = new Native.SP_DEVICE_INTERFACE_DATA();

            if (!Native.SetupDiEnumDeviceInterfaces(_deviceInfoSet, null, ref _classGuid, index, interfaceData))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != Native.ERROR_NO_MORE_ITEMS)
                {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    Logger.Log(LogLevel.Error, ex);
                    throw ex;
                }
                return null;
            }

            return interfaceData;
        }

        private Native.SP_DEVINFO_DATA GetDeviceData(Native.SP_DEVICE_INTERFACE_DATA interfaceData, out int size)
        {
            Native.SP_DEVINFO_DATA devData = new Native.SP_DEVINFO_DATA();
            size = 0;
            if (!Native.SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, IntPtr.Zero, 0, ref size, devData))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != Native.ERROR_INSUFFICIENT_BUFFER)
                {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    Logger.Log(LogLevel.Error, ex);
                    throw ex;
                }
            }

            return devData;
        }

        private string GetDevicePath(Native.SP_DEVICE_INTERFACE_DATA interfaceData, Native.SP_DEVINFO_DATA devData, int size)
        {
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Native.SP_DEVICE_INTERFACE_DETAIL_DATA detailData = new Native.SP_DEVICE_INTERFACE_DETAIL_DATA();
                Marshal.StructureToPtr(detailData, buffer, false);

                if (!Native.SetupDiGetDeviceInterfaceDetail(_deviceInfoSet, interfaceData, buffer, size, ref size, devData))
                {
                    Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                    Logger.Log(LogLevel.Error, ex);
                    throw ex;
                }

                IntPtr pDevicePath = (IntPtr)((int)buffer + Marshal.SizeOf(typeof(int)));
                return Marshal.PtrToStringAuto(pDevicePath);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion

        #region Helper Methods

        internal Native.SP_DEVINFO_DATA GetInfo(uint dnDevInst)
        {
            StringBuilder sb = new StringBuilder(Native.CM_BUFFER_SIZE);
            int hr = Native.CM_Get_Device_ID(dnDevInst, sb, sb.Capacity, 0);
            if (hr != 0)
            {
                Exception ex = Marshal.GetExceptionForHR(hr);
                Logger.Log(LogLevel.Error, ex);
                throw ex;
            }

            Native.SP_DEVINFO_DATA devData = new Native.SP_DEVINFO_DATA();
            if (!Native.SetupDiOpenDeviceInfo(_deviceInfoSet, sb.ToString(), IntPtr.Zero, 0, devData))
            {
                Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                Logger.Log(LogLevel.Error, ex);
                throw ex;
            }

            return devData;
        }

        internal string GetProperty(Native.SP_DEVINFO_DATA devData, int property, string defaultValue)
        {
            if (devData == null)
                throw new ArgumentNullException(nameof(devData));

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = Native.PROPERTY_BUFFER_SIZE;

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
                    {
                        Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                        Logger.Log(LogLevel.Error, ex);
                        throw ex;
                    }
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
                    {
                        Exception ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                        Logger.Log(LogLevel.Error, ex);
                        throw ex;
                    }
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

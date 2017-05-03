// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbEject.Library
{
    /// <summary>
    /// A generic base class for physical devices.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Device : IComparable
    {
        #region Constructors

        internal Device(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, ILogger logger)
        {
            if (deviceClass == null)
                throw new ArgumentNullException(nameof(deviceClass));

            if (deviceInfoData == null)
                throw new ArgumentNullException(nameof(deviceInfoData));

            DeviceClass = deviceClass;
            DeviceInfoData = deviceInfoData;
            Path = path; // may be null
            Index = index;
            Logger = logger;

            _class = new Lazy<string>(GetClass);
            _classGuid = new Lazy<string>(GetClassGuid);
            _description = new Lazy<string>(GetDescription);
            _friendlyName = new Lazy<string>(GetFriendlyName);
            _capabilities = new Lazy<DeviceCapabilities>(GetCapabilities);
            _isUsb = new Lazy<bool>(GetIsUsb);
            _parent = new Lazy<Device>(GetParent);
            _removableDevices = new Lazy<List<Device>>(GetRemovableDevices);
        }

        #endregion

        #region DeviceInfoData
        private Native.SP_DEVINFO_DATA DeviceInfoData
        {
            get;
        }
        #endregion

        #region Index
        /// <summary>
        /// Gets the device's index.
        /// </summary>
        public int Index
        {
            get;
        }
        #endregion

        #region DeviceClass
        /// <summary>
        /// Gets the device's class instance.
        /// </summary>
        [Browsable(false)]
        public DeviceClass DeviceClass
        {
            get;
        }
        #endregion

        #region Path
        /// <summary>
        /// Gets the device's path.
        /// </summary>
        public string Path
        {
            get;
        }
        #endregion

        #region Logger
        internal ILogger Logger
        {
            get;
        }
        #endregion

        #region InstanceHandle
        /// <summary>
        /// Gets the device's instance handle.
        /// </summary>
        public uint InstanceHandle
        {
            get
            {
                return DeviceInfoData.devInst;
            }
        }
        #endregion

        #region Class
        private readonly Lazy<string> _class;

        /// <summary>
        /// Gets the device's class name.
        /// </summary>
        public string Class
        {
            get
            {
                return _class.Value;
            }
        }

        private string GetClass()
        {
            return DeviceClass.GetProperty(DeviceInfoData, Native.SPDRP_CLASS, null);
        }
        #endregion

        #region ClassGuid
        private readonly Lazy<string> _classGuid;

        /// <summary>
        /// Gets the device's class Guid as a string.
        /// </summary>
        public string ClassGuid
        {
            get
            {
                return _classGuid.Value;
            }
        }

        private string GetClassGuid()
        {
            return DeviceClass.GetProperty(DeviceInfoData, Native.SPDRP_CLASSGUID, null);
        }
        #endregion

        #region Description
        private readonly Lazy<string> _description;

        /// <summary>
        /// Gets the device's description.
        /// </summary>
        public string Description
        {
            get
            {
                return _description.Value;
            }
        }

        private string GetDescription()
        {
            return DeviceClass.GetProperty(DeviceInfoData, Native.SPDRP_DEVICEDESC, null);
        }
        #endregion

        #region FriendlyName
        private readonly Lazy<string> _friendlyName;

        /// <summary>
        /// Gets the device's friendly name.
        /// </summary>
        public string FriendlyName
        {
            get
            {
                return _friendlyName.Value;
            }
        }

        private string GetFriendlyName()
        {
            return DeviceClass.GetProperty(DeviceInfoData, Native.SPDRP_FRIENDLYNAME, null);
        }
        #endregion

        #region Capabilities
        private readonly Lazy<DeviceCapabilities> _capabilities;

        /// <summary>
        /// Gets the device's capabilities.
        /// </summary>
        public DeviceCapabilities Capabilities
        {
            get
            {
                return _capabilities.Value;
            }
        }

        private DeviceCapabilities GetCapabilities()
        {
            return (DeviceCapabilities)DeviceClass.GetProperty(DeviceInfoData, Native.SPDRP_CAPABILITIES, 0);
        }
        #endregion

        #region IsUsb
        private readonly Lazy<bool> _isUsb;

        /// <summary>
        /// Gets a value indicating whether this device is a USB device.
        /// </summary>
        public bool IsUsb
        {
            get
            {
                return _isUsb.Value;
            }
        }

        internal virtual bool GetIsUsb()
        {
            if (Class == "USB")
                return true;

            if (Parent == null)
                return false;

            return Parent.IsUsb;
        }
        #endregion

        #region Parent
        private readonly Lazy<Device> _parent;

        /// <summary>
        /// Gets the device's parent device or null if this device has not parent.
        /// </summary>
        public Device Parent
        {
            get
            {
                return _parent.Value;
            }
        }

        private Device GetParent()
        {
            uint parentDevInst = 0;
            int hr = Native.CM_Get_Parent(ref parentDevInst, DeviceInfoData.devInst, 0);
            if (hr == 0)
            {
                Native.SP_DEVINFO_DATA info = DeviceClass.GetInfo(parentDevInst);
                return new Device(DeviceClass, info, null, -1, Logger);
            }

            return null;
        }
        #endregion

        #region RemovableDevices
        private readonly Lazy<List<Device>> _removableDevices;

        /// <summary>
        /// Gets this device's list of removable devices.
        /// Removable devices are parent devices that can be removed.
        /// </summary>
        public List<Device> RemovableDevices
        {
            get
            {
                return _removableDevices.Value;
            }
        }

        internal virtual List<Device> GetRemovableDevices()
        {
            List<Device> removableDevices = new List<Device>();

            if ((Capabilities & DeviceCapabilities.Removable) != 0)
            {
                removableDevices.Add(this);
            }
            else if (Parent != null)
            {
                removableDevices.AddRange(Parent.RemovableDevices);
            }

            return removableDevices;
        }
        #endregion

        #region Eject

        /// <summary>
        /// Ejects the device.
        /// </summary>
        /// <param name="allowUI">Pass true to allow the Windows shell to display any related UI element, false otherwise.</param>
        /// <returns>null if no error occured, otherwise a contextual text.</returns>
        public string Eject(bool allowUI)
        {
            if (allowUI)
            {
                EjectNoUI();
                return null;
            }

            return Eject();
        }

        private void EjectNoUI()
        {
            foreach (Device device in RemovableDevices)
            {
                int hr = Native.CM_Request_Device_Eject_NoUi(device.InstanceHandle, IntPtr.Zero, null, 0, 0);
                if (hr != 0)
                {
                    Exception ex = Marshal.GetExceptionForHR(hr);
                    Logger.Write(LogLevel.Error, "Error ejecting {0}: {1}", device.InstanceHandle, ex);
                    // don't throw exceptions, there should be a UI for this
                }
            }
        }

        private string Eject()
        {
            StringBuilder sb = new StringBuilder(Native.CM_BUFFER_SIZE);
            foreach (Device device in RemovableDevices)
            {
                Native.PNP_VETO_TYPE veto;
                int hr = Native.CM_Request_Device_Eject(device.InstanceHandle, out veto, sb, sb.Capacity, 0);
                if (hr != 0)
                {
                    Exception ex = Marshal.GetExceptionForHR(hr);
                    if (ex != null)
                    {
                        Logger.Write(LogLevel.Error, "Error ejecting {0}: {1}", device.InstanceHandle, ex);
                        throw ex;
                    }
                }

                if (veto != Native.PNP_VETO_TYPE.Ok)
                {
                    if (sb.Length > 0)
                        Logger.Write(LogLevel.Warning, "Vetoed ejecting {0}: {1} (2)", device.InstanceHandle, veto, sb.ToString());
                    else
                        Logger.Write(LogLevel.Warning, "Vetoed ejecting {0}: {1}", device.InstanceHandle, veto);
                    return veto.ToString();
                }
            }

            return null;
        }

        #endregion

        #region Member Overrides

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
        public virtual int CompareTo(object obj)
        {
            Device device = obj as Device;
            if (device == null)
                throw new ArgumentException();

            return Index.CompareTo(device.Index);
        }

        #endregion
    }
}

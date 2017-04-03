// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UsbEject.Library
{
    /// <summary>
    /// A generic base class for physical devices.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Device : IComparable
    {
        private Native.SP_DEVINFO_DATA _deviceInfoData;

        internal Device(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum = -1)
        {
            if (deviceClass == null)
                throw new ArgumentNullException("deviceClass");

            if (deviceInfoData == null)
                throw new ArgumentNullException("deviceInfoData");

            _deviceClass = deviceClass;
            _path = path; // may be null
            _deviceInfoData = deviceInfoData;
            _index = index;
            _diskNumber = disknum;
        }

        private readonly int _index;

        /// <summary>
        /// Gets the device's index.
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
        }

        private readonly DeviceClass _deviceClass;

        /// <summary>
        /// Gets the device's class instance.
        /// </summary>
        [Browsable(false)]
        public DeviceClass DeviceClass
        {
            get
            {
                return _deviceClass;
            }
        }

        private readonly string _path;

        /// <summary>
        /// Gets the device's path.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        private readonly int _diskNumber;

        /// <summary>
        /// Gets the device's disk number.
        /// </summary>
        public int DiskNumber
        {
            get
            {
                return _diskNumber;
            }
        }

        /// <summary>
        /// Gets the device's instance handle.
        /// </summary>
        public int InstanceHandle
        {
            get
            {
                return _deviceInfoData.devInst;
            }
        }

        private string _class;

        /// <summary>
        /// Gets the device's class name.
        /// </summary>
        public string Class
        {
            get
            {
                if (_class == null)
                {
                    _class = GetClass();
                }
                return _class;
            }
        }

        private string GetClass()
        {
            return _deviceClass.GetProperty(_deviceInfoData, Native.SPDRP_CLASS, null);
        }

        private string _classGuid;

        /// <summary>
        /// Gets the device's class Guid as a string.
        /// </summary>
        public string ClassGuid
        {
            get
            {
                if (_classGuid == null)
                {
                    _classGuid = GetClassGuid();
                }
                return _classGuid;
            }
        }

        private string GetClassGuid()
        {
            return _deviceClass.GetProperty(_deviceInfoData, Native.SPDRP_CLASSGUID, null);
        }

        private string _description;

        /// <summary>
        /// Gets the device's description.
        /// </summary>
        public string Description
        {
            get
            {
                if (_description == null)
                {
                    _description = GetDescription();
                }
                return _description;
            }
        }

        private string GetDescription()
        {
            return _deviceClass.GetProperty(_deviceInfoData, Native.SPDRP_DEVICEDESC, null);
        }

        private string _friendlyName;

        /// <summary>
        /// Gets the device's friendly name.
        /// </summary>
        public string FriendlyName
        {
            get
            {
                if (_friendlyName == null)
                {
                    _friendlyName = GetFriendlyName();
                }
                return _friendlyName;
            }
        }

        private string GetFriendlyName()
        {
            return _deviceClass.GetProperty(_deviceInfoData, Native.SPDRP_FRIENDLYNAME, null);
        }

        private DeviceCapabilities? _capabilities;

        /// <summary>
        /// Gets the device's capabilities.
        /// </summary>
        public DeviceCapabilities Capabilities
        {
            get
            {
                if (_capabilities == null)
                {
                    _capabilities = GetCapabilities();
                }
                return _capabilities.Value;
            }
        }

        private DeviceCapabilities GetCapabilities()
        {
            return (DeviceCapabilities)_deviceClass.GetProperty(_deviceInfoData, Native.SPDRP_CAPABILITIES, 0);
        }

        private bool? _isUsb;

        /// <summary>
        /// Gets a value indicating whether this device is a USB device.
        /// </summary>
        public bool IsUsb
        {
            get
            {
                if (_isUsb == null)
                {
                    _isUsb = GetIsUsb();
                }
                return _isUsb.Value;
            }
        }

        protected virtual bool GetIsUsb()
        {
            if (Class == "USB")
                return true;

            if (Parent == null)
                return false;

            return Parent.IsUsb;
        }

        private Device _parent;

        /// <summary>
        /// Gets the device's parent device or null if this device has not parent.
        /// </summary>
        public Device Parent
        {
            get
            {
                if (_parent == null)
                {
                    _parent = GetParent();
                }
                return _parent;
            }
        }

        private Device GetParent()
        {
            int parentDevInst = 0;
            int hr = Native.CM_Get_Parent(ref parentDevInst, _deviceInfoData.devInst, 0);
            if (hr == 0)
            {
                return new Device(_deviceClass, _deviceClass.GetInfo(parentDevInst), null, -1);
            }

            return null;
        }

        private List<Device> _removableDevices;

        /// <summary>
        /// Gets this device's list of removable devices.
        /// Removable devices are parent devices that can be removed.
        /// </summary>
        public List<Device> RemovableDevices
        {
            get
            {
                if (_removableDevices == null)
                {
                    _removableDevices = GetRemovableDevices();
                }
                return _removableDevices;
            }
        }

        protected virtual List<Device> GetRemovableDevices()
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

        /// <summary>
        /// Ejects the device.
        /// </summary>
        /// <param name="allowUI">Pass true to allow the Windows shell to display any related UI element, false otherwise.</param>
        /// <returns>null if no error occured, otherwise a contextual text.</returns>
        public string Eject(bool allowUI)
        {
            foreach (Device device in RemovableDevices)
            {
                if (allowUI)
                {
                    Native.CM_Request_Device_Eject_NoUi(device.InstanceHandle, IntPtr.Zero, null, 0, 0);
                    // don't handle errors, there should be a UI for this
                }
                else
                {
                    StringBuilder sb = new StringBuilder(1024);

                    Native.PNP_VETO_TYPE veto;
                    int hr = Native.CM_Request_Device_Eject(device.InstanceHandle, out veto, sb, sb.Capacity, 0);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    if (veto != Native.PNP_VETO_TYPE.Ok)
                        return veto.ToString();
                }

            }
            return null;
        }

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
    }
}

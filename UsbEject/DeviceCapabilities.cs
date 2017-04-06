// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;

namespace UsbEject.Library
{
    /// <summary>
    /// Contains constants for determining devices capabilities.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum DeviceCapabilities
    {
        /// <summary>
        /// 
        /// </summary>
        Unknown = 0x00000000,
        // matches cfmgr32.h CM_DEVCAP_* definitions

        /// <summary>
        /// Specifies whether the device supports physical-device locking that prevents device ejection.
        /// </summary>
        LockSupported = 0x00000001,

        /// <summary>
        /// Specifies whether the device supports software-controlled device ejection while the system is in the PowerSystemWorking state.
        /// </summary>
        EjectSupported = 0x00000002,

        /// <summary>
        /// Specifies whether the device can be dynamically removed from its immediate parent.
        /// </summary>
        Removable = 0x00000004,

        /// <summary>
        /// Specifies whether the device is a docking peripheral.
        /// </summary>
        DockDevice = 0x00000008,

        /// <summary>
        /// Specifies whether the device's instance ID is unique system-wide.
        /// </summary>
        UniqueId = 0x00000010,

        /// <summary>
        /// Specifies whether Device Manager should suppress all installation dialog boxes; except required dialog boxes such as "no compatible drivers found."
        /// </summary>
        SilentInstall = 0x00000020,

        /// <summary>
        /// Specifies whether the driver for the underlying bus can drive the device if there is no function driver (for example, SCSI devices in pass-through mode).
        /// </summary>
        RawDeviceOk = 0x00000040,

        /// <summary>
        /// Specifies whether the function driver for the device can handle the case where the device is removed before Windows can send IRP_MN_QUERY_REMOVE_DEVICE to it.
        /// </summary>
        SurpriseRemovalOk = 0x00000080,

        /// <summary>
        /// When set, this flag specifies that the device's hardware is disabled.
        /// </summary>
        HardwareDisabled = 0x00000100,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        NonDynamic = 0x00000200,
    }
}

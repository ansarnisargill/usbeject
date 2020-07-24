// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UsbEject
{
    internal static class Native
    {
        // from winbase.h
        internal const int INVALID_HANDLE_VALUE = -1;
        internal const int FILE_SHARE_READ = 0x00000001;
        internal const int FILE_SHARE_WRITE = 0x00000002;
        internal const int OPEN_EXISTING = 3;

        internal const int IOCTL_BUFFER_SIZE = 1024;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetVolumeNameForVolumeMountPoint(
            string volumeName,
            StringBuilder uniqueVolumeName,
            int uniqueNameBufferCapacity);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool DeviceIoControl(SafeFileHandle hDevice, int dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        // from winerror.h
        internal const int ERROR_NO_MORE_ITEMS = 259;
        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const int ERROR_INVALID_DATA = 13;

        // from winioctl.h
        internal static readonly Guid GUID_DEVINTERFACE_VOLUME = new Guid("53f5630d-b6bf-11d0-94f2-00a0c91efb8b");
        internal static readonly Guid GUID_DEVINTERFACE_DISK = new Guid("53f56307-b6bf-11d0-94f2-00a0c91efb8b");

        internal const int IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS = 0x00560000;
        internal const int IOCTL_STORAGE_GET_DEVICE_NUMBER = 0x002d1080;

        [StructLayout(LayoutKind.Sequential)]
        internal struct DISK_EXTENT
        {
            internal int DiskNumber;
            internal long StartingOffset;
            internal long ExtentLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct STORAGE_DEVICE_NUMBER
        {
            public int DeviceType;
            public int DeviceNumber;
            public int PartitionNumber;
        }

        // from cfg.h
        internal enum PNP_VETO_TYPE
        {
            Ok,

            TypeUnknown,
            LegacyDevice,
            PendingClose,
            WindowsApp,
            WindowsService,
            OutstandingOpen,
            Device,
            Driver,
            IllegalDeviceRequest,
            InsufficientPower,
            NonDisableable,
            LegacyDriver,
            InsufficientRights,
        }

        internal const int CM_BUFFER_SIZE = 1024;

        // from cfgmgr32.h
        [DllImport("setupapi.dll")]
        internal static extern int CM_Get_Parent(
            ref uint pdnDevInst,
            uint dnDevInst,
            int ulFlags);

        [DllImport("setupapi.dll")]
        internal static extern int CM_Get_Device_ID(
            uint dnDevInst,
            StringBuilder buffer,
            int bufferLen,
            int ulFlags);

        [DllImport("setupapi.dll")]
        internal static extern int CM_Request_Device_Eject(
            uint dnDevInst,
            out PNP_VETO_TYPE pVetoType,
            StringBuilder pszVetoName,
            int ulNameLength,
            int ulFlags
            );

        [DllImport("setupapi.dll", EntryPoint = "CM_Request_Device_Eject")]
        internal static extern int CM_Request_Device_Eject_NoUi(
            uint dnDevInst,
            IntPtr pVetoType,
            StringBuilder pszVetoName,
            int ulNameLength,
            int ulFlags
            );

        // from setupapi.h
        internal const int DIGCF_PRESENT = (0x00000002);
        internal const int DIGCF_DEVICEINTERFACE = (0x00000010);

        internal const int SPDRP_DEVICEDESC = 0x00000000;
        internal const int SPDRP_CAPABILITIES = 0x0000000F;
        internal const int SPDRP_CLASS = 0x00000007;
        internal const int SPDRP_CLASSGUID = 0x00000008;
        internal const int SPDRP_FRIENDLYNAME = 0x0000000C;

        internal const int PROPERTY_BUFFER_SIZE = 1024;

        [StructLayout(LayoutKind.Sequential)]
        internal class SP_DEVINFO_DATA
        {
            private readonly int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            internal Guid classGuid = Guid.Empty; // temp
            internal uint devInst = 0; // dumy
            internal UIntPtr reserved = UIntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            private readonly int cbSize = Marshal.SizeOf(typeof(int)) + (Marshal.SizeOf(typeof(IntPtr)) == 8 ? 4 : Marshal.SystemDefaultCharSize);
            internal char devicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SP_DEVICE_INTERFACE_DATA
        {
            private readonly int cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));
            internal Guid interfaceClassGuid; // temp
            internal uint flags = 0;
            internal UIntPtr reserved = UIntPtr.Zero;
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            int enumerator,
            IntPtr hwndParent,
            int flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr deviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            ref Guid interfaceClassGuid,
            int memberIndex,
            SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiOpenDeviceInfo(
            IntPtr deviceInfoSet,
            string deviceInstanceId,
            IntPtr hwndParent,
            int openFlags,
            SP_DEVINFO_DATA deviceInfoData
            );

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr deviceInfoSet,
            SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize,
            ref int requiredSize,
            SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            SP_DEVINFO_DATA deviceInfoData,
            int property,
            out int propertyRegDataType,
            IntPtr propertyBuffer,
            int propertyBufferSize,
            out int requiredSize
            );

        [DllImport("setupapi.dll")]
        internal static extern uint SetupDiDestroyDeviceInfoList(
            IntPtr deviceInfoSet);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if NET45
using VolumeCollection = System.Collections.Generic.IReadOnlyCollection<UsbEject.Volume>;
#else
using VolumeCollection = System.Collections.Generic.IEnumerable<UsbEject.Volume>;
#endif

namespace UsbEject
{
    /// <summary>
    /// The device class for volume devices.
    /// </summary>
    public sealed class VolumeDeviceClass : DeviceClass, VolumeCollection
    {
        #region Constructors

        internal VolumeDeviceClass()
            : base(Native.GUID_DEVINTERFACE_VOLUME)
        {
            _logicalDrives = new Lazy<IDictionary<string, string>>(GetLogicalDrives);
            _volumes = new Lazy<VolumeCollection>(GetVolumes);
        }

        #endregion

        #region CreateDevice

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
        {
            return new Volume(this, deviceInfoData, path, index);
        }

        #endregion

        #region LogicalDrives
        private readonly Lazy<IDictionary<string, string>> _logicalDrives;

        internal IDictionary<string, string> LogicalDrives
        {
            get
            {
                return _logicalDrives.Value;
            }
        }

        private IDictionary<string, string> GetLogicalDrives()
        {
            Dictionary<string, string> logicalDrives = new Dictionary<string, string>();

            StringBuilder sb = new StringBuilder(Native.CM_BUFFER_SIZE);
            foreach (string drive in Environment.GetLogicalDrives())
            {
                if (Native.GetVolumeNameForVolumeMountPoint(drive, sb, sb.Capacity))
                {
                    string volumeName = sb.ToString();
                    logicalDrives.Add(volumeName, drive.Replace("\\", ""));
                }
            }

            return logicalDrives;
        }
        #endregion

        #region Volumes
        private readonly Lazy<VolumeCollection> _volumes;

        /// <summary>
        /// Gets the list of volumes.
        /// </summary>
        public VolumeCollection Volumes
        {
            get
            {
                return _volumes.Value;
            }
        }

        private VolumeCollection GetVolumes()
        {
            List<Volume> volumes = new List<Volume>();

            foreach (Volume volume in Devices)
            {
                volumes.Add(volume);
            }

            return volumes;
        }
        #endregion

        #region VolumeCollection

        /// <inheritdoc/>
        public IEnumerator<Volume> GetEnumerator()
        {
            return Volumes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Volumes.GetEnumerator();
        }

#if NET45
        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return Volumes.Count;
            }
        }
#endif

        #endregion
    }
}

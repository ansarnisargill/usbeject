// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

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
    public class VolumeDeviceClass : DeviceClass, VolumeCollection
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the VolumeDeviceClass class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="loggerOwner">Indicates whether the device class instance owns <paramref name="logger"/>.</param>
        public VolumeDeviceClass(ILogger logger = null, bool loggerOwner = false)
            : base(new Guid(Native.GUID_DEVINTERFACE_VOLUME), logger, loggerOwner)
        {
            _logicalDrives = new Lazy<IDictionary<string, string>>(GetLogicalDrives);
            _volumes = new Lazy<VolumeCollection>(GetVolumes);
        }

        #endregion

        #region CreateDevice

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
        {
            return new Volume(this, deviceInfoData, path, index, Logger);
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
                    Logger.Log(LogLevel.Trace, "{0} ==> {1}", drive, volumeName);
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

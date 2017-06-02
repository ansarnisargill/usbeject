// UsbEject version 2.0 May 2017
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>
// updated by Dmitry Shechtman

using Chimp.Logging;
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

        /// <summary>
        /// Initializes a new instance of the VolumeDeviceClass class.
        /// </summary>
        public VolumeDeviceClass()
            : this(NoOpLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the VolumeDeviceClass class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        public VolumeDeviceClass(ILoggerFactory loggerFactory)
            : this(loggerFactory.CreateLogger<VolumeDeviceClass>())
        {
        }

        internal VolumeDeviceClass(ILogger logger)
            : base(Native.GUID_DEVINTERFACE_VOLUME, logger)
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

// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UsbEject.Library
{
    /// <summary>
    /// The device class for volume devices.
    /// </summary>
    public class VolumeDeviceClass : DeviceClass, IEnumerable<Volume>
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
            _volumes = new Lazy<IEnumerable<Volume>>(GetVolumes);
        }

        #endregion

        #region Member Overrides

        internal override Device CreateDevice(Native.SP_DEVINFO_DATA deviceInfoData, string path, int index, int disknum)
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

            StringBuilder sb = new StringBuilder(1024);
            foreach (string drive in Environment.GetLogicalDrives())
            {
                if (Native.GetVolumeNameForVolumeMountPoint(drive, sb, sb.Capacity))
                {
                    string volumeName = sb.ToString();
                    logicalDrives.Add(volumeName, drive.Replace("\\", ""));
                    Logger.Write("{0} ==> {1}", drive, volumeName);
                }
            }

            return logicalDrives;
        }
        #endregion

        #region Volumes
        private readonly Lazy<IEnumerable<Volume>> _volumes;

        /// <summary>
        /// Gets the list of volumes.
        /// </summary>
        public IEnumerable<Volume> Volumes
        {
            get
            {
                return _volumes.Value;
            }
        }

        private IEnumerable<Volume> GetVolumes()
        {
            List<Volume> volumes = new List<Volume>();

            foreach (Volume volume in Devices)
            {
                volumes.Add(volume);
            }

            return volumes;
        }
        #endregion

        #region IEnumerable

        /// <inheritdoc/>
        public IEnumerator<Volume> GetEnumerator()
        {
            return Volumes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Volumes.GetEnumerator();
        }

        #endregion
    }
}

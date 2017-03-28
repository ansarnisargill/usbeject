using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsbEject.Library;

namespace Eject
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            foreach (var volumeName in args)
            {
                VolumeDeviceClass volumes = new VolumeDeviceClass();
                foreach (Volume vol in volumes.Devices)
                {
                    if (volumeName.Equals(vol.LogicalDrive))
                    {
                        vol.Eject(false);
                        break;
                    }
                }
            }
        }
    }
}

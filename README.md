# usbeject
Working C# code to safely eject removable storage (Windows 8.1, compiled 32-bit)

This is a class library of code stolen from [this CodeProject article](https://www.codeproject.com/Articles/13530/Eject-USB-disks-using-C)
with the following modifications/additions:

* Fixed a bug where it wasn't ejecting the drive it claimed to be ejecting (@mthiffau)
* Built for .NET 2.0, 3.5 and 4.0 in addition to .NET 4.5
* Implemented thread-safe properties
* Implemented Disposable pattern

All the copyright headers are from the original author.

I'm running this code on Windows 8.1 Embedded Industrial (64-bit), but compiled as a 32-bit dll. I don't think it will work when
compiled for 64-bit, and I don't plan on doing anything about that. If you can/do, feel free to send me a pull request. Preferably
you can modify the code in such a way that it will work for both (#ifdefs, etc) so I don't have to figure out what you changed and
go back and add them myself. 

THIS WORK IS PROVIDED "AS IS", "WHERE IS" AND "AS AVAILABLE", WITHOUT ANY EXPRESS OR IMPLIED WARRANTIES OR CONDITIONS OR GUARANTEES.
YOU, THE USER, ASSUME ALL RISK IN ITS USE, INCLUDING COPYRIGHT INFRINGEMENT, PATENT INFRINGEMENT, SUITABILITY, ETC.
AUTHOR EXPRESSLY DISCLAIMS ALL EXPRESS, IMPLIED OR STATUTORY WARRANTIES OR CONDITIONS, INCLUDING WITHOUT LIMITATION, WARRANTIES OR
CONDITIONS OF MERCHANTABILITY, MERCHANTABLE QUALITY OR FITNESS FOR A PARTICULAR PURPOSE, OR ANY WARRANTY OF TITLE OR NON-INFRINGEMENT,
OR THAT THE WORK (OR ANY PORTION THEREOF) IS CORRECT, USEFUL, BUG-FREE OR FREE OF VIRUSES. YOU MUST PASS THIS DISCLAIMER ON WHENEVER
YOU DISTRIBUTE THE WORK OR DERIVATIVE WORKS.

## Usage example:

```csharp
using (VolumeDeviceClass volumes = new VolumeDeviceClass())
{
  foreach (Volume vol in volumes.Devices)
  {
    if (eject_drive.Equals(vol.LogicalDrive))
    {
      eventLog.WriteEntry("Attempting to eject drive: " + eject_drive);
      vol.Eject(false);
      eventLog.WriteEntry("Done ejecting drive.");
      break;
    }
  }
}
```

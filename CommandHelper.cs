/*
    K5TOOL UV-K5 toolkit utility
    Copyright (C) 2024  qrp73
    https://github.com/qrp73/K5TOOL

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Text;
using System.Threading;
using K5TOOL.Packets;

namespace K5TOOL
{
    public class CommandHelper
    {
        public static bool ReadEeprom(ProtocolBase protocol, int offset, int length, string fileName)
        {
            Console.WriteLine("Create {0}", fileName);
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                return protocol.ReadEeprom(offset, length, (dataOffset, data) => {
                    stream.Write(data, 0, data.Length);
                });
            }
        }

        public static bool WriteEeprom(ProtocolBase protocol, int offset, string fileName)
        {
            Console.WriteLine("Read EEPROM image from {0}", fileName);
            var data = File.ReadAllBytes(fileName);
            return protocol.WriteEeprom(offset, data);
        }

        public static bool WriteFlashPacked(Device device, string fileName)
        {
            Console.WriteLine("Read packed FLASH image from {0}...", fileName);
            var data = File.ReadAllBytes(fileName);
            Console.WriteLine("Unpack image...");
            string version;
            data = FirmwareHelper.Unpack(data, out version);
            return WriteFlash(device, version, data);
        }

        public static bool WriteFlashUnpacked(Device device, string version, string fileName)
        {
            Console.WriteLine("Read unpacked FLASH image from {0}...", fileName);
            var data = File.ReadAllBytes(fileName);
            return CommandHelper.WriteFlash(device, version, data);
        }

        public static bool WriteFlash(Device device, string version, byte[] data)
        {
            var protocol = ProtocolBase.WaitForBeacon(device);
            return protocol.WriteFlash(version, data);
        }
    }
}

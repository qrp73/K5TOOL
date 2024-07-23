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
using System.Linq;
using System.Text;

namespace K5TOOL.Packets
{
    // old:   18050000 010202020e53504a3747ff018b00c000
    // b2new: 18052000 010202061c53504a3747ff0f8c005300 322e30302e303600340a000000000020
    // b5:    7a052000 010202061c53504a3747ff1093008900 352e30302e303100280c000000000020
    public class PacketFlashBeaconAck : Packet
    {
        public const ushort ID = 0x0518;
        public const ushort ID2 = 0x057a;

        public PacketFlashBeaconAck(byte[] rawData)
            : base(rawData, true)
        {
            if (base.HdrId != ID && base.HdrId != ID2)
                throw new InvalidOperationException();
            // bootloader 2: 
            //    can be 20 bytes or 36 bytes
            //    for 20 bytes there is no version and HdrSize=0
            if (rawData.Length != 20 && rawData.Length != 36)
            {
                Console.WriteLine(
                    "WARN: sizeof({0}) = {1}, expected 20 or 36",
                    this.GetType().Name,
                    base.HdrSize);
            }
            if (rawData.Length != 20 && base.HdrSize != _rawData.Length-4)
            {
                Console.WriteLine(
                    "WARN: {0}.HdrSize != rawData.Length-4 ({1} != {2}-4)",
                    this.GetType().Name,
                    HdrSize,
                    _rawData.Length);
            }
        }

        // bootloader 2.00.06: 18052000 010202061c53504a3747ff0f8c005300 322e30302e303600340a000000000020
        // bootloader 5.00.01: 7a052000 010202061c53504a3747ff1093008900 352e30302e303100280c000000000020
        public PacketFlashBeaconAck(bool isBL5=false)
            : this(isBL5 ? 
                Utils.FromHex("7a052000010202061c53504a3747ff1093008900352e30302e303100280c000000000020") :
                Utils.FromHex("18052000010202061c53504a3747ff0f8c005300322e30302e303600340a000000000020"))
        {
        }

        // These keys are used to encrypt data for packet id=0x0516 (CMD_NVR_PUT)
        // It happens during AgencyId activation, it get 4 letter and encrypt it to 104 bytes
        public uint K0
        {
            get { return (uint)(_rawData[4] | (_rawData[5] << 8) | (_rawData[6] << 16) | (_rawData[7] << 24)); }
        }
        public uint K1
        {
            get { return (uint)(_rawData[8] | (_rawData[9] << 8) | (_rawData[10] << 16) | (_rawData[11] << 24)); }
        }
        public uint K2
        {
            get { return (uint)(_rawData[12] | (_rawData[13] << 8) | (_rawData[14] << 16) | (_rawData[15] << 24)); }
        }
        public uint K3
        {
            get { return (uint)(_rawData[16] | (_rawData[17] << 8) | (_rawData[18] << 16) | (_rawData[19] << 24)); }
        }


        public string Version
        {
            get
            {
                var len = Math.Min(16, _rawData.Length - 20);
                if (len <= 0)
                    return null;
                for (var i = 0; i < len; i++)
                    if (_rawData[20 + i] == 0)
                    {
                        len = i;
                        break;
                    }
                return Encoding.UTF8.GetString(_rawData, 20, len);
            }
        }

        public override string ToString()
        {
            if (Version == null)
            {
                return string.Format(
                    "{0} {{\n" +
                    "  HdrId=0x{1:x4}\n" +
                    "  HdrSize={2}\n" +
                    "  K0=0x{3:x8}\n" +
                    "  K1=0x{4:x8}\n" +
                    "  K2=0x{5:x8}\n" +
                    "  K3=0x{6:x8}\n" +
                    "}}",
                    this.GetType().Name,
                    HdrId,
                    HdrSize,
                    K0,
                    K1,
                    K2,
                    K3);
            }
            return string.Format(
                "{0} {{\n" +
                "  HdrId=0x{1:x4}\n" +
                "  HdrSize={2}\n" +
                "  Version=\"{3}\"\n"+
                "  K0=0x{4:x8}\n" +
                "  K1=0x{5:x8}\n" +
                "  K2=0x{6:x8}\n" +
                "  K3=0x{7:x8}\n" +
                "}}",
                this.GetType().Name,
                HdrId,
                HdrSize,
                Version,
                K0,
                K1,
                K2,
                K3);
        }
    }
}

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
    // bootloader 2: 
    //    can be 20 bytes or 36 bytes
    //    for 20 bytes there is no version
    //                   
    public class PacketFlashBeaconAck : Packet
    {
        public const ushort ID = 0x0518;
        public const ushort ID2 = 0x057a;

        public PacketFlashBeaconAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID && base.HdrId != ID2)
                throw new InvalidOperationException();
            if (base.HdrSize < 18 || base.HdrSize > 50)
            {
                /*throw new InvalidOperationException(
                    string.Format(
                        "{0}.HdrSize = {1}, expected range {2}..{3}", 
                        this.GetType().Name, 
                        base.HdrSize, 18, 50));
                */                       
                Console.WriteLine(
                    "WARN: {0}.HdrSize = {1}, expected range {2}..{3}",
                    this.GetType().Name,
                    base.HdrSize, 18, 50);
            }
        }

        // bootloader 2.00.06
        // 18 05 20 00
        // 01 02 02 0B 0C 53 46 34 52 59 FF 08 8C 00 32 00 
        // 32 2E 30 30 2E 30 36 00 34 0A 00 00 00 00 00 20
        // bootloader 5.00.01
        // 7a 05 20 00
        // 01 02 02 06 1c 53 50 4a 37 47 ff 10 93 00 89 00 
        // 35 2e 30 30 2e 30 31 00 28 0c 00 00 00 00 00 20
        public PacketFlashBeaconAck(bool isBL5=false)
            : this(isBL5 ? 
                Utils.FromHex("7a052000010202061c53504a3747ff1093008900352e30302e303100280c000000000020") :
                Utils.FromHex("180520000102020b0c5346345259ff088c003200322e30302e303600340a000000000020"))
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
                var len = Math.Min(12, _rawData.Length - 0x10 - 4);
                if (len < 0)
                    return null;
                for (var i = 0; i < len; i++)
                    if (_rawData[i + 4 + 0x10] == 0)
                    {
                        len = i;
                        break;
                    }
                return Encoding.UTF8.GetString(_rawData, 4 + 0x10, len);
            }
        }

        public override string ToString()
        {
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

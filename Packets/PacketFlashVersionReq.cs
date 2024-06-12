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
    // 30051000322e30312e3333000000000000000000
    public class PacketFlashVersionReq : Packet
    {
        public const ushort ID = 0x0530;

        public PacketFlashVersionReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 0x10)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 0x10);
            }
        }

        public PacketFlashVersionReq(string versionString = "2.01.23"/*"*.01.23"*/)
            : base(MakePacketBuffer(versionString))
        {
        }

        // 0x30, 0x5, 0x10, 0x0, '2', '.', '0', '1', '.', '2', '3', 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
        // 0x30, 0x5, 0x10, 0x0, 0x32, 0x2e, 0x30, 0x31, 0x2e, 0x32, 0x33, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
        private static byte[] MakePacketBuffer(string versionString)
        {
            if (versionString.Length > 15)
                throw new ArgumentOutOfRangeException("versionString");
            var data = Encoding.ASCII.GetBytes(versionString);
            var hdrSize = 16;

            var buf = new byte[4 + hdrSize];      //20
            buf[0] = 0x30;
            buf[1] = 0x05;
            buf[2] = (byte)hdrSize;             // 0x10
            buf[3] = (byte)(hdrSize >> 8);      // 0x00
            Array.Copy(data, 0, buf, 4, data.Length);
            for (var i=data.Length; i < 16; i++)
                buf[4+i] = 0x00;
            return buf;
        }

        public string Version
        {
            get
            {
                var size = Math.Min(HdrSize, _rawData.Length - 4);
                for (var i=0; i < size; i++)
                {
                    if (_rawData[4+i] == 0)
                    {
                        size = i;
                        break;
                    }
                }
                return Encoding.ASCII.GetString(_rawData, 4, size);
            }
        }

        public uint Unknown0
        {
            get { return (uint)(_rawData[12] | (_rawData[13] << 8) | (_rawData[14] << 16) | (_rawData[15] << 24)); }
        }

        public uint Unknown1
        {
            get { return (uint)(_rawData[16] | (_rawData[17] << 8) | (_rawData[18] << 16) | (_rawData[19] << 24)); }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Version={2}\n" +
                "  Unknown0={3}\n" +
                "  Unknown1={4}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Version,
                Unknown0,
                Unknown1);
        }
    }
}

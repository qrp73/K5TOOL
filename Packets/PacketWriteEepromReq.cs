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

namespace K5TOOL.Packets
{
    public class PacketWriteEepromReq : Packet
    {
        public const ushort ID = 0x051d;

        public PacketWriteEepromReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 8)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 8);
            }
        }

        public PacketWriteEepromReq(ushort offset, byte[] data, byte bAllowPassword=1, uint timestamp=0x6457396a)
            : base (MakePacketBuffer(offset, data, bAllowPassword, timestamp))
        {
        }

        private static byte[] MakePacketBuffer(ushort offset, byte[] data, byte bAllowPassword, uint timestamp)
        {
            if (data.Length > 0xff-8)
                throw new ArgumentOutOfRangeException("data");
            var buf = new byte[12 + data.Length];
            buf[0] = 0x1d;
            buf[1] = 0x05;
            buf[2] = (byte)(data.Length + 8);
            buf[3] = 0;
            buf[4] = (byte)offset;
            buf[5] = (byte)(offset >> 8);
            buf[6] = (byte)data.Length;
            buf[7] = bAllowPassword;
            buf[8] = (byte)timestamp;
            buf[9] = (byte)(timestamp >> 8);
            buf[10] = (byte)(timestamp >> 16);
            buf[11] = (byte)(timestamp >> 24);
            Array.Copy(data, 0, buf, 12, data.Length);
            return buf;
        }

        public ushort Offset
        {
            get { return (ushort)(_rawData[4] | (_rawData[5] << 8)); }
        }

        public byte Size
        {
            get { return _rawData[6]; }
        }

        public byte IsAllowPassword
        {
            get { return _rawData[7]; }
        }

        public uint Timestamp
        {
            get { return (uint)(_rawData[8] | (_rawData[9] << 8) | (_rawData[10] << 16) | (_rawData[11] << 24)); }
        }

        public byte[] Data
        {
            get
            {
                var data = new byte[Size];
                Array.Copy(_rawData, 12, data, 0, data.Length);
                return data;
            }
        }



        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Offset=0x{2:x4}\n" +
                "  Size=0x{3:x2}\n" +
                "  IsAllowPassword={4}\n" +
                "  Timestamp=0x{5:x8}\n" +
                "  Data={6}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Offset,
                Size,
                IsAllowPassword,
                Timestamp,
                Utils.ToHex(Data));
        }
    }
}

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
namespace K5TOOL.Packets
{
    // ?? 1a050800 8a8d9f1d 0000 0000
    public class PacketFlashWriteAck : Packet
    {
        public const ushort ID = 0x051a;

        public PacketFlashWriteAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize != 8)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 8);
            }
        }

        public PacketFlashWriteAck(ushort offset, uint id = 0x1d9f8d8a)
            : this(MakePacketBuffer(id, offset, 0x0000))
        {
        }

        private static byte[] MakePacketBuffer(uint id, ushort offset, ushort padding)
        {
            var hdrSize = 8;
            var buf = new byte[12];
            buf[0] = 0x1a;
            buf[1] = 0x05;
            buf[2] = (byte)hdrSize;
            buf[3] = (byte)(hdrSize >> 8);
            buf[4] = (byte)id;
            buf[5] = (byte)(id >> 8);
            buf[6] = (byte)(id >> 16);
            buf[7] = (byte)(id >> 24);
            buf[8] = (byte)(offset >> 8);
            buf[9] = (byte)offset;
            buf[10] = (byte)(padding >> 8);
            buf[11] = (byte)padding;
            return buf;
        }

        public uint Id
        {
            get { return (uint)(_rawData[4] | (_rawData[5] << 8) | (_rawData[6] << 16) | (_rawData[7] << 24)); }
        }

        public ushort Offset
        {
            get { return (ushort)((_rawData[8] << 8) | _rawData[9]); }
        }

        public ushort Padding
        {
            get { return (ushort)((_rawData[10] << 8) | _rawData[11]); }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Id=0x{2:x8}\n" +
                "  Offset=0x{3:x4}\n" +
                "  Padding=0x{4:x4}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Id,
                Offset,
                Padding);
        }
    }
}

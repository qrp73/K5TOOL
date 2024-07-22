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
    // OK:      1a0508008a8d9f1d00000000
    // bad id:  1a0508000000000000000100
    // bad ver: 1a0508000000000000000100
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

        public PacketFlashWriteAck(ushort chunkNumber, uint sequenceId = 0x1d9f8d8a)
            : this(MakePacketBuffer(sequenceId, chunkNumber, 0x0000))
        {
        }

        private static byte[] MakePacketBuffer(uint sequenceId, ushort chunkNumber, ushort padding)
        {
            var hdrSize = 8;
            var buf = new byte[12];
            buf[0] = 0x1a;
            buf[1] = 0x05;
            buf[2] = (byte)hdrSize;
            buf[3] = (byte)(hdrSize >> 8);
            buf[4] = (byte)sequenceId;
            buf[5] = (byte)(sequenceId >> 8);
            buf[6] = (byte)(sequenceId >> 16);
            buf[7] = (byte)(sequenceId >> 24);
            buf[8] = (byte)chunkNumber;
            buf[9] = (byte)(chunkNumber >> 8);
            buf[10] = (byte)padding;
            buf[11] = (byte)(padding >> 8);
            return buf;
        }

        public uint SequenceId
        {
            get { return (uint)(_rawData[4] | (_rawData[5] << 8) | (_rawData[6] << 16) | (_rawData[7] << 24)); }
        }

        public ushort ChunkNumber
        {
            get { return (ushort)(_rawData[8] | (_rawData[9] << 8)); }
        }

        // 0=OK
        // 1=Error
        public byte Result
        {
            get { return _rawData[10]; }
        }
        // What is this?
        public byte V0
        {
            get { return _rawData[11]; }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  SequenceId=0x{2:x8}\n" +
                "  ChunkNumber=0x{3:x4}\n" +
                "  Result={4}\n" +
                "  V0={5}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                SequenceId,
                ChunkNumber,
                Result,
                V0);
        }
    }
}

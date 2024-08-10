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
namespace K5TOOL.Packets.V2
{
    // OK:      1a0508008a8d9f1d00000000
    // bad id:  1a0508000000000000000100
    // bad ver: 1a0508000000000000000100
    public class Packet2FlashWriteAck : PacketFlashWriteAck
    {
        public const ushort ID = 0x051a;

        public Packet2FlashWriteAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
        }

        public Packet2FlashWriteAck(ushort chunkNumber, uint sequenceId = 0x1d9f8d8a)
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
    }
}

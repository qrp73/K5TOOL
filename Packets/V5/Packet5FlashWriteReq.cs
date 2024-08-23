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

namespace K5TOOL.Packets.V5
{
    // $ arm-none-eabi-objdump -D -b binary -marm --start-address=0x338 --stop-address=0x464 -Mforce-thumb bootloader.bin
    // 7b05 0c01 945d6a2c 0000 e600 0001 0000 88130020d5000000d9000000db00000000000000000000000000000000000000000000000000000000000000dd0000000000000000000000df00000025c40000e3000000e5000000e7000000e9000000eb000000ed000000ef000000f1000000f3000000f5000000f7000000f9000000fb000000fd000000ff00000001010000030100000501000007010000090100000b0100000d0100000f01000011010000130100001501000017010000190100001b0100001d0100001f010000210100000348854600f058fb00480047edd100008813002013480047fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7
    // NOTE: data content should be encrypted with AES-CBC-128 with key selected by Packet5FlashVersionReq.KeyNumber
    public class Packet5FlashWriteReq : PacketFlashWriteReq
    {
        public const ushort ID = 0x057b;

        public Packet5FlashWriteReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
        }

        public Packet5FlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data, int dataLength)
            : this(chunkNumber, chunkCount, data, dataLength, 0x1d9f8d8a)
        {
        }

        public Packet5FlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data, int dataLength, uint id/*=0x1d9f8d8a*/)
            : base(MakePacketBuffer(id, chunkNumber, chunkCount, data, dataLength, 0x0000))
        {
            if (chunkNumber > chunkCount)
                throw new ArgumentOutOfRangeException("chunkNumber");
            if (chunkCount*0x100 > FirmwareConstraints.MaxFlashAddr + 1)
                throw new InvalidOperationException(
                    string.Format(
                        "chunkCount={0:x4}!",
                        chunkCount));
        }

        private static byte[] MakePacketBuffer(uint id, ushort chunkNumber, ushort chunkCount, byte[] data, int dataLength, ushort padding)
        {
            if (data.Length != 0x100)
                throw new ArgumentOutOfRangeException("data");
            if (dataLength > 0x100)
                throw new ArgumentOutOfRangeException("data");
            if ((chunkCount & 0xff00) != 0)
                throw new ArgumentOutOfRangeException("chunkCount>0x100 is not tested yet");
            var length = dataLength;
            var buf = new byte[16 + 0x100];
            var hdrSize = buf.Length - 4;
            if (hdrSize != 0x010c)
                throw new ArgumentOutOfRangeException("hdrSize");
            buf[0] = 0x7b;
            buf[1] = 0x05;
            buf[2] = (byte)hdrSize;             // 0x0c
            buf[3] = (byte)(hdrSize >> 8);      // 0x01                   
            buf[4] = (byte)id;                  // 0x8a
            buf[5] = (byte)(id >> 8);           // 0x8d
            buf[6] = (byte)(id >> 16);          // 0x9f
            buf[7] = (byte)(id >> 24);          // 0x1d
            buf[8] = (byte)chunkNumber;
            buf[9] = (byte)(chunkNumber >> 8);
            buf[10] = (byte)chunkCount;
            buf[11] = (byte)(chunkCount >> 8);
            buf[12] = (byte)length;
            buf[13] = (byte)(length >> 8);
            buf[14] = (byte)padding;            // 0x00
            buf[15] = (byte)(padding >> 8);     // 0x00
            Array.Copy(data, 0, buf, 16, data.Length);
            // fill unused page space with 0xff to extend flash memory lifetime
            for (var i = 16 + data.Length; i < buf.Length; i++)
            {
                buf[i] = 0xff;
            }
            return buf;
        }
    }
}

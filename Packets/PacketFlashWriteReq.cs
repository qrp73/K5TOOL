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
    // 19050c01945d6a2c0000e6000001000088130020d5000000d9000000db00000000000000000000000000000000000000000000000000000000000000dd0000000000000000000000df00000025c40000e3000000e5000000e7000000e9000000eb000000ed000000ef000000f1000000f3000000f5000000f7000000f9000000fb000000fd000000ff00000001010000030100000501000007010000090100000b0100000d0100000f01000011010000130100001501000017010000190100001b0100001d0100001f010000210100000348854600f058fb00480047edd100008813002013480047fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7fee7
    // 19050c01272f5d07e500e600f4000000fff74eff2e4948602c480830fff748ff2b49886029480c30fff742ff2849c860f920c000fff736ff2549403988632548fff730ff22494039c86322482030fff729ff214908623f204001fff723ff1e494862fb20c000fff71dff1b49086319481030fff717ff184948637d200001fff711ff1549886313480830fff70bff1249c863f720c000fff705ff104908600d480838fff7fffe044601208007806801214906084349018860e0b209490863a001800d48630020fff713ff10bd18f0000080000040c40700000008004078040020c0a00b40e6006cdc023093e8034201021c014201021801021001d2ff53ffff04ebff00ff000000000000000000000000
    public class PacketFlashWriteReq : Packet
    {
        public const ushort ID = 0x0519;

        public PacketFlashWriteReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 12)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 12);
            }
        }

        public PacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data)
            : this(chunkNumber, chunkCount, data, 0x1d9f8d8a)
        {
        }

        public PacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data, uint id/*=0x1d9f8d8a*/)
            : base(MakePacketBuffer(id, chunkNumber, chunkCount, data, 0x0000))
        {
            if (chunkNumber > chunkCount)
                throw new ArgumentOutOfRangeException("chunkNumber");
            if (chunkCount > FirmwareConstraints.MaxFlashAddr + 1)
                throw new InvalidOperationException(
                    string.Format(
                        "chunkCount={0:x4}!",
                        chunkCount));
        }

        // 0x19, 0x5, 0xc, 0x1, 0x8a, 0x8d, 0x9f, 0x1d, address_msb, address_lsb, address_final_msb, address_final_lsb, length_msb, length_lsb, 0x0, 0x0, ...data
        private static byte[] MakePacketBuffer(uint id, ushort chunkNumber, ushort chunkCount, byte[] data, ushort padding)
        {
            if (data.Length > 0x100)
                throw new ArgumentOutOfRangeException("data");
            if ((chunkCount & 0xff00) != 0)
                throw new ArgumentOutOfRangeException("chunkCount>0x100 is not tested yet");
            var length = data.Length;
            var buf = new byte[16 + 0x100];
            var hdrSize = buf.Length - 4;
            if (hdrSize != 0x010c)
                throw new ArgumentOutOfRangeException("hdrSize");
            buf[0] = 0x19;
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

        public ushort ChunkCount
        {
            get { return (ushort)(_rawData[10] | (_rawData[11] << 8)); }
        }

        public ushort Size
        {
            get { return (ushort)(_rawData[12] | (_rawData[13] << 8)); }
        }

        public ushort Padding
        {
            get { return (ushort)(_rawData[14] | (_rawData[15] << 8)); }
        }

        public byte[] Data
        {
            get
            {
                var data = new byte[Size];
                Array.Copy(_rawData, 16, data, 0, data.Length);
                return data;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  SequenceId=0x{2:x8}\n" +
                "  ChunkNumber=0x{3:x4}\n" +
                "  ChunkCount=0x{4:x4}\n" +
                "  Size=0x{5:x2}\n" +
                "  Padding=0x{6:x4}\n" +
                "  Data={7}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                SequenceId,
                ChunkNumber,
                ChunkCount,
                Size,
                Padding,
                Utils.ToHex(Data));
        }
    }
}

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
    public abstract class PacketFlashWriteReq : Packet
    {
        public PacketFlashWriteReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrSize < 12)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 12);
            }
        }

        public virtual uint SequenceId
        {
            get { return (uint)(_rawData[4] | (_rawData[5] << 8) | (_rawData[6] << 16) | (_rawData[7] << 24)); }
        }

        public virtual ushort ChunkNumber
        {
            get { return (ushort)(_rawData[8] | (_rawData[9] << 8)); }
        }

        public virtual ushort ChunkCount
        {
            get { return (ushort)(_rawData[10] | (_rawData[11] << 8)); }
        }

        public virtual ushort Size
        {
            get { return (ushort)(_rawData[12] | (_rawData[13] << 8)); }
        }

        public virtual ushort Padding
        {
            get { return (ushort)(_rawData[14] | (_rawData[15] << 8)); }
        }

        public virtual byte[] Data
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

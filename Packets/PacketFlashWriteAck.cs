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
    public abstract class PacketFlashWriteAck : Packet
    {
        public PacketFlashWriteAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrSize != 8)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 8);
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

        // 0=OK
        // 1=Error
        public virtual byte Result
        {
            get { return _rawData[10]; }
        }

        // What is this?
        //public virtual byte V0
        //{
        //    get { return _rawData[11]; }
        //}

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  SequenceId=0x{2:x8}\n" +
                "  ChunkNumber=0x{3:x4}\n" +
                "  Result={4}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                SequenceId,
                ChunkNumber,
                Result);
        }
    }
}

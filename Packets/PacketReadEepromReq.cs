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
    public class PacketReadEepromReq : Packet
    {
        public const ushort ID = 0x051b;

        public PacketReadEepromReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize != 8)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 8);
            }
        }

        public PacketReadEepromReq(ushort offset, byte size, uint timestamp=0x6457396a)
            : base (new byte[] { 0x1b, 0x05, 0x08, 0x00, 0x80, 0x0e, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00 })
        {
            _rawData[4] = (byte)offset;
            _rawData[5] = (byte)(offset >> 8);
            _rawData[6] = (byte)size;
            _rawData[8] = (byte)timestamp;
            _rawData[9] = (byte)(timestamp >> 8);
            _rawData[10] = (byte)(timestamp >> 16);
            _rawData[11] = (byte)(timestamp >> 24);
        }

        public ushort Offset
        {
            get { return (ushort)(_rawData[4] | (_rawData[5] << 8)); }
        }

        public byte Size
        {
            get { return _rawData[6]; }
        }

        public byte Padding
        {
            get { return _rawData[7]; }
        }

        public uint Timestamp
        {
            get { return (uint)(_rawData[8] | (_rawData[9] << 8) | (_rawData[10] << 16) | (_rawData[11] << 24)); }
        }


        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Offset=0x{2:x4}\n" +
                "  Size=0x{3:x2}\n" +
                "  Padding=0x{4:x2}\n" +
                "  Timestamp=0x{5:x8}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Offset,
                Size,
                Padding,
                Timestamp);
        }
    }
}

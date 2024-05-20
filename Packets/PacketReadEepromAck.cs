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
    public class PacketReadEepromAck : Packet
    {
        public const ushort ID = 0x051c;

        public PacketReadEepromAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 8)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 8);
            }
            if (rawData.Length != base.HdrSize+4 || base.HdrSize != Size + 4)
            {
                Console.WriteLine("WARN: invalid {0} size constraint", this.GetType().Name);
            }
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

        public byte[] Data
        {
            get
            {
                var data = new byte[Size];
                Array.Copy(_rawData, 8, data, 0, data.Length);
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
                "  Padding=0x{4:x2}\n" +
                "  Data={5}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Offset,
                Size,
                Padding,
                Utils.ToHex(Data));
        }
    }
}

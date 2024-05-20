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
    // 280504007a003b29
    public class PacketReadRssiAck : Packet
    {
        public const ushort ID = 0x0528;

        public PacketReadRssiAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize != 4)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 4);
            }
        }

        public PacketReadRssiAck()
            : this(Utils.FromHex("280504007a003b29"))
        {
        }

        public ushort RSSI
        {
            get { return (ushort)(_rawData[4] | (_rawData[5] << 8)); }
        }

        public byte ExNoiseIndicator
        {
            get { return _rawData[6]; }
        }

        public byte GlitchIndicator
        {
            get { return _rawData[7]; }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  RSSI={1}\n" +
                "  ExNoiseIndicator={2}\n" +
                "  GlitchIndicator={3}\n" +
                "}}",
                this.GetType().Name,
                RSSI,
                ExNoiseIndicator,
                GlitchIndicator);
        }
    }
}

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
    // not tested yet
    public class PacketHelloTestReq : Packet
    {
        public const ushort ID = 0x052f;

        public PacketHelloTestReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize != 4)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 4);
            }
        }

        public PacketHelloTestReq(uint timestamp = 0x6457396a)
            : base(MakePacketBuffer(timestamp))
        {
        }

        private static byte[] MakePacketBuffer(uint timestamp)
        {
            var buf = new byte[4+4];
            buf[0] = 0x2f;
            buf[1] = 0x05;
            buf[2] = 0x04;
            buf[3] = 0x00;
            buf[4] = (byte)timestamp;
            buf[5] = (byte)(timestamp >> 8);
            buf[6] = (byte)(timestamp >> 16);
            buf[7] = (byte)(timestamp >> 24);
            return buf;
        }

        public uint Timestamp
        {
            get { return (uint)(_rawData[4] | (_rawData[5] << 8) | (_rawData[6] << 16) | (_rawData[7] << 24)); }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  Timestamp=0x{1:x8}\n" +
                "}}",
                this.GetType().Name,
                Timestamp);
        }
    }
}

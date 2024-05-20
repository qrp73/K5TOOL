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
using System.Text;

namespace K5TOOL.Packets
{
    public class PacketFlashBeaconAck : Packet
    {
        public const ushort ID = 0x0518;

        public PacketFlashBeaconAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 18 || base.HdrSize > 50)
            {
                /*throw new InvalidOperationException(
                    string.Format(
                        "{0}.HdrSize = {1}, expected range {2}..{3}", 
                        this.GetType().Name, 
                        base.HdrSize, 18, 50));
                */                       
                Console.WriteLine(
                    "WARN: {0}.HdrSize = {1}, expected range {2}..{3}",
                    this.GetType().Name,
                    base.HdrSize, 18, 50);
            }
        }

        // 01 02 02 0B 0C 53 46 34 52 59 FF 08 8C 00 32 00 
        // 32 2E 30 30 2E 30 36 00 34 0A 00 00 00 00 00 20
        public PacketFlashBeaconAck()
            : this(new byte[] {
            0x18,0x05,0x20,0x00,0x01,0x02,0x02,0x0b,0x0c,0x53,0x46,0x34,0x52,0x59,0xff,0x08,
            0x8c,0x00,0x32,0x00,0x32,0x2e,0x30,0x30,0x2e,0x30,0x36,0x00,0x34,0x0a,0x00,0x00,
            0x00,0x00,0x00,0x20 })
        {
        }

        // TODO: what means first 16 bytes?

        public string Version
        {
            get
            {
                var len = Math.Min(12, _rawData.Length - 0x10 - 4);
                if (len < 0)
                    return null;
                for (var i = 0; i < len; i++)
                    if (_rawData[i + 4 + 0x10] == 0)
                    {
                        len = i;
                        break;
                    }
                return Encoding.UTF8.GetString(_rawData, 4 + 0x10, len);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Version=\"{2}\"\n"+
                "  Data={3}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Version,
                Utils.ToHex(_rawData.Skip(4).Take(0x10)));
        }
    }
}

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

namespace K5TOOL.Packets.V5
{
    // $ arm-none-eabi-objdump -D -b binary -marm --start-address=0x2a8 --stop-address=0x30c -Mforce-thumb bootloader.bin
    // fake: 7d05 1000 352e30302e3035000000000000000000 00
    public class Packet5FlashVersionReq : PacketFlashVersionReq
    {
        public const ushort ID = 0x057d;

        public Packet5FlashVersionReq(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize < 0x11)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected >= {2}", this.GetType().Name, base.HdrSize, 0x11);
            }
        }

        public Packet5FlashVersionReq(byte keyNumber, string versionString = "5.00.05")
            : base(MakePacketBuffer(versionString, keyNumber))
        {
        }

        // 7d05 1000 352e30302e3035000000000000000000 00
        private static byte[] MakePacketBuffer(string versionString, byte keyNumber)
        {
            if (versionString.Length > 16)
                throw new ArgumentOutOfRangeException("versionString");
            if (keyNumber >= 16)
                throw new ArgumentOutOfRangeException("keyNumber");

            var data = Encoding.ASCII.GetBytes(versionString);
            var hdrSize = 17;
            if ((hdrSize % 4) != 0) // align hdrSize to 4 with zero padding at the end
            { 
                hdrSize += 4 - (hdrSize % 4);
            }

            var buf = new byte[4 + hdrSize];      //21
            buf[0] = 0x7d;
            buf[1] = 0x05;
            buf[2] = (byte)hdrSize;             // 0x11
            buf[3] = (byte)(hdrSize >> 8);      // 0x00
            Array.Copy(data, 0, buf, 4, data.Length);
            for (var i=data.Length; i < 16; i++)
                buf[4+i] = 0x00;
            buf[20] = keyNumber;
            return buf;
        }

        public override string Version
        {
            get
            {
                var size = Math.Min((int)HdrSize, 0x10);
                for (var i=0; i < size; i++)
                {
                    if (_rawData[4+i] == 0)
                    {
                        size = i;
                        break;
                    }
                }
                return Encoding.ASCII.GetString(_rawData, 4, size);
            }
        }

        // Select (key,iv) pair which will be used for AES decryption
        public byte KeyNumber
        {
            get { return _rawData.Length > 20 ? _rawData[20] : (byte)255; }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrSize={1}\n" +
                "  Version={2}\n" +
                "  KeyNumber={3}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Version,
                KeyNumber);
        }
    }
}

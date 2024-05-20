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
using System.Text;

namespace K5TOOL.Packets
{
    public class PacketHelloAck : Packet
    {
        public const ushort ID = 0x0515;

        public PacketHelloAck(byte[] rawData)
            : base(rawData)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
            if (base.HdrSize != 36)
            {
                Console.WriteLine("WARN: {0}.HdrSize = {1}, expected {2}", this.GetType().Name, base.HdrSize, 36);
            }
        }


        public string Version
        {
            get
            {
                var len = Math.Min(16, _rawData.Length - 4);
                if (len < 0)
                    return null;
                for (var i=0; i < len; i++)
                    if (_rawData[i+4] == 0)
                    {
                        len = i;
                        break;
                    }
                return Encoding.UTF8.GetString(_rawData, 4, len);
            }
        }

        public byte HasCustomAesKey
        {
            get { return _rawData.Length - 4 > 16 ? _rawData[4 + 16] : (byte)0; }
        }

        public byte IsPasswordLocked
        {
            get { return _rawData.Length - 4 > 17 ? _rawData[4 + 17] : (byte)0; }
        }

        public byte GetPadding(int index)
        {
            var offset = 4 + 18 + index;
            if (offset > _rawData.Length)
                return 0;
            return _rawData[offset]; 
        }

        public uint GetChallenge(int index)
        {
            var offset = 4 + 20 + 4 * index;
            if (offset > _rawData.Length)
                return 0;
            return (uint)(
                _rawData[offset] | 
                (_rawData[offset + 1] << 8) | 
                (_rawData[offset + 2] << 16) | 
                (_rawData[offset + 3] << 24));
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" + 
                "  HdrSize={1}\n" + 
                "  Version=\"{2}\"\n" + 
                "  HasCustomAesKey={3}\n" +
                "  IsPasswordLocked={4}\n" +
                "  Padding[0]=0x{5:x2}\n" +
                "  Padding[1]=0x{6:x2}\n" +
                "  Challenge[0]=0x{7:x8}\n" +
                "  Challenge[1]=0x{8:x8}\n" +
                "  Challenge[2]=0x{9:x8}\n" +
                "  Challenge[3]=0x{10:x8}\n" +
                "}}",
                this.GetType().Name,
                HdrSize,
                Version,
                HasCustomAesKey,
                IsPasswordLocked,
                GetPadding(0),
                GetPadding(1),
                GetChallenge(0),
                GetChallenge(1),
                GetChallenge(2),
                GetChallenge(3));
        }
    }
}

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
    public class Envelope
    {
        private readonly byte[] _rawData;

        public Envelope(byte[] rawData)
        {
            _rawData = rawData;
        }

        public Envelope(Packet packet)
        {
            Logger.LogTxPacket(packet);
            var data = packet.Serialize();
            Logger.LogTx(data);
            _rawData = Encode(data);
            Logger.LogTxRaw(_rawData);
        }

        public byte[] Serialize()
        {
            return _rawData;
        }

        public Packet Deserialize()
        {
            Logger.LogRxRaw(_rawData);
            var decoded = Decode(_rawData);
            Logger.LogRx(decoded);
            var packet = Packet.Deserialize(decoded);
            Logger.LogRxPacket(packet);
            return packet;
        }

        private static byte[] Encode(byte[] data)
        {
            if (data.Length > 0xffff)
            {
                throw new ArgumentOutOfRangeException("data.Length",
                    string.Format("Encode: data.Length > 0xffff ({0})", data.Length));
            }
            var encoded = new byte[data.Length + 8];
            encoded[0] = 0xab;
            encoded[1] = 0xcd;
            encoded[2] = (byte)data.Length;
            encoded[3] = (byte)(data.Length >> 8);
            Array.Copy(data, 0, encoded, 4, data.Length);
            var crc = Utils.Crc16(encoded, 4, data.Length, 0);
            encoded[4 + data.Length] = (byte)crc;
            encoded[5 + data.Length] = (byte)(crc >> 8);
            XorEncrypt(encoded, 4, data.Length + 2);
            encoded[6 + data.Length] = 0xdc;
            encoded[7 + data.Length] = 0xba;
            return encoded;
        }

        private static byte[] Decode(byte[] data)
        {
            var size = data[2] | (data[3] << 8);
            if (data[0] != 0xab || data[1] != 0xcd)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Decode: bad header {0}",
                        Utils.ToHex(data)));
            }
            if (data[6 + size] != 0xdc || data[7 + size] != 0xba)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Decode: bad footer {0}",
                        Utils.ToHex(data)));
            }
            var decoded = new byte[size + 2];
            Array.Copy(data, 4, decoded, 0, size + 2);
            XorEncrypt(decoded, 0, decoded.Length);
            var dec_crc = decoded[size] | (decoded[size + 1] << 8);

            //var crc = Utils.Crc16(decoded, 0, len, 0);
            var crc0 = _xorTable[(size + 0) % 16] ^ 0xFF;
            var crc1 = _xorTable[(size + 1) % 16] ^ 0xFF;
            var crc = crc0 | (crc1 << 8);
            //Console.WriteLine("size={0:x4}({1}) dec_crc={2:x4} crc={3:x4}", size, size % 16, dec_crc, crc);

            // 0xffff - running mode
            if (dec_crc != 0xffff && dec_crc != crc)
            {
                Console.WriteLine("WARN: Decode: crc=0x{0:x4}, expected=0x{1:x4}", crc, dec_crc);
            }
            var trim = new byte[size];
            Array.Copy(decoded, 0, trim, 0, size);
            return trim;
        }

        private static readonly byte[] _xorTable = new byte[16] {
            0x16 , 0x6c , 0x14 , 0xe6 , 0x2e , 0x91 , 0x0d , 0x40 ,
            0x21 , 0x35 , 0xd5 , 0x40 , 0x13 , 0x03 , 0xe9 , 0x80 
        };

        private static void XorEncrypt(byte[] data, int offset, int length)
        {
            for (var i = 0; i < length; i++)
            {
                data[offset + i] ^= _xorTable[i % _xorTable.Length];
            }
        }
    }
}

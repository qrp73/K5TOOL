﻿/*
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


namespace K5TOOL
{
    public static class FirmwareHelper
    {
        private static readonly ushort[] Crc16Tab = {
            0, 4129, 8258, 12387, 16516, 20645, 24774, 28903, 33032, 37161, 41290, 45419, 49548, 53677, 57806, 61935,
            4657, 528, 12915, 8786, 21173, 17044, 29431, 25302, 37689, 33560, 45947, 41818, 54205, 50076, 62463, 58334,
            9314, 13379, 1056, 5121, 25830, 29895, 17572, 21637, 42346, 46411, 34088, 38153, 58862, 62927, 50604, 54669,
            13907, 9842, 5649, 1584, 30423, 26358, 22165, 18100, 46939, 42874, 38681, 34616, 63455, 59390, 55197, 51132,
            18628, 22757, 26758, 30887, 2112, 6241, 10242, 14371, 51660, 55789, 59790, 63919, 35144, 39273, 43274, 47403,
            23285, 19156, 31415, 27286, 6769, 2640, 14899, 10770, 56317, 52188, 64447, 60318, 39801, 35672, 47931, 43802,
            27814, 31879, 19684, 23749, 11298, 15363, 3168, 7233, 60846, 64911, 52716, 56781, 44330, 48395, 36200, 40265,
            32407, 28342, 24277, 20212, 15891, 11826, 7761, 3696, 65439, 61374, 57309, 53244, 48923, 44858, 40793, 36728,
            37256, 33193, 45514, 41451, 53516, 49453, 61774, 57711, 4224, 161, 12482, 8419, 20484, 16421, 28742, 24679,
            33721, 37784, 41979, 46042, 49981, 54044, 58239, 62302, 689, 4752, 8947, 13010, 16949, 21012, 25207, 29270,
            46570, 42443, 38312, 34185, 62830, 58703, 54572, 50445, 13538, 9411, 5280, 1153, 29798, 25671, 21540, 17413,
            42971, 47098, 34713, 38840, 59231, 63358, 50973, 55100, 9939, 14066, 1681, 5808, 26199, 30326, 17941, 22068,
            55628, 51565, 63758, 59695, 39368, 35305, 47498, 43435, 22596, 18533, 30726, 26663, 6336, 2273, 14466, 10403,
            52093, 56156, 60223, 64286, 35833, 39896, 43963, 48026, 19061, 23124, 27191, 31254, 2801, 6864, 10931, 14994,
            64814, 60687, 56684, 52557, 48554, 44427, 40424, 36297, 31782, 27655, 23652, 19525, 15522, 11395, 7392, 3265,
            61215, 65342, 53085, 57212, 44955, 49082, 36825, 40952, 28183, 32310, 20053, 24180, 11923, 16050, 3793, 7920
        };

        private static ushort Crc16Ccitt(byte[] data)
        {
            ushort crc = 0;
            foreach (var b in data)
            {
                crc = (ushort)(Crc16Tab[(crc >> 8 ^ b) & 0xff] ^ (crc << 8));
            }
            return crc;
        }

        private static byte[] Crc16CcittLe(byte[] data)
        {
            ushort crc = Crc16Ccitt(data);
            return new byte[] { (byte)(crc & 0xff), (byte)(crc >> 8) };
        }

        private static readonly byte[] XOR_ARRAY = {
            0x47, 0x22, 0xc0, 0x52, 0x5d, 0x57, 0x48, 0x94, 0xb1, 0x60, 0x60, 0xdb, 0x6f, 0xe3, 0x4c, 0x7c,
            0xd8, 0x4a, 0xd6, 0x8b, 0x30, 0xec, 0x25, 0xe0, 0x4c, 0xd9, 0x00, 0x7f, 0xbf, 0xe3, 0x54, 0x05,
            0xe9, 0x3a, 0x97, 0x6b, 0xb0, 0x6e, 0x0c, 0xfb, 0xb1, 0x1a, 0xe2, 0xc9, 0xc1, 0x56, 0x47, 0xe9,
            0xba, 0xf1, 0x42, 0xb6, 0x67, 0x5f, 0x0f, 0x96, 0xf7, 0xc9, 0x3c, 0x84, 0x1b, 0x26, 0xe1, 0x4e,
            0x3b, 0x6f, 0x66, 0xe6, 0xa0, 0x6a, 0xb0, 0xbf, 0xc6, 0xa5, 0x70, 0x3a, 0xba, 0x18, 0x9e, 0x27,
            0x1a, 0x53, 0x5b, 0x71, 0xb1, 0x94, 0x1e, 0x18, 0xf2, 0xd6, 0x81, 0x02, 0x22, 0xfd, 0x5a, 0x28,
            0x91, 0xdb, 0xba, 0x5d, 0x64, 0xc6, 0xfe, 0x86, 0x83, 0x9c, 0x50, 0x1c, 0x73, 0x03, 0x11, 0xd6,
            0xaf, 0x30, 0xf4, 0x2c, 0x77, 0xb2, 0x7d, 0xbb, 0x3f, 0x29, 0x28, 0x57, 0x22, 0xd6, 0x92, 0x8b
        };

        private static byte[] FirmwareXor(byte[] fwcontent)
        {
            int xorLen = XOR_ARRAY.Length;
            for (int i = 0; i < fwcontent.Length; i++)
            {
                fwcontent[i] ^= XOR_ARRAY[i % xorLen];
            }
            return fwcontent;
        }

        public static byte[] Unpack(byte[] encodedFirmware, out string rawVersion)
        {
            if (Crc16CcittLe(encodedFirmware.Take(encodedFirmware.Length - 2).ToArray()).SequenceEqual(encodedFirmware.Skip(encodedFirmware.Length - 2).ToArray()))
            {
                Console.WriteLine("CRC check passed...");
            }
            else
            {
                throw new InvalidOperationException("WARNING: CRC CHECK FAILED! FIRMWARE NOT VALID!\nMake sure to choose a flashable bin file.");
            }

            byte[] decodedFirmware = FirmwareXor(encodedFirmware.Take(encodedFirmware.Length - 2).ToArray());
            const int versionInfoOffset = 0x2000;
            const int versionInfoLength = 16;
            byte[] result = new byte[decodedFirmware.Length - versionInfoLength];

            Array.Copy(decodedFirmware, 0, result, 0, versionInfoOffset);
            Array.Copy(decodedFirmware, versionInfoOffset + versionInfoLength, result, versionInfoOffset, decodedFirmware.Length - versionInfoOffset - versionInfoLength);

            var rawVersionBuf = decodedFirmware.Skip(versionInfoOffset).Take(versionInfoLength).ToArray();
            var rawVersionSize = rawVersionBuf.Length;
            for (var i = 0; i < rawVersionBuf.Length; i++)
            {
                if (rawVersionBuf[i] == 0)
                {
                    rawVersionSize = i;
                    break;
                }
            }
            rawVersion = Encoding.ASCII.GetString(rawVersionBuf, 0, rawVersionSize);
            return result;
        }

        public static byte[] Pack(byte[] decodedFirmware, string rawVersion)
        {
            if (rawVersion.Length > 16)
                throw new ArgumentOutOfRangeException("rawVersion");
            const int versionInfoOffset = 0x2000;
            const int versionInfoLength = 16;

            var rawVersionBuf = new byte[versionInfoLength];
            var tmp = Encoding.ASCII.GetBytes(rawVersion);
            Array.Copy(tmp, rawVersionBuf, Math.Min(tmp.Length, rawVersionBuf.Length));

            byte[] result = new byte[versionInfoLength + decodedFirmware.Length];

            Array.Copy(decodedFirmware, 0, result, 0, versionInfoOffset);
            Array.Copy(rawVersionBuf, 0, result, versionInfoOffset, versionInfoLength);
            Array.Copy(decodedFirmware, versionInfoOffset, result, versionInfoOffset + versionInfoLength, decodedFirmware.Length - versionInfoOffset);

            byte[] firmwareWithVersionEncoded = FirmwareXor(result);
            byte[] crc = Crc16CcittLe(firmwareWithVersionEncoded);

            byte[] packedFirmware = new byte[firmwareWithVersionEncoded.Length + crc.Length];
            Array.Copy(firmwareWithVersionEncoded, 0, packedFirmware, 0, firmwareWithVersionEncoded.Length);
            Array.Copy(crc, 0, packedFirmware, firmwareWithVersionEncoded.Length, crc.Length);

            return packedFirmware;
        }
    }
}

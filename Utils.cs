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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace K5TOOL
{
    public class Utils
    {
        public static ushort Crc16(byte[] data, int offset, int length, ushort initCrc)
        {
            const int poly = 0x1021;
            int crc = initCrc;
            int index = 0;
            for (var num = length; num > 0; num--)               /* Step through bytes in memory */
            {
                crc = crc ^ (data[offset + index++] << 8);      /* Fetch byte from memory, XOR into CRC top byte*/
                for (var i = 0; i < 8; i++)              /* Prepare to rotate 8 bits */
                {
                    crc = crc << 1;                /* rotate */
                    if ((crc & 0x10000) != 0)             /* bit 15 was set (now bit 16)... */
                        crc = (crc ^ poly) & 0xFFFF; /* XOR with XMODEM polynomic */
                                                     /* and ensure CRC remains 16-bit value */
                }                              /* Loop for 8 bits */
            }                                /* Loop until num=0 */
            return (ushort)crc;                     /* Return updated CRC */
        }

        public static uint ToUnixTimeSeconds(DateTime dt)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (uint)dt.Subtract(unixEpoch).TotalSeconds;
        }

        public static int ParseNumber(string v)
        {
            v = v.Trim();
            if (v.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return int.Parse(v.Substring(2), NumberStyles.HexNumber);
            return int.Parse(v);
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Trim().Replace(" ", "").Replace("\t", "").ToLowerInvariant();
            if (!hex.ToCharArray().All(arg => char.IsDigit(arg) || (arg >= 'a' && arg <= 'f')))
                throw new ArgumentOutOfRangeException("hex");
            if ((hex.Length & 1)==1)
                throw new ArgumentOutOfRangeException("hex");
            var data = new byte[hex.Length / 2];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(hex.Substring(i*2, 2), 16);
            }
            return data;
        }

        public static string ToHex(IEnumerable<byte> data)
        {
            return string.Join("", data.Select(arg => arg.ToString("x2")).ToArray());
        }
    }
}

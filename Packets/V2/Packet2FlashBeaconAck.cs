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

namespace K5TOOL.Packets.V2
{
    // old:   18050000 010202020e53504a3747ff018b00c000
    // b2new: 18052000 010202061c53504a3747ff0f8c005300 322e30302e303600340a000000000020
    public class Packet2FlashBeaconAck : PacketFlashBeaconAck
    {
        public const ushort ID = 0x0518;

        public Packet2FlashBeaconAck(byte[] rawData)
            : base(rawData, true)
        {
            if (base.HdrId != ID)
                throw new InvalidOperationException();
        }

        // bootloader 2.00.06: 18052000 010202061c53504a3747ff0f8c005300 322e30302e303600340a000000000020
        // bootloader 5.00.01: 7a052000 010202061c53504a3747ff1093008900 352e30302e303100280c000000000020
        public Packet2FlashBeaconAck()
            : this(Utils.FromHex("18052000010202061c53504a3747ff0f8c005300322e30302e303600340a000000000020"))
        {
        }
    }
}

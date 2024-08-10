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
using K5TOOL.Packets.V2;
using K5TOOL.Packets.V5;

namespace K5TOOL.Packets
{
    // https://github.com/losehu/uv-k5-firmware-custom/blob/main/app/uart.c
    public class Packet
    {
        protected readonly byte[] _rawData;

        public Packet(byte[] rawData, bool suppressWarning=false)
        {
            _rawData = rawData;
            // validate packet size constraints
            if (!suppressWarning && _rawData.Length != HdrSize+4)
            {
                Console.WriteLine(
                    "WARN: {0}.HdrSize != rawData.Length-4 ({1} != {2}-4)",
                    this.GetType().Name,
                    HdrSize,
                    _rawData.Length);
            }
        }

        public byte[] RawData { get { return _rawData; } }

        public ushort HdrId
        {
            get { return (ushort)(_rawData[0] | (_rawData[1] << 8)); }
        }

        public ushort HdrSize
        {
            get { return (ushort)(_rawData[2] | (_rawData[3] << 8)); }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {{\n" +
                "  HdrId=0x{1:x4}\n" +
                "  HdrSize=0x{2:x4}\n" +
                "  Data={3}\n" +
                "}}",
                this.GetType().Name,
                HdrId,
                HdrSize,
                Utils.ToHex(_rawData.Skip(4)));
        }

        public virtual byte[] Serialize()
        {
            return _rawData;
        }

        public static Packet Deserialize(byte[] data)
        {
            if (data.Length < 4)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Packet.Deserialize: packet length < 2 ({0})", data.Length));
            }
            var id = data[0] | (data[1] << 8);
            switch (id)
            {
                // bootloader
                case Packet2FlashBeaconAck.ID: return new Packet2FlashBeaconAck(data);
                case Packet2FlashVersionReq.ID: return new Packet2FlashVersionReq(data);
                case Packet2FlashWriteAck.ID: return new Packet2FlashWriteAck(data);
                case Packet2FlashWriteReq.ID: return new Packet2FlashWriteReq(data);
                // bootloader 5.00.01
                case Packet5FlashBeaconAck.ID: return new Packet5FlashBeaconAck(data);
                case Packet5FlashVersionReq.ID: return new Packet5FlashVersionReq(data);
                case Packet5FlashWriteAck.ID: return new Packet5FlashWriteAck(data);
                case Packet5FlashWriteReq.ID: return new Packet5FlashWriteReq(data);

                // firmware
                case PacketHelloAck.ID: return new PacketHelloAck(data);
                case PacketHelloReq.ID: return new PacketHelloReq(data);
                case PacketHelloTestReq.ID: return new PacketHelloTestReq(data);
                case PacketReadAdcAck.ID: return new PacketReadAdcAck(data);
                case PacketReadAdcReq.ID: return new PacketReadAdcReq(data);
                case PacketReadEepromAck.ID: return new PacketReadEepromAck(data);
                case PacketReadEepromReq.ID: return new PacketReadEepromReq(data);
                case PacketReadRssiAck.ID: return new PacketReadRssiAck(data);
                case PacketReadRssiReq.ID: return new PacketReadRssiReq(data);
                case PacketRebootReq.ID: return new PacketRebootReq(data);
                case PacketWriteEepromAck.ID: return new PacketWriteEepromAck(data);
                case PacketWriteEepromReq.ID: return new PacketWriteEepromReq(data);

                default:
                    var p = new Packet(data);
                    Console.WriteLine("WARN: unknown packet {0}", p);
                    return p;
            }
        }
    }
}

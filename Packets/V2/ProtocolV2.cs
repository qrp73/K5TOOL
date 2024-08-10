using System;

namespace K5TOOL.Packets.V2
{
    public class ProtocolV2 : ProtocolBase
    {
        public ProtocolV2(Device device)
            : base(device)
        {
        }

        public override PacketFlashVersionReq CreatePacketFlashVersionReq(string version)
        {
            return version != null ?
                new Packet2FlashVersionReq(version) :
                new Packet2FlashVersionReq();
        }

        public override PacketFlashWriteReq CreatePacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] subData, uint seqId)
        {
            return new Packet2FlashWriteReq(chunkNumber, chunkCount, subData, seqId);
        }

        public override PacketFlashBeaconAck CreatePacketFlashBeaconAck()
        {
            return new Packet2FlashBeaconAck();
        }

        public override PacketFlashWriteAck CreatePacketFlashWriteAck(ushort chunkNumber, uint sequenceId)
        {
            return new Packet2FlashWriteAck(chunkNumber, sequenceId);
        }
    }
}

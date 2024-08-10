using System;
using System.Linq;
using System.Security.Cryptography;

namespace K5TOOL.Packets.V5
{
    public class ProtocolV5 : ProtocolBase
    {
        private readonly byte _keyNumber = 0;

        public ProtocolV5(Device device)
            : base(device)
        {
        }

        public override PacketFlashVersionReq CreatePacketFlashVersionReq(string version)
        {
            return version != null ?
                new Packet5FlashVersionReq(_keyNumber, version) :
                new Packet5FlashVersionReq(_keyNumber);
        }

        public override PacketFlashWriteReq CreatePacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] subData, uint seqId)
        {
            return new Packet5FlashWriteReq(chunkNumber, chunkCount, subData, seqId);
        }

        public override PacketFlashBeaconAck CreatePacketFlashBeaconAck()
        {
            return new Packet5FlashBeaconAck();
        }

        public override PacketFlashWriteAck CreatePacketFlashWriteAck(ushort chunkNumber, uint sequenceId)
        {
            return new Packet5FlashWriteAck(chunkNumber, sequenceId);
        }

/*
        key 00: e16e0d29e0c83418987f9433f5ff620e
        iv  00: 14b7a2be0223e259b2066d8886977e36
        key 01: b0d93af7761a50cacb966eb8a805bcbb
        iv  01: 916c50fb9e480693b155b2e555cb780a
        key 02: 1357cc24d138a57799dd0eec5b9a18f9
        iv  02: fb149d4c45e7d7a95aa64bc22765a8c0
        key 03: a8ef4f917688c5c1487f2a6a811e554d
        iv  03: 32c860dfce65ca302ec534aa5f88884b
        key 04: 97a387567b234b55c921cd82f65f0087
        iv  04: e8ab4e0e344cb5a0b1e7db7a05d468c3
        key 05: f0240e02ed5da965539d815306f2d34b
        iv  05: c6116bd5bea38673746205ee534d589f
        key 06: 734dd8ac84c0c422cbca28fec8856473
        iv  06: 4bfe35582436578014e970ed8dab9ea0
        key 07: 7f67480570ba5254d7cee59ca2a483b5
        iv  07: 96858cdf59454a5fa84faa3663748b8d
        key 08: 67ec7c38cefa9edb38bb89b58986eb1a
        iv  08: 2f9a34bcec302bc6c5b6bc552c166dc9
        key 09: 08ea3a7dc8a6e961b7061f7e52ec8e6d
        iv  09: 5f6f2ecd835d760b5c1e78f0be1a8787
        key 10: 4e324c565e32be8a6aaf26d9ff37ee87
        iv  10: 3d680b15a6274c72bc3313a183f1372b
        key 11: 4fd9d7243916543c23ad995937e4bb7c
        iv  11: a10be7032960c6d5dcbdc6b4ecfbe31a
        key 12: a1d6c61c8c5c2281dc7e371ca3c3f168
        iv  12: 925eb778232e8b4b1f06a582498b2149
        key 13: 11e065ed3f9e8b96bae61f79a7d8a317
        iv  13: 8b67978471fbd371dcf44ed92f56562d
        key 14: c4636958e1830f23c6d9ce15b2ddc35a
        iv  14: 5b7237ddb9c5290e15519018ceacba76
        key 15: bf9862d680f948195f5c90545e1d8578
        iv  15: 8172ea14916b606855ff2aabe52e993c
*/

        private Aes _aes;
        private ICryptoTransform _aesEncoder;

        public override void WriteFlashInit()
        {
            // WARNING: do not remove exception, with no proper key it can brick your radio
            throw new NotImplementedException("AES key initialization not implemented yet");
            var key = Utils.FromHex("e16e0d29e0c83418987f9433f5ff620e");
            var iv = Utils.FromHex("14b7a2be0223e259b2066d8886977e36");
            //TODO: implement key2 generation from (key,iv) pair, or just put pre-computed value
            var key2 = Utils.FromHex("00000000000000000000000000000000");
            _aes = Aes.Create();
            _aes.Key = key;
            _aes.IV  = iv;
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.None;
            _aes.BlockSize = 128;
            _aesEncoder = _aes.CreateEncryptor(key2, iv);
        }

        public override void WriteFlashFinish()
        {
            if (_aesEncoder != null)
            {
                _aesEncoder.Dispose();
                _aesEncoder = null;
            }
            if (_aes != null)
            {
                _aes.Dispose();
                _aes = null;
            }
        }

        public override byte[] WriteFlashEncrypt(byte[] data)
        {
            // TODO: check if byte order is correct
            var subData = Enumerable.Range(0, data.Length / 4)
                .Select(arg => BitConverter.ToUInt32(data, arg * 4))
                .Select(bswap_32)
                .SelectMany(arg => BitConverter.GetBytes(arg))
                .ToArray();
            var encoded = new byte[subData.Length];
            _aesEncoder.TransformBlock(subData, 0, subData.Length, encoded, 0);
            subData = Enumerable.Range(0, encoded.Length / 4)
                .Select(i => BitConverter.ToUInt32(data, i * 4))
                .Select(bswap_32)
                .SelectMany(arg => BitConverter.GetBytes(arg))
                .ToArray();
            return subData;
        }

        private static uint bswap_32(uint x)
        {
            return ((x & 0x000000FF) << 24) |
                   ((x & 0x0000FF00) << 8) |
                   ((x & 0x00FF0000) >> 8) |
                   ((x & 0xFF000000) >> 24);
        }
    }
}

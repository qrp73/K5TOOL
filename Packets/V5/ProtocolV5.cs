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

        public override PacketFlashWriteReq CreatePacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data, int dataLength, uint seqId)
        {
            return new Packet5FlashWriteReq(chunkNumber, chunkCount, data, dataLength, seqId);
        }

        public override PacketFlashBeaconAck CreatePacketFlashBeaconAck()
        {
            return new Packet5FlashBeaconAck();
        }

        public override PacketFlashWriteAck CreatePacketFlashWriteAck(ushort chunkNumber, uint sequenceId)
        {
            return new Packet5FlashWriteAck(chunkNumber, sequenceId);
        }

        private static byte[][] _keys = new byte[][]
        {
            Utils.FromHex("e16e0d29e0c83418987f9433f5ff620e"),  // key 00
            Utils.FromHex("14b7a2be0223e259b2066d8886977e36"),  // iv  00 
            Utils.FromHex("b0d93af7761a50cacb966eb8a805bcbb"),  // key 01 
            Utils.FromHex("916c50fb9e480693b155b2e555cb780a"),  // iv  01 
            Utils.FromHex("1357cc24d138a57799dd0eec5b9a18f9"),  // key 02
            Utils.FromHex("fb149d4c45e7d7a95aa64bc22765a8c0"),  // iv  02
            Utils.FromHex("a8ef4f917688c5c1487f2a6a811e554d"),  // key 03
            Utils.FromHex("32c860dfce65ca302ec534aa5f88884b"),  // iv  03
            Utils.FromHex("97a387567b234b55c921cd82f65f0087"),  // key 04
            Utils.FromHex("e8ab4e0e344cb5a0b1e7db7a05d468c3"),  // iv  04
            Utils.FromHex("f0240e02ed5da965539d815306f2d34b"),  // key 05
            Utils.FromHex("c6116bd5bea38673746205ee534d589f"),  // iv  05
            Utils.FromHex("734dd8ac84c0c422cbca28fec8856473"),  // key 06
            Utils.FromHex("4bfe35582436578014e970ed8dab9ea0"),  // iv  06
            Utils.FromHex("7f67480570ba5254d7cee59ca2a483b5"),  // key 07
            Utils.FromHex("96858cdf59454a5fa84faa3663748b8d"),  // iv  07
            Utils.FromHex("67ec7c38cefa9edb38bb89b58986eb1a"),  // key 08
            Utils.FromHex("2f9a34bcec302bc6c5b6bc552c166dc9"),  // iv  08
            Utils.FromHex("08ea3a7dc8a6e961b7061f7e52ec8e6d"),  // key 09
            Utils.FromHex("5f6f2ecd835d760b5c1e78f0be1a8787"),  // iv  09
            Utils.FromHex("4e324c565e32be8a6aaf26d9ff37ee87"),  // key 10
            Utils.FromHex("3d680b15a6274c72bc3313a183f1372b"),  // iv  10
            Utils.FromHex("4fd9d7243916543c23ad995937e4bb7c"),  // key 11
            Utils.FromHex("a10be7032960c6d5dcbdc6b4ecfbe31a"),  // iv  11
            Utils.FromHex("a1d6c61c8c5c2281dc7e371ca3c3f168"),  // key 12
            Utils.FromHex("925eb778232e8b4b1f06a582498b2149"),  // iv  12
            Utils.FromHex("11e065ed3f9e8b96bae61f79a7d8a317"),  // key 13
            Utils.FromHex("8b67978471fbd371dcf44ed92f56562d"),  // iv  13
            Utils.FromHex("c4636958e1830f23c6d9ce15b2ddc35a"),  // key 14
            Utils.FromHex("5b7237ddb9c5290e15519018ceacba76"),  // iv  14 
            Utils.FromHex("bf9862d680f948195f5c90545e1d8578"),  // key 15
            Utils.FromHex("8172ea14916b606855ff2aabe52e993c"),  // iv  15
        };

        private static void GetKey(int keyNumber, out byte[] key, out byte[] iv)
        {
            if (keyNumber < 0 || keyNumber >= 0x10)
                throw new ArgumentOutOfRangeException("keyNumber");
            key = _keys[keyNumber * 2 + 0];
            iv = _keys[keyNumber * 2 + 1];
            key = ReverseBytesU32(key);
            iv = ReverseBytesU32(iv);
        }

        private ICryptoTransform _aesEncoder;

        public override void EncryptFlashInit()
        {
            byte[] key;
            byte[] iv;
            GetKey(_keyNumber, out key, out iv);
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.BlockSize = 128;
                _aesEncoder = aes.CreateEncryptor(key, iv);
            }
        }

        public override void EncryptFlashFinish()
        {
            if (_aesEncoder != null)
            {
                _aesEncoder.Dispose();
                _aesEncoder = null;
            }
        }

        public override void EncryptFlashProcess(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
        {
            _aesEncoder.TransformBlock(src, srcIndex, length, dst, dstIndex);
        }

        private ICryptoTransform _aesDecoder;

        public override void DecryptFlashInit()
        {
            byte[] key;
            byte[] iv;
            GetKey(_keyNumber, out key, out iv);
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.BlockSize = 128;
                _aesDecoder = aes.CreateDecryptor(key, iv);
            }
        }

        public override void DecryptFlashFinish()
        {
            if (_aesDecoder != null)
            {
                _aesDecoder.Dispose();
                _aesDecoder = null;
            }
        }

        public override void DecryptFlashProcess(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
        {
            _aesDecoder.TransformBlock(src, srcIndex, length, dst, dstIndex);
        }

        private static byte[] ReverseBytesU32(byte[] data)
        {
            return Enumerable.Range(0, data.Length / 4)
                .Select(arg => BitConverter.ToUInt32(data, arg * 4))
                .Select(arg => BitConverter.ToUInt32(BitConverter.GetBytes(arg).Reverse().ToArray(), 0))
                .SelectMany(arg => BitConverter.GetBytes(arg))
                .ToArray();
        }
    }
}

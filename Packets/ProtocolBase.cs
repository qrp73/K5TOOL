using System;
using System.Text;
using System.Threading;
using K5TOOL.Packets.V2;
using K5TOOL.Packets.V5;

namespace K5TOOL.Packets
{
    public abstract class ProtocolBase
    {
        private Device _device;

        public ProtocolBase(Device device)
        {
            Console.WriteLine("Using {0}...", this.GetType().Name);
            _device = device;
        }

        public static ProtocolBase CreateV2(Device device)
        {
            return new ProtocolV2(device);
        }

        public static ProtocolBase CreateV5(Device device)
        {
            return new ProtocolV5(device);
        }

        #region Bootloader

        public static ProtocolBase WaitForBeacon(Device device)
        {
            Console.WriteLine("Waiting for bootloader beacon...");
            var packet = device.Recv();
            var pktBeacon = packet as PacketFlashBeaconAck;
            if (pktBeacon == null)
            {
                throw new Exception(
                    string.Format("Unexpected response {0}", packet));
            }
            Console.WriteLine("   Bootloader: \"{0}\"", pktBeacon.Version);
            if (pktBeacon is Packet2FlashBeaconAck)
            {
                return CreateV2(device);
            }
            if (pktBeacon is Packet5FlashBeaconAck)
            {
                return CreateV5(device);
            }
            throw new NotImplementedException();
        }

        public virtual bool WriteFlash(string version, byte[] data)
        {
            if (data.Length > 0x10000)
                throw new ArgumentOutOfRangeException("data.Length");
            const int chunkSize = 0x100;
            var chunkCount = (ushort)(data.Length / chunkSize);
            if ((data.Length % chunkSize) != 0)
                chunkCount++;
            var offsetFinal = chunkCount * chunkSize;
            if (offsetFinal > FirmwareConstraints.MaxFlashAddr + 1)
                throw new InvalidOperationException(
                    string.Format(
                        "DANGEROUS FLASH ADDRESS WRITE! size=0x{0:x4}, offsetFinal=0x{1:x4}",
                        data.Length,
                        offsetFinal));

            Console.WriteLine("Write FLASH size=0x{0:x4} (space usage {1:f2} %)", data.Length, data.Length*100f/0xf000);

            var pktVersion = CreatePacketFlashVersionReq(version);
            Console.WriteLine("Send version \"{0}\"...", pktVersion.Version);
            _device.Send(pktVersion);
            var packet = _device.Recv();
            var pktBeacon = packet as PacketFlashBeaconAck;
            if (pktBeacon == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   Bootloader: \"{0}\"", pktBeacon.Version);

            var seqId = GenerateId();

            EncryptFlashInit();
            try
            {
                return ProcessAddressSpace(0x0000, data.Length, chunkSize, FirmwareConstraints.MinFlashAddr, FirmwareConstraints.MaxFlashAddr, MaxFlashBlock,
                    (absOffset, blockOffset, blockLength) =>
                    {
                        Console.Write("   Write {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                        var subData = new byte[chunkSize];
                        for (var i = 0; i < subData.Length; i++)
                            subData[i] = 0xff;
                        Array.Copy(data, blockOffset, subData, 0, blockLength);
                        var cipher = new byte[subData.Length];
                        EncryptFlashProcess(subData, 0, cipher, 0, cipher.Length);
                        var chunkNumber = (ushort)(absOffset / chunkSize);
                        _device.Send(CreatePacketFlashWriteReq(chunkNumber, chunkCount, cipher, blockLength, seqId));
                        for (var counter = 0; ; counter++)
                        {
                            packet = _device.Recv();
                            if (packet is PacketFlashBeaconAck)
                                Console.Write("[beacon]");
                            else
                                break;
                            if (counter > 10)
                            {
                                Logger.Error("No response");
                                return false;
                            }
                        }
                        var wrpacket = packet as PacketFlashWriteAck;
                        if (wrpacket == null)
                        {
                            Logger.Error("Unexpected response {0}", packet);
                            return false;
                        }
                        if (wrpacket.Result != 0)
                        {
                            Logger.Error("Write failed with error code {0}", wrpacket.Result);
                            return false;
                        }
                        if (wrpacket.ChunkNumber != chunkNumber)
                        {
                            Logger.Warn("Unexpected ChunkNumber in response {0}", packet);
                            return false;
                        }
                        Console.WriteLine("OK");
                        return true;
                    });
            }
            finally
            {
                EncryptFlashFinish();
            }
        }

        public abstract PacketFlashWriteReq CreatePacketFlashWriteReq(ushort chunkNumber, ushort chunkCount, byte[] data, int dataLength, uint seqId);

        public abstract PacketFlashVersionReq CreatePacketFlashVersionReq(string version);

        public abstract PacketFlashBeaconAck CreatePacketFlashBeaconAck();

        public virtual void EncryptFlashInit()
        {
        }

        public virtual void EncryptFlashFinish()
        {
        }

        public virtual void EncryptFlashProcess(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
        {
            Array.Copy(src, srcIndex, dst, dstIndex, length);
        }

        public virtual void DecryptFlashInit()
        {
        }

        public virtual void DecryptFlashFinish()
        {
        }

        public virtual void DecryptFlashProcess(byte[] src, int srcIndex, byte[] dst, int dstIndex, int length)
        {
            Array.Copy(src, srcIndex, dst, dstIndex, length);
        }

        #endregion Bootloader


        #region Firmware

        public static ProtocolBase Hello(Device device)
        {
            Console.WriteLine("Handshake...");
            device.Send(new PacketHelloReq());
            var packet = device.Recv();
            var helloAck = packet as PacketHelloAck;
            if (helloAck == null)
            {
                throw new InvalidOperationException(
                    string.Format("Unexpected response {0}", packet));
            }
            Console.WriteLine("   Firmware:         \"{0}\"", helloAck.Version);
            Console.WriteLine("   HasCustomAesKey:  {0}", helloAck.HasCustomAesKey);
            Console.WriteLine("   IsPasswordLocked: {0}", helloAck.IsPasswordLocked);
            return CreateV2(device);
        }

        public virtual string Reboot()
        {
            Console.WriteLine("Reboot device...");
            _device.Send(new PacketRebootReq());

            // rx: 32 2e 30 30 2e 30 36 00 34 0a 00 00 0d 0a
            Thread.Sleep(1000);
            var data = _device.ReadRawBuffer();
            Logger.LogRxRaw(data);
            //Console.WriteLine("rx: {0}", string.Join(" ", data.Select(arg => arg.ToString("x2")).ToArray()));
            string bootloaderVersion;
            var len = Math.Min(16, data.Length);
            if (len <= 0)
            {
                bootloaderVersion = null;
            }
            else
            {
                for (var i = 0; i < len; i++)
                    if (data[i] == 0)
                    {
                        len = i;
                        break;
                    }
                bootloaderVersion = Encoding.UTF8.GetString(data, 0, len);
            }
            if (bootloaderVersion == null)
                throw new InvalidOperationException("Missing bootloader message");
            Console.WriteLine("   Bootloader:       \"{0}\"", bootloaderVersion);
            return bootloaderVersion;
        }

        private const int MaxEepromBlock = 0xff;
        private const int MaxFlashBlock = 0x100;

        public virtual bool ReadEeprom(int offset, int length, Action<int, byte[]> callback)
        {
            Console.WriteLine("Read EEPROM offset=0x{0:x4}, size=0x{1:x4}", offset, length);

            return ProcessAddressSpace(offset, length, 0x80, FirmwareConstraints.MinEepromAddr, FirmwareConstraints.MaxEepromAddr, MaxEepromBlock,
                (absOffset, blockOffset, blockLength) => {
                    Console.Write("   Read {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                    _device.Send(new PacketReadEepromReq((ushort)absOffset, (byte)blockLength));
                    var packet = _device.Recv();
                    var rdpacket = packet as PacketReadEepromAck;
                    if (rdpacket == null)
                    {
                        Logger.Error("Unexpected response {0}", packet);
                        return false;
                    }
                    if (rdpacket.Offset != absOffset)
                    {
                        Logger.Error("Unexpected offset in response {0}", packet);
                        return false;
                    }
                    if (rdpacket.Size != blockLength)
                    {
                        Logger.Error("Unexpected size in response {0}", packet);
                        return false;
                    }
                    Console.WriteLine("OK");
                    if (callback != null)
                        callback(absOffset, rdpacket.Data);
                    return true;
                });
        }

        public virtual bool WriteEeprom(int offset, byte[] data)
        {
            Console.WriteLine("Write EEPROM offset=0x{0:x4}, size=0x{1:x4}", offset, data.Length);

            return ProcessAddressSpace(offset, data.Length, 0x80, FirmwareConstraints.MinEepromAddr, FirmwareConstraints.MaxEepromAddr, MaxEepromBlock,
                (absOffset, blockOffset, blockLength) => {
                    Console.Write("   Write {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                    var subData = new byte[blockLength];
                    Array.Copy(data, blockOffset, subData, 0, subData.Length);
                    _device.Send(new PacketWriteEepromReq((ushort)absOffset, subData));
                    var packet = _device.Recv();
                    var wrpacket = packet as PacketWriteEepromAck;
                    if (wrpacket == null)
                    {
                        Logger.Error("Unexpected response {0}", packet);
                        return false;
                    }
                    if (wrpacket.Offset != absOffset)
                    {
                        Logger.Error("Unexpected offset in response {0}", packet);
                        return false;
                    }
                    Console.WriteLine("OK");
                    return true;
                });
        }

        public PacketReadAdcAck ReadAdc()
        {
            Console.WriteLine("Read ADC...");
            _device.Send(new PacketReadAdcReq());
            var packet = _device.Recv();
            var readAdcAck = packet as PacketReadAdcAck;
            if (readAdcAck == null)
                throw new InvalidOperationException(
                    string.Format("Unexpected response {0}", packet));
            return readAdcAck;
        }

        public PacketReadRssiAck ReadRssi()
        {
            Console.WriteLine("Read RSSI...");
            _device.Send(new PacketReadRssiReq());
            var packet = _device.Recv();
            var rssi = packet as PacketReadRssiAck;
            if (rssi == null)
                throw new InvalidOperationException(
                    string.Format("Unexpected response {0}", packet));
            return rssi;
        }

        #endregion Firmware


        #region Utils

        private static uint GenerateId()
        {
            var random = new Random();
            return (uint)random.Next(1, int.MaxValue);
        }

        private static bool ProcessAddressSpace(
            int offset,
            int length,
            int blockSize,
            int minAllowedAddr,
            int maxAllowedAddr,
            int maxBlockSize,
            Func<int, int, int, bool> callback)
        {
            // validate arguments
            if (offset < minAllowedAddr || offset > maxAllowedAddr)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0 || length > maxAllowedAddr - minAllowedAddr + 1 || offset + length < minAllowedAddr || offset + length > maxAllowedAddr + 1)
                throw new ArgumentOutOfRangeException("length");
            for (var blockOffset = 0; blockOffset < length; blockOffset += blockSize)
            {
                var blockLength = Math.Min(length - blockOffset, blockSize);
                var absOffset = offset + blockOffset;

                // validate packet parameters
                if (absOffset < minAllowedAddr || absOffset > maxAllowedAddr)
                    throw new ArgumentOutOfRangeException("absOffset");
                if (blockLength < 0 || blockLength > maxBlockSize || absOffset + blockLength < minAllowedAddr || absOffset + blockLength > maxAllowedAddr + 1)
                    throw new ArgumentOutOfRangeException("blockLength");

                if (!callback(absOffset, blockOffset, blockLength))
                    return false;
            }
            return true;
        }

        public abstract PacketFlashWriteAck CreatePacketFlashWriteAck(ushort chunkNumber, uint sequenceId);

        #endregion Utils
    }
}

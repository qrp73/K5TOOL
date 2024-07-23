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
using System.IO;
using System.Text;
using System.Threading;
using K5TOOL.Packets;

namespace K5TOOL
{
    public class CommandHelper
    {
        public static bool ReadAdc(Device device)
        {
            Console.WriteLine("Read ADC...");
            device.Send(new PacketReadAdcReq());
            var packet = device.Recv();
            var readAdcAck = packet as PacketReadAdcAck;
            if (readAdcAck == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   Voltage:          {0}", readAdcAck.Voltage);
            Console.WriteLine("   Current:          {0}", readAdcAck.Current);
            return true;
        }

        public static bool ReadRssi(Device device)
        {
            Console.WriteLine("Read RSSI...");
            device.Send(new PacketReadRssiReq());
            var packet = device.Recv();
            var ack = packet as PacketReadRssiAck;
            if (ack == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   RSSI:             {0}", ack.RSSI);
            Console.WriteLine("   ExNoiseIndicator: {0}", ack.ExNoiseIndicator);
            Console.WriteLine("   GlitchIndicator:  {0}", ack.GlitchIndicator);
            return true;
        }

        public static bool Reboot(Device device)
        {
            Console.WriteLine("Reboot device...");
            device.Send(new PacketRebootReq());

            // rx: 32 2e 30 30 2e 30 36 00 34 0a 00 00 0d 0a
            Thread.Sleep(1000);
            var data = device.ReadRawBuffer();
            Logger.LogRxRaw(data);
            //Console.WriteLine("rx: {0}", string.Join(" ", data.Select(arg => arg.ToString("x2")).ToArray()));
            string bootloaderVersion;
            var len = Math.Min(12, data.Length);
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
            if (bootloaderVersion != null)
            {
                Console.WriteLine("   Bootloader:       \"{0}\"", bootloaderVersion);
                return true;
            }
            return false;
        }

        public static bool Handshake(Device device)
        {
            Console.WriteLine("Handshake...");
            device.Send(new PacketHelloReq());
            var packet = device.Recv();
            var helloAck = packet as PacketHelloAck;
            if (helloAck == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   Firmware:         \"{0}\"", helloAck.Version);
            Console.WriteLine("   HasCustomAesKey:  {0}", helloAck.HasCustomAesKey);
            Console.WriteLine("   IsPasswordLocked: {0}", helloAck.IsPasswordLocked);
            return true;
        }

        private const int MaxEepromBlock = 0xff;
        private const int MaxFlashBlock = 0x100;

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

        public static bool ReadEeprom(Device device, int offset, int length, string fileName)
        {
            Console.WriteLine("Read EEPROM offset=0x{0:x4}, size=0x{1:x4} to {2}",
                offset, length, fileName);
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                return ReadEeprom(device, offset, length, (dataOffset, data) => {
                    stream.Write(data, 0, data.Length);
                });
            }
        }

        public static bool ReadEeprom(Device device, int offset, int length, Action<int, byte[]> callback)
        {
            return ProcessAddressSpace(offset, length, 0x80, FirmwareConstraints.MinEepromAddr, FirmwareConstraints.MaxEepromAddr, MaxEepromBlock,
                (absOffset, blockOffset, blockLength) => {
                    Console.Write("   Read {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                    device.Send(new PacketReadEepromReq((ushort)absOffset, (byte)blockLength));
                    var packet = device.Recv();
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

        public static bool WriteEeprom(Device device, int offset, string fileName)
        {
            var data = File.ReadAllBytes(fileName);
            Console.WriteLine("Write EEPROM offset=0x{0:x4}, size=0x{1:x4} from {2}",
                offset, data.Length, fileName);
            return WriteEeprom(device, offset, data);
        }

        public static bool WriteEeprom(Device device, int offset, byte[] data)
        {
            return ProcessAddressSpace(offset, data.Length, 0x80, FirmwareConstraints.MinEepromAddr, FirmwareConstraints.MaxEepromAddr, MaxEepromBlock,
                (absOffset, blockOffset, blockLength) => {
                Console.Write("   Write {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                var subData = new byte[blockLength];
                Array.Copy(data, blockOffset, subData, 0, subData.Length);
                device.Send(new PacketWriteEepromReq((ushort)absOffset, subData));
                var packet = device.Recv();
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

        public static bool WriteFlashPacked(Device device, string fileName)
        {
            Console.WriteLine("Read packed FLASH image from {0}...", fileName);
            var data = File.ReadAllBytes(fileName);
            Console.WriteLine("Unpack image...");
            string version;
            data = FirmwareHelper.Unpack(data, out version);
            return WriteFlash(device, version, data);
        }

        public static bool WriteFlashUnpacked(Device device, string version, string fileName)
        {
            Console.WriteLine("Read unpacked FLASH image from {0}...", fileName);
            var data = File.ReadAllBytes(fileName);
            return CommandHelper.WriteFlash(device, version, data);
        }

        private static uint GenerateId()
        {
            var random = new Random();
            return (uint)random.Next(1, int.MaxValue);
        }

        public static bool WriteFlash(Device device, string version, byte[] data)
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

            Console.WriteLine("Write FLASH size=0x{0:x4}", data.Length);
            Console.WriteLine("Waiting for bootloader beacon...");
            var packet = device.Recv();
            var pktBeacon = packet as PacketFlashBeaconAck;
            if (pktBeacon == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   Bootloader: \"{0}\"", pktBeacon.Version);
            if (pktBeacon.HdrId != PacketFlashBeaconAck.ID)
            {
                Logger.Error("Sorry, this bootloader is not supported yet");
                return false;
            }
            var pktVersion = version != null ? 
                new PacketFlashVersionReq(version) :
                new PacketFlashVersionReq();
            Console.WriteLine("Send version \"{0}\"...", pktVersion.Version);
            device.Send(pktVersion);
            packet = device.Recv();
            pktBeacon = packet as PacketFlashBeaconAck;
            if (pktBeacon == null)
            {
                Logger.Error("Unexpected response {0}", packet);
                return false;
            }
            Console.WriteLine("   Bootloader: \"{0}\"", pktBeacon.Version);

            var seqId = GenerateId();

            return ProcessAddressSpace(0x0000, data.Length, chunkSize, FirmwareConstraints.MinFlashAddr, FirmwareConstraints.MaxFlashAddr, MaxFlashBlock,
                (absOffset, blockOffset, blockLength) => {
                Console.Write("   Write {0:x4}...{1:x4}: ", absOffset, absOffset + blockLength);
                var subData = new byte[blockLength];
                Array.Copy(data, blockOffset, subData, 0, subData.Length);
                var chunkNumber = (ushort)(absOffset / chunkSize);
                device.Send(new PacketFlashWriteReq(chunkNumber, chunkCount, subData, seqId));
                //Packet packet;
                for (var counter=0; ; counter++)
                {
                    packet = device.Recv();
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
                //if (wrpacket.Id != id)
                //{
                //    Logger.Warn("Unexpected response Id (0x{0:x8}!=0x{1:x8})", wrpacket.SequenceId, id);
                //}
                Console.WriteLine("OK");
                return true;
            });
        }
    }
}

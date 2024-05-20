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
using System.Linq;
using K5TOOL.Packets;


namespace K5TOOL
{
    class Program
    {
        static Program()
        {
            CommandLineParser.RegisterCommand("-hello",      OnCommand_Hello,      "",
                "Check connection with the radio");
            CommandLineParser.RegisterCommand("-reboot",     OnCommand_Reboot,     "",
                "Reboot the radio");
            CommandLineParser.RegisterCommand("-rdadc",      OnCommand_RdAdc,      "",
                "Read battery status from the radio");
            CommandLineParser.RegisterCommand("-rdrssi",     OnCommand_RdRssi, "",
                "Read RSSI from the radio");
            CommandLineParser.RegisterCommand("-rdee",       OnCommand_RdEe,       "[<offset> <size>] [<fileName>]",
                "Read EEPROM data from the radio (default fileName=eeprom-{offset}-{size}.raw)");
            CommandLineParser.RegisterCommand("-wree",       OnCommand_WrEe,       "[<offset>] <fileName>",
                "Write EEPROM data to the radio");
            CommandLineParser.RegisterCommand("-wrflash",    OnCommand_WrFlash,    "<fileName>",
                "Write flash from packed image (default format) to the radio. Needs to be run in flashing mode.");
            CommandLineParser.RegisterCommand("-wrflashraw", OnCommand_WrFlashRaw, "[<version>] <fileName>",
                "Write flash from unpacked image to the radio. Needs to be run in flashing mode.");
            CommandLineParser.RegisterCommand("-unpack",     OnCommand_Unpack,     "<fileName> [<outputName>]",
                "Unpack packed flash image to unpacked flash image (default outputName=fileName-{version}.raw)");
            CommandLineParser.RegisterCommand("-pack",       OnCommand_Pack,       "<version> <fileName> [<outputName>]",
                "Pack unpacked flash image to packed flash image (default outputName=fileName.bin)");
            CommandLineParser.RegisterCommand("-test",       OnCommand_Test,       null);
            CommandLineParser.RegisterCommand("-simula",     OnCommand_Simula,     null);
        }

        static int Main(string[] args)
        {
            try
            {
                var portName = CommandLineParser.GetPortName(ref args);
                if (portName == null)
                {
                    portName = System.IO.Ports.SerialPort.GetPortNames().LastOrDefault();
                    if (portName == null)
                        throw new FileNotFoundException("Cannot find serial port");
                }
                return CommandLineParser.Process(portName, ref args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return -2;
            }
        }

        private static int OnCommand_Hello(string name, string[] v)
        {
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_Reboot(string name, string[] v)
        {
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                if (!CommandHelper.Reboot(device))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_RdAdc(string name, string[] v)
        {
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                if (!CommandHelper.ReadAdc(device))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_RdRssi(string name, string[] v)
        {
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                if (!CommandHelper.ReadRssi(device))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_Pack(string name, string[] args)
        {
            string version = null;
            string inputFile = null;
            string outputFile = null;
            // <version> <fileName> [<outputName>]
            switch(args.Length)
            {
                case 2:
                    version = args[0];
                    inputFile = args[1];
                    outputFile = string.Format("{0}.bin", 
                        Path.GetFileNameWithoutExtension(inputFile));
                    break;
                case 3:
                    version = args[0];
                    inputFile = args[1];
                    outputFile = args[2];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            var decoded = File.ReadAllBytes(inputFile);
            var encoded = FirmwareHelper.Pack(decoded, version);
            Console.WriteLine("Write {0}...", outputFile);
            File.WriteAllBytes(outputFile, encoded);
            Console.WriteLine("Done");
            return 0;
        }

        private static int OnCommand_Unpack(string name, string[] args)
        {
            string inputFile = null;
            string outputFile = null;
            // <fileName> [<outputName>]
            switch (args.Length)
            {
                case 1:
                    inputFile = args[0];
                    break;
                case 2:
                    inputFile = args[0];
                    outputFile = args[1];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            var encoded = File.ReadAllBytes(inputFile);
            string version;
            var decoded = FirmwareHelper.Unpack(encoded, out version);
            Console.WriteLine("   Version: {0}", version);
            if (outputFile == null)
            {
                outputFile = string.Format("{0}-{1}.raw",
                    Path.GetFileNameWithoutExtension(inputFile),
                    version);
            }
            Console.WriteLine("Write {0}...", outputFile);
            File.WriteAllBytes(outputFile, decoded);
            Console.WriteLine("Done");
            return 0;
        }

        private static int OnCommand_WrFlashRaw(string name, string[] args)
        {
            string version = null;
            string fileName = null;
            // [<version>] <fileName>
            switch (args.Length)
            {
                case 1:
                    fileName = args[0];
                    break;
                case 2:
                    version = args[0];
                    fileName = args[1];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            using (var device = new Device(name))
            {
                if (!CommandHelper.WriteFlashUnpacked(device, version, fileName))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_WrFlash(string name, string[] args)
        {
            string fileName = null;
            // <fileName>
            switch (args.Length)
            {
                case 1:
                    fileName = args[0];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            using (var device = new Device(name))
            {
                if (!CommandHelper.WriteFlashPacked(device, fileName))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_RdEe(string name, string[] args)
        {
            var offset = 0;
            var size = 0x2000;
            string fileName = null;
            // [<offset> <size>] [<fileName>]
            switch (args.Length)
            {
                case 0:
                    break;
                case 1:
                    fileName = args[0];
                    break;
                case 2:
                    offset = Utils.ParseNumber(args[0]);
                    size = Utils.ParseNumber(args[1]);
                    break;
                case 3:
                    offset = Utils.ParseNumber(args[0]);
                    size = Utils.ParseNumber(args[1]);
                    fileName = args[2];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            if (fileName == null)
            {
                fileName = string.Format("eeprom-{0:x4}-{1:x4}.raw", offset, size);
            }
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                if (!CommandHelper.ReadEeprom(device, offset, size, fileName))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_WrEe(string name, string[] args)
        {
            var offset = 0;
            string fileName = null;
            // [<offset>] <fileName>
            switch (args.Length)
            {
                case 1:
                    offset = 0;
                    fileName = args[0];
                    break;
                case 2:
                    offset = Utils.ParseNumber(args[0]);
                    fileName = args[1];
                    break;
                default:
                    Console.WriteLine("ERROR: invalid arguments");
                    return -1;
            }
            using (var device = new Device(name))
            {
                if (!CommandHelper.Handshake(device))
                    return -2;
                if (!CommandHelper.WriteEeprom(device, offset, fileName))
                    return -2;
                Console.WriteLine("Done");
                return 0;
            }
        }

        private static int OnCommand_Simula(string name, string[] v)
        {
            // bootloader simulator
            using (var device = new Device(name)) // "/dev/serial0"
            {
                var isMute = false;
                byte[] fwdata = null;
                string fwvers = null;
                for (; ; )
                {
                    if (!isMute)
                        device.Send(new PacketFlashBeaconAck());
                    try
                    {
                        var packet = device.Recv();
                        //Console.WriteLine("recv {0}", packet);
                        var packetVer = packet as PacketFlashVersionReq;
                        if (packetVer != null)
                        {
                            fwvers = packetVer.Version;
                            Console.WriteLine("flashVersion: {0}", packetVer.Version);
                            device.Send(new PacketFlashBeaconAck());
                        }
                        var packetWrite = packet as PacketFlashWriteReq;
                        if (packetWrite != null)
                        {
                            if (fwdata == null)
                                fwdata = new byte[packetWrite.OffsetFinal];
                            Array.Copy(packetWrite.Data, 0, fwdata, packetWrite.Offset, packetWrite.Size);
                            if (packetWrite.Size < 0x100)
                            {
                                var shrink = 0x100 - packetWrite.Size;
                                var buf = new byte[fwdata.Length - shrink];
                                Array.Copy(fwdata, buf, buf.Length);
                                fwdata = buf;
                                var fileName = string.Format("firmware-{0}.raw", fwvers);
                                Console.WriteLine("Write {0}", fileName);
                                File.WriteAllBytes(fileName, fwdata);
                                fwdata = null;
                            }
                            Console.WriteLine("write 0x{0:x4}, 0x{1:x4}, [0x{2:x4}]", packetWrite.Offset, packetWrite.Size, packetWrite.OffsetFinal);
                            device.Send(new PacketFlashWriteAck(packetWrite.Offset, packetWrite.Id));
                        }
                        isMute = true;
                    }
                    catch (TimeoutException)
                    {
                    }
                }
            }
        }

        private static int OnCommand_Test(string name, string[] v)
        {
            new Envelope(
                // sim original updater
                Utils.FromHex("abcd10010f6918e7bacc676c213533401302e9809e7f14c6fb910d40f835d540c803e980166c14e62e910d402135d5401303e980166c14e62e910d402135d540ce03e980166c14e62e910d40fe35d54036c7e980f56c14e6cb910d40c635d540fa03e980fd6c14e6c3910d40ce35d540e203e980e56c14e6db910d40d635d540ea03e980ed6c14e6d3910d40de35d5401202e980156d14e62b900d402634d5401a02e9801d6d14e623900d402e34d5400202e980056d14e63b900d403634d5400a02e9800d6d14e633900d403e34d5403202e980152491a02e6155bb217dd507fed2e9809e7f14c63dd90d07dfd22ba7ede41767e88bea01d076f3a7dfd22ba7ede41767e88bea01d076f3a7dfd22ba7ede4176775e1dcba")
            ).Deserialize();

            //new Envelope(new byte[] {
            //flasher-2023
            //0xab,0xcd,0x24,0x00,0x0e,0x69,0x34,0xe6,0x2f,0x93,0x0f,0x4b,0x2d,0x66,0x93,0x74,
            //0x41,0x5a,0x16,0x88,0x9a,0x6c,0x26,0xe6,0x1c,0xbf,0x3d,0x70,0x0f,0x05,0xe3,0x40,
            //0x27,0x09,0xe9,0x80,0x16,0x6c,0x14,0xc6,0xff,0xff,0xdc,0xba,                      

            //flasher-2024
            //0xab,0xcd,0x24,0x00,0x0e,0x69,0x34,0xe6,0x2f,0x93,0x0e,0x42,0x2d,0x66,0x9f,0x73,
            //0x5e,0x40,0x16,0x8f,0x65,0x6c,0xb9,0xe6,0x1c,0xbf,0x3d,0x70,0x0f,0x05,0xe3,0x40,
            //0x27,0x09,0xe9,0x80,0x16,0x6c,0x14,0xc6,0xff,0xff,0xdc,0xba,
            //}).Deserialize();

            //new Envelope(new PacketReqHello(0x1)).Serialize();
            //new Envelope(new PacketReqRdEEPROM(0x0080, 0x80)).Serialize();
            //new Envelope(new PacketReqWrEEPROM(0xaabb, new byte[0x80])).Serialize();
            //new Envelope(new PacketReqBatteryInfo()).Serialize();
            //new Envelope(new PacketWriteFlashReq(0x1111, 0x0022, new byte[0x100])).Serialize();
            //new Envelope(new PacketFlashVersionReq()).Serialize();

            return 0;
        }
    }
}

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
            CommandLineParser.RegisterCommand("-parse",      OnCommand_Parse,      "<hex data>",
                "Parse provided hex data and print details");
            CommandLineParser.RegisterCommand("-parse-plain",OnCommand_ParsePlain, "<hex data>",
                "Parse provided hex data as plain packet (without envelope) and print details");
            CommandLineParser.RegisterCommand("-sniffer", OnCommand_Sniffer,       "",
                "Sniffer mode (listen for packets parse and print it to console)");
            CommandLineParser.RegisterCommand("-test",       OnCommand_Test, null);
            CommandLineParser.RegisterCommand("-simula",     OnCommand_Simula, null);
        }

        static int Main(string[] args)
        {
            try
            {
                var portName = CommandLineParser.GetPortName(ref args);
                if (portName == string.Empty)
                {
                    // port list requested
                    foreach (var name in System.IO.Ports.SerialPort.GetPortNames())
                    {
                        Console.WriteLine("{0}", name);
                    }
                    return 0;
                }
                if (portName == null)
                {
                    var ports = System.IO.Ports.SerialPort.GetPortNames();
                    var isLinuxStyle = ports.Any(arg => arg.StartsWith("/dev/tty", StringComparison.InvariantCulture));
                    if (isLinuxStyle)
                    {
                        // Linux: prefer ttyUSB* device as default
                        ports = ports
                            .OrderBy(arg => arg.IndexOf("USB", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            .ToArray();
                    }
                    portName = ports.LastOrDefault();
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

        private static int OnCommand_Parse(string name, string[] args)
        {
            Logger.IsPrintRaw = true;
            Logger.IsPrintPlain = true;
            Logger.IsPrintPacket = true;

            var data = Utils.FromHex(string.Join(" ", args));
            Console.WriteLine("{0} bytes", data.Length);
            new Envelope(data).Deserialize();

            Console.WriteLine("Done");
            return 0;
        }

        private static int OnCommand_ParsePlain(string name, string[] args)
        {
            Logger.IsPrintRaw = true;
            Logger.IsPrintPlain = true;
            Logger.IsPrintPacket = true;

            var data = Utils.FromHex(string.Join(" ", args));
            Console.WriteLine("{0} bytes", data.Length);
            Logger.LogRx(data);
            var packet = Packet.Deserialize(data);
            Logger.LogRxPacket(packet);

            Console.WriteLine("Done");
            return 0;
        }

        private static int OnCommand_Sniffer(string name, string[] v)
        {
            using (var device = new Device(name))
            {
                Console.Error.WriteLine("===SNIFFER MODE===");
                for (; ; )
                {
                    try
                    {
                        var packet = device.Recv();
                        Console.WriteLine("{0}", packet);
                    }
                    catch (TimeoutException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("[ERROR] {0}: {1}", ex.GetType().Name, ex.Message);
                    }
                }
            }
        }

        private static int OnCommand_Simula(string name, string[] v)
        {
            // bootloader simulator
            Envelope.IsRadioEndpoint = true;
            using (var device = new Device(name)) // "/dev/serial0"
            {
                var isMute = false;
                byte[] fwdata = null;
                string fwvers = null;
                var packetFlashBeacon = new PacketFlashBeaconAck(false);
                for (; ; )
                {
                    if (!isMute)
                        device.Send(packetFlashBeacon);
                    try
                    {
                        var packet = device.Recv();
                        //Console.WriteLine("recv {0}", packet);
                        var packetVer = packet as PacketFlashVersionReq;
                        if (packetVer != null)
                        {
                            fwvers = packetVer.Version;
                            Console.WriteLine("flashVersion: {0}", packetVer.Version);
                            continue;
                            //device.Send(packetFlashBeacon);
                        }
                        var packetWrite = packet as PacketFlashWriteReq;
                        if (packetWrite != null)
                        {
                            Console.WriteLine("flash chunkNumber=0x{0:x4}, size=0x{1:x4}, chunkCount=0x{2:x4}", packetWrite.ChunkNumber, packetWrite.Size, packetWrite.ChunkCount);
                            if (fwdata == null)
                            {
                                fwdata = new byte[packetWrite.ChunkCount*0x100];
                                if (packetWrite.ChunkNumber != 0)
                                {
                                    Console.WriteLine("WARN: started from chunkNumber=0x{0:x4}", packetWrite.ChunkNumber);
                                }
                            }
                            else if (fwdata.Length != packetWrite.ChunkCount*0x100)
                            {
                                Console.WriteLine("WARN: chunkCount unexpectedly changed 0x{0:x4} => 0x{1:x4}", fwdata.Length/0x100, packetWrite.ChunkCount);
                                var expectedSize = packetWrite.ChunkCount * 0x100;
                                if (fwdata.Length < expectedSize)
                                {
                                    var buf = new byte[expectedSize];
                                    Array.Copy(fwdata, buf, fwdata.Length);
                                }
                            }
                            //Array.Copy(packetWrite.Data, 0, fwdata, packetWrite.Offset, packetWrite.Size);
                            Array.Copy(packetWrite.RawData, 16, fwdata, packetWrite.ChunkNumber*0x100, packetWrite.HdrSize - 12);

                            // detect flashing complete event
                            if (packetWrite.ChunkNumber == packetWrite.ChunkCount-1)
                            {
                                var shrink = 0x100 - packetWrite.Size;
                                //var shrink = 0;
                                var buf = new byte[fwdata.Length - shrink];
                                Array.Copy(fwdata, buf, buf.Length);
                                fwdata = buf;
                                var fileName = string.Format("firmware-{0}.raw", fwvers);
                                Console.WriteLine("Write {0}", fileName);
                                File.WriteAllBytes(fileName, fwdata);
                                fwdata = null;
                            }

                            device.Send(new PacketFlashWriteAck(packetWrite.ChunkNumber, packetWrite.SequenceId));
                        }
                        isMute = true;
                    }
                    catch (TimeoutException)
                    {
                    }
                }
            }
        }

        private static int OnCommand_Test(string name, string[] args)
        {
            Logger.IsPrintRaw = true;
            Logger.IsPrintPlain = true;
            Logger.IsPrintPacket = true;

            new Envelope(new PacketHelloReq(Utils.ToUnixTimeSeconds(DateTime.UtcNow))).Serialize();
            //new Envelope(new PacketReqRdEEPROM(0x0080, 0x80)).Serialize();
            //new Envelope(new PacketReqWrEEPROM(0xaabb, new byte[0x80])).Serialize();
            //new Envelope(new PacketReqBatteryInfo()).Serialize();
            //new Envelope(new PacketWriteFlashReq(0x1111, 0x0022, new byte[0x100])).Serialize();
            //new Envelope(new PacketFlashVersionReq()).Serialize();

            Console.WriteLine("Done");
            return 0;
        }
    }
}

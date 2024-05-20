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
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text;
using K5TOOL.Packets;


namespace K5TOOL
{
    public class Device : IDisposable
    {
        private SerialPort _serial;

        public Device(string portName, int baudRate = 38400)
        {
            Console.WriteLine("Opening {0}", portName);
            Logger.Trace("Opening {0}", portName);
            _serial = new SerialPort();
            _serial.PortName = portName;
            _serial.BaudRate = baudRate;
            _serial.Parity = Parity.None;
            _serial.DataBits = 8;
            _serial.StopBits = StopBits.One;
            _serial.ReadTimeout = 1000;
            _serial.Open();

            // empty rx buffer
            if (_serial.BytesToRead > 0)
            {
                var data = RecvRaw(_serial.BytesToRead);
                Logger.Trace("open rx: {0}", Utils.ToHex(data));
            }
        }

        public void Dispose()
        {
            _serial.Dispose();
            _serial = null;
        }

        public byte[] ReadRawBuffer()
        {
            return RecvRaw(_serial.BytesToRead);
        }

        private byte[] RecvRaw(int length)
        {
            var data = new byte[length];
            for (var index=0; index < length;)
            {
                try
                {
                    var read = _serial.Read(data, index, data.Length - index);
                    //Logger.Trace("rxx: ", data.Skip(index).Take(read));
                    if (read < 0)
                        throw new InvalidOperationException();
                    index += read;
                }
                catch
                {
                    Logger.LogRxRaw(data.Take(index).ToArray());
                    throw;
                }
            }
            return data;
        }

        public void SendRaw(byte[] data)
        {
            _serial.Write(data, 0, data.Length);
            _serial.BaseStream.Flush();
        }

        public void Send(Packet packet)
        {
            SendRaw(new Envelope(packet).Serialize());
        }

        private void LogRecvWithTail(byte[] data)
        {
            // read the tail of packet data
            Thread.Sleep(1000);
            var tail = RecvRaw(_serial.BytesToRead);
            var fullData = new byte[data.Length + tail.Length];
            Array.Copy(data, 0, fullData, 0, data.Length);
            Array.Copy(tail, 0, fullData, data.Length, tail.Length);
            Logger.LogRxRaw(fullData);
        }

        public Packet Recv()
        {
            var data = RecvRaw(4);
            if (data.Length != 4 || data[0] != 0xab || data[1] != 0xcd)
            {
                LogRecvWithTail(data);
                throw new InvalidOperationException(
                    string.Format(
                        "Recv: invalid protocol header {0}", 
                        Utils.ToHex(data)));
            }
            var len = data[2] | (data[3] << 8);
            //if (len > 0xff)
            //{
            //    LogRecvWithTail(data);
            //    throw new InvalidOperationException(
            //        string.Format(
            //            "Recv: invalid protocol length {0}", len));
            //}
            var tail = RecvRaw(len + 4);
            var fullData = new byte[tail.Length + data.Length];
            Array.Copy(data, 0, fullData, 0, data.Length);
            Array.Copy(tail, 0, fullData, data.Length, tail.Length);
            if (tail.Length != len + 4)
            {
                LogRecvWithTail(fullData);
                throw new InvalidOperationException(
                    string.Format(
                        "Recv: invalid tail length {0}, expected {1}", tail.Length, len));
            }
            return new Envelope(fullData).Deserialize();
        }

    }
}

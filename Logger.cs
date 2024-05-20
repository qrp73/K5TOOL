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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using K5TOOL.Packets;

namespace K5TOOL
{
    public static class Logger
    {
        public static bool IsPrintRaw = false;
        public static bool IsPrintPlain = false;
        public static bool IsPrintPacket = false;

        private static readonly FileStream _stream;

        static Logger()
        {
            try
            {
                const string fileName = "K5TOOL.log";
                if (File.Exists(fileName))
                {
                    var fileNameBak = fileName + ".bak";
                    if (File.Exists(fileNameBak))
                        File.Delete(fileNameBak);
                    File.Move(fileName, fileName + ".bak");
                }
                _stream = new FileStream(fileName, /*FileMode.OpenOrCreate*/ FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                _stream.Seek(0, SeekOrigin.End);
                LogToFile(string.Format("====[UTC:{0:yyyy-MM-ddTHH:mm:ss}]{1}",
                    DateTime.UtcNow,
                    new string('=', 80-4-25)));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void Trace(string fmt, params object[] args)
        {
            LogToFile(string.Format("[TRACE] {0}", args.Length != 0 ? string.Format(fmt, args) : fmt));
        }

        public static void Warn(string fmt, params object[] args)
        {
            var msg = string.Format("[WARN ] {0}", args.Length != 0 ? string.Format(fmt, args) : fmt);
            LogToFile(msg);
            LogToConsole(ConsoleColor.Yellow, msg);
        }

        public static void Error(string fmt, params object[] args)
        {
            var msg = string.Format("[ERROR] {0}", args.Length != 0 ? string.Format(fmt, args) : fmt);
            LogToFile(msg);
            LogToConsole(ConsoleColor.Red, msg);
        }

        public static void Error(Exception ex)
        {
            var msg = string.Format("[ERROR] {0}: {1}{2}{3}", ex.GetType().Name, ex.Message, Environment.NewLine, ex.StackTrace);
            LogToFile(msg);
            msg = string.Format("[ERROR] {0}: {1}", ex.GetType().Name, ex.Message);
            LogToConsole(ConsoleColor.Red, msg);
        }

        private static void LogToConsole(ConsoleColor color, string message)
        {
            // https://github.com/mono/mono/issues/21773
            //var tmp = Console.ForegroundColor;
            //Console.ForegroundColor = color;
            Console.WriteLine(message);
            //Console.ForegroundColor = tmp;
        }

        private static void LogToFile(string msg)
        {
            if (_stream == null)
                return;
            var data = Encoding.UTF8.GetBytes(msg);
            data = data.Concat(Encoding.UTF8.GetBytes(Environment.NewLine)).ToArray();
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }

        public static void LogRxRaw(byte[] data)
        {
            var msg = string.Format("rx: {0}", Utils.ToHex(data));
            Trace(msg);
            if (!IsPrintRaw)
                return;
            Console.WriteLine(msg);
        }

        public static void LogRx(byte[] data)
        {
            var msg = string.Format("RX: {0}", Utils.ToHex(data));
            Trace(msg);
            if (!IsPrintPlain)
                return;
            Console.WriteLine(msg);
        }

        public static void LogRxPacket(Packet packet)
        {
            var msg = string.Format("recv {0}", packet);
            Trace(msg);
            if (!IsPrintPacket)
                return;
            Console.WriteLine(msg);
        }

        public static void LogTxRaw(byte[] data)
        {
            var msg = string.Format("tx: {0}", Utils.ToHex(data));
            Trace(msg);
            if (!IsPrintRaw)
                return;
            Console.WriteLine(msg);
        }

        public static void LogTx(byte[] data)
        {
            var msg = string.Format("TX: {0}", Utils.ToHex(data));
            Trace(msg);
            if (!IsPrintPlain)
                return;
            Console.WriteLine(msg);
        }

        public static void LogTxPacket(Packet packet)
        {
            var msg = string.Format("send {0}", packet);
            Trace(msg);
            if (!IsPrintPacket)
                return;
            Console.WriteLine(msg);
        }
    }
}

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
using System.Collections.Generic;
using System.Reflection;

namespace K5TOOL
{
    public static class CommandLineParser
    {
        private static Dictionary<string, Func<string, string[], int>> _commands = new Dictionary<string, Func<string, string[], int>>();
        private static Dictionary<string, string> _helps = new Dictionary<string, string>();
        private static Dictionary<string, string> _descs = new Dictionary<string, string>();

        public static void RegisterCommand(string command, Func<string, string[], int> handler, string help, string desc=null)
        {
            _commands.Add(command, handler);
            _helps.Add(command, help);
            if (desc != null)
                _descs.Add(command, desc);
        }

        public static string GetPortName(ref string[] args)
        {
            if (args.Length < 1) return null;
            if (args[0] == "-port")
            {
                if (args.Length > 1)
                {
                    var name = args[1];
                    args = args.Skip(2).ToArray();
                    return name;
                }
                return string.Empty; // port list request
            }
            return null;
        }

        public static int Process(string portName, ref string[] args)
        {
            if (args.Length > 0)
            {
                var command = args[0];
                var commandArgs = args.Skip(1).ToArray();
                if (_commands.ContainsKey(command))
                {
                    return _commands[command](portName, commandArgs);
                }
                Console.WriteLine("ERROR: unknown command {0}", command);
            }
            // Print Usage
            Console.WriteLine("K5TOOL v{0}", GetVersionString());
            Console.WriteLine("UV-K5 radio toolkit (c)2024 qrp73");
            Console.WriteLine();
            Console.WriteLine("USGAGE: k5tool [-port <portName>] <command> [<args>]");
            Console.WriteLine("Commands:");
            foreach (var cmd in _commands.Keys)
            {
                Console.WriteLine("  {0} {1}", cmd, _helps[cmd]);
                if (_descs.ContainsKey(cmd))
                    Console.WriteLine("      {0}", _descs[cmd]);
            }
            return -1;
        }

        private static string GetVersionString()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}

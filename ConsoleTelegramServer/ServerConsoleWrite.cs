using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTelegramServer
{
    internal class ServerConsoleWrite
    {
        public static void SimpleWrite(string message)
        {
            Console.WriteLine(DateTime.Now + ": " + message);
        }
        public static void ErrorWrite(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now + ": " + message);
            Console.ForegroundColor = color;
        }
    }
}

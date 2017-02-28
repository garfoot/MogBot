using System;
using MogBot.Host.Extensions;

namespace MogBot.Host
{
    public class ConsoleTrace : ITrace
    {
        public void WriteLine(string message, ConsoleColor color)
        {
            ConsoleExtensions.WriteLine(message, color);
        }

        public void Write(string message, ConsoleColor color)
        {
            ConsoleExtensions.Write(message, color);
        }
    }
}
using System;

namespace MogBot.Host
{
    public interface ITrace
    {
        void WriteLine(string message, ConsoleColor color);
        void Write(string message, ConsoleColor color);
    }
}
using daemon_core;
using System;

namespace daemon_console
{
    public class ConsoleInputProvider : IInputProvider
    {
        public string ReadInput()
        {
            return Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace daemon_core
{
    public interface ILogger
    {
        void Error(string message);
        void Info(string message);
    }
}

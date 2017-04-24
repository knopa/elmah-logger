using System;

namespace ElmahLogger
{
    internal class LogException : Exception
    {
        public LogException(string message)
            : base(message)
        {
            
        }
    }
}

using System;

namespace ElmahLogger
{
    public interface ILogger
    {
        void Log(string message, string type = null);
        void Log(string message, Exception exception, string type = null);
        void Log(Exception exception, string type = null);
    }
}

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace ElmahLogger
{
    public class LogEvent
    {
        public int UserStackFrameNumber { get; set; }

        public StackTrace StackTrace { get; set; }

        private StackFrame UserStackFrame => StackTrace?.GetFrame(UserStackFrameNumber);

        public (string name, string file) Name
        {
            get
            {
                StackFrame frame = UserStackFrame;
                if (frame != null)
                {
                    MethodBase methodBase = frame.GetMethod();
                    Type type = methodBase.DeclaringType;

                    if (type == null)
                    {
                        return (null, null);
                    }

                    return
                            (
                            $"{type.FullName}.{CleanMethodName(methodBase.Name)}",
                            $"{frame.GetFileName()}:{frame.GetFileLineNumber().ToString(CultureInfo.InvariantCulture)}"
                            );
                }

                return (null, null);
            }
        }

        private string CleanMethodName(string methodName)
        {
            if (methodName.Contains("__") && methodName.StartsWith("<") && methodName.Contains(">"))
            {
                int startIndex = methodName.IndexOf('<') + 1;
                int endIndex = methodName.IndexOf('>');

                methodName = methodName.Substring(startIndex, endIndex - startIndex);
            }

            return methodName;
        }
    }
}

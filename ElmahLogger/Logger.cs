using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using Elmah;

namespace ElmahLogger
{
    public class Logger : ILogger
    {
        private const string Info = "Info";
        private const int StackTraceSkipMethods = 0;
        private readonly Type loggerType = typeof(Logger);
        public string Name { get; private set; }
        public LogFactory Factory { get; private set; }
        private static readonly Assembly nlogAssembly = typeof(Logger).Assembly;
        private static readonly Assembly mscorlibAssembly = typeof(string).Assembly;
        private static readonly Assembly systemAssembly = typeof(Debug).Assembly;

        internal void Initialize(string name, LogFactory factory)
        {
            Name = name;
            Factory = factory;
        }

        private void Write(string message = null, Exception exception = null, string type = null)
        {
            try
            {
                LogEvent logEvent = CreateEvent();

                (string name, string file) logInfo = logEvent.Name;

                HttpContext httpContext = HttpContext.Current;
                Error error = exception == null
                                  ? new Error()
                                  : httpContext != null ? new Error(exception, httpContext) : new Error(exception);

                string source = logInfo.name;
                string typeLog = error.Exception != null
                                 ? error.Exception.GetType().FullName
                                 : type;
                message = message ?? exception?.Message;
                error.Type = typeLog ?? Info;
                error.Message = message;
                error.Time = DateTime.Now;
                error.HostName = Environment.MachineName;
                error.Source = source;
                error.Detail = exception == null ? logInfo.file : exception.StackTrace;

                ErrorLog.GetDefault(httpContext)
                        .Log(error);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Logger.Write message:{message} exception:{exception} type:{type} ex:{ex}");
            }
        }

        private LogEvent CreateEvent()
        {
            StackTrace stackTrace = new StackTrace(StackTraceSkipMethods, true);
            int firstUserFrame = FindCallingMethodOnStackTrace(stackTrace, loggerType);
            return new LogEvent
            {
                StackTrace =  stackTrace,
                UserStackFrameNumber = firstUserFrame
            };
        }

        
        internal static int FindCallingMethodOnStackTrace(StackTrace stackTrace, Type loggerType)
        {
            var stackFrames = stackTrace.GetFrames();
            if (stackFrames == null)
                return 0;

            var allStackFrames = stackFrames.Select((f, i) => new StackFrameWithIndex(i, f)).ToList();
            var filteredStackframes = allStackFrames.Where(p => !SkipAssembly(p.StackFrame)).ToList();
            var intermediate = filteredStackframes.SkipWhile(p => !IsLoggerType(p.StackFrame, loggerType));
            var stackframesAfterLogger = intermediate.SkipWhile(p => IsLoggerType(p.StackFrame, loggerType)).ToList();

            var candidateStackFrames = stackframesAfterLogger;
            if (!candidateStackFrames.Any())
            {
                candidateStackFrames = filteredStackframes;
            }

            return FindIndexOfCallingMethod(allStackFrames, candidateStackFrames);
        }

        private static int FindIndexOfCallingMethod(List<StackFrameWithIndex> allStackFrames, List<StackFrameWithIndex> candidateStackFrames)
        {
            var stackFrameWithIndex = candidateStackFrames.FirstOrDefault();
            var last = stackFrameWithIndex;

            if (last != null)
            {
                if (last.StackFrame.GetMethod().Name == "MoveNext")
                {

                    if (allStackFrames.Count > last.StackFrameIndex)
                    {
                        var next = allStackFrames[last.StackFrameIndex + 1];
                        var declaringType = next.StackFrame.GetMethod().DeclaringType;
                        if (declaringType == typeof(AsyncTaskMethodBuilder) ||
                            declaringType == typeof(AsyncTaskMethodBuilder<>))
                        {
                            candidateStackFrames = candidateStackFrames.Skip(1).ToList();
                            return FindIndexOfCallingMethod(allStackFrames, candidateStackFrames);
                        }
                    }
                }

                return last.StackFrameIndex;
            }
            return 0;
        }

        private static bool SkipAssembly(StackFrame frame)
        {
            var method = frame.GetMethod();
            var assembly = method.DeclaringType != null ? method.DeclaringType.Assembly : method.Module.Assembly;
            var skipAssembly = SkipAssembly(assembly);
            return skipAssembly;
        }

        private static bool IsLoggerType(StackFrame frame, Type loggerType)
        {
            var method = frame.GetMethod();
            Type declaringType = method.DeclaringType;
            var isLoggerType = declaringType != null && loggerType == declaringType;
            return isLoggerType;
        }

        private static bool SkipAssembly(Assembly assembly)
        {
            if (assembly == nlogAssembly)
            {
                return true;
            }

            if (assembly == mscorlibAssembly)
            {
                return true;
            }

            if (assembly == systemAssembly)
            {
                return true;
            }

            if (LogManager.IsHiddenAssembly(assembly))
            {
                return true;
            }

            return false;
        }

        private class StackFrameWithIndex
        {
            public int StackFrameIndex { get; private set; }

            public StackFrame StackFrame { get; private set; }

            public StackFrameWithIndex(int stackFrameIndex, StackFrame stackFrame)
            {
                StackFrameIndex = stackFrameIndex;
                StackFrame = stackFrame;
            }
        }

        #region Implementation of ILogger

        public void Log(string message, string type = null)
        {
            Write(message, type: type);
        }

        public void Log(string message, Exception exception, string type = null)
        {
            Write(message, exception, type);
        }

        public void Log(Exception exception, string type = null)
        {
            Write(exception: exception, type: type);
        }
        #endregion
    }
}

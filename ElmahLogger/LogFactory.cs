using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ElmahLogger
{
    public class LogFactory
    {
        private readonly object syncRoot = new object();
        private readonly LoggerCache loggerCache = new LoggerCache();

        public Logger GetLogger(string name)
        {
            return GetLogger(new LoggerCacheKey(name, typeof(Logger)));
        }

        public T GetLogger<T>(string name) where T : Logger
        {
            return (T)GetLogger(new LoggerCacheKey(name, typeof(T)));
        }

        public Logger GetLogger(string name, Type loggerType)
        {
            return GetLogger(new LoggerCacheKey(name, loggerType));
        }

        private Logger GetLogger(LoggerCacheKey cacheKey)
        {
            lock (syncRoot)
            {
                Logger existingLogger = loggerCache.Retrieve(cacheKey);
                if (existingLogger != null)
                {
                    return existingLogger;
                }

                Logger newLogger;

                if (cacheKey.ConcreteType != null && cacheKey.ConcreteType != typeof(Logger))
                {
                    string fullName = cacheKey.ConcreteType.FullName;
                    try
                    {
                        if (cacheKey.ConcreteType.IsStaticClass())
                        {
                            string errorMessage = $"GetLogger / GetCurrentClassLogger is '{fullName}' as loggerType can be a static class and should inherit from Logger";
                            Trace.TraceError(errorMessage);

                            newLogger = CreateDefaultLogger(ref cacheKey);
                        }
                        else
                        {

                            var instance = FactoryHelper.CreateInstance(cacheKey.ConcreteType);
                            newLogger = instance as Logger;
                            if (newLogger == null)
                            {
                                string errorMessage = $"GetLogger / GetCurrentClassLogger got '{fullName}' as loggerType which doesn't inherit from Logger";
                                Trace.TraceError(errorMessage);

                                newLogger = CreateDefaultLogger(ref cacheKey);

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"GetLogger / GetCurrentClassLogger. Cannot create instance of type '{fullName}'. It should have an default contructor. ex:{ex}");

                        newLogger = CreateDefaultLogger(ref cacheKey);
                    }
                }
                else
                {
                    newLogger = new Logger();
                }

                if (cacheKey.ConcreteType != null)
                {
                    newLogger.Initialize(cacheKey.Name, this);
                }

                loggerCache.InsertOrUpdate(cacheKey, newLogger);
                return newLogger;
            }
        }

        private static Logger CreateDefaultLogger(ref LoggerCacheKey cacheKey)
        {
            cacheKey = new LoggerCacheKey(cacheKey.Name, typeof(Logger));

            var newLogger = new Logger();
            return newLogger;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger()
        {
            StackFrame frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod()?.DeclaringType?.FullName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public T GetCurrentClassLogger<T>() where T : Logger
        {
            StackFrame frame = new StackFrame(1, false);

            return (T)GetLogger(frame.GetMethod()?.DeclaringType?.FullName, typeof(T));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Logger GetCurrentClassLogger(Type loggerType)
        {
            var frame = new StackFrame(1, false);

            return GetLogger(frame.GetMethod()?.DeclaringType?.FullName, loggerType);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

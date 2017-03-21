using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ElmahLogger
{
    public sealed class LogManager
    {
        private static readonly LogFactory factory = new LogFactory();
        private static ICollection<Assembly> _hiddenAssemblies;

        private static readonly object lockObject = new object();

        private LogManager()
        {
        }

        internal static LogFactory LogFactory => factory;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            return factory.GetLogger(GetClassFullName());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(Type loggerType)
        {
            return factory.GetLogger(GetClassFullName(), loggerType);
        }

        public static Logger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        public static Logger GetLogger(string name, Type loggerType)
        {
            return factory.GetLogger(name, loggerType);
        }

        private static string GetClassFullName()
        {
            string className;
            Type declaringType;
            int framesToSkip = 2;

            do
            {
                StackFrame frame = new StackFrame(framesToSkip, false);
                MethodBase method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return className;
        }

        internal static bool IsHiddenAssembly(Assembly assembly)
        {
            return _hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHiddenAssembly(Assembly assembly)
        {
            lock (lockObject)
            {
                if (_hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly))
                    return;

                _hiddenAssemblies = new HashSet<Assembly>(_hiddenAssemblies ?? Enumerable.Empty<Assembly>())
                {
                    assembly
                };
            }
        }
    }
}

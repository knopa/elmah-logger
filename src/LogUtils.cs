using System;

namespace ElmahLogger
{
    internal static class LogUtils
    {
        public static bool IsStaticClass(this Type type)
        {
            return type.IsClass && type.IsAbstract && type.IsSealed;
        }
    }
}

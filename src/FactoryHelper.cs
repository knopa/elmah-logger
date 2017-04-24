using System;
using System.Reflection;

namespace ElmahLogger
{
    internal class FactoryHelper
    {
        private FactoryHelper()
        {
        }

        internal static object CreateInstance(Type t)
        {
            ConstructorInfo constructor = t.GetConstructor(ArrayHelper.Empty<Type>());
            if (constructor != null)
            {
                return constructor.Invoke(ArrayHelper.Empty<object>());
            }
            throw new LogException($"Cannot access the constructor of type: {t.FullName}. Is the required permission granted?");
        }
    }
}

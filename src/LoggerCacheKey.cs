using System;

namespace ElmahLogger
{
    internal class LoggerCacheKey : IEquatable<LoggerCacheKey>
    {
        public string Name { get; }

        public Type ConcreteType { get; }

        public LoggerCacheKey(string name, Type concreteType)
        {
            Name = name;
            ConcreteType = concreteType;
        }

        public override int GetHashCode()
        {
            return ConcreteType.GetHashCode() ^ Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            LoggerCacheKey key = obj as LoggerCacheKey;
            if (ReferenceEquals(key, null))
            {
                return false;
            }

            return (ConcreteType == key.ConcreteType) && (key.Name == Name);
        }

        public bool Equals(LoggerCacheKey key)
        {
            if (ReferenceEquals(key, null))
            {
                return false;
            }

            return (ConcreteType == key.ConcreteType) && (key.Name == Name);
        }
    }
}

using System;
using System.Collections.Generic;

namespace ElmahLogger
{
    internal class LoggerCache
    {
        private readonly Dictionary<LoggerCacheKey, WeakReference> loggerCache =
                new Dictionary<LoggerCacheKey, WeakReference>();

        public void InsertOrUpdate(LoggerCacheKey cacheKey, Logger logger)
        {
            loggerCache[cacheKey] = new WeakReference(logger);
        }

        public Logger Retrieve(LoggerCacheKey cacheKey)
        {
            WeakReference loggerReference;
            if (loggerCache.TryGetValue(cacheKey, out loggerReference))
            {
                return loggerReference.Target as Logger;
            }

            return null;
        }

        public IEnumerable<Logger> Loggers => GetLoggers();

        private IEnumerable<Logger> GetLoggers()
        {
            List<Logger> values = new List<Logger>(loggerCache.Count);

            foreach (WeakReference loggerReference in loggerCache.Values)
            {
                Logger logger = loggerReference.Target as Logger;
                if (logger != null)
                {
                    values.Add(logger);
                }
            }

            return values;
        }
    }
}

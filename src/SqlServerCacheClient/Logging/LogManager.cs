using System;
using System.Reflection;

//WARNING: Do not include any reference to any type in the log4net or NLog namespace in this class. 

namespace SqlServerCacheClient.Logging
{
    internal static class LogManager
    {
        private enum LoggingFramework
        {
            Log4net,
            Nlog,
            Console,
            Null
        }

        private static readonly LoggingFramework loggingFramework;

        static LogManager()
        {
            loggingFramework = LoggingFramework.Null;
            try
            {
                Assembly.Load("log4net");
                loggingFramework = LoggingFramework.Log4net;
            }
            catch
            {
            }

            if (loggingFramework == LoggingFramework.Null)
            {
                try
                {
                    Assembly.Load("NLog");
                    loggingFramework = LoggingFramework.Nlog;
                }
                catch
                {
                }
            }
#if DEBUG
            if (loggingFramework == LoggingFramework.Null)
            {
                loggingFramework = LoggingFramework.Console;
            }
#endif
        }

        public static ILogger GetLogger(Type type)
        {
            if (loggingFramework == LoggingFramework.Log4net)
            {
                return new Log4NetWrapper(type);
            }

            if (loggingFramework == LoggingFramework.Nlog)
            {
                return new NlogWrapper(type);
            }

            if (loggingFramework == LoggingFramework.Console)
            {
                return ConsoleLogger.Instance;
            }

            return NullLogger.Instance;
        }
    }
}

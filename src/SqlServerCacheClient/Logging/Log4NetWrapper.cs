using System;
using log4net;

namespace SqlServerCacheClient.Logging
{
    internal class Log4NetWrapper : ILogger
    {
        private static bool configured = false;
        private static void ConfigureLog4Net()
        {
            if (!configured)
            {
                log4net.Config.XmlConfigurator.Configure();
                configured = true;
            }
        }

        private readonly ILog iLog;

        public Log4NetWrapper(Type type)
        {
            ConfigureLog4Net();
            this.iLog = log4net.LogManager.GetLogger(type);
        }

        public bool IsDebugEnabled
        {
            get { return iLog.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return iLog.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return iLog.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return iLog.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return iLog.IsWarnEnabled; }
        }

        public void Debug(object message)
        {
            iLog.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            iLog.Debug(message, exception);
        }

        public void DebugFormat(string format, object arg0)
        {
            iLog.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, params object[] args)
        {
            iLog.DebugFormat(format, args);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            iLog.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            iLog.DebugFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            iLog.DebugFormat(provider, format, args);
        }

        public void Info(object message)
        {
            iLog.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            iLog.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            iLog.InfoFormat(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            iLog.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            iLog.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            iLog.InfoFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            iLog.InfoFormat(provider, format, args);
        }

        public void Warn(object message)
        {
            iLog.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            iLog.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            iLog.WarnFormat(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            iLog.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            iLog.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            iLog.WarnFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            iLog.WarnFormat(provider, format, args);
        }

        public void Error(object message)
        {
            iLog.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            iLog.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            iLog.ErrorFormat(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            iLog.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            iLog.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            iLog.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            iLog.ErrorFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            iLog.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            iLog.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            iLog.FatalFormat(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            iLog.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            iLog.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            iLog.FatalFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            iLog.FatalFormat(provider, format, args);
        }
    }
}
using System;

namespace SqlServerCacheClient.Logging
{
    internal class NlogWrapper : ILogger
    {
        protected readonly NLog.Logger logger;

        public NlogWrapper(Type type)
        {
            logger = NLog.LogManager.GetLogger(type.Name, type);
        }

        public bool IsDebugEnabled
        {
            get { return logger.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return logger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return IsWarnEnabled; }
        }

        public void Debug(object message)
        {
            logger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            //nlog v2 - v3
            //logger.Debug(message.ToString(), exception);
            //nlog v4
            logger.Debug(exception, message.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            logger.Debug(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            logger.Debug(format, arg0);
        }
        
        public void DebugFormat(string format, object arg0, object arg1)
        {
            logger.Debug(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            logger.Debug(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.Debug(provider, format, args);
        }

        public void Info(object message)
        {
            logger.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            //nlog v2 - v3
            //logger.Info(message.ToString(), exception);
            //nlog v4
            logger.Info(exception, message.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            logger.Info(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            logger.Info(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            logger.Info(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            logger.Info(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.Info(provider, format, args);
        }

        public void Warn(object message)
        {
            logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            //nlog v2 - v3
            //logger.Warn(message.ToString(), exception);
            //nlog v4
            logger.Warn(exception, message.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            logger.Warn(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            logger.Warn(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            logger.Warn(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            logger.Warn(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.Warn(provider, format, args);
        }

        public void Error(object message)
        {
            logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            //nlog v2 - v3
            //logger.Error(message.ToString(), exception);
            //nlog v4
            logger.Error(exception, message.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            logger.Error(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            logger.Error(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            logger.Error(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            logger.Error(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.Error(provider, format, args);
        }

        public void Fatal(object message)
        {
            logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            //nlog v2 - v3
            //logger.Fatal(message.ToString(), exception);
            //nlog v4
            logger.Fatal(exception, message.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            logger.Fatal(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            logger.Fatal(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            logger.Fatal(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            logger.Fatal(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            logger.Fatal(provider, format, args);
        }
    }
}
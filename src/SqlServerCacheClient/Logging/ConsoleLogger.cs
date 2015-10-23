using System;

namespace SqlServerCacheClient.Logging
{
    internal class ConsoleLogger : ILogger
    {
        public readonly static ConsoleLogger Instance = new ConsoleLogger();

        private ConsoleLogger()
        {
            IsDebugEnabled = true;
            IsInfoEnabled = true;
            IsWarnEnabled = true;
            IsErrorEnabled = true;
            IsFatalEnabled = true;
        }

        public bool IsDebugEnabled { get; private set; }

        public bool IsErrorEnabled { get; private set; }

        public bool IsFatalEnabled { get; private set; }

        public bool IsInfoEnabled { get; private set; }

        public bool IsWarnEnabled { get; private set; }

        public void SetVerbosity(LoggingVerbosity verbosity)
        {
            if (verbosity == LoggingVerbosity.Debug)
            {
                IsDebugEnabled = true;
                IsInfoEnabled = true;
                IsWarnEnabled = true;
                IsErrorEnabled = true;
                IsFatalEnabled = true;
            }
            else if (verbosity == LoggingVerbosity.Info)
            {
                IsDebugEnabled = false;
                IsInfoEnabled = true;
                IsWarnEnabled = true;
                IsErrorEnabled = true;
                IsFatalEnabled = true;
            }
            else if (verbosity == LoggingVerbosity.Warn)
            {
                IsDebugEnabled = false;
                IsInfoEnabled = false;
                IsWarnEnabled = true;
                IsErrorEnabled = true;
                IsFatalEnabled = true;
            }
            else if (verbosity == LoggingVerbosity.Error)
            {
                IsDebugEnabled = false;
                IsInfoEnabled = false;
                IsWarnEnabled = false;
                IsErrorEnabled = true;
                IsFatalEnabled = true;
            }
            else if (verbosity == LoggingVerbosity.Fatal)
            {
                IsDebugEnabled = false;
                IsInfoEnabled = false;
                IsWarnEnabled = false;
                IsErrorEnabled = false;
                IsFatalEnabled = true;
            }
            else
            {
                IsDebugEnabled = false;
                IsInfoEnabled = false;
                IsWarnEnabled = false;
                IsErrorEnabled = false;
                IsFatalEnabled = false;
            }
        }

        public void Debug(object message)
        {
            if (IsDebugEnabled) Write(message);
        }

        public void Debug(object message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                Write(message);
                Write(exception.ToString());
            }
        }

        public void DebugFormat(string format, object arg0)
        {
            if (IsDebugEnabled) WriteFormat(format, arg0);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled) WriteFormat(format, args);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            if (IsDebugEnabled) WriteFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsDebugEnabled) WriteFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsDebugEnabled) WriteFormat(provider, format, args);
        }

        public void Info(object message)
        {
            if (IsInfoEnabled) Write(message);
        }

        public void Info(object message, Exception exception)
        {
            if (IsInfoEnabled)
            {
                Write(message);
                Write(exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled) WriteFormat(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            if (IsInfoEnabled) WriteFormat(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            if (IsInfoEnabled) WriteFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsInfoEnabled) WriteFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsInfoEnabled) WriteFormat(provider, format, args);
        }

        public void Warn(object message)
        {
            if (IsWarnEnabled) Write(message);
        }

        public void Warn(object message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                Write(message);
                Write(exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled) WriteFormat(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            if (IsWarnEnabled) WriteFormat(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            if (IsWarnEnabled) WriteFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsWarnEnabled) WriteFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsWarnEnabled) WriteFormat(provider, format, args);
        }

        public void Error(object message)
        {
            if (IsErrorEnabled) Write(message);
        }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                Write(message);
                Write(exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled) WriteFormat(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            if (IsErrorEnabled) WriteFormat(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            if (IsErrorEnabled) WriteFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsErrorEnabled) WriteFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsErrorEnabled) WriteFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            if (IsFatalEnabled) Write(message);
        }

        public void Fatal(object message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                Write(message);
                Write(exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled) WriteFormat(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            if (IsFatalEnabled) WriteFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            if (IsFatalEnabled) WriteFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            if (IsFatalEnabled) WriteFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            if (IsFatalEnabled) WriteFormat(provider, format, args);
        }

        private void WriteFormat(IFormatProvider formatProvider, string formatString, params object[] args )
        {
            //Console.WriteLine(string.Format(formatProvider, formatString, args));
            System.Diagnostics.Debug.WriteLine(string.Format(formatProvider, formatString, args));
        }

        private void WriteFormat(string formatString, params object[] args)
        {
            //Console.WriteLine(formatString, args);
            System.Diagnostics.Debug.WriteLine(string.Format(formatString, args));
        }

        private void Write(object message)
        {
            //Console.WriteLine(message);
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
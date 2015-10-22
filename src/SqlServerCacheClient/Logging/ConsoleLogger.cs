using System;

namespace SqlServerCacheClient.Logging
{
    internal class ConsoleLogger : ILogger
    {
        public readonly static ConsoleLogger Instance = new ConsoleLogger();

        private ConsoleLogger()
        {
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void Debug(object message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.ToString());
        }

        public void DebugFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(string.Format(provider, format, args));
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void Info(object message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(string.Format(provider, format, args));
        }

        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        public void Warn(object message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(string.Format(provider, format, args));
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void Error(object message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(string.Format(provider, format, args));
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void Fatal(object message, Exception exception)
        {
            Console.WriteLine(message);
            Console.WriteLine(exception.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Console.WriteLine(string.Format(provider, format, args));
        }
    }
}
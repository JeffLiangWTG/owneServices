using System;
using System.Text;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.Tests
{
    public class StringLogger : ILog
    {
        readonly StringBuilder log;
        readonly ILog internalLogger;

        public StringLogger()
        {
            log = new StringBuilder();
        }

        public StringLogger(ILog logger)
            : this()
        {
            internalLogger = logger;
        }

        public override string ToString()
        {
            return log.ToString();
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Debug(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if (internalLogger != null) internalLogger.Debug(formatProvider, formatMessageCallback);
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Debug(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Debug(formatMessageCallback);
        }

        public void Debug(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Debug(message, exception);
            log.AppendLine((string)message).AppendLine(exception.ToString());
        }

        public void Debug(object message)
        {
            if(internalLogger!= null) internalLogger.Debug(message);
            log.AppendLine((string)message);
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.DebugFormat(formatProvider, format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.DebugFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.DebugFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void DebugFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.DebugFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Error(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Error(formatProvider, formatMessageCallback);
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Error(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Error(formatMessageCallback);
        }

        public void Error(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Error(message, exception);
            log.AppendLine(exception.ToString());
        }


        public void Error(object message)
        {
            if(internalLogger!= null) internalLogger.Error(message);
            log.AppendLine((string)message);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.ErrorFormat(formatProvider, format, exception, args);
            log.AppendFormat(formatProvider, format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.ErrorFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.ErrorFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.ErrorFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Fatal(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Fatal(formatProvider, formatMessageCallback);
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Fatal(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Fatal(formatMessageCallback);
        }

        public void Fatal(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Fatal(message, exception);
            log.AppendLine((string)message).AppendLine(exception.ToString());
        }

        public void Fatal(object message)
        {
            if(internalLogger!= null) internalLogger.Fatal(message);
            log.AppendLine((string)message);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.FatalFormat(formatProvider, format, exception, args);
            log.AppendFormat(formatProvider, format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.FatalFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.FatalFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void FatalFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.FatalFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Info(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Info(formatProvider, formatMessageCallback);
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Info(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Info(formatMessageCallback);
        }

        public void Info(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Info(message, exception);
            log.AppendLine((string)message).AppendLine(exception.ToString());
        }

        public void Info(object message)
        {
            if(internalLogger!= null) internalLogger.Info(message);
            log.AppendLine((string)message);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.InfoFormat(formatProvider, format, exception, args);
            log.AppendFormat(formatProvider, format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.InfoFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.InfoFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void InfoFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.InfoFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }

        public bool IsDebugEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsInfoEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsInfoEnabled; }
        }

        public bool IsTraceEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsTraceEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return internalLogger == null ? true : internalLogger.IsWarnEnabled; }
        }

        public IVariablesContext GlobalVariablesContext { get { throw new NotImplementedException(); } }
        public IVariablesContext ThreadVariablesContext { get { throw new NotImplementedException(); } }
        public INestedVariablesContext NestedThreadVariablesContext { get { throw new NotImplementedException(); } }

		public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Trace(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Trace(formatProvider, formatMessageCallback);
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Trace(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Trace(formatMessageCallback);
        }

        public void Trace(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Trace(message, exception);
            log.AppendLine((string)message).AppendLine(exception.ToString());
        }

        public void Trace(object message)
        {
            if(internalLogger!= null) internalLogger.Trace(message);
            log.AppendLine((string)message);
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.TraceFormat(formatProvider, format, exception, args);
            log.AppendFormat(formatProvider, format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.TraceFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.TraceFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void TraceFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.TraceFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Warn(formatProvider, formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Warn(formatProvider, formatMessageCallback);
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Warn(formatMessageCallback, exception);
            log.AppendLine(exception.ToString());
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback)
        {
            if(internalLogger!= null) internalLogger.Warn(formatMessageCallback);
        }

        public void Warn(object message, Exception exception)
        {
            if(internalLogger!= null) internalLogger.Warn(message, exception);
            log.AppendLine((string)message).AppendLine(exception.ToString());
        }

        public void Warn(object message)
        {
            if(internalLogger!= null) internalLogger.Warn(message);
            log.AppendLine((string)message);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.WarnFormat(formatProvider, format, exception, args);
            log.AppendFormat(formatProvider, format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.WarnFormat(formatProvider, format, args);
            log.AppendFormat(formatProvider, format, args).AppendLine();
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
            if(internalLogger!= null) internalLogger.WarnFormat(format, exception, args);
            log.AppendFormat(format, args).AppendLine().AppendLine(exception.ToString());
        }

        public void WarnFormat(string format, params object[] args)
        {
            if(internalLogger!= null) internalLogger.WarnFormat(format, args);
            log.AppendFormat(format, args).AppendLine();
        }
    }
}

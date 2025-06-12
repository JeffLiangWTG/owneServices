using Common.Logging;

using System;
using System.Text;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.CorrelationTests
{
    class MockLogger : ILog
    {
        readonly StringBuilder log = new StringBuilder();

        public string Log
        {
            get
            {
                return log.ToString();
            }
        }

        public bool IsTraceEnabled
        {
            get { return true; }
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

        public IVariablesContext GlobalVariablesContext { get { throw new NotImplementedException(); } }
        public IVariablesContext ThreadVariablesContext { get { throw new NotImplementedException(); } }
        public INestedVariablesContext NestedThreadVariablesContext { get { throw new NotImplementedException(); } }


        public void Debug(object message)
        {
            log.Append("Debug - ").AppendLine(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }

        public void Error(object message)
        {
            log.Append("Error - ").AppendLine(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }

        public void Fatal(object message)
        {
            log.Append("Fatal - ").AppendLine(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void FatalFormat(string format, params object[] args)
        {
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }

        public void Info(object message)
        {
            log.Append("Info - ").AppendLine(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }

        public void Trace(object message)
        {
            log.Append("Trace - ").AppendLine(message.ToString());
        }

        public void Trace(object message, Exception exception)
        {
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void TraceFormat(string format, params object[] args)
        {
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }

        public void Warn(object message)
        {
            log.Append("Warn - ").AppendLine(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
        }

        public void WarnFormat(string format, params object[] args)
        {
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
        }
    }
}

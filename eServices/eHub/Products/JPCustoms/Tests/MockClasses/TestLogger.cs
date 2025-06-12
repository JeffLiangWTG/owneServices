using System;
using System.Text;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	class TestLogger : ILog
	{
		readonly StringBuilder log = new StringBuilder();

		public string Log
		{
			get
			{
				return log.ToString();
			}
		}

		public void Debug(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{
		}

		public void Debug(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{
		}

		public void Debug(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{
		}

		public void Debug(System.Action<FormatMessageHandler> formatMessageCallback)
		{
		}

		public void Debug(object message, System.Exception exception)
		{
		}

		public void Debug(object message)
		{
		}

		public void DebugFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void DebugFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void DebugFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void DebugFormat(string format, params object[] args)
		{

		}

		public void Error(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Error(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Error(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Error(System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Error(object message, System.Exception exception)
		{
			log.Append("Error - ").Append(message.ToString()).Append(" Exception message: ").AppendLine(exception.Message);
		}

		public void Error(object message)
		{
			log.Append("Error - ").AppendLine(message.ToString());
		}

		public void ErrorFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void ErrorFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void ErrorFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void ErrorFormat(string format, params object[] args)
		{

		}

		public void Fatal(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Fatal(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Fatal(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Fatal(System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Fatal(object message, System.Exception exception)
		{
			log.Append("Fatal - ").Append(message.ToString()).Append(" Exception message: ").AppendLine(exception.Message);
		}

		public void Fatal(object message)
		{
			log.Append("Fatal - ").AppendLine(message.ToString());
		}

		public void FatalFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void FatalFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void FatalFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void FatalFormat(string format, params object[] args)
		{

		}

		public void Info(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Info(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Info(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Info(System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Info(object message, System.Exception exception)
		{

		}

		public void Info(object message)
		{
			log.Append("Info - ").AppendLine(message.ToString());
		}

		public void InfoFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void InfoFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void InfoFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void InfoFormat(string format, params object[] args)
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

		public bool IsTraceEnabled
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

		public void Trace(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Trace(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Trace(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Trace(System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Trace(object message, System.Exception exception)
		{

		}

		public void Trace(object message)
		{

		}

		public void TraceFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void TraceFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void TraceFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void TraceFormat(string format, params object[] args)
		{

		}

		public void Warn(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Warn(System.IFormatProvider formatProvider, System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Warn(System.Action<FormatMessageHandler> formatMessageCallback, System.Exception exception)
		{

		}

		public void Warn(System.Action<FormatMessageHandler> formatMessageCallback)
		{

		}

		public void Warn(object message, System.Exception exception)
		{

		}

		public void Warn(object message)
		{
			log.Append("Warn - ").AppendLine(message.ToString());
		}

		public void WarnFormat(System.IFormatProvider formatProvider, string format, System.Exception exception, params object[] args)
		{

		}

		public void WarnFormat(System.IFormatProvider formatProvider, string format, params object[] args)
		{

		}

		public void WarnFormat(string format, System.Exception exception, params object[] args)
		{

		}

		public void WarnFormat(string format, params object[] args)
		{

		}
	}
}

using System;
using System.Collections.Generic;
using Common.Logging;

namespace eServices.LoggingEnhancement.Log4netImpl.CommonLogging
{
	public class LoggingProxy : ILog
	{
		private readonly ILog _internalLogger;
		public Dictionary<string, object> ContextData { get; set; }

		static LoggingProxy()
		{
			log4net.Util.SystemInfo.NullText = string.Empty;
		}

		public LoggingProxy(ILog logger)
		{
			_internalLogger = logger;
		}

		private void SetContext()
		{
			foreach (var keyValuePair in ContextData)
			{
				_internalLogger.ThreadVariablesContext.Set(keyValuePair.Key, $" [{keyValuePair.Key}={keyValuePair.Value}]");
			}
		}

		private void ClearContext()
		{
			foreach (var keyValuePair in ContextData)
			{
				_internalLogger.ThreadVariablesContext.Set(keyValuePair.Key, null);
			}
		}

		public void Trace(object message)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(message);
			ClearContext();
		}

		public void Trace(object message, Exception exception)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(message, exception);
			ClearContext();
		}

		public void TraceFormat(string format, params object[] args)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.TraceFormat(format, args);
			ClearContext();
		}

		public void TraceFormat(string format, Exception exception, params object[] args)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.TraceFormat(format, exception, args);
			ClearContext();
		}

		public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.TraceFormat(formatProvider, format, args);
			ClearContext();
		}

		public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.TraceFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Trace(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(formatMessageCallback);
			ClearContext();
		}

		public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(formatMessageCallback, exception);
			ClearContext();
		}

		public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsTraceEnabled) return;

			SetContext();
			_internalLogger.Trace(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public void Debug(object message)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(message);
			ClearContext();
		}

		public void Debug(object message, Exception exception)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(message, exception);
			ClearContext();
		}

		public void DebugFormat(string format, params object[] args)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.DebugFormat(format, args);
			ClearContext();
		}

		public void DebugFormat(string format, Exception exception, params object[] args)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.DebugFormat(format, exception, args);
			ClearContext();
		}

		public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.DebugFormat(formatProvider, format, args);
			ClearContext();
		}

		public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.DebugFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Debug(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(formatMessageCallback);
			ClearContext();
		}

		public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(formatMessageCallback, exception);
			ClearContext();
		}

		public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsDebugEnabled) return;

			SetContext();
			_internalLogger.Debug(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public void Info(object message)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(message);
			ClearContext();
		}

		public void Info(object message, Exception exception)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(message, exception);
			ClearContext();
		}

		public void InfoFormat(string format, params object[] args)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.InfoFormat(format, args);
			ClearContext();
		}

		public void InfoFormat(string format, Exception exception, params object[] args)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.InfoFormat(format, exception, args);
			ClearContext();
		}

		public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.InfoFormat(formatProvider, format, args);
			ClearContext();
		}

		public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.InfoFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Info(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(formatMessageCallback);
			ClearContext();
		}

		public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(formatMessageCallback, exception);
			ClearContext();
		}

		public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsInfoEnabled) return;

			SetContext();
			_internalLogger.Info(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public void Warn(object message)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(message);
			ClearContext();
		}

		public void Warn(object message, Exception exception)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(message, exception);
			ClearContext();
		}

		public void WarnFormat(string format, params object[] args)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.WarnFormat(format, args);
			ClearContext();
		}

		public void WarnFormat(string format, Exception exception, params object[] args)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.WarnFormat(format, exception, args);
			ClearContext();
		}

		public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.WarnFormat(formatProvider, format, args);
			ClearContext();
		}

		public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.WarnFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Warn(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(formatMessageCallback);
			ClearContext();
		}

		public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(formatMessageCallback, exception);
			ClearContext();
		}

		public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsWarnEnabled) return;

			SetContext();
			_internalLogger.Warn(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public void Error(object message)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(message);
			ClearContext();
		}

		public void Error(object message, Exception exception)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(message, exception);
			ClearContext();
		}

		public void ErrorFormat(string format, params object[] args)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.ErrorFormat(format, args);
			ClearContext();
		}

		public void ErrorFormat(string format, Exception exception, params object[] args)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.ErrorFormat(format, exception, args);
			ClearContext();
		}

		public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.ErrorFormat(formatProvider, format, args);
			ClearContext();
		}

		public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.ErrorFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Error(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(formatMessageCallback);
			ClearContext();
		}

		public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(formatMessageCallback, exception);
			ClearContext();
		}

		public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsErrorEnabled) return;

			SetContext();
			_internalLogger.Error(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public void Fatal(object message)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(message);
			ClearContext();
		}

		public void Fatal(object message, Exception exception)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(message, exception);
			ClearContext();
		}

		public void FatalFormat(string format, params object[] args)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.FatalFormat(format, args);
			ClearContext();
		}

		public void FatalFormat(string format, Exception exception, params object[] args)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.FatalFormat(format, exception, args);
			ClearContext();
		}

		public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.FatalFormat(formatProvider, format, args);
			ClearContext();
		}

		public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.FatalFormat(formatProvider, format, exception, args);
			ClearContext();
		}

		public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(formatMessageCallback);
			ClearContext();
		}

		public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(formatMessageCallback, exception);
			ClearContext();
		}

		public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(formatProvider, formatMessageCallback);
			ClearContext();
		}

		public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
		{
			if (!IsFatalEnabled) return;

			SetContext();
			_internalLogger.Fatal(formatProvider, formatMessageCallback, exception);
			ClearContext();
		}

		public bool IsTraceEnabled => _internalLogger.IsTraceEnabled;

		public bool IsDebugEnabled => _internalLogger.IsDebugEnabled;

		public bool IsErrorEnabled => _internalLogger.IsErrorEnabled;

		public bool IsFatalEnabled => _internalLogger.IsFatalEnabled;

		public bool IsInfoEnabled => _internalLogger.IsInfoEnabled;

		public bool IsWarnEnabled => _internalLogger.IsWarnEnabled;

		public IVariablesContext GlobalVariablesContext => _internalLogger.GlobalVariablesContext;

		public IVariablesContext ThreadVariablesContext => _internalLogger.ThreadVariablesContext;

		public INestedVariablesContext NestedThreadVariablesContext => _internalLogger.NestedThreadVariablesContext;
	}
}
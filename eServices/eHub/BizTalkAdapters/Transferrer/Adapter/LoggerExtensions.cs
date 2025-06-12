using System;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public static class LoggerExtensions
	{
		public static void Log(this ILog logger, string activityId, LogLevel level, string message, Exception exception, params object[] args)
			=> logger.Log(level, $"({activityId}) {message}", exception, args);

		public static void Log(this ILog logger, string activityId, LogLevel level, string message, params object[] args)
			=> logger.Log(level, $"({activityId}) {message}", null, args);

		private static void Log(this ILog logger, LogLevel level, string message, Exception exception, params object[] args)
		{
			switch (level)
			{
				case LogLevel.Trace when logger.IsTraceEnabled && args.Length == 0:
					logger.Trace(message, exception);
					break;
				case LogLevel.Trace when logger.IsTraceEnabled && args.Length > 0:
					logger.TraceFormat(message, exception, args);
					break;
				case LogLevel.Debug when logger.IsDebugEnabled && args.Length == 0:
					logger.Debug(message, exception);
					break;
				case LogLevel.Debug when logger.IsDebugEnabled && args.Length > 0:
					logger.DebugFormat(message, exception, args);
					break;
				case LogLevel.Info when logger.IsInfoEnabled && args.Length == 0:
					logger.Info(message, exception);
					break;
				case LogLevel.Info when logger.IsInfoEnabled && args.Length > 0:
					logger.InfoFormat(message, exception, args);
					break;
				case LogLevel.Warn when logger.IsWarnEnabled && args.Length == 0:
					logger.Warn(message, exception);
					break;
				case LogLevel.Warn when logger.IsWarnEnabled && args.Length > 0:
					logger.WarnFormat(message, exception, args);
					break;
				case LogLevel.Error when logger.IsErrorEnabled && args.Length == 0:
					logger.Error(message, exception);
					break;
				case LogLevel.Error when logger.IsErrorEnabled && args.Length > 0:
					logger.ErrorFormat(message, exception, args);
					break;
				case LogLevel.Fatal when logger.IsFatalEnabled && args.Length == 0:
					logger.Fatal(message, exception);
					break;
				case LogLevel.Fatal when logger.IsFatalEnabled && args.Length > 0:
					logger.FatalFormat(message, exception, args);
					break;
			}
		}
	}
}

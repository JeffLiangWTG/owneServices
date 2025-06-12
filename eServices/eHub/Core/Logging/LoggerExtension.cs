using System;
using Common.Logging;

namespace CargoWise.eHub.Core.Logging.LoggerExtensions
{
	public static class LoggerExtension
	{
		public static void Debug(this ILog logger, Func<string> callback)
		{
			if(logger.IsDebugEnabled)
			{
				logger.Debug(callback());
			}
		}

		public static void Error(this ILog logger, Func<string> callback)
		{
			if (logger.IsErrorEnabled)
			{
				logger.Error(callback());
			}
		}

		public static void Info(this ILog logger, Func<string> callback)
		{
			if (logger.IsInfoEnabled)
			{
				logger.Info(callback());
			}
		}

		public static void Trace(this ILog logger, Func<string> callback)
		{
			if (logger.IsTraceEnabled)
			{
				logger.Trace(callback());
			}
		}

		public static void Warn(this ILog logger, Func<string> callback)
		{
			if (logger.IsWarnEnabled)
			{
				logger.Warn(callback());
			}
		}
	}
}

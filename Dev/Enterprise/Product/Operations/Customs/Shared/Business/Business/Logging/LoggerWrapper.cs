using System;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.Customs.Business.Logging
{
	public class LoggerWrapper : ICommonLogger
	{
		public LoggerWrapper(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		readonly ILogger logger;

		public void Log(LogType logLevel, string message)
		{
			logger.Log(logLevel, message);
		}

		public void Log(LogType logLevel, string message, Exception ex)
		{
			logger.Log(logLevel, message, ex);
		}

		public void LogFormat(LogType logLevel, string format, params object[] args)
		{
			logger.Log(logLevel, string.Format(format, args));
		}

		public void BumpSectionProgress()
		{
		}

		public void SetSectionProgressMax(int max)
		{
		}
	}
}

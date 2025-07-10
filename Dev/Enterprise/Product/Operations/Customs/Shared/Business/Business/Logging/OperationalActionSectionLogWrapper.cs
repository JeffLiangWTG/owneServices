using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.Business.Logging
{
	public class OperationalActionSectionLogWrapper : ICommonLogger
	{
		public OperationalActionSectionLogWrapper(IOperationalActionSectionLog logger)
		{
			actionLog = Argument.NotNull(logger, nameof(logger));
		}
		readonly IOperationalActionSectionLog actionLog;

		public void Log(LogType logLevel, string message)
		{
			actionLog.Notify(convertLogType(logLevel), message);
		}

		public void Log(LogType logLevel, string message, Exception ex)
		{
			actionLog.Notify(convertLogType(logLevel), message);
		}

		public void LogFormat(LogType logLevel, string format, params object[] args)
		{
			actionLog.NotifyFormat(convertLogType(logLevel), format, args);
		}

		public void BumpSectionProgress() => actionLog.BumpSectionProgress();
		public void SetSectionProgressMax(int max) => actionLog.SetSectionProgressMax(max);

		OperationalActionLogErrorLevel convertLogType(LogType level)
		{
			switch (level)
			{
				case LogType.Debug:
					return OperationalActionLogErrorLevel.Debug;
				case LogType.Information:
					return OperationalActionLogErrorLevel.Informational;
				case LogType.Warning:
					return OperationalActionLogErrorLevel.Warning;
				case LogType.Error:
					return OperationalActionLogErrorLevel.Error;
				default:
					return OperationalActionLogErrorLevel.Informational;
			}
		}
	}
}

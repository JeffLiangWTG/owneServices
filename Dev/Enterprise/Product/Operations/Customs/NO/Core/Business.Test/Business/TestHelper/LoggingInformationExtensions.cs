using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.NO.Business.Testing;

static class LoggingInformationExtensions
{
	public static (LogType type, string message)[] CaptureLogMessages(this LoggingInformation logger, Action action)
	{
		Argument.NotNull(action, nameof(action));
		using (logger.CaptureLogMessagesDisposable(out var logs))
		{
			action();
			return logs.ToArray();
		}
	}

	public static IDisposable CaptureLogMessagesDisposable(this LoggingInformation logger, out IEnumerable<(LogType type, string message)> logs)
	{
		Argument.NotNull(logger, nameof(logger));
		var logsList = new List<(LogType type, string message)>();
		logs = logsList.AsEnumerable();

		return new DisposableAction(
			createAction: () => logger.OnLogInfoAdded += CaptureLogMessage,
			disposeAction: () => logger.OnLogInfoAdded -= CaptureLogMessage);

		void CaptureLogMessage(string log, LogType type)
		{
			if (log.StartsWith(" *** "))
			{
				log = log.Substring(5);
			}
			logsList.Add((type, log.Trim()));
		}
	}
}

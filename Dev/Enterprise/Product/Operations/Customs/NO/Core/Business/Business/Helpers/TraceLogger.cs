using System;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.NO.Business;

sealed class TraceLogger : IDisposable
{
	public TraceLogger(string message, LoggingInformation logger)
	{
		Message = Argument.NotNullOrEmpty(message, nameof(message));
		Logger = Argument.NotNull(logger, nameof(logger));
		Logger.Log(LogType.Information, $"Started {Message}");
	}

	string Message { get; }
	LoggingInformation Logger { get; }

	void IDisposable.Dispose()
	{
		Logger.Log(LogType.Information, $"Finished {Message}");
	}
}

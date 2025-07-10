using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(TraceLogger))]
sealed class TraceLoggerTest : TestCaseWithFactory
{
	public void TestTraceLogger_ShouldThrowOnInvalidParameters()
	{
		const string message = "my message";
		var logger = new LoggingInformation();

		AssertArgumentExceptionThrown<ArgumentNullException>("When message is null", nameof(message), () => _ = new TraceLogger(null, logger));
		AssertArgumentExceptionThrown<ArgumentException>("When message is empty", nameof(message), () => _ = new TraceLogger(string.Empty, logger));
		AssertArgumentExceptionThrown<ArgumentNullException>("When logger is null", nameof(logger), () => _ = new TraceLogger(message, null));

		AssertNoExceptionThrown(() => _ = new TraceLogger(message, logger));
	}

	public void TestTraceLogger_ShouldAddTraceLogs()
	{
		const string traceMessage = "trace me";
		var logger = new LoggingInformation();
		var logs = new List<(LogType type, string message)>();
		logger.OnLogInfoAdded += CaptureLogMessage;

		using (new TraceLogger("trace me", logger))
		{
			AssertContainsExactElementsInExactOrder("On enter", [
				(LogType.Information, $"Started {traceMessage}"),
			], logs);
		}
		AssertContainsExactElementsInExactOrder("On exit", [
				(LogType.Information, $"Started {traceMessage}"),
				(LogType.Information, $"Finished {traceMessage}"),
		], logs);

		void CaptureLogMessage(string log, LogType type) => logs.Add((type, log.Trim()));
	}
}

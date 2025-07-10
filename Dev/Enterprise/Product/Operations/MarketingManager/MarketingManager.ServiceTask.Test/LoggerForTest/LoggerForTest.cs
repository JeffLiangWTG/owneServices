using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	public sealed class LoggerForTest : ILogger
	{
		public void Log(LogType type, string message, Exception ex)
		{
			Logs.Add(new LogForTest(type, message, ex));
		}

		public void Log(LogType type, string message)
		{
			Logs.Add(new LogForTest(type, message, null));
		}

		public List<LogForTest> Logs
		{
			get { return logs ?? (logs = new List<LogForTest>()); }
		}
		List<LogForTest> logs;
	}
}

using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class MockLogger : ILogger
	{
		public void Log(LogType type, string message, Exception ex) => Logs.Add(message + ex.ToString());

		public void Log(LogType type, string message) => Logs.Add(message);

		public IList<string> Logs => logs ?? (logs = new List<string>());

		IList<string> logs;
	}
}

using System;
using System.Collections.Generic;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Logging.Testing
{
	sealed class LoggerWrapperTest : TestCase
	{
		public void TestICommonLogger()
		{
			var logger = new LoggerForTest();
			var commonLogger = new LoggerWrapper(logger);
			commonLogger.Log(LogType.Debug, "Debug");
			commonLogger.Log(LogType.Warning, "Warning");
			commonLogger.Log(LogType.Error, "Error", new Exception("exception"));
			commonLogger.LogFormat(LogType.Information, "{0} {1}", "Information", "Message");
			commonLogger.SetSectionProgressMax(10);
			commonLogger.BumpSectionProgress();

			AssertEquals(@"Debug|Debug
Warning|Warning
Error|Error Exception:exception
Information|Information Message", logger.ToString());
		}

		class LoggerForTest : ILogger
		{
			void ILogger.Log(LogType type, string message, Exception ex)
			{
				logs.Add(string.Format("{0}|{1} Exception:{2}", type, message, ex.Message));
			}

			void ILogger.Log(LogType type, string message)
			{
				logs.Add(string.Format("{0}|{1}", type, message));
			}

			public IEnumerable<string> Logs
			{
				get { return logs; }
			}
			readonly List<string> logs = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", Logs);
			}
		}
	}
}

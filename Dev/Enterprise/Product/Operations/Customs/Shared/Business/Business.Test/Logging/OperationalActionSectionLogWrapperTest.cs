using System;
using Enterprise.Integration;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Logging.Testing
{
	sealed class OperationalActionSectionLogWrapperTest : TestCase
	{
		public void TestIOperationalActionSectionLog()
		{
			var logger = new DummyOperationalActionSectionLog();
			var commonLogger = new OperationalActionSectionLogWrapper(logger);
			commonLogger.Log(LogType.Debug, "Debug");
			commonLogger.Log(LogType.Warning, "Warning");
			commonLogger.Log(LogType.Error, "Error", new Exception("exception"));
			commonLogger.LogFormat(LogType.Information, "{0} {1}", "Information", "Message");
			commonLogger.SetSectionProgressMax(10);
			commonLogger.BumpSectionProgress();

			AssertEquals(@"WARNING: Warning
ERROR: Error
INFO: Information Message", logger.MessagesString().Replace("\n", "\r\n"));
		}
	}
}

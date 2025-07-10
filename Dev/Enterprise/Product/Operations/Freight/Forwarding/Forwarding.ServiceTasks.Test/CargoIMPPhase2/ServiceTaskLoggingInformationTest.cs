using CargoWise.EntityFramework.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMPPhase2.Test
{
	internal class ServiceTaskLoggingInformationTest : TestCaseWithFactory
	{
		public void TestLog()
		{
			TestServiceLogger innerLog = new TestServiceLogger();
			ServiceTaskLoggingInformation log = new ServiceTaskLoggingInformation(innerLog);
			log.Log("Message 1");
			log.DebugLog("Message 2");
			AssertEquals(2, innerLog.Count);
			AssertContains("Message 1", innerLog[0]);
			AssertContains("Information", innerLog[0]);
			AssertContains("Message 2", innerLog[1]);
			AssertContains("Debug", innerLog[1]);
		}
	}
}

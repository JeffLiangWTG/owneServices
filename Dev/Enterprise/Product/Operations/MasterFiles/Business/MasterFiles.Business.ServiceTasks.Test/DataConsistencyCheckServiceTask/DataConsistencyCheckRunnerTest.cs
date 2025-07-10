using CargoWise.EntityFramework.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterFiles.Business.ServiceTasks.Test
{
	sealed class DataConsistencyCheckRunnerTest : TestCaseWithFactory
	{
		public void TestCheckRunner()
		{
			var testLogger = new TestServiceLogger();
			var runner = new DataConsistencyCheckRunner(testLogger);
			Assert(testLogger.Count == 0);

			runner.PerformChecks();

			var testLoggerString = testLogger.ToString();

			Assert(testLogger.Count > 0);
			AssertContains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - started.", testLoggerString);
			AssertContains("Information|Data Consistency Check - Orphan WIPs or Accruals detection - finished.", testLoggerString);
		}
	}
}

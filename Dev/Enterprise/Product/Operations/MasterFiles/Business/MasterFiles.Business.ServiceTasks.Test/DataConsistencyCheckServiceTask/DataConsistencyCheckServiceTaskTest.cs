using System.Collections.Generic;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterFiles.Business.ServiceTasks.Test
{
	abstract class DataConsistencyCheckServiceTaskTest : ServiceTaskTestCase<DataConsistencyCheckServiceTask>
	{
		public void TestServiceTask()
		{
			DataConsistencyCheckServiceTask serviceTask = new DataConsistencyCheckServiceTask();

			PrepareData();

			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);

			AssertResult();
			AssertLog(log.ToString());

			FixData();

			RunTaskSchedule(serviceTask);
			AssertResultAfterDataFixed();
			AssertLogAfterDataFixed(log.ToString());
		}

		protected abstract void PrepareData();
		protected abstract void AssertResult();
		protected abstract void AssertLog(string log);

		protected virtual void FixData()
		{
		}

		protected virtual void AssertResultAfterDataFixed()
		{
		}

		protected virtual void AssertLogAfterDataFixed(string log)
		{
		}

		// No nudging: no queue table.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("ADC");
			AssertNull("No queue table for this service task; polls internal data for inconsistencies.", queueProvider);
		}
	}
}

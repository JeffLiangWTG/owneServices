using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Test
{
	[TestedType(typeof(ContainerStorageExceptionGeneratorServiceTask))]
	internal class ContainerStorageExceptionGeneratorServiceTaskTest : ServiceTaskTestCase<ContainerStorageExceptionGeneratorServiceTask>
	{
		public void TestRunTask()
		{
			ContainerStorageExceptionGeneratorServiceTask task = new ContainerStorageExceptionGeneratorServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);
		}

		public void TestHostedServiceAttributes()
		{
			var result = GetHostedServiceAttributes().SingleOrDefault();

			AssertNotNull(result);
			AssertEquals(true, result.IsMandatory);
			AssertEquals(false, result.AllowsMultipleInstances);
			AssertEquals(false, result.IsScheduleReadOnly);
			AssertEquals("20minutes", result.MinimumPeriod);
			AssertEquals(true, result.CanRunInAnyBranch);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}

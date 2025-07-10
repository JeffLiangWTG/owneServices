using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transit.ServiceTasks.Testing
{
	[TestedType(typeof(DispatchTransportationUnitFinalisedTimeServiceTask))]
	class DispatchTransportationUnitFinalisedTimeServiceTaskTest : ServiceTaskTestCase<DispatchTransportationUnitFinalisedTimeServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("15minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new DispatchTransportationUnitFinalisedTimeServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			AssertEquals(
				"Since there are no Dispatch Transportation Unit to set the Finalized Time, Information should be shown.",
				@"Information|Finalize Dispatch Transportation Units.
No Dispatch Transportation Units found.",
				logger.ToString().Trim());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}

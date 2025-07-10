using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Packing.ServiceTasks.Testing
{
	[TestedType(typeof(JobOrphanScanPurgerServiceTask))]
	class JobOrphanScanPurgerServiceTaskTest : ServiceTaskTestCase<JobOrphanScanPurgerServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertEquals("Anonymous Packages Purger", hostedServiceAttribute.Description);
			AssertEquals("APP", hostedServiceAttribute.Code);
			AssertEquals("PKG", hostedServiceAttribute.Category);
			AssertEquals(true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals(typeof(JobOrphanScanPurgerServiceTask), hostedServiceAttribute.Type);
			AssertEquals("1hour", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("1day", hostedServiceAttribute.DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new JobOrphanScanPurgerServiceTask { ServiceLogger = logger };
			serviceTask.RunTask();
			AssertEquals("logger.ToString()", "Information|Did not find any Anonymous Packages to Purge.", logger.ToString().Trim());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}

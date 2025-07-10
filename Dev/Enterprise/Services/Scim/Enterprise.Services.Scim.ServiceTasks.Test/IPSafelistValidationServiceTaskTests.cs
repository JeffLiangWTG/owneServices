using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Test;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Services.Scim.Business.Test
{
	[TestedType(typeof(IPSafelistValidationServiceTask))]
	public class IPSafelistValidationServiceTaskTests : ServiceTaskTestCase<IPSafelistValidationServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[UseSnapshotProtection]
		public void TestRunTask_UpdatesIPS()
		{
			System.Environment.SetEnvironmentVariable("DAT_IS_TESTING", "true");

			var testHelper = new TestIPSafelistHelper();
			var dbStore = new IPSafelistDBStore(testHelper);
			var process = new IPSafelistValidationServiceTask(dbStore);

			SystemDataRegistry.Instance.EnableScimService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown(() => process.RunTask());
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}
	}
}

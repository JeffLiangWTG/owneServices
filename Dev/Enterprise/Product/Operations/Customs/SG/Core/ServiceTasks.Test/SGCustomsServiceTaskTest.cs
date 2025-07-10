using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.SG.V4.ServiceTasks.Testing
{
	[TestedType(typeof(SGCustomsServiceTask))]
	sealed class SGCustomsServiceTaskTest : ServiceTaskTestCase<SGCustomsServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(SGCustomsServiceTask).GetMethod(nameof(SGCustomsServiceTask.CheckServiceTaskEnvironmentIsValid));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementsAttribute)));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCPServiceTask))]
	sealed class UCPServiceTaskTest : ServiceTaskTestCase<UCPServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(UCPServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == UniversalCustomsMessagingConstants.ServiceTaskCodes.Packing);
			CombineAssertions(() =>
			{
				AssertEquals("Description", "UniversalCustomsMessaging Process:Packing outgoing EDIMessage to EDIInterchange", attribute.Description);
				AssertEquals("Category", "CUS", attribute.Category);
				AssertEquals("ConfigControlType", typeof(UCPServiceTask), attribute.Type);
				AssertEquals("AllowsMultipleInstances", true, attribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("DefaultScheduleRunEvery", "1minute", attribute.DefaultScheduleRunEvery);
				Assert("ActiveByDefault", attribute.ActiveByDefault);
			});
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}

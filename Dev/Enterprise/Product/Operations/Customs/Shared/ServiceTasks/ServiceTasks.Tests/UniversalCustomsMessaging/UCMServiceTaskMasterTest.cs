using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCMServiceTaskMaster))]
	class UCMServiceTaskMasterTest : ServiceTaskTestCase<UCMServiceTaskMaster>
	{
		public TestServiceLogger InitialiseAndRunTaskSchedule() => InitialiseAndRunTaskSchedule(new UCMServiceTaskMaster());

		public void TestOptions()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestHasUniversalCustomsMessagingSubscribers()
		{
			var methodInfo = typeof(UCMServiceTaskMaster).GetMethod(nameof(UCMServiceTaskMaster.HasUniversalCustomsMessagingSubscribers), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (new UCMPProcessorsRegistrationSubstitute())
			{
				AssertEquals("UCMServiceTaskMaster.HasUniversalCustomsMessagingSubscribers()", "There is no Customs message subscribed to Universal Customs Message Processing", UCMServiceTaskMaster.HasUniversalCustomsMessagingSubscribers());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}

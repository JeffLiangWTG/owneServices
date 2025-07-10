using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	[TestedType(typeof(TelematicsRimDataSendingServiceTask))]
	public class TelematicsRimDataSendingServiceTaskTests : ServiceTaskTestCase<TelematicsRimDataSendingServiceTask>
	{
		public void TestCode()
		{
			AssertEquals("TES", TelematicsRimDataSendingServiceTask.Code);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsRimDataSendingServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == TelematicsRimDataSendingServiceTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsRimDataSendingServiceTask), result.Type);
			});
		}

		public void TestHostedServiceBusinessObjectBindingAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsRimDataSendingServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceBusinessObjectBindingAttribute>();

			// Act
			var result = attributes
				.SingleOrDefault(attribute => attribute.ServiceTaskCode == TelematicsRimDataSendingServiceTask.Code);

			// Assert
			AssertNull(result);
		}

		public void TestMinimumPeriod()
		{
			var minimumPeriod = typeof(TelematicsRimDataSendingServiceTask).Assembly
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.SingleOrDefault(attribute => attribute.Code == TelematicsRimDataSendingServiceTask.Code)
				?.MinimumPeriod;
			AssertEquals("1minute", minimumPeriod);
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			var result = thisClassAttribute.CanRunInAnyBranch;
			AssertEquals("This service task should be able to be run in any branch.", true, result);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}

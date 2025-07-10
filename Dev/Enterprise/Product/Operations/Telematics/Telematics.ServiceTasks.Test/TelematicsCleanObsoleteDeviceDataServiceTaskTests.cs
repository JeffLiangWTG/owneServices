using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	[TestedType(typeof(TelematicsCleanObsoleteDeviceDataServiceTask))]
	class TelematicsCleanObsoleteDeviceDataServiceTaskTests : ServiceTaskTestCase<TelematicsCleanObsoleteDeviceDataServiceTask>
	{
		public void TestCode()
		{
			AssertEquals("Telematics data cleaner service tasks have code of \"TCL\"", "TCL", TelematicsCleanObsoleteDeviceDataServiceTask.Code);
		}

		public void TestServiceTaskIsRegistered()
		{
			// Arrange
			var assembly = typeof(TelematicsCleanObsoleteDeviceDataServiceTask).Assembly;
			var attributes = assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);

			// Act
			var result = attributes
				.Cast<HostedServiceAttribute>()
				.Count(attribute => attribute.Code == TelematicsCleanObsoleteDeviceDataServiceTask.Code);

			// Assert
			AssertEquals("No hosted service attributes found on service task", 1, result);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsCleanObsoleteDeviceDataServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == TelematicsCleanObsoleteDeviceDataServiceTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsCleanObsoleteDeviceDataServiceTask), result.Type);
			});
		}

		public void TestMinimumPeriod()
		{
			var minimumPeriod = typeof(TelematicsCleanObsoleteDeviceDataServiceTask).Assembly
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.SingleOrDefault(attribute => attribute.Code == TelematicsCleanObsoleteDeviceDataServiceTask.Code)
				?.MinimumPeriod;
			AssertEquals("1day", minimumPeriod);
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

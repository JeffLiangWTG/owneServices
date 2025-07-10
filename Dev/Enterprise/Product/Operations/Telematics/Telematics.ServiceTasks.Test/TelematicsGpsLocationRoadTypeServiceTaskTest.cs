using System.Collections.Generic;
using System.Linq;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	[TestedType(typeof(TelematicsGpsLocationRoadTypeServiceTask))]
	class TelematicsGpsLocationRoadTypeServiceTaskTest : ServiceTaskTestCase<TelematicsGpsLocationRoadTypeServiceTask>
	{
		public void TestCode()
		{
			AssertEquals("Telematics service task have code of \"TGP\"", "TGP", TelematicsGpsLocationRoadTypeServiceTask.Code);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsGpsLocationRoadTypeServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == TelematicsGpsLocationRoadTypeServiceTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsGpsLocationRoadTypeServiceTask), result.Type);
			});
		}

		public void TestHostedServiceBusinessObjectBindingAttributeParameters()
		{
			Test("Gps Locations with unknown road type", "V2_RoadType=U");

			void Test(string bindingName, params string[] predicates)
			{
				// Arrange
				var attributes = typeof(TelematicsGpsLocationRoadTypeServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceBusinessObjectBindingAttribute>();
				var expectedPredicates = predicates;

				// Act
				var result = attributes
					.Where(attribute => attribute.QueueName == bindingName)
					.Single(attribute => attribute.ServiceTaskCode == TelematicsGpsLocationRoadTypeServiceTask.Code);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("GlbDeviceLocation", result.Table);
					AssertContainsExactElementsInAnyOrder(expectedPredicates, result.Predicates);
				});
			}
		}

		public void TestMinimumPeriod()
		{
			var minimumPeriod = typeof(TelematicsGpsLocationRoadTypeServiceTask).Assembly
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.SingleOrDefault(attribute => attribute.Code == TelematicsGpsLocationRoadTypeServiceTask.Code)
				?.MinimumPeriod;
			AssertEquals("1second", minimumPeriod);
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			var result = thisClassAttribute.CanRunInAnyBranch;
			AssertEquals("This service task should be able to be run in any branch.", true, result);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						GlbDeviceLocationSchema.Constants.TableName,
						"Gps Locations with unknown road type",
						GlbDeviceLocationSchema.Constants.V2_RoadType + "=" + GlbDeviceLocationRoadTypes.Codes.Unknown),
				};
			}
		}
	}
}

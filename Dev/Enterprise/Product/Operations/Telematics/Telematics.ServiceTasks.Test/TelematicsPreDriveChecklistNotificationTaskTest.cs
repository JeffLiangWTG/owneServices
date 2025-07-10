using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	[TestedType(typeof(TelematicsPreDriveChecklistNotificationTask))]
	class TelematicsPreDriveChecklistNotificationTaskTest : ServiceTaskTestCase<TelematicsPreDriveChecklistNotificationTask>
	{
		public void TestCode()
		{
			AssertEquals("TCH", TelematicsPreDriveChecklistNotificationTask.Code);
		}

		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(TelematicsPreDriveChecklistNotificationTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == TelematicsPreDriveChecklistNotificationTask.Code);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("TEL", result.Category);
				AssertEquals(typeof(TelematicsPreDriveChecklistNotificationTask), result.Type);
			});
		}

		public void TestHostedServiceBusinessObjectBindingAttributeParameters()
		{
			CombineAssertions(() =>
			{
				Test("Unprocessed Pre-Drive checklists", $"{TelPreDriveChecklistHeaderSchema.Constants.TPH_IsProcessed}=0");
			});

			void Test(string bindingName, params string[] predicates)
			{
				// Arrange
				var attributes = typeof(TelematicsPreDriveChecklistNotificationTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceBusinessObjectBindingAttribute>();
				var expectedPredicates = predicates;

				// Act
				var result = attributes
					.Where(attribute => attribute.QueueName == bindingName)
					.Single(attribute => attribute.ServiceTaskCode == TelematicsPreDriveChecklistNotificationTask.Code);

				// Assert
				AssertEquals(TelPreDriveChecklistHeaderSchema.Constants.TableName, result.Table);
				AssertContainsExactElementsInAnyOrder(expectedPredicates, result.Predicates);
			}
		}

		public void TestMinimumPeriod()
		{
			var minimumPeriod = typeof(TelematicsPreDriveChecklistNotificationTask).Assembly
				.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.SingleOrDefault(attribute => attribute.Code == TelematicsPreDriveChecklistNotificationTask.Code)
				?.MinimumPeriod;
			AssertEquals("1minute", minimumPeriod);
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
						TelPreDriveChecklistHeaderSchema.Constants.TableName,
						"Unprocessed Pre-Drive checklists",
						TelPreDriveChecklistHeaderSchema.Constants.TPH_IsProcessed + "=0"),
				};
			}
		}
	}
}

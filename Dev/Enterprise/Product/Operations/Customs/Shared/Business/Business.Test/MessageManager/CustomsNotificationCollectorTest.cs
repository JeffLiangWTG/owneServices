using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsNotificationCollectorTest : TestCaseWithFactory
	{
		[TestDate(2013, 06, 11)]
		public void TestTransportNotificationsExcluded()
		{
			var voyage = Factory.New<JobVoyage>();
			var destination = voyage.Destinations.AddNew();
			destination.JB_A_ARV = ZDateTime.Today.AddDays(1);
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			var sailing = voyage.Sailings.AddNew();
			sailing.JX_JB = destination.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			var transport = declaration.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;

			AssertHasErrors("ATA cannot be a future date", transport.Sailing.Destination.JB_A_ARVInfo);

			const string expectedNotification = "Actual Time of Arrival: The Actual Time of Arrival cannot be set in the future. The date 12-Jun-13 00:00 is in the future for AUSYD (UTC+0).";
			var notification = new ZNotificationCollector(declaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString();
			AssertContains("Pre-Condition: ZNotificationCollector has Transport notifications", expectedNotification, notification);

			notification = new CustomsNotificationCollector(declaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString();
			AssertNotContains("Transport notifications should be excluded", expectedNotification, notification);
		}
	}
}

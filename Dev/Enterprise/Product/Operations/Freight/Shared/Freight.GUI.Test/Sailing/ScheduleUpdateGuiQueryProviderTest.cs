using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ScheduleUpdateGuiQueryProviderTest : TestCaseWithFactory
	{
		const string NoText = "None ";
		const string ShouldUpdateAgencyShipmentDatesFromATDText = "Question There are some shipping manager shipments departing this port that do not have a shipped on board date and/or issue date set, would you like to default the shipped on board and issued dates for these shipments?";
		const string ShouldUpdateRelatedShipmentsETDText = @"Question You have updated Port of Loading Date.

Do you want the system to automatically update Origin ETD for Shipments and Declarations on the same schedule?

	Click""Yes"" - if you want to update Shipments and Declarations.
	Click""No"" - if you do not want Shipments and Declarations updated.";

		const string ShouldUpdateRelatedShipmentsETAText = @"Question You have updated Port of Discharge Date.

Do you want the system to automatically update Destination ETA for Shipments and Declarations on the same Schedule?

	Click""Yes"" - if you want to update Shipments and Declarations.
	Click""No"" - if you do not want Shipments and Declarations updated.";

		public void TestSet()
		{
			AssertEquals("ScheduleUpdateNullQueryProvider", ScheduleUpdateQueryProviderFactory.Get(Factory).GetType().Name);

			ScheduleUpdateGuiQueryProvider.Set(Factory, null, null);
			AssertType(typeof(ScheduleUpdateGuiQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));
		}

		public void TestShouldUpdateAgencyShipmentDatesFromATD_Yes()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("first access", true, provider.ShouldUpdateAgencyShipmentDatesFromATD);
			AssertEquals("ask", ShouldUpdateAgencyShipmentDatesFromATDText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", true, provider.ShouldUpdateAgencyShipmentDatesFromATD);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestShouldUpdateAgencyShipmentDatesFromATD_No()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("first access", false, provider.ShouldUpdateAgencyShipmentDatesFromATD);
			AssertEquals("ask", ShouldUpdateAgencyShipmentDatesFromATDText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", false, provider.ShouldUpdateAgencyShipmentDatesFromATD);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestShouldUpdateRelatedShipmentsETD_Yes()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("first access", true, provider.ShouldUpdateRelatedShipmentsETD);
			AssertEquals("ask", ShouldUpdateRelatedShipmentsETDText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", true, provider.ShouldUpdateRelatedShipmentsETD);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestShouldUpdateRelatedShipmentsETD_No()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("first access", false, provider.ShouldUpdateRelatedShipmentsETD);
			AssertEquals("ask", ShouldUpdateRelatedShipmentsETDText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", false, provider.ShouldUpdateRelatedShipmentsETD);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestShouldUpdateRelatedShipmentsETA_Yes()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("first access", true, provider.ShouldUpdateRelatedShipmentsETA);
			AssertEquals("ask", ShouldUpdateRelatedShipmentsETAText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", true, provider.ShouldUpdateRelatedShipmentsETA);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestShouldUpdateRelatedShipmentsETA_No()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("first access", false, provider.ShouldUpdateRelatedShipmentsETA);
			AssertEquals("ask", ShouldUpdateRelatedShipmentsETAText, UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("second access", false, provider.ShouldUpdateRelatedShipmentsETA);
			AssertEquals("cache the result from the first access instead of asking again", NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void ShouldSendDelayAlerts()
		{
			ScheduleUpdateGuiQueryProvider provider = new ScheduleUpdateGuiQueryProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, provider.ShouldSendDelayAlerts(true, true));
			AssertEquals("Do you wish to send a delay alert document to each affected importer and exporter?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, provider.ShouldSendDelayAlerts(true, true));
			AssertEquals("Do you wish to send a delay alert document to each affected importer and exporter?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, provider.ShouldSendDelayAlerts(true, false));
			AssertEquals("Do you wish to send a delay alert document to each affected importer?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, provider.ShouldSendDelayAlerts(true, false));
			AssertEquals("Do you wish to send a delay alert document to each affected importer?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, provider.ShouldSendDelayAlerts(false, true));
			AssertEquals("Do you wish to send a delay alert document to each affected exporter?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, provider.ShouldSendDelayAlerts(false, true));
			AssertEquals("Do you wish to send a delay alert document to each affected exporter?", UnitTestUserNotification.Instance.LastMessage.ToString());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(false, provider.ShouldSendDelayAlerts(false, false));
			AssertEquals(NoText, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		#region TestNoMessageWhenIsUserInteractiveIsFalse

		public void TestNoMessagePopsUpWhenIsUserInteractiveIsFalse()
		{
			AssertNoMessagePopsUpWhenIsUserInteractiveIsFalse("AskShouldUpdateAgencyShipmentDatesFromATD");
			AssertNoMessagePopsUpWhenIsUserInteractiveIsFalse("AskShouldUpdateRelatedShipmentsETD");
			AssertNoMessagePopsUpWhenIsUserInteractiveIsFalse("AskShouldUpdateRelatedShipmentsETA");
		}

		void AssertNoMessagePopsUpWhenIsUserInteractiveIsFalse(string methodName)
		{
			var provider = new ScheduleUpdateGuiQueryProvider();
			var methodInfo = provider.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			AssertNotNull(methodInfo);

			Globals.IsUserInteractive = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var value = methodInfo.Invoke(provider, null);
			AssertEquals(false, value);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			Globals.IsUserInteractive = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			value = methodInfo.Invoke(provider, null);
			AssertEquals(true, value);
			AssertNotNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion
	}
}

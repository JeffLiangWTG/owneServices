using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CusEntryHeaderMessageProcessingExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetSuppresseMessageLogs()
		{
			var declaration = Factory.New<JobDeclaration>();
			(ZString reference, bool redAndBold) = declaration.GetSuppresseMessageLogs();
			AssertEquals(ZString.Empty, reference);
			AssertEquals(true, redAndBold);

			var newEvent = new EventValue(Events.SuppressSendingMessage, eventTime: new ZDateTimeOffset(2022, 04, 25), reference: "TEST");
			declaration.Logs.AddNew(newEvent);
			(reference, redAndBold) = declaration.GetSuppresseMessageLogs();
			AssertEquals("TEST", reference);
			AssertEquals(true, redAndBold);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 04, 26);
			var eventReference = string.Format(AutomaticSTUConditionChecker.NotificationSuppressSTUPendingAndNewDateNotDifferent, declaration.US_PreliminaryStatementPrintDate.Date.ToShortDateString());
			newEvent = new EventValue(Events.SuppressSendingMessage, eventTime: new ZDateTimeOffset(2022, 04, 26), reference: eventReference);
			declaration.Logs.AddNew(newEvent);
			(reference, redAndBold) = declaration.GetSuppresseMessageLogs();
			AssertEquals(eventReference, reference);
			AssertEquals(false, redAndBold);
		}
	}
}

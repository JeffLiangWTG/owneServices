using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class CensusWarningOverrideMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetNotifications()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var notifications = new CensusWarningOverrideMessageManager().GetNotificationsForSendingCWO(entry);
			Assert(notifications.ContainsError(CensusWarningOverrideMessageManager.NotAcceptedYet));

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			notifications = new CensusWarningOverrideMessageManager().GetNotificationsForSendingCWO(entry);
			Assert(!notifications.ContainsError(CensusWarningOverrideMessageManager.NotAcceptedYet));
		}

		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			new CensusWarningOverrideMessageManager().SendMessage(entry);

			AssertEquals(1, entry.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.CensusWarningOverride, entry.Messages[0].EM_MessageSubType);

			AssertEquals(ImportMessageStatusList.Codes.AwaitingCensusWarningOverride, entry.US_CWOStatus);
		}
	}
}

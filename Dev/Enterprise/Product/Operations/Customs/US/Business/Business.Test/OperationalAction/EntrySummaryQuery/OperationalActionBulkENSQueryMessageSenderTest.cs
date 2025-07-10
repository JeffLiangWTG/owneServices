using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class OperationalActionBulkENSQueryMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendWhenDeclarationIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "111";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkENSQueryMessageSender(declaration);
			sender.OperationalActionSendMessage(true, log);

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals("Message should be sent", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
		}

		public void TestNotSendWhenDeclarationIsFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "111";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkENSQueryMessageSender(declaration);
			AssertEquals("Message should not be sent", SaveResult.Fail, sender.OperationalActionSendMessage(true, log));
		}

		public void TestSendWhenDeclarationIsRecon()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;

			var reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EntryFilerCode = "XJ5";
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.EntryNumber = "12345678";

			AssertNotNull(declaration.ReconDeclaration.ReconEntry);
			AssertNotNull(declaration.ReconDeclaration.ReconEntry.GetEntry());

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkENSQueryMessageSender(declaration);
			sender.OperationalActionSendMessage(true, log);

			AssertEquals("Message should be sent", 1, declaration.ReconDeclaration.ReconEntry.Messages.Count);
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class SendEntrySummaryQueryOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestJobsWithConcurrency()
		{
			var declaration1 = GetMergedDutiableDeclaration("XJ6");
			var declaration2 = GetMergedDutiableDeclaration("XJ7");
			var declaration3 = GetNotMergedDutiableDeclaration("XJ8");
			var declaration4 = GetNotMergedDutiableDeclaration("XJ9");

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration3, declaration4 };
			var runner = new SendEntrySummaryQueryOperationalActionRunner(log);

			var targetsWithConcurrency = new BusinessObject[] { declaration2, declaration4 };
			runner.SetUpJobsWithConcurrency(targetsWithConcurrency.Cast<JobDeclaration>().Take(2));
			var jobs = runner.PerformFunctionOperationalAction(true, targets);

			AssertEquals("2 jobs should be successfully sent", 2, jobs.Count());
			var entry = declaration1.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Only one message should exists", 1, entry.Messages.Count);
			AssertMessageType(entry.Messages[0].EM_MessageType);

			entry = declaration2.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Only one message should exists", 1, entry.Messages.Count);
			AssertMessageType(entry.Messages[0].EM_MessageType);

			entry = declaration3.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(null, entry);

			entry = declaration4.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(null, entry);
		}

		public void TestNoMessageSendWhenHasWarningMessage()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDeclaration = new ReconDeclaration(declaration3);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.US_EntryFilerCode = "123";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			Factory.Save();

			AssertNotNull(declaration3.ReconDeclaration);

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2, declaration3, declaration4 };
			var runner = new SendEntrySummaryQueryOperationalActionRunner(log);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(4, log.messages.Count);
			AssertContains("Entry Summary query cannot be sent for this job.", log.messages[0]);
			AssertContains("Entry Summary query cannot be sent. Entry summary entry does not exist for this job.", log.messages[1]);
			AssertContains("Entry Summary query cannot be sent. Recon entry does not exists for this job.", log.messages[2]);
			AssertContains("Entry Summary query cannot be sent. Entry summary entry does not exist for this job.", log.messages[3]);
		}

		JobDeclaration GetNotMergedDutiableDeclaration(ZString entryFilerCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = entryFilerCode;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.SanMarino;
			invoice.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Italy;
			invoice.Charges.AddNew("OFT", 20m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 1000m;
			return declaration;
		}

		public JobDeclaration GetMergedDutiableDeclaration(ZString entryFilerCode)
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = entryFilerCode;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.Charges.AddNew("OFT", 50m, "USD");

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAName = "Blah";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 45m, invoiceLine.CusEntryLine.DutyAmount);
			return declaration;
		}

		void AssertMessageType(ZString msgType)
		{
			AssertEquals("Should be ENS Query message", ACEApplicationIdentifierCodeList.Codes.EntrySummaryQuery, msgType);
		}
	}
}

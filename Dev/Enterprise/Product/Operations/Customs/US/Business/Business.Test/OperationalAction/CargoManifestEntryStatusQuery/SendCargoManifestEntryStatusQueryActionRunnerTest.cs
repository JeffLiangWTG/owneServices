using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class SendCargoManifestEntryStatusQueryActionRunnerTest : TestCaseWithFactory
	{
		public void TestJobsWithConcurrency()
		{
			var declaration1 = GetMergedDutiableDeclarationForACE("XJ6");
			var declaration2 = GetMergedDutiableDeclarationForACE("XJ7");
			var declaration3 = GetNotMergedDutiableDeclaration("XJ8");
			var declaration4 = GetNotMergedDutiableDeclaration("XJ9");

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration3, declaration4 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.Entry,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);

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

		public void TestNoMessageSendWhenIsNotImport()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.InBond,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(1, log.messages.Count);
			AssertContains("Cargo/Manifest/Entry query can only be sent for import declarations.", log.messages[0]);
		}

		public void TestNoMessageSendWhenActionIsMAWB()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = TransportTypeList.Codes.Road;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Air;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.MAWB,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(2, log.messages.Count);
			AssertContains("can only be used when transport mode is Air.", log.messages[0]);
			AssertContains("There is no master bill found.", log.messages[1]);
		}

		public void TestNoMessageSendWhenActionIsHAWB()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = TransportTypeList.Codes.Road;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Air;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.HAWB,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(2, log.messages.Count);
			AssertContains("can only be used when transport mode is Air.", log.messages[0]);
			AssertContains("There is no house bill found.", log.messages[1]);
		}

		public void TestNoMessageSendWhenActionIsEntry()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.Entry,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(2, log.messages.Count);
			AssertContains("Action ‘ENT’ is not available for FTZ job.", log.messages[0]);
			AssertContains("There is no entry found.", log.messages[1]);
		}

		public void TestNoMessageSendWhenActionIsInBond()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.InBond,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(2, log.messages.Count);
			AssertContains("Action ‘INB’ is not available for FTZ job.", log.messages[0]);
			AssertContains("There is no IT numbers found.", log.messages[1]);
		}

		public void TestNoMessageSendWhenActionIsOceanRailTruckBill()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = TransportTypeList.Codes.Air;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Sea;

			Factory.Save();

			var log = new DummyOperationalActionSectionLog();
			var targets = new BusinessObject[] { declaration1, declaration2 };
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(
				log,
				CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill,
				LimitOutputCodeList.Descriptions._0MostRecentResults,
				false,
				false);
			var jobs = runner.PerformFunctionOperationalAction(false, targets);
			AssertEquals(0, jobs.Count());
			AssertEquals(2, log.messages.Count);
			AssertContains("Action ‘ORT’ cannot be used when transport mode is Air.", log.messages[0]);
			AssertContains("There is no bill found.", log.messages[1]);
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

		public JobDeclaration GetMergedDutiableDeclarationForACE(ZString entryFilerCode)
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
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 45m, invoiceLine.CusEntryLine.DutyAmount);
			return declaration;
		}

		void AssertMessageType(ZString msgType)
		{
			AssertEquals("Should be CargoManifestEntry message", ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery, msgType);
		}
	}
}

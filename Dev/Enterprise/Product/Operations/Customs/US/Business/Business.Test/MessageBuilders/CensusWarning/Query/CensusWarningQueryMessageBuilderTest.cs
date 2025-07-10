using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class CensusWarningQueryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEndToEndForEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "70022114";

			var message = new CensusWarningQueryMessageBuilder(entry).PopulateMessage();
			var cj1 = message.MessageBlock.MessageBlocks.OfType<ACWQCJ1>().FirstOrDefault();

			AssertEquals(ZDate.Empty, cj1.RequestedFromDate);
			AssertEquals(ZDate.Empty, cj1.RequestedToDate);
			AssertEquals(ZString.Empty, cj1.DistrictPortOfEntry);
			AssertEquals("70022114", cj1.EntryNumber1);
		}

		public void TestEndToEndForEntryForRLF()
		{
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "XJ5" });
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "3902");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "2704";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "70022114";

			var message = new CensusWarningQueryMessageBuilder(entry).PopulateMessage();
			//should be 3902, not 2704
			AssertContains("B  3902XJ5CJ", message.EM_MessageText);
		}

		public void TestEndToEndForStandAloneQuery()
		{
			var messageData = new CensusWarningQuery(Factory);
			messageData.DateFrom = ZDateTime.Today.AddDays(-1);
			messageData.DateTo = ZDateTime.Today;
			messageData.DistrictPortCode = "3902";
			var message = new CensusWarningQueryMessageBuilder(messageData).PopulateMessage();

			var cj1 = message.MessageBlock.MessageBlocks.OfType<ACWQCJ1>().FirstOrDefault();

			AssertEquals(ZDateTime.Today.AddDays(-1), cj1.RequestedFromDate);
			AssertEquals(ZDateTime.Today, cj1.RequestedToDate);
			AssertEquals(ZString.Empty, cj1.EntryNumber1);
			AssertEquals("3902", cj1.DistrictPortOfEntry);
		}
	}
}

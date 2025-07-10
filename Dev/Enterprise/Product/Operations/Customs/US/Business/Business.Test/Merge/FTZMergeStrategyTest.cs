using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZMergeStrategyTest : TestCaseWithFactory
	{
		public void TestMergeWhenChangingRelatedBillAfterLodged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "M432890";

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "H432890";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "H132890";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";
			invoice1.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice1.JZ_InvoiceDisplaySequence = 1;
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_CU_RelatedHouseBill = houseBill2.PK;
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = "USD";
			invoice3.JZ_CU_RelatedHouseBill = houseBill2.PK;
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var invoiceLine4 = invoice3.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.FTZEntry;
			AssertEquals(4, entry.MergedLines.Count);

			AssertEquals("line number:First houseBill", (short)1, invoiceLine1.CusEntryLine.CL_LineNumber);
			AssertEquals("line number:Second houseBill", (short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("line number:Second houseBill", (short)2, invoiceLine3.CusEntryLine.CL_LineNumber);
			AssertEquals("line number:Second houseBill", (short)3, invoiceLine4.CusEntryLine.CL_LineNumber);
		}

		public void TestChangeJobType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("CH_MessageType", CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, entry.CH_MessageType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("FTZ entry is deleted", true, entry.IsDeleted);

			entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("CH_MessageType", CusEntryHeaderMessageTypeList.Codes.EntrySummary, entry.CH_MessageType);
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZLineNumberAssignerTest : TestCaseWithFactory
	{
		public void TestAssignLineNumberForEachBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.FTZEntry);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine3.CusEntryLine.CL_LineNumber);
		}

		public void TestAssignLineNumberForEachBill_MergeByTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.FTZEntry);

			AssertNotEquals(invoiceLine.CusEntryLine, invoiceLine2.CusEntryLine);
			AssertEquals(invoiceLine2.CusEntryLine, invoiceLine3.CusEntryLine);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, invoiceLine3.CusEntryLine.CL_LineNumber);
		}

		public void TestAssignLineNumberPerBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "M1";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_CU_RelatedHouseBill = bill.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.FTZEntry);

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)3, invoiceLine3.CusEntryLine.CL_LineNumber);

			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_Tariff = "4";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";

			invoice2.JZ_CU_RelatedHouseBill = bill2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals((short)1, invoiceLine.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine1_2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)1, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals((short)2, invoiceLine3.CusEntryLine.CL_LineNumber);
		}

		public void TestAssignLineNumberForSecondaryLineAndSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "6307906800";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_SupTariff = "9802008068";
			invoiceLine2.JI_Tariff = "6307906800";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "4818200020";
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.FTZEntry;

			AssertEquals("MergedLines", 5, entry.MergedLines.Count);

			var supLine1 = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, true);
			var normalLine1 = invoiceLine.CusEntryLine;
			var supLine2 = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone, true);
			var normalLine2 = invoiceLine2.CusEntryLine;
			var normalLine3 = invoiceLine3.CusEntryLine;

			AssertEquals("LineNumber", (short)1, supLine1.CL_LineNumber);
			AssertEquals("LineNumber", (short)1, normalLine1.CL_LineNumber);
			AssertEquals("LineNumber", (short)1, supLine2.CL_LineNumber);
			AssertEquals("LineNumber", (short)1, normalLine2.CL_LineNumber);
			AssertEquals("LineNumber", (short)1, normalLine3.CL_LineNumber);
		}
	}
}

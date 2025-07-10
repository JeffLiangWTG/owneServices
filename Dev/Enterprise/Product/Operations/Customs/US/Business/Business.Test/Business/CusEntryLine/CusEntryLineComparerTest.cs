using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryLineComparerTest : TestCaseWithFactory
	{
		public void TestComparer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 1;
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryline1 = entry.MergedLines.AddNew();
			entryline1.CL_LineNumber = 2;
			entryline1.CL_AdValoremTariff = "2000000000";
			CusEntryLine entryline2 = entry.MergedLines.AddNew();
			entryline2.CL_LineNumber = 3;
			entryline2.CL_AdValoremTariff = "3000000000";
			CusEntryLine entryline3 = entry.MergedLines.AddNew();
			entryline3.CL_LineNumber = 1;
			entryline3.CL_AdValoremTariff = "1000000000";
			CusEntryLine entryline1Child1 = entry.MergedLines.AddNew();
			entryline1Child1.CL_LineNumber = 2;
			entryline1Child1.US_CL_ParentLine = entryline1.PK;
			entryline1Child1.CL_AdValoremTariff = "2020000000";
			invoiceLine2.JI_CL = entryline1Child1.PK;
			CusEntryLine entryline1Child2 = entry.MergedLines.AddNew();
			entryline1Child2.CL_LineNumber = 2;
			entryline1Child2.US_CL_ParentLine = entryline1.PK;
			entryline1Child2.CL_AdValoremTariff = "2010000000";
			invoiceLine1.JI_CL = entryline1Child2.PK;
			entry.MergedLines.Sort<CusEntryLine>(new CusEntryLineComparer());
			AssertEquals(entryline3, entry.MergedLines[0]);
			AssertEquals(entryline1, entry.MergedLines[1]);
			AssertEquals(entryline1Child2, entry.MergedLines[2]);
			AssertEquals(entryline1Child1, entry.MergedLines[3]);
			AssertEquals(entryline2, entry.MergedLines[4]);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone;
			entry.MergedLines.Sort<CusEntryLine>(new CusEntryLineComparer());
			AssertEquals(entryline3, entry.MergedLines[0]);
			AssertEquals(entryline1, entry.MergedLines[1]);
			AssertEquals(entryline2, entry.MergedLines[2]);
			AssertEquals(entryline1Child2, entry.MergedLines[3]);
			AssertEquals(entryline1Child1, entry.MergedLines[4]);
		}
	}
}

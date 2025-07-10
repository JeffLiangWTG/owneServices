using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconEntryLineWrapTest : TestCaseWithFactory
	{
		public void TestIReconOriginalEntryLine()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "XJ3Entry1111";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigEntryLineNo = "1";
			var entryLineWrap = new ReconOrigEntryLineWrap(invoiceLine);
			AssertEquals("XJ3", entryLineWrap.EntryFilerCode);
			AssertEquals("ENTRY1111", entryLineWrap.EntryNumber);
			AssertEquals("1", entryLineWrap.EntryLineNumber);
		}
	}
}

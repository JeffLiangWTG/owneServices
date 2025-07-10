using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCustomsChargeEntryTest : TestCaseWithFactory
	{
		public void TestGetFormattedEntryReferenceForHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_PresentationDate = new ZDateTime(2009, 06, 03);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.ImportEntryNumber = "12345678";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(ensEntry);
			AssertEquals("XJ5-1234567-8", ensEntry.GetFormattedEntryReferenceForHeader());
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PaymentDueDate = ZDateTime.Empty;
			AssertEquals("XJ5-1234567-8 (ACH Pay Type 3)", ensEntry.GetFormattedEntryReferenceForHeader());
			declaration.US_PaymentDueDate = new ZDateTime(2010, 1, 2);
			AssertEquals("XJ5-1234567-8 (ACH Pay Type 3 by 02-Jan-2010)", ensEntry.GetFormattedEntryReferenceForHeader());
		}
	}
}

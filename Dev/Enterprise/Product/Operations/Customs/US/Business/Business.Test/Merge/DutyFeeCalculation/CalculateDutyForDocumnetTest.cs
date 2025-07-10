using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CalculateDutyForDocumnetTest : TestCaseWithFactory
	{
		[TestDate(2019, 02, 25)]
		public void TestCalculateDutyForDocumnet()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice1 = declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.US_SupTariff = "9808003000";
			line1.JI_Tariff = "8457100075";
			line1.JI_LinePrice = 2000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entryHeader.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "8457100075");
			Assert(CalculateDutyForDocument.ShouldCalculateDutyOnlyForDocumnet(new EntryLineIEntryLineOrInvoiceLineDutyData(entryLine)));

			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals(84m, entryPrintLine.DutyAmount);
			AssertEquals("4.2%", entryPrintLine.DutyPercentAsString);
			var ehp = new EntryHeaderENS7501Print<ACEEntryHeaderENS7501Line>(entryHeader, (a, b, c) => new ACEEntryHeaderENS7501Line(a, b, c));
			AssertEquals(84m, ehp.TotalDutyAmt);
			AssertEquals(0m, ehp.Block40Total);
		}
	}
}

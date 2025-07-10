using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ReportingBookAccountingJournalPrintOptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAccReportingBookList()
		{
			var accountingTestObjectCreator = new AccountingTestObjectCreator(Factory);
			var globalChart = accountingTestObjectCreator.CreateAlternateChart("GLC", isGlobal: true);
			var reportingBook = accountingTestObjectCreator.CreateAccReportingBook("GLC", globalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");

			var nonGlobalChart = accountingTestObjectCreator.CreateAlternateChart("NGC", isGlobal: false);
			var nonGlobalReportingBook = accountingTestObjectCreator.CreateAccReportingBook("NGC", nonGlobalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");

			var nonCurrentCompanyChart = accountingTestObjectCreator.CreateAlternateChart("NCC", isGlobal: false);
			nonCurrentCompanyChart.AAC_GC_Company = accountingTestObjectCreator.NonCurrentCompany.PK;
			var nonCurrentCompanyReportingBook = accountingTestObjectCreator.CreateAccReportingBook("NCC", nonCurrentCompanyChart.PK, accountingTestObjectCreator.NonCurrentCompany.PK, "EET", "Description");

			Factory.Save();

			var reportingBookCollectionInDB = new AccReportingBookCollection(Factory, GlbCompany.CurrentCompany.PK);
			reportingBookCollectionInDB.Load();
			var reportingBookCodes = reportingBookCollectionInDB.OfType<AccReportingBook>()
				.Select(reportingBook => reportingBook.ARB_Code)
				.ToArray();

			var reportingBookAccountingJournalPrintOption = new ReportingBookAccountingJournalPrintOption();
			reportingBookAccountingJournalPrintOption.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var reportingBooksInLookups = reportingBookAccountingJournalPrintOption.Lookups.AccReportingBookList;
			reportingBooksInLookups.Load();
			AssertContainsExactElementsInAnyOrder(reportingBookCodes, reportingBooksInLookups.OfType<AccReportingBook>().Select(x => x.ARB_Code).ToArray());

			AssertEquals("GLC", ((AccReportingBook)reportingBooksInLookups.FindByPK(reportingBook.PK)).ARB_Code);
			AssertEquals("NGC", ((AccReportingBook)reportingBooksInLookups.FindByPK(nonGlobalReportingBook.PK)).ARB_Code);
			AssertNull(reportingBooksInLookups.FindByPK(nonCurrentCompanyReportingBook.PK));
		}
	}
}

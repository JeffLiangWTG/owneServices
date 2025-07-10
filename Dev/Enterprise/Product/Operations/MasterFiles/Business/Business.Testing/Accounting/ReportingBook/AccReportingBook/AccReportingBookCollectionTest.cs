using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccReportingBookCollection))]
	sealed class AccReportingBookCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccReportingBookCollection(Factory);
		}

		public void TestCreateRelationshipFilter()
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

			var reportingBookCollection = new AccReportingBookCollection(Factory);
			reportingBookCollection.Load();

			AssertEquals(2, reportingBookCollection.Count);
			Assert(reportingBookCollection.Any(x => x.PK == reportingBook.PK));
			Assert(reportingBookCollection.Any(x => x.PK == nonGlobalReportingBook.PK));

			reportingBookCollection = new AccReportingBookCollection(Factory, accountingTestObjectCreator.NonCurrentCompany.PK);
			reportingBookCollection.Load();

			AssertEquals(2, reportingBookCollection.Count);
			Assert(reportingBookCollection.Any(x => x.PK == reportingBook.PK));
			Assert(reportingBookCollection.Any(x => x.PK == nonCurrentCompanyReportingBook.PK));
		}
	}
}

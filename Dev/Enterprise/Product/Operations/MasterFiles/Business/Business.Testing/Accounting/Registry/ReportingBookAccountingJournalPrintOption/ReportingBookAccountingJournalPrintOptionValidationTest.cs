using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ReportingBookAccountingJournalPrintOptionValidationTest : TestCaseWithFactory
	{
		public void TestValidateReportingBookPK()
		{
			var reportingBookAccountingJournalPrintOption = ReportingBookAccountingJournalPrintOptionCollection.AddNew();

			AssertEquals(ZGuid.Empty, reportingBookAccountingJournalPrintOption.ReportingBook);
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();
			AssertHasError(reportingBookAccountingJournalPrintOption.ReportingBookInfo, "Please enter a value.");

			reportingBookAccountingJournalPrintOption.ReportingBook = ZGuid.BrettsGuid;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();
			AssertHasError(reportingBookAccountingJournalPrintOption.ReportingBookInfo, "Enter a valid selection.");

			reportingBookAccountingJournalPrintOption.ReportingBook = ReportingBook.PK;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();
			AssertNoErrors(reportingBookAccountingJournalPrintOption.ReportingBookInfo);

			var newReportingBookAccountingJournalPrintOption = ReportingBookAccountingJournalPrintOptionCollection.AddNew();
			newReportingBookAccountingJournalPrintOption.ReportingBook = ReportingBook.PK;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();
			AssertHasError(reportingBookAccountingJournalPrintOption.ReportingBookInfo, "At least one record has been set for the same reporting book.");
		}

		public void TestValidateDefault()
		{
			var globalChart = AccountingTestObjectCreator.CreateAlternateChart("GL2", isGlobal: true);
			var reportingBook = AccountingTestObjectCreator.CreateAccReportingBook("GL2", globalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");
			Factory.Save();

			var reportingBookAccountingJournalPrintOption = ReportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = ReportingBook.PK;
			reportingBookAccountingJournalPrintOption.Default = true;
			var newReportingBookAccountingJournalPrintOption = ReportingBookAccountingJournalPrintOptionCollection.AddNew();
			newReportingBookAccountingJournalPrintOption.ReportingBook = reportingBook.PK;
			newReportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();

			AssertHasError(reportingBookAccountingJournalPrintOption.DefaultInfo, "Only one default reporting book can be set.");

			reportingBookAccountingJournalPrintOption.Default = false;
			newReportingBookAccountingJournalPrintOption.Default = false;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();

			AssertHasError(reportingBookAccountingJournalPrintOption.DefaultInfo, "At least one Reporting Book must be marked as 'Default'.");

			reportingBookAccountingJournalPrintOption.Default = true;
			reportingBookAccountingJournalPrintOption.RunPreSaveValidation();

			AssertNoErrors(reportingBookAccountingJournalPrintOption.ReportingBookInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingTestObjectCreator = new AccountingTestObjectCreator(Factory);
			var globalChart = AccountingTestObjectCreator.CreateAlternateChart("GLC", isGlobal: true);
			ReportingBook = AccountingTestObjectCreator.CreateAccReportingBook("GLC", globalChart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");
			Factory.Save();

			ReportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
		}

		AccReportingBook ReportingBook;
		ReportingBookAccountingJournalPrintOptionCollection ReportingBookAccountingJournalPrintOptionCollection;
		AccountingTestObjectCreator AccountingTestObjectCreator;
	}
}

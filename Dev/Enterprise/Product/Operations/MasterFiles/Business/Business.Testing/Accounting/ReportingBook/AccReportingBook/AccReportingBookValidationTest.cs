using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccReportingBookValidationTest : BusinessObjectValidationTestCase
	{
		#region Override

		protected override void SetUp()
		{
			base.SetUp();

			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ARB";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			company.GC_RN_NKCountryCode = "AA";

			Factory.Save();

			CompanyPK = company.PK;
			AlternateChartPK = chart.PK;

			ReportingBook = Factory.New<AccReportingBook>();
		}

		AccReportingBook ReportingBook;

		ZGuid CompanyPK;

		ZGuid AlternateChartPK;

		#endregion

		public void TestCheckARB_Code()
		{
			ReportingBook.ARB_Code = "";
			AssertHasErrors("Check Entered", ReportingBook.ARB_CodeInfo);

			ReportingBook.ARB_Code = "1";
			AssertNoErrors(ReportingBook.ARB_CodeInfo);

			ReportingBook.ARB_Code = "ACB";
			AssertNoErrors(ReportingBook.ARB_CodeInfo);

			ReportingBook.ARB_Code = "ACB6666666";
			AssertNoErrors(ReportingBook.ARB_CodeInfo);

			ReportingBook.ARB_Code = "ACB666*&^";
			AssertHasError(ReportingBook.ARB_CodeInfo, "Invalid reporting book code. A valid code cannot include special characters.");
		}

		public void TestCheckARB_Description()
		{
			ReportingBook.ARB_Description = "";
			AssertHasErrors("Check Entered", ReportingBook.ARB_DescriptionInfo);

			ReportingBook.ARB_Description = "123";
			AssertNoErrors(ReportingBook.ARB_DescriptionInfo);
		}

		public void TestCheckARB_GC_CompanyOfPeriod()
		{
			ReportingBook.ARB_GC_CompanyOfPeriod = ZGuid.Empty;
			AssertNoErrors(ReportingBook.ARB_GC_CompanyOfPeriodInfo);

			ReportingBook.ARB_GC_CompanyOfPeriod = ZGuid.NewZGuid();
			AssertHasErrors("Error If Invalid PK", ReportingBook.ARB_GC_CompanyOfPeriodInfo);

			ReportingBook.ARB_GC_CompanyOfPeriod = CompanyPK;
			AssertNoErrors(ReportingBook.ARB_GC_CompanyOfPeriodInfo);
		}

		public void TestCheckARB_AAC_AlternateChart()
		{
			ReportingBook.ARB_AAC_AlternateChart = ZGuid.Empty;
			AssertHasErrors("Check Entered", ReportingBook.ARB_AAC_AlternateChartInfo);

			ReportingBook.ARB_AAC_AlternateChart = ZGuid.NewZGuid();
			AssertHasErrors("Error If Invalid PK", ReportingBook.ARB_AAC_AlternateChartInfo);

			ReportingBook.ARB_AAC_AlternateChart = AlternateChartPK;
			AssertNoErrors(ReportingBook.ARB_AAC_AlternateChartInfo);
		}

		public void TestCheckARB_IncludePresentationJournals()
		{
			ReportingBook.ARB_IncludePresentationJournals = "";
			AssertNoErrors(ReportingBook.ARB_IncludePresentationJournalsInfo);

			ReportingBook.ARB_IncludePresentationJournals = "123";
			AssertHasErrors("Error If Invalid Code", ReportingBook.ARB_IncludePresentationJournalsInfo);

			ReportingBook.ARB_IncludePresentationJournals = "ELM";
			AssertNoErrors(ReportingBook.ARB_IncludePresentationJournalsInfo);
		}

		public void TestSaveWithConstraint_ARB_RX_NKCurrencyCheck()
		{
			var localCurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Creator.CreateRefCurrency("111", false);
			Creator.CreateRefCurrency("222");

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "code";
			chart.AAC_Description = "description";

			var company = Factory.NewCompany("AT");
			ReportingBook.ARB_Code = "XXA";
			ReportingBook.ARB_GC_CompanyOfPeriod = company.PK;
			ReportingBook.ARB_AAC_AlternateChart = chart.PK;
			ReportingBook.ARB_Description = "Description";
			ReportingBook.ARB_IncludePresentationJournals = "ELM";
			ReportingBook.Validation.ValidateAll();
			Factory.Save();

			var reportingBook2 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXA"));
			AssertNotNull("Reporting Book should be saved", reportingBook2);
			AssertEquals("ARB_RX_NKCurrency allow input empty", "", reportingBook2.ARB_RX_NKCurrency);

			ReportingBook.ARB_RX_NKCurrency = "AR";
			AssertExceptionThrown("Length of ARB_RX_NKCurrency must be 3", typeof(ZSaveException), () => Factory.Save());

			ReportingBook.ARB_RX_NKCurrency = "111";
			AssertHasError(ReportingBook.ARB_RX_NKCurrencyInfo, "This Reporting Currency is inactive - it may not be used.");

			ReportingBook.ARB_Code = "XXB";
			ReportingBook.ARB_RX_NKCurrency = localCurrencyCode;
			Factory.Save();

			reportingBook2 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXB"));
			AssertNotNull("Reporting Book should be saved", reportingBook2);
			AssertEquals("Length of ARB_RX_NKCurrency must be 3", localCurrencyCode, reportingBook2.ARB_RX_NKCurrency);

			ReportingBook.ARB_Code = "XXC";
			ReportingBook.ARB_RX_NKCurrency = "222";
			AssertHasError(ReportingBook.ARB_RX_NKCurrencyInfo, "No currency translation rules have been configured for Alternate Chart \"code - description\", the rules must be configured before running the reports.");

			Creator.CreateAccAlternateChartCurrencyTranslation(chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.AccountType.ProfitAndLossAccount);
			ReportingBook.Validation.ValidateAll();
			AssertNoWarnings(ReportingBook.ARB_RX_NKCurrencyInfo);
			Factory.Save();
			reportingBook2 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXC"));
			AssertNotNull("Reporting Book should be saved", reportingBook2);
			AssertEquals("Length of ARB_RX_NKCurrency must be 3", "222", reportingBook2.ARB_RX_NKCurrency);
		}

		public void TestCheckARB_IsGlobal()
		{
			ReportingBook.ARB_Code = "XXA";
			ReportingBook.ARB_GC_CompanyOfPeriod = CompanyPK;
			ReportingBook.ARB_AAC_AlternateChart = AlternateChartPK;
			ReportingBook.ARB_Description = "Description";
			ReportingBook.ARB_IsGlobal = false;
			Factory.Save();

			ReportingBook.ARB_IsGlobal = true;
			ReportingBook.Validation.ValidateAll();
			AssertNoErrors(ReportingBook.ARB_IsGlobalInfo);
			Factory.Save();

			ReportingBook.ARB_IsGlobal = false;
			ReportingBook.ARB_GC_CompanyOfPeriod = CompanyPK;
			ReportingBook.Validation.ValidateAll();
			AssertNoErrors(ReportingBook.ARB_IsGlobalInfo);
			Factory.Save();

			CreateComplianceReport(ReportingBook.PK, CompanyPK);
			ReportingBook.ARB_IsGlobal = true;
			ReportingBook.Validation.ValidateAll();
			AssertHasError(ReportingBook.ARB_IsGlobalInfo, "Reporting Book has been used in at least one Compliance Report, 'Is Global' cannot be ticked.");
		}

		void CreateComplianceReport(ZGuid reportingBookPK, ZGuid companyPK)
		{
			var sql = @$"
INSERT INTO dbo.AccComplianceReport(ACR_PK,ACR_ReportType,ACR_Periodicity,ACR_DateFrom,ACR_DateTo,ACR_ARB_ReportingBook,ACR_GC_Company,ACR_Status,ACR_SystemCreateTimeUtc,ACR_SystemLastEditTimeUtc,ACR_SystemCreateUser,ACR_SystemLastEditUser)
	VALUES (NEWID(),'TST','PER','2020-05-01','2020-05-31','{reportingBookPK}','{companyPK}','GEN','2020-06-01','2020-06-01','~BP','~BP')";
			using var cmd = Db.Connection.Command(sql);
			cmd.ExecuteNonQuery();
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}

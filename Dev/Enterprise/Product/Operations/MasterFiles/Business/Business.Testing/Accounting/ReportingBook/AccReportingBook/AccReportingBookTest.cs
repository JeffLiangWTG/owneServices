using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccReportingBook))]
	sealed class AccReportingBookTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFieldMaxLength()
		{
			var reportingBook = Factory.New<AccReportingBook>();
			AssertEquals(10, reportingBook.ARB_CodeInfo.MaxLength);
			AssertEquals(255, reportingBook.ARB_DescriptionInfo.MaxLength);
			AssertEquals(3, reportingBook.ARB_RX_NKCurrencyInfo.MaxLength);
			AssertEquals(3, reportingBook.ARB_IncludePresentationJournalsInfo.MaxLength);
		}

		public void TestFieldDefaultValues()
		{
			var reportingBook = Factory.New<AccReportingBook>();
			Assert("ARB_IsActive default ticked", reportingBook.ARB_IsActive);
			Assert("ARB_IsGlobal default unchecked", !reportingBook.ARB_IsGlobal);
		}

		public void TestSave()
		{
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ARB";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;
			Creator.CreateAccAlternateChartCurrencyTranslation(chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.AccountType.ProfitAndLossAccount);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			company.GC_RN_NKCountryCode = "AA";

			var reportingBook = Creator.CreateAccReportingBook("XXA", chart.PK, company.PK, "ELM", "Description", "ARB", Core.Constants.ExchangeRateTypes.Code.BuyRate, isGlobal: false);
			reportingBook.Validation.ValidateAll();

			AssertNoErrors(reportingBook.ARB_CodeInfo);
			AssertNoErrors(reportingBook.ARB_DescriptionInfo);
			AssertNoErrors(reportingBook.ARB_AAC_AlternateChartInfo);
			AssertNoErrors(reportingBook.ARB_RX_NKCurrencyInfo);
			AssertNoErrors(reportingBook.ARB_IncludePresentationJournalsInfo);
			AssertNoErrors(reportingBook.ARB_GC_CompanyOfPeriodInfo);

			AssertNull("There are no records here", new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXA")));

			Factory.Save();

			Assert("Reporting Book should be saved", reportingBook.IsInDatabase);

			var reportingBook2 = new BusinessObjectFactory().LoadTop1<AccReportingBook>(new ZQuery(AccReportingBookSchema.ARB_Code, "XXA"));

			AssertNotNull("Reporting Book should be saved", reportingBook2);
			AssertEquals(reportingBook.ARB_Code, reportingBook2.ARB_Code);
			AssertEquals(ZBool.True, reportingBook2.ARB_IsActive);
			AssertEquals(reportingBook.ARB_Description, reportingBook2.ARB_Description);
			AssertEquals(reportingBook.ARB_AAC_AlternateChart, reportingBook2.ARB_AAC_AlternateChart);
			AssertEquals(reportingBook.ARB_RX_NKCurrency, reportingBook2.ARB_RX_NKCurrency);
			AssertEquals(reportingBook.ARB_IncludePresentationJournals, reportingBook2.ARB_IncludePresentationJournals);
			AssertEquals(reportingBook.ARB_GC_CompanyOfPeriod, reportingBook2.ARB_GC_CompanyOfPeriod);
		}

		public void TestSaveWithGlobalAlternateChart()
		{
			var chart = Creator.CreateAlternateChart("1", isGlobal: true);
			var chart2 = Creator.CreateAlternateChart("2", isGlobal: false);

			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_IsGlobal = true;
			AssertEquals("If Is global = true, then Alternate Chart Company is null", Guid.Empty, reportingBook.AlternateChartCompany);
			reportingBook.ARB_GC_CompanyOfPeriod = GlbCompany.CurrentCompany.PK;

			reportingBook.ARB_AAC_AlternateChart = chart2.PK;
			AssertHasError(reportingBook.ARB_AAC_AlternateChartInfo, "The Reporting Book is Global, please select a Global Alternate Chart.");

			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			AssertNoErrors(reportingBook.ARB_AAC_AlternateChartInfo);

			reportingBook.ARB_IsGlobal = false;
			AssertEquals("If Is global = false, then ARB_GC_CompanyOfPeriod is null", Guid.Empty, reportingBook.ARB_GC_CompanyOfPeriod);
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			AssertHasError(reportingBook.ARB_AAC_AlternateChartInfo, "The Reporting Book is Non-global, please select a Non-global Alternate Chart.");

			reportingBook.ARB_AAC_AlternateChart = chart2.PK;
			AssertNoErrors(reportingBook.ARB_AAC_AlternateChartInfo);

			Factory.Save();

			Assert("Reporting Book should be saved", reportingBook.IsInDatabase);
			reportingBook.ARB_GC_CompanyOfPeriod = GlbCompany.CurrentCompany.PK;
			reportingBook.Validation.ValidateAll();
			AssertNoErrors(reportingBook.ARB_AAC_AlternateChartInfo);
		}

		public void TestSaveWithNonGlobalAlternateChart()
		{
			var chart = Creator.CreateAlternateChart("1", isGlobal: false);
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			reportingBook.Validation.ValidateAll();
			AssertNoErrors(reportingBook.ARB_AAC_AlternateChartInfo);
			Factory.Save();

			Assert("Reporting Book should be saved", reportingBook.IsInDatabase);
			AssertEquals("If Alternate Chart is non-global, then Alternate Chart Company is the current company", GlbCompany.CurrentCompany.PK.ToGuid(), reportingBook.AlternateChartCompany);

			Assert("If Alternate Chart is non-global, the Is Global checkbox must be unchecked", !reportingBook.ARB_IsGlobal);
		}

		public void TestSaveWithLeftBlankReportingPeriod()
		{
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_GC_CompanyOfPeriod = Guid.Empty;
			reportingBook.Validation.ValidateAll();
			AssertNoErrors(reportingBook.ARB_GC_CompanyOfPeriodInfo);
			Factory.Save();

			Assert("Reporting Book should be saved", reportingBook.IsInDatabase);
			AssertEquals("If ARB_GC_CompanyOfPeriod is null, then save null into DB", ZGuid.Empty, reportingBook.ARB_GC_CompanyOfPeriod);
		}

		public void TestSaveWithUniqueCheck()
		{
			var companyPK = GlbCompany.CurrentCompany.PK;

			var list = new CodeDescriptionPairList();
			list.AddPair("EET", "Group 1");
			list.AddPair("IOS", "Category 2");
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.GLPresentationJournalCategoriesList(Guid.Empty))
				.Returns(list);
			mock.Setup(m => m.GLPresentationJournalCategoriesList(companyPK.ToGuid()))
				.Returns(list);

			using (ObjectFactory.Substitute(mock.Object))
			{
				Creator.CreateRefCurrency("111");
				Creator.CreateRefCurrency("222");
				var chart1 = Creator.CreateAlternateChart("ABC", isGlobal: true);
				Creator.CreateAccAlternateChartCurrencyTranslation(chart1, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.AccountType.ProfitAndLossAccount);
				var chart2 = Creator.CreateAlternateChart("BCD", isGlobal: false);
				chart2.AAC_GC_Company = companyPK;
				Creator.CreateAccAlternateChartCurrencyTranslation(chart2, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, Core.Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.AccountType.ProfitAndLossAccount);

				var reportingBook = Creator.CreateAccReportingBook("XXA", chart1.PK, companyPK, "EET", "Description", "111");
				var reportingBook2 = Creator.CreateAccReportingBook("XXB", chart2.PK, Guid.Empty, "EET", "Description", "222", isGlobal: false);

				Factory.Save();

				var reportingBook3 = Creator.CreateAccReportingBook("XXA", chart1.PK, Guid.Empty, "", "Description");
				reportingBook3.Validation.ValidateAll();
				AssertHasErrorContaining(reportingBook3.ARB_CodeInfo, "The code is already used by a Global Reporting Book.");

				reportingBook3.ARB_Code = "XXB";
				reportingBook3.Validation.ValidateAll();
				AssertHasErrorContaining(reportingBook3.ARB_CodeInfo, $"The code is already used by another Reporting Book in {GlbCompany.CurrentCompany.GC_Code} system company.");

				reportingBook3.ARB_Code = "XXC";
				reportingBook3.ARB_Description = "Description";
				reportingBook3.ARB_AAC_AlternateChart = chart1.PK;
				reportingBook3.ARB_RX_NKCurrency = "111";
				reportingBook3.ARB_IncludePresentationJournals = "EET";
				reportingBook3.ARB_IsGlobal = true;
				reportingBook3.ARB_GC_CompanyOfPeriod = companyPK;
				reportingBook3.Validation.ValidateAll();
				Assert(reportingBook3.RowErrors.Contains(
	@"A Global Reporting Book with the same configuration settings already existed.
	Code = XXA
	Description = Description
	Alternate Chart = ABC
	Currency = 111
	Presentation Journals = EET Code
	Include Child Presentation = N
	Reporting Period = EDI
"));
				reportingBook3.ARB_RX_NKCurrency = "";
				reportingBook3.Validation.ValidateAll();
				Assert(!reportingBook3.HasRowErrors);

				var reportingBook4 = Creator.CreateAccReportingBook("XXC", chart2.PK, Guid.Empty, "EET", "Description", "222", "", isGlobal: false);
				reportingBook4.Validation.ValidateAll();
				Assert(reportingBook4.RowErrors.Contains(@"Another reporting book with the same configuration settings already existed in EDI
	Code = XXB
	Description = Description
	Alternate Chart = BCD
	Currency = 222
	Presentation Journals = EET Code
	Include Child Presentation = N
"));
				reportingBook4.ARB_RX_NKCurrency = "";
				reportingBook4.Validation.ValidateAll();
				Assert(!reportingBook4.HasRowErrors);
			}
		}

		public void TestARB_CategorisWithChildren()
		{
			var category = "EET";
			var list = new CodeDescriptionPairList();
			list.AddPair("EET", "Group 1");
			list.AddPair("IOS", "Category 2");
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.GLPresentationJournalCategoriesList(GlbCompany.CurrentCompany.PK.ToGuid()))
				.Returns(list);
			mock.Setup(m => m.GetCategorisWithChildren(category)).Returns("EET,ORC,UUT");
			using (ObjectFactory.Substitute(mock.Object))
			{
				var chart1 = Creator.CreateAlternateChart("ABC", isGlobal: true);
				var reportingBook = Creator.CreateAccReportingBook("XXA", chart1.PK, GlbCompany.CurrentCompany.PK, category, "Description", "111");
				reportingBook.ARB_IncludeChildPresentation = ZBool.True;
				AssertEquals("Has Child", "EET,ORC,UUT", reportingBook.ARB_CategorisWithChildren);

				reportingBook.ARB_IncludeChildPresentation = ZBool.False;
				AssertEquals("No Child", "EET", reportingBook.ARB_CategorisWithChildren);
			}
		}

		public void TestNoStmALogs()
		{
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, reportingBook.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				reportingBook.ARB_Description = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				reportingBook.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestIsLocalCurrency()
		{
			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "USD";
			Factory.Save();

			reportingBook.ARB_RX_NKCurrency = string.Empty;
			Assert(reportingBook.IsLocalCurrency);

			reportingBook.ARB_RX_NKCurrency = "USD";
			Assert(reportingBook.IsLocalCurrency);

			reportingBook.ARB_RX_NKCurrency = "AUD";
			Assert(!reportingBook.IsLocalCurrency);
		}

		public void TestCompanyOfPeriodReadonlyAfterSettingIsGlobal()
		{
			var reportingBook = Factory.New<AccReportingBook>();
			Assert("ARB_GC_CompanyOfPeriod should be read-only when ARB_IsGlobal is false", reportingBook.ARB_GC_CompanyOfPeriodInfo.ReadOnly);

			reportingBook.ARB_IsGlobal = true;
			Assert("ARB_GC_CompanyOfPeriod should be editable when ARB_IsGlobal is true", !reportingBook.ARB_GC_CompanyOfPeriodInfo.ReadOnly);

			reportingBook.ARB_IsGlobal = false;
			Assert("ARB_GC_CompanyOfPeriod should be read-only when ARB_IsGlobal is false", reportingBook.ARB_GC_CompanyOfPeriodInfo.ReadOnly);
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}

using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccReportingBookLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExRateTypes()
		{
			var reportingBook = Factory.New<AccReportingBook>();

			Env.Security.GCBExchangeRateUpdate.IsAllowed = false;

			var exRateTypes = reportingBook.Lookups.ExRateTypes.Cast<CodeDescriptionPair>().Select(p => p.Code).ToList();

			AssertCollectionNotContains(Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl, exRateTypes);

			Env.Security.GCBExchangeRateUpdate.IsAllowed = true;

			// With all custom exchange rates enabled
			var itemsSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			itemsSystemLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, itemsSystemLevel);

			var itemsCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.Value;
			itemsCompanyLevel.Cast<CodeDescriptionBool>()
				.ForEach(x =>
				{
					x.Bool = true;
				});
			AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, itemsCompanyLevel);

			var expectedList = new string[]
			{
				Core.Constants.ExchangeRateTypes.Code.BuyRate,
				Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl
			};

			exRateTypes = reportingBook.Lookups.ExRateTypes.Cast<CodeDescriptionPair>().Select(p => p.Code).ToList();

			foreach (var rateType in expectedList)
			{
				AssertCollectionContains(rateType, exRateTypes);
			}
		}

		public void TestGetPresentationCategoryList()
		{
			var chart1 = Factory.NewWithValidTestData<AccAlternateChart>();
			chart1.AAC_Code = "1";
			chart1.AAC_IsGlobal = true;

			var chart2 = Factory.NewWithValidTestData<AccAlternateChart>();
			chart2.AAC_Code = "2";
			chart2.AAC_IsGlobal = false;

			Factory.Save();

			var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
			reportingBook.ARB_AAC_AlternateChart = chart1.PK;

			var presentationCategoryList = reportingBook.Lookups.PresentationCategoryList.ToArray().Select(p => p.Code).ToList();
			AssertCollectionContains("ELM", presentationCategoryList);

			ObjectFactory.Get<IAccounting>().SetupGLPresentationJournalCategoriesListRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), "GP1", "Group 1");
			reportingBook.ARB_AAC_AlternateChart = chart2.PK;

			presentationCategoryList = reportingBook.Lookups.PresentationCategoryList.ToArray().Select(p => p.Code).ToList();
			AssertCollectionNotContains("ELM", presentationCategoryList);
			AssertCollectionContains("GP1", presentationCategoryList);
		}

		public void TestAlternateCharts()
		{
			var creator = new AccountingTestObjectCreator(Factory);
			creator.CreateAlternateChart("MGT");
			creator.CreateAlternateChart("TRR", isGlobal: false);

			var nonCurrentCompanyChart = creator.CreateAlternateChart("TCC", isGlobal: false);
			nonCurrentCompanyChart.AAC_GC_Company = creator.NonCurrentCompany.PK;

			Factory.Save();

			var charts = Factory.New<AccReportingBook>().Lookups.AlternateCharts;
			charts.Load();

			AssertEquals(2, charts.Count);
			var nonCurrentCompanyChartCount = charts.Where(x => x.AAC_GC_Company == nonCurrentCompanyChart.PK);
			Assert(!nonCurrentCompanyChartCount.Any());
		}
	}
}

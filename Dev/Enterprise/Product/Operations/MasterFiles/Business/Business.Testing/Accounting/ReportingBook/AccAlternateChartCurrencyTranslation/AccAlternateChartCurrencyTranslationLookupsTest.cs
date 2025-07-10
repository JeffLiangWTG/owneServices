using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAlternateChartCurrencyTranslationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAccounTypeList()
		{
			var currencyTranslation = Factory.NewWithValidTestData<AccAlternateChartCurrencyTranslation>();
			var accounTypeList = currencyTranslation.Lookups.AccounTypeList;

			AssertEquals("AccounTypeList.Count", 2, accounTypeList.Count);

			AssertEquals("0th Element (Code)", Core.Constants.AccountType.BalanceSheetAccount, accounTypeList[0].Code);
			AssertEquals("0th Element (Description)", "Balance Sheet", accounTypeList[0].Description);

			AssertEquals("1th Element (Code)", Core.Constants.AccountType.ProfitAndLossAccount, accounTypeList[1].Code);
			AssertEquals("1th Element (Description)", "Profit & Loss", accounTypeList[1].Description);
		}

		public void TestCurrencyTranslationLevelList()
		{
			var currencyTranslation = Factory.NewWithValidTestData<AccAlternateChartCurrencyTranslation>();
			var currencyTranslationLevelList = currencyTranslation.Lookups.CurrencyTranslationLevelList;

			AssertEquals("CurrencyTranslationLevelList.Count", 2, currencyTranslationLevelList.Count);

			AssertEquals("0th Element (Code)", AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, currencyTranslationLevelList[0].Code);
			AssertEquals("0th Element (Description)", "Journal", currencyTranslationLevelList[0].Description);

			AssertEquals("1th Element (Code)", AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.ClosingBalance, currencyTranslationLevelList[1].Code);
			AssertEquals("1th Element (Description)", "Closing Balance", currencyTranslationLevelList[1].Description);
		}

		public void TestExRateTypeList()
		{
			var currencyTranslation = Factory.NewWithValidTestData<AccAlternateChartCurrencyTranslation>();
			var exRateTypeList = currencyTranslation.Lookups.ExRateTypeList;
			var exRateTypesInRegistry = new CodeDescriptionPairList();
			exRateTypesInRegistry.AddRange(AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetActiveCodeDescriptionPairList());
			exRateTypesInRegistry.AddRange(AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
			exRateTypesInRegistry.RemoveCode(ExchangeRateTypes.Code.IATARate);
			exRateTypesInRegistry.RemoveCode(ExchangeRateTypes.Code.CustomsRate);
			exRateTypesInRegistry.RemoveCode(ExchangeRateTypes.Code.CustomsRateSecondary);
			exRateTypesInRegistry.RemoveCode(ExchangeRateTypes.Code.CustomsMeasureEURExRate);

			AssertEquals("ExRateTypeList.Count", exRateTypesInRegistry.Count, exRateTypeList.Count);
			AssertContainsExactElementsInAnyOrder(exRateTypesInRegistry, exRateTypeList);
		}

		public void TestAlternateAccounts()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			var chartCurrencyTranslation = Creator.CreateAccAlternateChartCurrencyTranslation(chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, AccountType.ProfitAndLossAccount);
			var anotherChart = Factory.NewWithValidTestData<AccAlternateChart>();
			Factory.Save();

			var alternateGLAccount = Creator.CreateAccAlternateGlAccount(chart.PK, "111");
			var alternateGLAccountForAnotherChart = Creator.CreateAccAlternateGlAccount(anotherChart.PK, "222");

			Factory.Save();

			var alternateAccounts = chartCurrencyTranslation.Lookups.AlternateAccounts;
			alternateAccounts.Load();
			Assert(alternateAccounts.Any(x => x.PK == alternateGLAccount.PK));
			AssertEquals(false, alternateAccounts.Any(x => x.PK == alternateGLAccountForAnotherChart.PK));
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}

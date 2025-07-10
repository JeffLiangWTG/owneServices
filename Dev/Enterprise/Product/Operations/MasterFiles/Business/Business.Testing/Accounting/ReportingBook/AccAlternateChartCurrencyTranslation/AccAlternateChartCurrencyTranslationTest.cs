using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChartCurrencyTranslation))]
	sealed class AccAlternateChartCurrencyTranslationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestField()
		{
			var chartCurrencyTranslation = Factory.NewWithValidTestData<AccAlternateChartCurrencyTranslation>();
			AssertEquals(3, chartCurrencyTranslation.ART_TypeInfo.MaxLength);
			AssertEquals(3, chartCurrencyTranslation.ART_CurrencyTranslationLevelInfo.MaxLength);
			AssertEquals(3, chartCurrencyTranslation.ART_ExRateTypeInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChartCurrencyTranslation), nameof(AccAlternateChartCurrencyTranslation.ART_Type), false, attr => attr.ListDataSourceMember == "Lookups.AccounTypeList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChartCurrencyTranslation), nameof(AccAlternateChartCurrencyTranslation.ART_CurrencyTranslationLevel), false, attr => attr.ListDataSourceMember == "Lookups.CurrencyTranslationLevelList");
			AssertHasCustomAttribute<ListAttribute>(typeof(AccAlternateChartCurrencyTranslation), nameof(AccAlternateChartCurrencyTranslation.ART_ExRateType), false, attr => attr.ListDataSourceMember == "Lookups.ExRateTypeList");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			return Creator.CreateAccAlternateChartCurrencyTranslation(chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, AccountType.ProfitAndLossAccount);
		}

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}

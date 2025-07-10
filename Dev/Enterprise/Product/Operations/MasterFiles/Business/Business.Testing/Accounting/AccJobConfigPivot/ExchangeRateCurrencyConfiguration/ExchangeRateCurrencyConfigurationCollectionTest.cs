using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExchangeRateCurrencyConfigurationCollection))]
	sealed class ExchangeRateCurrencyConfigurationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionOnlyLoadsItsOwnExchangeRateCurrencyConfiguration()
		{
			var config1 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var config2 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			config1.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;
			config2.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			var currencyConfig1 = Factory.New<ExchangeRateCurrencyConfiguration>();
			var currencyConfig2 = Factory.New<ExchangeRateCurrencyConfiguration>();
			var collection1 = new ExchangeRateCurrencyConfigurationCollection(config1);
			var collection2 = new ExchangeRateCurrencyConfigurationCollection(config2);

			currencyConfig1.JCT_JCF_JobConfig = config1.PK;
			currencyConfig2.JCT_JCF_JobConfig = config2.PK;
			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = CurrencyCodes.UnitedStates;
			currencyConfig1.JCT_ExRateType = ExchangeRateTypes.Code.BuyRate;
			currencyConfig2.JCT_ExRateType = ExchangeRateTypes.Code.SellRate;
			config1.CurrencyConfigurations.Load();
			config2.CurrencyConfigurations.Load();
			Factory.Save();

			collection1.Load();
			collection2.Load();
			AssertContainsExactElementsInAnyOrder(currencyConfig1, collection1);
			AssertContainsExactElementsInAnyOrder(currencyConfig2, collection2);
		}

		public void TestCollectionOnlyLoadsProperTypeOfPivots()
		{
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			Factory.Save();

			var collection = GetCollectionToTest() as ExchangeRateCurrencyConfigurationCollection;
			Parent.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			var currencyConfig1 = collection.AddNew();
			currencyConfig1.JCT_Code = CurrencyCodes.Australia;

			var currencyConfig2 = collection.AddNew();
			currencyConfig2.JCT_Code = CurrencyCodes.NewZealand;

			var currencyConfig3 = TestObjectCreator.CreateExchangeRateCurrencyConfiguration(Parent);
			currencyConfig3.JCT_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			currencyConfig3.JCT_ParentId = chargeCode.PK;
			currencyConfig3.JCT_Code = ZString.Empty;
			currencyConfig3.JCT_ExRateType = ZString.Empty;
			Parent.CurrencyConfigurations.Load();

			using (SuspendTrigger("TG_AccJobConfigPivot_InsertUpdate", AccJobConfigPivotSchema.Constants.TableName))
			{
				Factory.Save();
			}

			AssertEquals("Pre-condition", 3, Factory.Load<ExchangeRateCurrencyConfiguration>(new ZQuery(AccJobConfigPivotSchema.JCT_JCF_JobConfig, Parent.PK)).Length);

			collection.Reload(true);

			AssertEquals("Only load rows with empty JCT_ParentTableCode", 2, collection.Count);
			var currencyConfigList = collection.Cast<ExchangeRateCurrencyConfiguration>();
			AssertNotNull(currencyConfigList.Single(x => x.PK == currencyConfig1.PK));
			AssertNotNull(currencyConfigList.Single(x => x.PK == currencyConfig2.PK));
		}

		public void TestSetDefaultForChild_ShouldHaveBUYExchangeRateType()
		{
			var collection = GetCollectionToTest() as ExchangeRateCurrencyConfigurationCollection;
			var currencyConfig = collection.AddNew();
			AssertEquals("Should have buy exchange rate type for new child", "BUY", currencyConfig.JCT_ExRateType);
		}

		public void TestGetRecordByCurrency()
		{
			var collection = GetCollectionToTest() as ExchangeRateCurrencyConfigurationCollection;
			var currencyConfig = collection.AddNew();

			AssertEquals(currencyConfig.PK, collection.GetRecord(ZString.Empty, ZDate.Empty).PK);
			AssertNull(collection.GetRecord(CurrencyCodes.Australia, ZDate.Empty));
		}

		public void TestGetRecordByCurrencyAndDate_FallBackToConfigForAllDateRange()
		{
			var collection = GetCollectionToTest() as ExchangeRateCurrencyConfigurationCollection;

			var currencyConfig1 = collection.AddNew();
			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig1.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig1.JCT_ExpiryDate = new ZDate(2020, 1, 15);

			var currencyConfig2 = collection.AddNew();
			currencyConfig2.JCT_Code = CurrencyCodes.Australia;

			AssertEquals("Found currency config within config's date range", currencyConfig1.PK, collection.GetRecord(CurrencyCodes.Australia, new ZDate(2020, 1, 10)).PK);
			AssertEquals("When no currency config within config's date range, fallback to empty date range curreny config", currencyConfig2.PK, collection.GetRecord(CurrencyCodes.Australia, new ZDate(2020, 2, 2)).PK);
		}

		public void TestGetRecordByCurrencyAndDate_WhenNoConfigForAllDateRange()
		{
			var collection = GetCollectionToTest() as ExchangeRateCurrencyConfigurationCollection;
			var currencyConfig1 = collection.AddNew();
			currencyConfig1.JCT_Code = CurrencyCodes.Australia;
			currencyConfig1.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig1.JCT_ExpiryDate = new ZDate(2020, 1, 15);

			AssertEquals("Found currency config with valid date range", currencyConfig1.PK, collection.GetRecord(CurrencyCodes.Australia, new ZDate(2020, 1, 10)).PK);
			AssertEquals("When no fallback setup, return null", null, collection.GetRecord(CurrencyCodes.Australia, new ZDate(2020, 2, 2)));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Parent = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			return new ExchangeRateCurrencyConfigurationCollection(Parent);
		}

		AccExchangeRateConfiguration Parent;

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		IDisposable SuspendTrigger(string triggerName, string tableName)
		{
			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => Db.Connection.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}"));
		}
	}
}

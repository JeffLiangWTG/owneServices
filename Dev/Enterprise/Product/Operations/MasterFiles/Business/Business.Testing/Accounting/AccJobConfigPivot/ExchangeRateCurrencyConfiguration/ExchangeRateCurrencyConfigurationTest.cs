using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.Testing.OrgCompanyDataTest;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExchangeRateCurrencyConfiguration))]
	sealed class ExchangeRateCurrencyConfigurationTest : AccJobConfigPivotTest
	{
		public void TestJobConfiguration()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var currencyConfig = CreateExchangeRateCurrencyConfiguration();
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedPivot = reloadFactory.Load<ExchangeRateCurrencyConfiguration>(currencyConfig.PK);
			AssertNotNull("The pivot links to an AccExRateConfiguration", reloadedPivot.AccExRateConfiguration);
			AssertEquals("The pivot links to the correct AccExRateConfiguration", currencyConfig.JCT_JCF_JobConfig, reloadedPivot.AccExRateConfiguration.PK);
		}

		public void TestReadOnly()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var currencyConfig = CreateExchangeRateCurrencyConfiguration();
			Factory.Save();

			var parent = currencyConfig.AccExRateConfiguration;
			AssertEquals("Should be consistent by default", parent.ReadOnly, currencyConfig.ReadOnly);

			parent.ReadOnly = false;
			AssertEquals("Should be consistent", parent.ReadOnly, currencyConfig.ReadOnly);

			parent.ReadOnly = true;
			AssertEquals("Should be consistent", parent.ReadOnly, currencyConfig.ReadOnly);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			base.TestSaveAndDeleteBusinessObject();
		}

		public void TestUniqueIndexFailureHandlerWhenHasDuplicateCurrencyCode()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var currencyConfig1 = CreateExchangeRateCurrencyConfiguration();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var testObjectCreator2 = new AccountingTestObjectCreator(newFactory);
			var accExRateConfig2 = newFactory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig2 = testObjectCreator2.CreateExchangeRateCurrencyConfiguration(accExRateConfig2);

			newFactory.Save();

			AssertEquals("Precondition", true, Globals.IsUserInteractive);
			AssertEquals(currencyConfig1.JCT_Code, currencyConfig2.JCT_Code);

			var notification = new NotificationHandlerForTest();
			var handler = ((IBusinessObjectInternals)currencyConfig1).UniqueIndexFailureHandlers.Single(x => x.HandledUniqueIndexNames.Single() == "NR_UC__vw_ExchangeRateCurrencyConfiguration");

			AssertExceptionThrown("Should have ZSaveException due to duplicate currency code", typeof(ZSaveException), () => Factory.Save());

			handler.NotifyUserAndAttemptToResolve(notification, handler.HandledUniqueIndexNames.Single());
			AssertContains(@"The same Currency Code already exists in the Job Billing Exchange Rate configuration.
Please check values for each parameter, make sure the currency lists don't overlap when you save multiple Job Exchange Rate Configurations with the same attributes (job type, transport mode, ledger, direction, and currency type).", notification.Message);
			AssertEquals(1, notification.ReportErrorCount);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateExchangeRateCurrencyConfiguration();
		}

		ExchangeRateCurrencyConfiguration CreateExchangeRateCurrencyConfiguration()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			return TestObjectCreator.CreateExchangeRateCurrencyConfiguration(accExRateConfig);
		}

		public override void TestBizObjectFields()
		{
			base.TestBizObjectFieldsCore(CreateExchangeRateCurrencyConfiguration());
		}

		public void TestExchangeRateType()
		{
			var exRateConfig = (ExchangeRateCurrencyConfiguration)GetNewBusinessObject();

			var expectedValues = new Dictionary<string, ExchangeRateType>
			{
				[Constants.ExchangeRateTypes.Code.BuyRate] = ExchangeRateType.Buy,
				[Constants.ExchangeRateTypes.Code.SellRate] = ExchangeRateType.Sell,
				[Constants.ExchangeRateTypes.Code.CustomsRate] = ExchangeRateType.Customs,
				[Constants.ExchangeRateTypes.Code.PeriodEndRate] = ExchangeRateType.PeriodEnd,
				[Constants.ExchangeRateTypes.Code.IATARate] = ExchangeRateType.IATA,
				[Constants.ExchangeRateTypes.Code.C01Rate] = ExchangeRateType.C01,
				[Constants.ExchangeRateTypes.Code.C02Rate] = ExchangeRateType.C02,
				[Constants.ExchangeRateTypes.Code.C03Rate] = ExchangeRateType.C03,
				[Constants.ExchangeRateTypes.Code.C04Rate] = ExchangeRateType.C04,
				[Constants.ExchangeRateTypes.Code.C05Rate] = ExchangeRateType.C05,
				[Constants.ExchangeRateTypes.Code.C06Rate] = ExchangeRateType.C06,
				[Constants.ExchangeRateTypes.Code.C07Rate] = ExchangeRateType.C07,
				[Constants.ExchangeRateTypes.Code.C08Rate] = ExchangeRateType.C08,
				[Constants.ExchangeRateTypes.Code.C09Rate] = ExchangeRateType.C09,
				[Constants.ExchangeRateTypes.Code.C10Rate] = ExchangeRateType.C10,
				[Constants.ExchangeRateTypes.Code.C11Rate] = ExchangeRateType.C11,
				[Constants.ExchangeRateTypes.Code.C12Rate] = ExchangeRateType.C12,
				[Constants.ExchangeRateTypes.Code.C13Rate] = ExchangeRateType.C13,
				[Constants.ExchangeRateTypes.Code.C14Rate] = ExchangeRateType.C14,
				[Constants.ExchangeRateTypes.Code.C15Rate] = ExchangeRateType.C15,
				[Constants.ExchangeRateTypes.Code.C16Rate] = ExchangeRateType.C16,
				[Constants.ExchangeRateTypes.Code.C17Rate] = ExchangeRateType.C17,
				[Constants.ExchangeRateTypes.Code.C18Rate] = ExchangeRateType.C18,
				[Constants.ExchangeRateTypes.Code.C19Rate] = ExchangeRateType.C19,
				[Constants.ExchangeRateTypes.Code.C20Rate] = ExchangeRateType.C20,
				[Constants.ExchangeRateTypes.Code.C21Rate] = ExchangeRateType.C21,
				[Constants.ExchangeRateTypes.Code.C22Rate] = ExchangeRateType.C22,
				[Constants.ExchangeRateTypes.Code.C23Rate] = ExchangeRateType.C23,
				[Constants.ExchangeRateTypes.Code.C24Rate] = ExchangeRateType.C24,
				[Constants.ExchangeRateTypes.Code.C25Rate] = ExchangeRateType.C25,
				[Constants.ExchangeRateTypes.Code.C26Rate] = ExchangeRateType.C26,
				[Constants.ExchangeRateTypes.Code.C27Rate] = ExchangeRateType.C27,
				[Constants.ExchangeRateTypes.Code.C28Rate] = ExchangeRateType.C28,
				[Constants.ExchangeRateTypes.Code.C29Rate] = ExchangeRateType.C29,
				[Constants.ExchangeRateTypes.Code.C30Rate] = ExchangeRateType.C30,
				[Constants.ExchangeRateTypes.Code.C31Rate] = ExchangeRateType.C31,
				[Constants.ExchangeRateTypes.Code.C32Rate] = ExchangeRateType.C32,
				[Constants.ExchangeRateTypes.Code.C33Rate] = ExchangeRateType.C33,
				[Constants.ExchangeRateTypes.Code.C34Rate] = ExchangeRateType.C34,
				[Constants.ExchangeRateTypes.Code.C35Rate] = ExchangeRateType.C35,
				[Constants.ExchangeRateTypes.Code.C36Rate] = ExchangeRateType.C36,
				[Constants.ExchangeRateTypes.Code.C37Rate] = ExchangeRateType.C37,
				[Constants.ExchangeRateTypes.Code.C38Rate] = ExchangeRateType.C38,
				[Constants.ExchangeRateTypes.Code.C39Rate] = ExchangeRateType.C39,
				[Constants.ExchangeRateTypes.Code.C40Rate] = ExchangeRateType.C40,
				[Constants.ExchangeRateTypes.Code.C41Rate] = ExchangeRateType.C41,
				[Constants.ExchangeRateTypes.Code.C42Rate] = ExchangeRateType.C42,
				[Constants.ExchangeRateTypes.Code.C43Rate] = ExchangeRateType.C43,
				[Constants.ExchangeRateTypes.Code.C44Rate] = ExchangeRateType.C44,
				[Constants.ExchangeRateTypes.Code.C45Rate] = ExchangeRateType.C45,
				[Constants.ExchangeRateTypes.Code.C46Rate] = ExchangeRateType.C46,
				[Constants.ExchangeRateTypes.Code.C47Rate] = ExchangeRateType.C47,
				[Constants.ExchangeRateTypes.Code.C48Rate] = ExchangeRateType.C48,
				[Constants.ExchangeRateTypes.Code.C49Rate] = ExchangeRateType.C49,
				[Constants.ExchangeRateTypes.Code.C50Rate] = ExchangeRateType.C50,
				[Constants.ExchangeRateTypes.Code.C51Rate] = ExchangeRateType.C51,
				[Constants.ExchangeRateTypes.Code.C52Rate] = ExchangeRateType.C52,
				[Constants.ExchangeRateTypes.Code.C53Rate] = ExchangeRateType.C53,
				[Constants.ExchangeRateTypes.Code.C54Rate] = ExchangeRateType.C54,
				[Constants.ExchangeRateTypes.Code.C55Rate] = ExchangeRateType.C55,
				[Constants.ExchangeRateTypes.Code.C56Rate] = ExchangeRateType.C56,
				[Constants.ExchangeRateTypes.Code.C57Rate] = ExchangeRateType.C57,
				[Constants.ExchangeRateTypes.Code.C58Rate] = ExchangeRateType.C58,
				[Constants.ExchangeRateTypes.Code.C59Rate] = ExchangeRateType.C59,
				[Constants.ExchangeRateTypes.Code.C60Rate] = ExchangeRateType.C60,
				[Constants.ExchangeRateTypes.Code.C61Rate] = ExchangeRateType.C61,
				[Constants.ExchangeRateTypes.Code.C62Rate] = ExchangeRateType.C62,
				[Constants.ExchangeRateTypes.Code.C63Rate] = ExchangeRateType.C63,
				[Constants.ExchangeRateTypes.Code.C64Rate] = ExchangeRateType.C64,
				[Constants.ExchangeRateTypes.Code.C65Rate] = ExchangeRateType.C65,
				[Constants.ExchangeRateTypes.Code.C66Rate] = ExchangeRateType.C66,
				[Constants.ExchangeRateTypes.Code.C67Rate] = ExchangeRateType.C67,
				[Constants.ExchangeRateTypes.Code.C68Rate] = ExchangeRateType.C68,
				[Constants.ExchangeRateTypes.Code.C69Rate] = ExchangeRateType.C69,
				[Constants.ExchangeRateTypes.Code.C70Rate] = ExchangeRateType.C70,
				[Constants.ExchangeRateTypes.Code.C71Rate] = ExchangeRateType.C71,
				[Constants.ExchangeRateTypes.Code.C72Rate] = ExchangeRateType.C72,
				[Constants.ExchangeRateTypes.Code.C73Rate] = ExchangeRateType.C73,
				[Constants.ExchangeRateTypes.Code.C74Rate] = ExchangeRateType.C74,
				[Constants.ExchangeRateTypes.Code.C75Rate] = ExchangeRateType.C75,
				[Constants.ExchangeRateTypes.Code.C76Rate] = ExchangeRateType.C76,
				[Constants.ExchangeRateTypes.Code.C77Rate] = ExchangeRateType.C77,
				[Constants.ExchangeRateTypes.Code.C78Rate] = ExchangeRateType.C78,
				[Constants.ExchangeRateTypes.Code.C79Rate] = ExchangeRateType.C79,
				[Constants.ExchangeRateTypes.Code.C80Rate] = ExchangeRateType.C80,
				[Constants.ExchangeRateTypes.Code.C81Rate] = ExchangeRateType.C81,
				[Constants.ExchangeRateTypes.Code.C82Rate] = ExchangeRateType.C82,
				[Constants.ExchangeRateTypes.Code.C83Rate] = ExchangeRateType.C83,
				[Constants.ExchangeRateTypes.Code.C84Rate] = ExchangeRateType.C84,
				[Constants.ExchangeRateTypes.Code.C85Rate] = ExchangeRateType.C85,
				[Constants.ExchangeRateTypes.Code.C86Rate] = ExchangeRateType.C86,
				[Constants.ExchangeRateTypes.Code.C87Rate] = ExchangeRateType.C87,
				[Constants.ExchangeRateTypes.Code.C88Rate] = ExchangeRateType.C88,
				[Constants.ExchangeRateTypes.Code.C89Rate] = ExchangeRateType.C89,
				[Constants.ExchangeRateTypes.Code.C90Rate] = ExchangeRateType.C90,
				[Constants.ExchangeRateTypes.Code.C91Rate] = ExchangeRateType.C91,
				[Constants.ExchangeRateTypes.Code.C92Rate] = ExchangeRateType.C92,
				[Constants.ExchangeRateTypes.Code.C93Rate] = ExchangeRateType.C93,
				[Constants.ExchangeRateTypes.Code.C94Rate] = ExchangeRateType.C94,
				[Constants.ExchangeRateTypes.Code.C95Rate] = ExchangeRateType.C95,
				[Constants.ExchangeRateTypes.Code.C96Rate] = ExchangeRateType.C96,
				[Constants.ExchangeRateTypes.Code.C97Rate] = ExchangeRateType.C97,
				[Constants.ExchangeRateTypes.Code.C98Rate] = ExchangeRateType.C98,
				[Constants.ExchangeRateTypes.Code.C99Rate] = ExchangeRateType.C99,
			};

			foreach (var kvp in expectedValues)
			{
				exRateConfig.JCT_ExRateType = kvp.Key;
				AssertEquals(kvp.Value, exRateConfig.ExchangeRateType);
			}
		}

		public void TestJCT_Code_IsReadOnlyWhenCurrencyTypeIsALL()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();

			accExRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL;

			var currencyConfig = accExRateConfig.CurrencyConfigurations.Cast<ExchangeRateCurrencyConfiguration>().First();

			AssertEquals("JCT_Code is read only when Currency Type is ALL", currencyConfig.JCT_CodeInfo.ReadOnly, true);

			accExRateConfig.JCE_Calc_CurrencyType = AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR;

			currencyConfig = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig.JCT_Code = Constants.CurrencyCodes.Australia;

			AssertEquals("JCT_Code is editable when Currency Type is CUR", currencyConfig.JCT_CodeInfo.ReadOnly, false);
		}

		public void TestReadonly_StartDateExpiryDate_ForSystemDefaultCurrencyConfig()
		{
			var sysCollection = new AccExchangeRateConfigurationCollection(Factory);

			sysCollection.Load();

			var rateConsumer = new Mock<IAccExchangeRateConfigurationRateConsumer>().Object;

			var sysExRateConfig = Factory.Load<AccExchangeRateConfiguration>(sysCollection.GetRecord(ZString.Empty, "ALL", "ALL", "ALL", ZString.Empty, ZString.Empty, rateConsumer).PK);

			var sysCurrencyConfig = sysExRateConfig.CurrencyConfigurations.GetRecord(ZString.Empty, ZDate.Empty);

			AssertEquals("System currency expiry date should be read-only.", true, sysCurrencyConfig.JCT_ExpiryDateInfo.ReadOnly);
			AssertEquals("System currency start date should be read-only.", true, sysCurrencyConfig.JCT_StartDateInfo.ReadOnly);
			AssertEquals("System currency cannot be deleted.", false, sysCurrencyConfig.CanDelete);
			AssertEquals("A currency configuration for ALL job types is mandatory and cannot be deleted.", sysCurrencyConfig.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		public void TestCheckDuplicate_WithoutDateRange_SameExRateConfig()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = Constants.CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = Constants.CurrencyCodes.Australia;

			AssertEquals("In same ex rate config, same currency code should be duplicate.", true, currencyConfig1.IsDuplicateOf(currencyConfig2));

			currencyConfig2.JCT_Code = Constants.CurrencyCodes.UnitedStates;
			AssertEquals("In same ex rate config, different currency code is not duplicate.", false, currencyConfig1.IsDuplicateOf(currencyConfig2));
		}

		public void TestCheckDuplicate_WithDateRange_SameExRateConfig()
		{
			var accExRateConfig = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = Constants.CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = Constants.CurrencyCodes.Australia;
			currencyConfig1.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig1.JCT_ExpiryDate = new ZDate(2020, 12, 31);
			currencyConfig2.JCT_StartDate = new ZDate(2020, 1, 1);
			currencyConfig2.JCT_ExpiryDate = new ZDate(2020, 12, 31);
			AssertEquals("In same ex rate config, date range need to be the same to be duplicate.", true, currencyConfig1.IsDuplicateOf(currencyConfig2));

			currencyConfig2.JCT_StartDate = new ZDate(2020, 2, 2);
			currencyConfig2.JCT_ExpiryDate = new ZDate(2020, 12, 30);
			AssertEquals("In same ex rate config, same currency code different date range will not be duplicate.", false, currencyConfig1.IsDuplicateOf(currencyConfig2));
		}

		public void TestCheckDuplicate_DifferentExRateConfig_RegardlessOfDateRange()
		{
			var accExRateConfig1 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var accExRateConfig2 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var currencyConfig1 = accExRateConfig1.CurrencyConfigurations.AddNew();
			var currencyConfig2 = accExRateConfig2.CurrencyConfigurations.AddNew();

			currencyConfig1.JCT_Code = Constants.CurrencyCodes.Australia;
			currencyConfig2.JCT_Code = Constants.CurrencyCodes.Australia;
			AssertEquals("In different ex rate config, as long as same currency is duplicate.", true, currencyConfig1.IsDuplicateOf(currencyConfig2));

			currencyConfig1.JCT_StartDate = new ZDate(2020, 1, 10);
			currencyConfig1.JCT_ExpiryDate = new ZDate(2020, 1, 15);
			currencyConfig2.JCT_StartDate = new ZDate(2020, 2, 1);
			currencyConfig2.JCT_ExpiryDate = new ZDate(2020, 2, 15);
			AssertEquals("In different ex rate config, as long as same currency is duplicate regardless of date range different", true, currencyConfig1.IsDuplicateOf(currencyConfig2));

			currencyConfig2.JCT_Code = Constants.CurrencyCodes.UnitedStates;
			AssertEquals("In different ex rate config, only different currency is not duplicate", false, currencyConfig1.IsDuplicateOf(currencyConfig2));
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}

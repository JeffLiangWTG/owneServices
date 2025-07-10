using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccAlternateChartCurrencyTranslationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckART_Type()
		{
			ChartCurrencyTranslation.ART_Type = string.Empty;
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertHasError(ChartCurrencyTranslation.ART_TypeInfo, "Please enter a value.");

			ChartCurrencyTranslation.ART_Type = "XXX";
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertHasError(ChartCurrencyTranslation.ART_TypeInfo, "Enter a valid selection.");

			ChartCurrencyTranslation.ART_Type = AccountType.ProfitAndLossAccount;
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertHasError(ChartCurrencyTranslation.ART_TypeInfo, "There must be one row each for P&L and BSH Account Type.");

			Creator.CreateAccAlternateChartCurrencyTranslation(Chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, AccountType.BalanceSheetAccount);
			var anotherPAndLChartCurrencyTranslation = Creator.CreateAccAlternateChartCurrencyTranslation(Chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, AccountType.ProfitAndLossAccount);
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertHasError(ChartCurrencyTranslation.ART_TypeInfo, "There must be one row each for P&L and BSH Account Type.");

			anotherPAndLChartCurrencyTranslation.Delete();
			ChartCurrencyTranslation.ART_AGA_AlternateAccount = AlternateGLAccount.PK;
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertHasError(ChartCurrencyTranslation.ART_TypeInfo, "Either 'Account Type' (i.e. P&L or BSH) or a specific 'Alternate Account' can be selected.");

			ChartCurrencyTranslation.ART_AGA_AlternateAccount = Guid.Empty;
			ChartCurrencyTranslation.Validation.ValidateART_Type();
			AssertNoErrors(ChartCurrencyTranslation.ART_TypeInfo);
		}

		public void TestCheckART_AGA_AlternateAccount()
		{
			ChartCurrencyTranslation.ART_Type = string.Empty;
			ChartCurrencyTranslation.ART_AGA_AlternateAccount = Guid.Empty;
			ChartCurrencyTranslation.Validation.ValidateART_AGA_AlternateAccount();
			AssertHasError(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo, "Please enter a value.");

			ChartCurrencyTranslation.ART_AGA_AlternateAccount = Chart.PK;
			ChartCurrencyTranslation.Validation.ValidateART_AGA_AlternateAccount();
			AssertHasError(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo, "Enter a valid selection.");

			ChartCurrencyTranslation.ART_Type = AccountType.ProfitAndLossAccount;
			ChartCurrencyTranslation.ART_AGA_AlternateAccount = AlternateGLAccount.PK;
			ChartCurrencyTranslation.Validation.ValidateART_AGA_AlternateAccount();
			AssertNotNullOrEmpty(ChartCurrencyTranslation.ART_Type);
			AssertHasError(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo, "Either 'Account Type' (i.e. P&L or BSH) or a specific 'Alternate Account' can be selected.");

			ChartCurrencyTranslation.ART_Type = string.Empty;
			var anotherChartCurrencyTranslationWithAlternateGLAccount = Creator.CreateAccAlternateChartCurrencyTranslation(Chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, alternateGLAccountPK: AlternateGLAccount.PK.ToGuid());
			ChartCurrencyTranslation.Validation.ValidateART_AGA_AlternateAccount();
			AssertHasError(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo, "None, one or more rows can be added for specify Alternate Account. No duplication allowed.");

			anotherChartCurrencyTranslationWithAlternateGLAccount.Delete();
			ChartCurrencyTranslation.Validation.ValidateART_AGA_AlternateAccount();
			AssertNoErrors(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo);
		}

		public void TestCheckART_AGA_AlternateAccountWithSuitableAccountType()
		{
			ChartCurrencyTranslation.ART_Type = string.Empty;
			var alternateGLAccountTTL = Creator.CreateAccAlternateGlAccount(Chart.PK, "112", accountType: Core.Constants.AccountType.Total);
			ChartCurrencyTranslation.ART_AGA_AlternateAccount = alternateGLAccountTTL.PK;
			AssertHasError(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo, "Only Alternate Account with BSH or P&L Account Type can be set in Currency Translation.");

			alternateGLAccountTTL.Delete();
			ChartCurrencyTranslation.ART_AGA_AlternateAccount = AlternateGLAccount.PK;
			AssertNoErrors(ChartCurrencyTranslation.ART_AGA_AlternateAccountInfo);
		}

		public void TestCheckART_CurrencyTranslationLevel()
		{
			ChartCurrencyTranslation.ART_CurrencyTranslationLevel = string.Empty;
			ChartCurrencyTranslation.Validation.ValidateART_CurrencyTranslationLevel();
			AssertHasError(ChartCurrencyTranslation.ART_CurrencyTranslationLevelInfo, "Please enter a value.");

			ChartCurrencyTranslation.ART_CurrencyTranslationLevel = "XXX";
			ChartCurrencyTranslation.Validation.ValidateART_CurrencyTranslationLevel();
			AssertHasError(ChartCurrencyTranslation.ART_CurrencyTranslationLevelInfo, "Enter a valid selection.");

			ChartCurrencyTranslation.ART_CurrencyTranslationLevel = AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal;
			ChartCurrencyTranslation.Validation.ValidateART_CurrencyTranslationLevel();
			AssertNoErrors(ChartCurrencyTranslation.ART_CurrencyTranslationLevelInfo);
		}

		public void TestCheckART_ExRateType()
		{
			ChartCurrencyTranslation.ART_ExRateType = string.Empty;
			ChartCurrencyTranslation.Validation.ValidateART_ExRateType();
			AssertHasError(ChartCurrencyTranslation.ART_ExRateTypeInfo, "Please enter a value.");

			ChartCurrencyTranslation.ART_ExRateType = "XXX";
			ChartCurrencyTranslation.Validation.ValidateART_ExRateType();
			AssertHasError(ChartCurrencyTranslation.ART_ExRateTypeInfo, "Enter a valid selection.");

			ChartCurrencyTranslation.ART_ExRateType = ExchangeRateTypes.Code.BuyRate;
			ChartCurrencyTranslation.Validation.ValidateART_ExRateType();
			AssertNoErrors(ChartCurrencyTranslation.ART_ExRateTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Chart = Factory.NewWithValidTestData<AccAlternateChart>();
			ChartCurrencyTranslation = Creator.CreateAccAlternateChartCurrencyTranslation(Chart, AccountingMasterFilesConstants.CurrencyTranslationLevelCodes.Journal, ExchangeRateTypes.Code.BuyRate, AccountType.ProfitAndLossAccount);
			AlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "111");
		}

		AccAlternateChart Chart;
		AccAlternateChartCurrencyTranslation ChartCurrencyTranslation;
		AccAlternateGLAccount AlternateGLAccount;

		AccountingTestObjectCreator Creator => creator ?? (creator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator creator;
	}
}

using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccountFeeSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAAF_FeeAmountValidation()
		{
			var accFee = new AccountFeeSettings(GlbCompany.CurrentCompany.PK, GlbCompany.CurrentCompany);
			accFee.OverrideSettings = true;
			accFee.AAF_FeeAmount = -250m;
			AssertHasError("Validation Error Expected", accFee.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");

			accFee.AAF_FeeAmount = 150m;
			AssertNoError("No Validation Error Expected", accFee.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");

			accFee.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee;
			accFee.AAF_FeeAmount = 0m;
			AssertNoError("No Validation Error Expected", accFee.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");

			accFee.AAF_FeeAmount = 250m;
			AssertHasError("Validation Error Expected", accFee.AAF_FeeAmountInfo, "As no Account Fee is charged, amount should be zero");
		}

		public void TestAAF_RX_NKFeeCurrencyValidation()
		{
			var accFee = new AccountFeeSettings(GlbCompany.CurrentCompany.PK, GlbCompany.CurrentCompany);
			accFee.OverrideSettings = true;
			accFee.AAF_RX_NKFeeCurrency = "ZZZ";
			AssertHasError("Validation Error Expected", accFee.AAF_RX_NKFeeCurrencyInfo, "Enter a valid selection.");

			accFee.AAF_RX_NKFeeCurrency = "AUD";
			AssertNoError("No Validation Error Expected", accFee.AAF_RX_NKFeeCurrencyInfo, "Enter a valid selection.");
		}

		public void TestAAF_RuleValidation()
		{
			var accFee = new AccountFeeSettings(GlbCompany.CurrentCompany.PK, GlbCompany.CurrentCompany);
			accFee.OverrideSettings = true;
			accFee.AAF_Rule = "ZZZ";
			AssertHasError("Validation Error Expected", accFee.AAF_RuleInfo, "Enter a valid selection.");

			accFee.AAF_Rule = "TCB";
			AssertNoError("No Validation Error Expected", accFee.AAF_RuleInfo, "Enter a valid selection.");
		}

		public void TestAAF_AG_GLAccountValidation()
		{
			var accFee = new AccountFeeSettings(GlbCompany.CurrentCompany.PK, GlbCompany.CurrentCompany);
			accFee.OverrideSettings = true;
			accFee.AAF_AG_GLAccount = new Guid();
			AssertHasError("Validation Error Expected", accFee.AAF_AG_GLAccountInfo, "Please enter a value.");

			var glHeader = Factory.New<AccGLHeader>();
			accFee.AAF_AG_GLAccount = glHeader.PK;
			AssertNoError("No Validation Error Expected", accFee.AAF_AG_GLAccountInfo, "Please enter a value.");
		}
	}
}


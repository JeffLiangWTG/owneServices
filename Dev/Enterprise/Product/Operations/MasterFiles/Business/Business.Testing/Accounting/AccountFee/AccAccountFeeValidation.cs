using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAccountFeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAAF_FeeAmountValidation()
		{
			var accFee = Factory.New<AccAccountFee>();
			accFee.AAF_FeeAmount = -250m;
			AssertHasError("Validation Error Expected", accFee.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");

			accFee.AAF_FeeAmount = 150m;
			AssertNoError("No Validation Error Expected", accFee.AAF_FeeAmountInfo, "Account Fee Amount must be a positive number");
		}

		public void TestAAF_RX_NKFeeCurrencyValidation()
		{
			var accFee = Factory.New<AccAccountFee>();
			accFee.AAF_RX_NKFeeCurrency = "ZZZ";
			AssertHasError("Validation Error Expected", accFee.AAF_RX_NKFeeCurrencyInfo, "Enter a valid selection.");

			accFee.AAF_RX_NKFeeCurrency = "AUD";
			AssertNoError("No Validation Error Expected", accFee.AAF_RX_NKFeeCurrencyInfo, "Enter a valid selection.");
		}

		public void TestAAF_RuleValidation()
		{
			var accFee = Factory.New<AccAccountFee>();
			accFee.AAF_Rule = "ZZZ";
			AssertHasError("Validation Error Expected", accFee.AAF_RuleInfo, "Enter a valid selection.");

			accFee.AAF_Rule = "TCB";
			AssertNoError("No Validation Error Expected", accFee.AAF_RuleInfo, "Enter a valid selection.");
		}
	}
}

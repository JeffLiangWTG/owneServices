using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditLimitDetailsTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var details = new CreditLimitDetails(1, 2, true);
			AssertEquals(1m, details.CreditLimit);
			AssertEquals(2m, details.TotalOutstandingAmount);
			AssertEquals(true, details.OnCreditHold);
			AssertEquals(false, details.IsGlobalCreditApproved);
			AssertNull(details.GlobalCreditCurrency);
			AssertEquals(0m, details.GlobalCreditLimit);
			AssertEquals(0m, details.GlobalTotalOutstandingAmount);
			AssertEquals(false, details.IsOnGlobalCreditHold);

			details = new CreditLimitDetails(1, 2, true, true, "USD", 3, 4, true);
			AssertEquals(1m, details.CreditLimit);
			AssertEquals(2m, details.TotalOutstandingAmount);
			AssertEquals(true, details.OnCreditHold);
			AssertEquals(true, details.IsGlobalCreditApproved);
			AssertEquals("USD", details.GlobalCreditCurrency);
			AssertEquals(3m, details.GlobalCreditLimit);
			AssertEquals(4m, details.GlobalTotalOutstandingAmount);
			AssertEquals(true, details.IsOnGlobalCreditHold);
		}
	}
}

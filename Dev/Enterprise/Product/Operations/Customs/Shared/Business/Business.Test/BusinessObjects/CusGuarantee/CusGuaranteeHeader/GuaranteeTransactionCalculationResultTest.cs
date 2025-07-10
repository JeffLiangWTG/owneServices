using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GuaranteeTransactionCalculationResultTest : TestCaseWithFactory
	{
		public void TestGetter()
		{
			var confirmedBalance = new Money(10m, GlbCompany.CurrentCompany.LocalCurrency);
			var pendingBalance = new Money(20m, GlbCompany.CurrentCompany.LocalCurrency);
			var totalBalance = new Money(30m, GlbCompany.CurrentCompany.LocalCurrency);
			var calculationTimeUtc = new ZDateTime(2017, 4, 29, 10, 0, 0, 7);

			var transactionCalculationResult = new GuaranteeTransactionCalculationResult(confirmedBalance, pendingBalance, totalBalance, calculationTimeUtc);

			AssertEquals(transactionCalculationResult.ConfirmedBalance, confirmedBalance);
			AssertEquals(transactionCalculationResult.PendingBalance, pendingBalance);
			AssertEquals(transactionCalculationResult.TotalBalance, totalBalance);
			AssertEquals(transactionCalculationResult.CalculationTimeUtc, calculationTimeUtc);
		}
	}
}

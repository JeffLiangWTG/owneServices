using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmptyMoneyTest : TestCase
	{
		public void TestBasicFunctionality()
		{
			EmptyMoney emptyMoney = new EmptyMoney();
			AssertEquals("emptyMoney.Amount", 0.00m, emptyMoney.Amount);
			AssertEquals("emptyMoney.Currency", GlbCompany.CurrentCompany.LocalCurrency, emptyMoney.Currency);
			AssertEquals("emptyMoney.IsValid", true, emptyMoney.IsValid);
		}

		public void TestRound()
		{
			AssertEquals(Money.Empty, Money.Empty.Round());
		}

		public void TestRoundWithDecimals()
		{
			AssertEquals(Money.Empty, Money.Empty.Round(2));
		}
	}
}

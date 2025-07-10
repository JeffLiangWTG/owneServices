using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

[TestedType(typeof(IntegerFeeRounder))]
sealed class IntegerFeeRounderTest : TestCaseWithFactory
{
	public void TestRound()
	{
		var chargeAmountRounder = new IntegerFeeRounder();

		CombineAssertions("Assert Round() return value", () =>
		{
			AssertEquals("Round() over .5", 2m, chargeAmountRounder.Round(1.501));
			AssertEquals("Round() .5", 1m, chargeAmountRounder.Round(0.5));
			AssertEquals("Round() below .5", 0m, chargeAmountRounder.Round(0.499));
			AssertEquals("Round() negative", -1m, chargeAmountRounder.Round(-1.499));
		});
	}
}

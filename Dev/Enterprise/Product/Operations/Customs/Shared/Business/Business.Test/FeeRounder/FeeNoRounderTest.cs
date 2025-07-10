using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class FeeNoRounderTest : TestCaseWithFactory
	{
		public void TestRound()
		{
			var chargeAmountRounder = new FeeNoRounder();

			CombineAssertions("Assert Round() return value", () =>
			{
				AssertEquals("Round() case #1", 1.496m, chargeAmountRounder.Round(1.496));
				AssertEquals("Round() case #2", 1.50442m, chargeAmountRounder.Round(1.50442));
				AssertEquals("Round() case #3", 0.003m, chargeAmountRounder.Round(0.003));
				AssertEquals("Round() case #4", -1.5043m, chargeAmountRounder.Round(-1.5043));
			});
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGTariffRateTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			var rateP1 = new SGTariffRate(0.2m, 0m, "");
			var rateP2 = new SGTariffRate(0.2m, 0m, "");
			var rateP3 = new SGTariffRate(0.3m, 0m, "");
			var rateU1 = new SGTariffRate(0m, 1m, "");
			var rateU2 = new SGTariffRate(0m, 1m, "");
			var rateU3 = new SGTariffRate(0m, 2m, "");
			AssertEquals(rateP1, rateP1);
			AssertEquals(rateP1, rateP2);
			AssertNotEquals(rateP1, rateP3);
			AssertEquals(rateU1, rateU1);
			AssertEquals(rateU1, rateU2);
			AssertNotEquals(rateU1, rateU3);
		}

		public void TestToString()
		{
			AssertEquals("PercentageRate=0.20, UnitRate=20.20, UnitQty=KG", new SGTariffRate(0.2m, 20.2m, "KG").ToString());
		}
	}
}

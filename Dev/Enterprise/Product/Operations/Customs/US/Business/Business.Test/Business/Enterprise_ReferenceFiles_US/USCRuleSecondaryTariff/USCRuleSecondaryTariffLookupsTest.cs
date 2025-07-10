using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCRuleSecondaryTariffLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffs()
		{
			var uscRuleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			var tariffs = new USCRuleSecondaryTariffLookups(uscRuleSecondaryTariff).Tariffs;
			AssertType<USCTariffCollection>(tariffs);
		}
	}
}

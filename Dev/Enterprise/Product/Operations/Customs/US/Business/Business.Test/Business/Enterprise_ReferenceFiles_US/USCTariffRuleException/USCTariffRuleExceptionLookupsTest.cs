using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCTariffRuleExceptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCTariffCollection()
		{
			var lookups = new USCTariffRuleExceptionLookups(Factory.New<USCTariffRuleException>());
			AssertType<USCTariffCollection>(lookups.Tariffs);
		}
	}
}

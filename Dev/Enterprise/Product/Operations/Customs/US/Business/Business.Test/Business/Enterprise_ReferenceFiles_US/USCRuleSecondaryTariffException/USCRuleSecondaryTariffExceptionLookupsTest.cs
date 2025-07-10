using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCRuleSecondaryTariffExceptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffs()
		{
			var uscRuleSecondaryTariffException = Factory.New<USCRuleSecondaryTariffException>();
			var tariffs = new USCRuleSecondaryTariffExceptionLookups(uscRuleSecondaryTariffException).Tariffs;
			AssertType<USCTariffCollection>(tariffs);
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class CO2eHelperTest : TestCaseWithFactory
{
	public void TestGetFormattedCO2e()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
		{
			AssertEquals("100,000.124", CO2eHelper.GetFormattedCO2e(100000.1239m));
			AssertEquals("100,000.12", CO2eHelper.GetFormattedCO2e(100000.12m));
			AssertEquals("100,000", CO2eHelper.GetFormattedCO2e(100000m));
		}

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
		{
			AssertEquals("100.000,124", CO2eHelper.GetFormattedCO2e(100000.1239m));
			AssertEquals("100.000,12", CO2eHelper.GetFormattedCO2e(100000.12m));
			AssertEquals("100.000", CO2eHelper.GetFormattedCO2e(100000m));
		}
	}
}

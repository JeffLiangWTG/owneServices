using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(BritishIndianOceanTerritoryComplianceInfo))]
	sealed class BritishIndianOceanTerritoryComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.BritishIndianOceanTerritory;
	}
}

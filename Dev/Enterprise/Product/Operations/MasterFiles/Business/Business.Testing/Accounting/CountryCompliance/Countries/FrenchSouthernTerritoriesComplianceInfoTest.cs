using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(FrenchSouthernTerritoriesComplianceInfo))]
	sealed class FrenchSouthernTerritoriesComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.FrenchSouthernTerritories;
	}
}

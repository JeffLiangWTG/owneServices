using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NorfolkIslandComplianceInfo))]
	sealed class NorfolkIslandComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.NorfolkIsland;
	}
}

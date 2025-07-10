using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(TurksAndCaicosIslandsComplianceInfo))]
	sealed class TurksAndCaicosIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.TurksAndCaicosIslands;
	}
}

using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(HeardAndMcdonaldIslandsComplianceInfo))]
	sealed class HeardAndMcdonaldIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.HeardAndMcdonaldIslands;
	}
}

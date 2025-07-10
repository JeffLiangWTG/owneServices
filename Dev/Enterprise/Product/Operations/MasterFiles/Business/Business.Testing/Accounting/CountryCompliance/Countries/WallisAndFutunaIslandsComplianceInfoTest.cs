using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(WallisAndFutunaIslandsComplianceInfo))]
	sealed class WallisAndFutunaIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.WallisAndFutunaIslands;
	}
}

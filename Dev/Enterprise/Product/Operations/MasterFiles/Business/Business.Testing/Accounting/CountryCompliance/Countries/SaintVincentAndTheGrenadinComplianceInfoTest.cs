using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaintVincentAndTheGrenadinComplianceInfo))]
	sealed class SaintVincentAndTheGrenadinComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SaintVincentAndTheGrenadin;
	}
}

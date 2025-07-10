using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(WesternSaharaComplianceInfo))]
	sealed class WesternSaharaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.WesternSahara;
	}
}

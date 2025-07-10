using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaintBarthelemyComplianceInfo))]
	sealed class SaintBarthelemyComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SaintBarthelemy;
	}
}

using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SaoTomeAndPrincipeComplianceInfo))]
	sealed class SaoTomeAndPrincipeComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SaoTomeAndPrincipe;
	}
}

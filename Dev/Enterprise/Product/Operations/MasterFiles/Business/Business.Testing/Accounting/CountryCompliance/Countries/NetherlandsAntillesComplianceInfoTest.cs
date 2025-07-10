using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(NetherlandsAntillesComplianceInfo))]
	sealed class NetherlandsAntillesComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.NetherlandsAntilles;
	}
}

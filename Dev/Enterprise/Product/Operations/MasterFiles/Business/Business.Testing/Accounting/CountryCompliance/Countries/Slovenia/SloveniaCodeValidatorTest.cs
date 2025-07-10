using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SloveniaComplianceInfo))]
	sealed class SloveniaCodeValidatorTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Slovenia;
	}
}

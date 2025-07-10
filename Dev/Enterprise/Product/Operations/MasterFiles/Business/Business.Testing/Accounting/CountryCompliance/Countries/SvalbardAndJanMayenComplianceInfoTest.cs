using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SvalbardAndJanMayenComplianceInfo))]
	sealed class SvalbardAndJanMayenComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SvalbardAndJanMayen;
	}
}

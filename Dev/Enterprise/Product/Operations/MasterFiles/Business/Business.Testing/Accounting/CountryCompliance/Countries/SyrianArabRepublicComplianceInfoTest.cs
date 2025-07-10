using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(SyrianArabRepublicComplianceInfo))]
	sealed class SyrianArabRepublicComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.SyrianArabRepublic;
	}
}

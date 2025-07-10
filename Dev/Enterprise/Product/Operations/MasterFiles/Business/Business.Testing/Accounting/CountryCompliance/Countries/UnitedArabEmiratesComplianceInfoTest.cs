using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(UnitedArabEmiratesComplianceInfo))]
	sealed class UnitedArabEmiratesComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedArabEmirates;
	}
}

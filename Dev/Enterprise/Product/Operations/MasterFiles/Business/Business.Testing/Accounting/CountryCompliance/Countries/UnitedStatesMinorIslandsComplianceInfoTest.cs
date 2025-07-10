using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(UnitedStatesMinorIslandsComplianceInfo))]
	sealed class UnitedStatesMinorIslandsComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStatesMinorIslands;
	}
}

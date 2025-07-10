using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PrimarySpecProgramIndicatorListTest : TestCase
	{
		public void TestIndicatorIsExcludedFromDBCheck()
		{
			AssertEquals(true, PrimarySpecProgramIndicatorList.IndicatorIsExcludedFromDBCheck(PrimarySpecProgramIndicatorList.Codes.N));
			AssertEquals(false, PrimarySpecProgramIndicatorList.IndicatorIsExcludedFromDBCheck(PrimarySpecProgramIndicatorList.Codes.A));
		}

		public void TestIsNonCountrySpecific()
		{
			AssertEquals(true, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.C));
			AssertEquals(false, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.B));

			AssertEquals(true, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.K));
			AssertEquals(false, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.A));

			AssertEquals(true, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.L));
			AssertEquals(false, PrimarySpecProgramIndicatorList.IsNonCountrySpecific(PrimarySpecProgramIndicatorList.Codes.D));
		}

		public void TestHasNAFTAEquivalentSPICountryCode()
		{
			AssertEquals(true, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.C));
			AssertEquals(false, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.A));

			AssertEquals(true, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.K));
			AssertEquals(false, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.Y));

			AssertEquals(true, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.L));
			AssertEquals(false, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.D));

			AssertEquals(true, PrimarySpecProgramIndicatorList.HasNAFTAEquivalentSPICountryCode(PrimarySpecProgramIndicatorList.Codes.B));
		}

		public void TestIsDutyFreeSPI()
		{
			AssertEquals(true, PrimarySpecProgramIndicatorList.IsDutyFreeSPI(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(false, PrimarySpecProgramIndicatorList.IsDutyFreeSPI(PrimarySpecProgramIndicatorList.Codes.E));
			AssertEquals(true, PrimarySpecProgramIndicatorList.IsDutyFreeSPI(PrimarySpecProgramIndicatorList.Codes.K));
		}
	}
}

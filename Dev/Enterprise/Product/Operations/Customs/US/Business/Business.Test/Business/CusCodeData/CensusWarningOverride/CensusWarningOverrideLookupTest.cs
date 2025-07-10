using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CensusWarningOverrideLookupTest : TestCaseWithFactory
	{
		public void TestCensusOverrideList()
		{
			CensusWarningOverride censusOverride = Factory.New<CensusWarningOverride>();
			censusOverride.CY_Code = CensusWarningCodeList.Codes.MaximumChargeExceeded;
			AssertEquals(1, censusOverride.Lookups.CensusOverrideList.Count);
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._51));
			censusOverride.CY_Code = CensusWarningCodeList.Codes.ImprobableCountry;
			AssertEquals(5, censusOverride.Lookups.CensusOverrideList.Count);
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._01));
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._02));
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._03));
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._49));
			Assert(censusOverride.Lookups.CensusOverrideList.ContainsCode(CensusOverrideCodeList.Codes._50));
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SpecialProgramListTest : TestCaseWithFactory
	{
		public void TestIsNAFTASPI()
		{
			AssertEquals(true, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.BSharp));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.BH));

			AssertEquals(true, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.CSharp));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.CA));

			AssertEquals(true, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.KSharp));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.MA));

			AssertEquals(true, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.LSharp));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.MX));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.S));
			AssertEquals(false, SpecialProgramList.IsNAFTASPI(SpecialProgramList.Codes.SPlus));
		}
	}
}

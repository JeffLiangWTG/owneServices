using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PGADispositionCodeListTest : TestCaseWithFactory
	{
		public void TestIsDocRequiredPGADispositionCode()
		{
			AssertEquals(false, PGADispositionCodeList.IsDocRequiredPGADispositionCode("01"));
			AssertEquals(false, PGADispositionCodeList.IsDocRequiredPGADispositionCode("05"));
			AssertEquals(true, PGADispositionCodeList.IsDocRequiredPGADispositionCode("10"));
		}

		public void TestIsFinalCode()
		{
			AssertEquals(false, PGADispositionCodeList.IsFinalCode("01"));
			AssertEquals(true, PGADispositionCodeList.IsFinalCode("07"));
			AssertEquals(true, PGADispositionCodeList.IsFinalCode("MC"));
		}

		public void TestGetPGADispositionCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "TV", "Test Value", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var list = PGADispositionCodeList.GetPGADispositionCodeList(Factory);

			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(PGADispositionCodeList.MarkAsClosedCode));
		}
	}
}

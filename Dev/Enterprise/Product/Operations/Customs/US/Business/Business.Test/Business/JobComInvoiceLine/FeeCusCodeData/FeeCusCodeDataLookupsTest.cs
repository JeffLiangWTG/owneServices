using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeCusCodeDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_SelectedRateTypeList()
		{
			var fee = Factory.New<FeeCusCodeData>();
			AssertEquals(typeof(RateTypeList), fee.Lookups.CY_SelectedRateTypeList.GetType());
		}

		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, "Accounting Class Fee Code", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"124", "Pecan Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"125", "Christmas Tree Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var fee = Factory.New<FeeCusCodeData>();
			var list = fee.Lookups.CY_CodeList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("124"));
			AssertEquals(true, list.ContainsCode("125"));
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconRefundedChargeLookupsTest : TestCaseWithFactory
	{
		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, "Accounting Class Fee Code", Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Pecan, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.ChristmasTree, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.DistilledSpirits, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Tobacco, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.Wines, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				Core.Constants.USCustoms.FeeCodes.OtherExcise, "description", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var charge = Factory.New<ReconRefundedCharge>();
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.Pecan));
			AssertEquals(true, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.ChristmasTree));

			AssertEquals(false, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.DistilledSpirits));
			AssertEquals(false, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals(false, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertEquals(false, charge.Lookups.CY_CodeList.ContainsCode(Core.Constants.USCustoms.FeeCodes.OtherExcise));
		}
	}
}

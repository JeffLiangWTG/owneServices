using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementLineChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryChargeTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.AccountingClassFeeCode, RefCusCodeListTypes.Codes.AccountingClassFeeCode);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"124", "Pecan Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.AccountingClassFeeCode,
				"125", "Christmas Tree Fee", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var lineCharge = Factory.New<CusStatementLineCharge>();
			var lookups = new CusStatementLineChargeLookups(lineCharge);

			AssertEquals("Pecan Fee", lookups.EntryChargeTypeList.GetDescriptionFromCode("124"));
			AssertEquals("Christmas Tree Fee", lookups.EntryChargeTypeList.GetDescriptionFromCode("125"));

			AssertEquals("Antidumping Duty", lookups.EntryChargeTypeList.GetDescriptionFromCode("ADD"));
			AssertEquals("Countervailing Duty", lookups.EntryChargeTypeList.GetDescriptionFromCode("CVD"));
			AssertEquals("Duty", lookups.EntryChargeTypeList.GetDescriptionFromCode("DTY"));
			AssertEquals("Excise Tax Payable", lookups.EntryChargeTypeList.GetDescriptionFromCode("ETP"));
			AssertEquals("Excise Tax Deferred", lookups.EntryChargeTypeList.GetDescriptionFromCode("ETD"));
			AssertEquals("Interest Amount For Reconciliation Summary", lookups.EntryChargeTypeList.GetDescriptionFromCode("ARS"));
		}
	}
}

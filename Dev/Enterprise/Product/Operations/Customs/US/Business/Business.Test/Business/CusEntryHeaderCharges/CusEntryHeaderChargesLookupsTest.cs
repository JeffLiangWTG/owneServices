using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeaderCharges()
		{
			CusEntryHeaderCharges parent = Factory.New<CusEntryHeaderCharges>();
			AssertEquals(parent.Lookups.EntryHeaderCharges, parent);
		}

		[TestDate(2023, 10, 25)]
		public void TestC1_ChargeTypeList()
		{
			var charges = Factory.New<CusEntryHeaderCharges>();
			var lookups = new CusEntryHeaderChargesLookups(charges);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);

			AssertEquals(codes.Length + 3, lookups.C1_ChargeTypeList.Count);

			AssertEquals("Duty", lookups.C1_ChargeTypeList.GetDescriptionFromCode("DTY"));
			AssertEquals("Interest Amount For Reconciliation Summary", lookups.C1_ChargeTypeList.GetDescriptionFromCode("ARS"));
			AssertEquals("MPF As Calculated And Unadjusted", lookups.C1_ChargeTypeList.GetDescriptionFromCode("MPC"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}

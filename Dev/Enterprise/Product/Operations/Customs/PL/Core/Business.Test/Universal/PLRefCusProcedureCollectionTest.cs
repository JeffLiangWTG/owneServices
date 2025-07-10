using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLRefCusProcedureCollection))]
sealed class PLRefCusProcedureCollectionTest : ActiveBusinessObjectCollectionTestCase<PLRefCusProcedureCollection>
{
	public void TestLoadCustomsProcedureCodesForPolandWithCountryGrouping()
	{
		CombineAssertions(() =>
		{
			var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "11";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
			testCusProcedure.ZZ6_ShipmentType = "IMP";
			testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "13";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure.ZZ6_ShipmentType = "IMP";

			Factory.Save();
			var testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.UnitedKingdom, "IMP", ZDateTime.Today);
			AssertEquals("Collection should contain 1 item for GB", 1, testCollection.Count);
			AssertEquals("The first item in collection for GB should have procedure code equal to 11", "11", testCollection[0].ZZ6_ProcedureCode);
			testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.Italy, "IMP", ZDateTime.Today);
			AssertEquals("Collection should contain 1 item for IT", 1, testCollection.Count);
			AssertEquals("The first item in collection for IT should have procedure code equal to 13", "13", testCollection[0].ZZ6_ProcedureCode);
		});
	}

	public void TestLoadCustomsProcedureCodesForPolandWithShipmentType()
	{
		CombineAssertions(() =>
		{
			var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "11";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
			testCusProcedure.ZZ6_ShipmentType = "IMP";
			testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "13";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.UnitedKingdom;
			testCusProcedure.ZZ6_ShipmentType = "EXP";

			Factory.Save();
			var testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.UnitedKingdom, "IMP", ZDateTime.Today);
			AssertEquals("Collection should contain 1 item in IMP", 1, testCollection.Count);
			AssertEquals("The first item in collection for GB in IMP should have procedure code equal to 11", "11", testCollection[0].ZZ6_ProcedureCode);
			testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.UnitedKingdom, "EXP", ZDateTime.Today);
			AssertEquals("Collection should contain 1 item in EXP", 1, testCollection.Count);
			AssertEquals("The first item in collection for GB in EXP should have procedure code equal to 13", "13", testCollection[0].ZZ6_ProcedureCode);
		});
	}

	public void TestLoadCustomsProcedureCodesForPolandWithDate()
	{
		CombineAssertions(() =>
		{
			var testCusProcedure = Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = CoreConstants.CountryCodes.Italy;
			testCusProcedure.ZZ6_ShipmentType = "IMP";
			testCusProcedure.ZZ6_ProcedureCode = "11";
			testCusProcedure.ZZ6_StartDate = new ZDateTime(2016, 1, 1);
			testCusProcedure.ZZ6_EndDate = new ZDateTime(2016, 12, 31);

			Factory.Save();
			var testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.Italy, "IMP", new ZDateTime(2016, 6, 8));
			AssertEquals("Collection should contain 1 item for IT in IMP and 2016", 1, testCollection.Count);
			AssertEquals("The first item in collection for IT in IMP and 2016 should have start date equal to (2016, 1, 1)", new ZDateTime(2016, 1, 1), testCollection[0].ZZ6_StartDate);
			AssertEquals("The first item in collection for IT in IMP and 2016 should have procedure code equal to 11", "11", testCollection[0].ZZ6_ProcedureCode);
			testCollection = PLRefCusProcedureCollection.LoadCustomsProcedureCodesForPoland(Factory, CoreConstants.CountryCodes.Italy, "IMP", new ZDateTime(2017, 6, 8));
			AssertEquals("Collection should not contain any item for IT in IMP and 2017", 0, testCollection.Count);
		});
	}

	protected override PLRefCusProcedureCollection GetCollectionToTest()
	{
		return new PLRefCusProcedureCollection(Factory);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(CoreConstants.CountryCodes.Italy);
		helper.CreateNewOrGetExistingDataGrouping(CoreConstants.CountryCodes.UnitedKingdom);
	}
}

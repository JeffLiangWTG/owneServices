using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusProcedureModule))]
	public class ZZRefCusProcedureModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.ZZRefCusProcedure;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var testCusProcedure = collection.Factory.NewWithValidTestData<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "88";
			testCusProcedure.ZZ6_PreviousProcedureCode = "88";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.UnitedKingdom;
			testCusProcedure.ZZ6_Category = "CAT";
			testCusProcedure.ZZ6_Concession = "888";
			testCusProcedure.ZZ6_Description = "Seven Export";
			testCusProcedure.ZZ6_ShipmentType = "EXP";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure01GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Latvia, "CAT", "11", "11", "111", "One Import", "IMP");
			var procedure02GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Latvia, "CAT", "22", "22", "222", "Two Export", "EXP");
			var procedure03ItImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "33", "33", "333", "Three Import", "IMP");
			var procedure04ItExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Italy, "CAT", "44", "44", "444", "Four Export", "EXP");
			var procedure05GbImp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "55", "55", "555", "Five Import", "IMP");
			var procedure06GbExp = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "CAT", "66", "77", "666", "Six Export", "EXP");
			Factory.Save();
			collection.Factory.Save();
		}

		public void TestItemsIsAllowed()
		{
			using (var module = new ZZRefCusProcedureModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is not allowed.", false, module.AllowNew);
				AssertEquals("Edit is not allowed.", false, module.AllowEdit);
				AssertEquals("Delete is not allowed.", false, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
		}
	}
}

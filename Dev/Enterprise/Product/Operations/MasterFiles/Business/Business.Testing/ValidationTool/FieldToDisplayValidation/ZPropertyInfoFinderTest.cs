using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ZPropertyInfoFinderTest : TestCaseWithFactory
{
	public void TestGetZPropertyInfo() => CombineAssertions(() =>
	{
		MasterFilesTestHelper.ClearWorkflowTables();

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		MasterFilesTestHelper.CreateCustomField(template1, "HIGH VALUE ENTERED", AddOnColumnDataType.Codes.Boolean);

		Factory.Save();

		var entity = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();

		AssertNull("null entity for non custom field", new ZPropertyInfoFinder(null).GetZPropertyInfo("JE_GoodsDescription", false));
		AssertNull("JE_GoodsDescription is not a custom field, but IsCustom true requested", new ZPropertyInfoFinder(entity).GetZPropertyInfo("JE_GoodsDescription", true));
		AssertNotNull("JE_GoodsDescription", new ZPropertyInfoFinder(entity).GetZPropertyInfo("JE_GoodsDescription", false));

		AssertNull("null entity for custom field", new ZPropertyInfoFinder(null).GetZPropertyInfo("__HIGH VALUE ENTERED__prop__ZBool", true));
		AssertNull("HIGH VALUE ENTERED is a custom field, but IsCustom false requested", new ZPropertyInfoFinder(entity).GetZPropertyInfo("__HIGH VALUE ENTERED__prop__ZBool", false));
		AssertNotNull("HIGH VALUE ENTERED", new ZPropertyInfoFinder(entity).GetZPropertyInfo("__HIGH VALUE ENTERED__prop__ZBool", true));
	});
}

using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class FiledToDisplayValidationStaticInfoTest : TestCaseWithFactory
{
	public void TestBusinessObjectType()
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertEquals("Enterprise.Customs.AU.Declaration.Business.JobDeclaration", check.BusinessObjectType.FullName);
	}

	public void TestFieldName()
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertEquals("JE_GoodsDescription", check.FieldName);
	}

	public void TestFieldValueType()
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertEquals(typeof(ZString), check.FieldValueType);
	}

	public void TestIsCustomField() => CombineAssertions(() =>
	{
		MasterFilesTestHelper.ClearWorkflowTables();

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		MasterFilesTestHelper.CreateCustomField(template1, "HIGH VALUE ENTERED", AddOnColumnDataType.Codes.Boolean);

		Factory.Save();

		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		AssertEquals("Not Custom", false, new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType).IsCustomField);
		AssertEquals("Custom", true, new FieldToDisplayValidationResolver("GetCustomField(HIGH VALUE ENTERED)").GetFieldToDisplayValidationStaticInfo(componentType).IsCustomField);
	});

	public void TestIsZPropertyInfo()
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertEquals(true, check.IsZPropertyInfo);
	}

	public void TestGetTableName()
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertEquals("JobDeclaration", check.GetTableName());
	}

	public void TestGetZPropertyInfo() => CombineAssertions(() =>
	{
		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		var check = new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationStaticInfo(componentType);
		AssertNull("null entity", check.GetZPropertyInfo(null));
		AssertNotNull("not null entity", check.GetZPropertyInfo(Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>()));
	});
}

using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class FieldToDisplayValidationResolverTest : TestCaseWithFactory
{
	public void TestGetFieldToDisplayValidationZPropertyInfo()
	{
		MasterFilesTestHelper.ClearWorkflowTables();

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		MasterFilesTestHelper.CreateCustomField(template1, "HIGH VALUE ENTERED", AddOnColumnDataType.Codes.Boolean);

		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "CIL Task Template";
		template2.P0_ProcessType = "CIL";
		MasterFilesTestHelper.CreateCustomField(template2, "CIL_TEST");

		Factory.Save();

		var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		declaration.Invoices.AddNew().AddNewInvoiceLine();

		AssertNull("null entity", new FieldToDisplayValidationResolver("GetCustomField(HIGH VALUE ENTERED)").GetFieldToDisplayValidationZPropertyInfo(null));
		AssertNull("empty fieldToDisplayValidation", new FieldToDisplayValidationResolver("").GetFieldToDisplayValidationZPropertyInfo(declaration));
		AssertNotNull("GetCustomField(HIGH VALUE ENTERED)", new FieldToDisplayValidationResolver("GetCustomField(HIGH VALUE ENTERED)").GetFieldToDisplayValidationZPropertyInfo(declaration));
		AssertNotNull("GetCustomFieldWithType(HIGH VALUE ENTERED, BOO)", new FieldToDisplayValidationResolver("GetCustomFieldWithType(HIGH VALUE ENTERED, BOO)").GetFieldToDisplayValidationZPropertyInfo(declaration));
		AssertNotNull("InvoiceLines.First().GetCustomField(CIL_TEST)", new FieldToDisplayValidationResolver("InvoiceLines.First().GetCustomField(CIL_TEST)").GetFieldToDisplayValidationZPropertyInfo(declaration));
		AssertNotNull("InvoiceLines.First().JI_Description", new FieldToDisplayValidationResolver("InvoiceLines.First().JI_Description").GetFieldToDisplayValidationZPropertyInfo(declaration));
		AssertNotNull("JE_GoodsDescription", new FieldToDisplayValidationResolver("JE_GoodsDescription").GetFieldToDisplayValidationZPropertyInfo(declaration));
	}

	public void TestGetFieldToDisplayValidationStaticInfo() => CombineAssertions(() =>
	{
		MasterFilesTestHelper.ClearWorkflowTables();

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		MasterFilesTestHelper.CreateCustomField(template1, "HIGH VALUE ENTERED", AddOnColumnDataType.Codes.Boolean);

		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "CIL Task Template";
		template2.P0_ProcessType = "CIL";
		MasterFilesTestHelper.CreateCustomField(template2, "CIL_TEST");

		Factory.Save();

		var componentType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();

		AssertNull("null componentType", new FieldToDisplayValidationResolver("GetCustomField(HIGH VALUE ENTERED)").GetFieldToDisplayValidationStaticInfo(null));
		AssertNull("empty fieldToDisplayValidation", new FieldToDisplayValidationResolver("").GetFieldToDisplayValidationStaticInfo(componentType));

		AssertFieldToDisplayValidationStaticInfo("GetCustomField(HIGH VALUE ENTERED)", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", "__HIGH VALUE ENTERED__prop__ZBool", typeof(ZBool), true);
		AssertFieldToDisplayValidationStaticInfo("GetCustomFieldWithType(HIGH VALUE ENTERED, BOO)", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", "__HIGH VALUE ENTERED__prop__ZBool", typeof(ZBool), true);
		AssertFieldToDisplayValidationStaticInfo("InvoiceLines.First().GetCustomField(CIL_TEST)", "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine", "__CIL`=TEST__prop__ZString", typeof(ZString), true);
		AssertFieldToDisplayValidationStaticInfo("InvoiceLines.First().JI_Description", "Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine", "JI_Description", typeof(ZString), false);
		AssertFieldToDisplayValidationStaticInfo("JE_GoodsDescription", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration", "JE_GoodsDescription", typeof(ZString), false);

		return;

		void AssertFieldToDisplayValidationStaticInfo(string fieldToDisplayValidation, string businessObjectType, string fieldName, Type fieldValueType, bool isCustomField)
		{
			var check = new FieldToDisplayValidationResolver(fieldToDisplayValidation).GetFieldToDisplayValidationStaticInfo(componentType);
			AssertEquals($"{fieldToDisplayValidation}-BusinessObjectType", businessObjectType, check.BusinessObjectType.FullName);
			AssertEquals($"{fieldToDisplayValidation}-FieldName", fieldName, check.FieldName);
			AssertEquals($"{fieldToDisplayValidation}-FieldValueType", fieldValueType, check.FieldValueType);
			AssertEquals($"{fieldToDisplayValidation}-IsCustomField", isCustomField, check.IsCustomField);
		}
	});
}

using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class FieldToDisplayValidationInjectorTest : TestCaseWithFactory
{
	public void TestInject() => CombineAssertions(() =>
	{
		using var enableWorkflowValidationRules = WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } });

		MasterFilesTestHelper.ClearWorkflowTables();

		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		MasterFilesTestHelper.CreateCustomField(template1, "HIGH VALUE ENTERED", AddOnColumnDataType.Codes.Boolean);

		var templateValidation1 = template1.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation1.P0V_Description = "D1";
		templateValidation1.P0V_FieldToDisplayValidation = "JE_GoodsDescription";

		var templateValidation2 = template1.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation2.P0V_Description = "D2";
		templateValidation2.P0V_FieldToDisplayValidation = "GetCustomField(HIGH VALUE ENTERED)";

		var templateValidation3 = template1.ProcessTemplateValidations.AddNew();
		templateValidation3.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation3.P0V_Description = "D3";
		templateValidation3.P0V_FieldToDisplayValidation = "InvoiceLines.First().JI_Description";

		var templateValidation4 = template1.ProcessTemplateValidations.AddNew();
		templateValidation4.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation4.P0V_Description = "D4";
		templateValidation4.P0V_FieldToDisplayValidation = "InvoiceLines.First().GetCustomField(CIL_TEST)";

		var template2 = Factory.New<ProcessTaskTemplate>();
		template2.P0_Name = "CIL Task Template";
		template2.P0_ProcessType = "CIL";
		MasterFilesTestHelper.CreateCustomField(template2, "CIL_TEST");

		var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		((BusinessObject)declaration).FillWithValidTestData();

		Factory.Save();

		new FieldToDisplayValidationInjector(declaration).Inject();

		AssertNormalFieldInjected("Roof", (BusinessObject)declaration, "JE_GoodsDescription");
		AssertCustomFieldInjected("Roof", (BusinessObject)declaration, "__HIGH VALUE ENTERED__prop__ZBool");

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.AddNewInvoiceLine();
		var invoiceLine2 = invoiceHeader.AddNewInvoiceLine();
		((BusinessObject)invoiceLine1).HasChanges = true;
		((BusinessObject)invoiceLine2).HasChanges = true;

		AssertNormalFieldInjected("Leaf 1", (BusinessObject)invoiceLine1, "JI_Description");
		AssertCustomFieldInjected("Leaf 1", (BusinessObject)invoiceLine1, "__CIL`=TEST__prop__ZString");
		AssertNormalFieldInjected("Leaf 2", (BusinessObject)invoiceLine2, "JI_Description");
		AssertCustomFieldInjected("Leaf 2", (BusinessObject)invoiceLine2, "__CIL`=TEST__prop__ZString");
	});

	internal static void AssertNormalFieldInjected(string message, BusinessObject entity, string propertyNameOrIdentifier)
	{
		var dictionary = GetInnerAdditionalValidationDictionary(entity);
		var injected = dictionary.TryGetValue(propertyNameOrIdentifier, out var innerDictionary) && innerDictionary.TryGetValue(entity, out var runValidationInvoker) && runValidationInvoker is not null;
		AssertEquals(message + "->" + propertyNameOrIdentifier, true, injected);
	}

	static void AssertCustomFieldInjected(string message, BusinessObject entity, string propertyNameOrIdentifier)
	{
		var dictionary = GetInnerAdditionalValidationDictionary(entity);
		var injected = dictionary.TryGetValue(propertyNameOrIdentifier, out var innerDictionary) && innerDictionary.TryGetValue(ZPropertyInfoFinder.GetCustomBusinessObject(entity), out var runValidationInvoker) && runValidationInvoker is not null;
		AssertEquals(message + "->" + propertyNameOrIdentifier, true, injected);
	}

	static Dictionary<string, Dictionary<BusinessObject, RunValidationInvoker>> GetInnerAdditionalValidationDictionary(BusinessObject entity)
	{
		var propertyInfoStoragePropertyInfo = typeof(BusinessObject).GetProperty("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
		var propertyInfoStorage = propertyInfoStoragePropertyInfo.GetValue(entity);
		var additionalValidationDictionaryPropertyInfo = propertyInfoStorage.GetType().GetProperty("AdditionalValidationDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		var additionalValidationDictionary = additionalValidationDictionaryPropertyInfo.GetValue(propertyInfoStorage);
		var dictionaryFieldInfo = additionalValidationDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
		var dictionary = (Dictionary<string, Dictionary<BusinessObject, RunValidationInvoker>>)dictionaryFieldInfo.GetValue(additionalValidationDictionary);
		return dictionary;
	}
}

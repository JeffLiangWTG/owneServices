using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class CustomFieldProcessTaskTemplate : TestCaseWithFactory
	{
		#region Setup

		DummyWithCustomFields GetDummy(string subtype1 = "", string subtype2 = "")
		{
			return CustomFieldProcessTaskTemplateTestHelper.GetDummy(Factory, subtype1, subtype2);
		}

		ProcessTaskTemplate GetTemplate(string subtype1 = "", string subtype2 = "")
		{
			return CustomFieldProcessTaskTemplateTestHelper.GetTemplate(Factory, subtype1, subtype2);
		}

		static GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, string name, string type = "STR")
		{
			return CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, name, type);
		}

		GenCustomAddOnRule SetNewAddOnRule(GenCustomColumnDefinition column, string name, string listCode)
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(listCode, "Some addOnRule desc");

			var addOnRule = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			addOnRule.SetRules(new InvalidCodeRule() { List = list, IsEnabled = true });
			addOnRule.XR_Code = name;
			addOnRule.XR_Description = name;
			addOnRule.XR_IsSystemDefined = false;
			column.XC_XR = addOnRule.PK;

			return addOnRule;
		}

		void AssertFieldExists(DummyWithCustomFields dummy, string field)
		{
			AssertNotNull(dummy.GetCustomFieldAccessor(field, null));
		}

		void AssertAddOnRuleList(DummyWithCustomFields dummy, string field, string code)
		{
			var accessor = dummy.GetCustomFieldAccessor(field, null);
			AssertNotNull(accessor);
			var data = (CodeDescriptionPairList)accessor.MetaData.MetaData.First(m => m.Id == "ListDataSource").Value;
			Assert("There should be a code list metadata type containing " + code, data.ContainsCode(code));
		}

		void AssertFieldDoesNotExist(DummyWithCustomFields dummy, string field)
		{
			AssertNull(dummy.GetCustomFieldAccessor(field, null));
		}

		#endregion

		public void TestCustomField_NeverFallback_Default()
		{
			var template1 = GetTemplate("XXX");
			var template2 = GetTemplate();
			AddCustomField(template1, "Field1");
			AddCustomField(template2, "Field2");
			Factory.Save();

			AssertFieldExists(GetDummy("XXX"), "Field1");
			AssertFieldDoesNotExist(GetDummy("XXX"), "Field2");
			AssertFieldDoesNotExist(GetDummy(""), "Field1");
			AssertFieldExists(GetDummy(""), "Field2");
		}

		public void TestCustomField_AlwaysFallback()
		{
			var template1 = GetTemplate("XXX");
			template1.P0_CustomFieldFallback = FallbackTypeList.Codes.AlwaysFallback;
			var template2 = GetTemplate();
			AddCustomField(template1, "Field1");
			AddCustomField(template2, "Field2");
			Factory.Save();

			AssertFieldExists(GetDummy("XXX"), "Field1");
			AssertFieldExists(GetDummy("XXX"), "Field2");
		}

		public void TestCustomField_MergeIdenticalFields()
		{
			var template1 = GetTemplate("XXX");
			template1.P0_CustomFieldFallback = FallbackTypeList.Codes.AlwaysFallback;
			var template2 = GetTemplate();
			AddCustomField(template1, "Field1");
			AddCustomField(template2, "Field1");
			Factory.Save();

			AssertFieldExists(GetDummy("XXX"), "Field1");
		}

		public void TestCustomField_AlwaysFallbackUntilNeverFallback()
		{
			var template1 = GetTemplate("XXX", "YYY");
			template1.P0_CustomFieldFallback = FallbackTypeList.Codes.AlwaysFallback;
			var template2 = GetTemplate("XXX");
			var template3 = GetTemplate();
			AddCustomField(template1, "Field1");
			AddCustomField(template2, "Field2");
			AddCustomField(template3, "Field3");
			Factory.Save();

			AssertFieldExists(GetDummy("XXX", "YYY"), "Field1");
			AssertFieldExists(GetDummy("XXX", "YYY"), "Field2");
			AssertFieldDoesNotExist(GetDummy("XXX", "YYY"), "Field3");
		}

		public void TestCustomField_CheckAddOnRule()
		{
			var template1 = GetTemplate("XXX", "YYY");
			template1.P0_CustomFieldFallback = FallbackTypeList.Codes.AlwaysFallback;
			var template2 = GetTemplate("XXX");
			var addOnRule1 = SetNewAddOnRule(AddCustomField(template1, "Field1"), "R1", "MIN");
			var addOnRule2 = SetNewAddOnRule(AddCustomField(template2, "Field1"), "R2", "BOG");
			Factory.Save();

			AssertAddOnRuleList(GetDummy("XXX", "YYY"), "Field1", "MIN");
			AssertAddOnRuleList(GetDummy("XXX"), "Field1", "BOG");
		}

		public void TestCustomField_AddOnRuleValidation()
		{
			var fieldName = "FunkBonger";
			var fieldAlias = "__FUNKBONGER__prop__ZString";
			var template = GetTemplate("XXX");
			var stringField = MasterFilesTestHelper.CreateCustomField(template, fieldName);
			stringField.XC_XR = MasterFilesTestHelper.CreateAddOnRule(Factory).PK;
			var invalidCode = stringField.CustomAddOnRule.AllRules.Cast<AvailableRule>().Single(r => r.Rule is InvalidCodeRule);
			invalidCode.IsEnabled = true;
			var list = new CodeDescriptionPairList();
			list.AddPair("NONG", "PONG");
			((InvalidCodeRule)invalidCode.Rule).List = list;

			Factory.Save();

			var customFieldProvider = (ICustomFieldProvider)GetDummy("XXX");

			Factory.Save();

			var cusObj = customFieldProvider.GetCustomBusinessObject();
			cusObj[fieldAlias] = "GOBB";
			var info = cusObj.ZPropertyInfoHash.GetPropertySafe(fieldAlias);
			AssertHasError(info, "Enter a valid FunkBonger.");
			cusObj[fieldAlias] = "NONG";
			AssertNoErrors(info);
			cusObj[fieldAlias] = "";
			AssertNoErrors(info);
		}

		public void TestCustomField_ValueChangeTriggersEDT()
		{
			DummyWithWorkflow.AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLogged;

			var fieldName = "FunkBonger";
			var fieldAlias = "__FUNKBONGER__prop__ZString";

			var template = CustomFieldProcessTaskTemplateTestHelper.GetTemplate(Factory, "XXX");
			var stringField = MasterFilesTestHelper.CreateCustomField(template, fieldName);

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithCustomFields>();
			dummy.SubType1 = "XXX";
			dummy.Z0_Code = "";

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			dummy.GetCustomBusinessObject()[fieldAlias] = "FunkBongerValue";

			Factory.Save();

			dummy.GetCustomBusinessObject()[fieldAlias] = "FunkBongerValue2";

			Factory.Save();

			AssertEquals("Trigger should have been fired", 0, (int)trigger.TriggerConditions.TriggerFiredCountdown);
		}
	}
}

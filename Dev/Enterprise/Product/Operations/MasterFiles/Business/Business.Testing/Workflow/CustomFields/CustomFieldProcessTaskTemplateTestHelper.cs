using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class CustomFieldProcessTaskTemplateTestHelper
	{
		public static DummyWithCustomFields GetDummy(BusinessObjectFactory factory, string subtype1 = "", string subtype2 = "")
		{
			var dummy = factory.NewWithValidTestData<DummyWithCustomFields>();
			dummy.SubType1 = subtype1;
			dummy.Z0_Code = subtype2;
			return dummy;
		}

		public static ProcessTaskTemplate GetTemplate(BusinessObjectFactory factory, string subtype1 = "", string subtype2 = "")
		{
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_CustomFieldFallback = FallbackTypeList.Codes.NeverFallback;
			template.P0_SubType1 = subtype1;
			template.P0_SubType2 = subtype2;

			return template;
		}

		public static GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, string name, string type = "STR", GenCustomAddOnRule addOnRule = null)
		{
			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = name;
			def.XC_Type = type;

			if (addOnRule != null)
			{
				def.XC_XR = addOnRule.PK;
			}

			return def;
		}

		public static GenCustomAddOnRule AddAddOnRules(BusinessObjectFactory factory, string ruleName, string rulesAsXML)
		{
			GenCustomAddOnRule addOnRule = factory.New<GenCustomAddOnRule>();
			addOnRule.XR_Code = ruleName;
			addOnRule.XR_SourceCode = rulesAsXML;
			return addOnRule;
		}
	}
}

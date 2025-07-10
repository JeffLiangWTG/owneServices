using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTemplateValidation))]
	sealed class ProcessTemplateValidationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFieldToDisplayValidationStaticInfo() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			AssertNull("Empty P0V_FieldToDisplayValidation", templateValidation.FieldToDisplayValidationStaticInfo);
			templateValidation.P0V_FieldToDisplayValidation = "JE_GoodsDescription";
			AssertNull("null WorkflowDescriptor", templateValidation.FieldToDisplayValidationStaticInfo);
			template.P0_ProcessType = "BRK";
			AssertNotNull("Bingo", templateValidation.FieldToDisplayValidationStaticInfo);
		});

		public void TestHasValidationAction() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			AssertEquals("ActionSourceCode is empty", false, templateValidation.HasValidationAction(""));
			templateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
			AssertEquals("SAV exists", true, templateValidation.HasValidationAction("SAV"));
			AssertEquals("FSV not exists", false, templateValidation.HasValidationAction("FSV"));
		});

		public void TestGetValidationAction() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			AssertNull("ActionSourceCode is empty", templateValidation.GetValidationAction(""));
			templateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
			AssertNotNull("SAV exists", templateValidation.GetValidationAction("SAV"));
			AssertNull("FSV not exists", templateValidation.GetValidationAction("FSV"));
		});

		public void TestGetValidationToolChecker() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			var entity = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			AssertNull("null WorkflowDescriptor", templateValidation.GetValidationToolChecker(entity));
			template.P0_ProcessType = "BRK";
			AssertNotNull("Bingo", templateValidation.GetValidationToolChecker(entity));
		});

		public void TestIProcessTemplateValidation() => CombineAssertions(() =>
		{
			var company = Factory.New<GlbCompany>();
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			var requestType = Factory.New<ExternalRequestType>();
			templateValidation.P0V_Condition1 = "CN1";
			templateValidation.P0V_Condition2 = "CN2";
			templateValidation.P0V_Condition2Value = "2V";
			templateValidation.P0V_GC_Company = company.PK;
			templateValidation.P0V_ValidationRule = "Rule Body";
			templateValidation.P0V_Severity = "ERR";
			templateValidation.P0V_Message = "Rule Message";
			templateValidation.P0V_FieldToDisplayValidation = "Rule Field";
			templateValidation.P0V_Description = "Rule Description";
			templateValidation.P0V_LogValidationFailEvent = ZBool.True;
			templateValidation.P0V_ContextType = "POR";
			templateValidation.P0V_RQT_RequestTypeOnFailure = requestType.PK;

			var provider = (IProcessTemplateValidation)templateValidation;
			AssertEquals("P0V_Condition1", "CN1", provider.P0V_Condition1);
			AssertEquals("P0V_Condition2", "CN2", provider.P0V_Condition2);
			AssertEquals("P0V_Condition2Value", "2V", provider.P0V_Condition2Value);
			AssertEquals("P0V_GC_Company", company.PK, provider.P0V_GC_Company);
			AssertEquals("P0V_ValidationRule", "Rule Body", provider.P0V_ValidationRule);
			AssertEquals("P0V_Severity", "ERR", provider.P0V_Severity);
			AssertEquals("P0V_Message", "Rule Message", provider.P0V_Message);
			AssertEquals("P0V_FieldToDisplayValidation", "Rule Field", provider.P0V_FieldToDisplayValidation);
			AssertEquals("P0V_Description", "Rule Description", provider.P0V_Description);
			AssertEquals("P0_GC", template.P0_GC, provider.P0_GC);
			AssertEquals("P0V_LogValidationFailEvent", templateValidation.P0V_LogValidationFailEvent, provider.P0V_LogValidationFailEvent);
			AssertEquals("P0V_ContextType", "POR", templateValidation.P0V_ContextType);
			AssertEquals("P0V_RQT_RequestTypeOnFailure", requestType.PK, templateValidation.RequestTypeOnFailure.PK);
		});

		public void TestTemplateCountryCode() => CombineAssertions(() =>
		{
			var templateValidation = Factory.New<ProcessTemplateValidation>();
			AssertNullOrEmpty("Without P0V_P0_Parent", templateValidation.TemplateCountryCode);

			var template = Factory.New<ProcessTaskTemplate>();
			templateValidation.P0V_P0_WorkflowTemplate = template.PK;
			AssertEquals("All set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, templateValidation.TemplateCountryCode);
		});

		public void TestPropertyCaptions() => CombineAssertions(() =>
		{
			var testedObject = Factory.New<ProcessTemplateValidation>();
			AssertPropertyCaption(testedObject.P0V_DescriptionInfo, "Rule Description", expectedShortCaption: "Desc.");
			AssertPropertyCaption(testedObject.P0V_Condition1Info, "Condition 1", expectedShortCaption: "Cond. 1");
			AssertPropertyCaption(testedObject.P0V_Condition2Info, "Condition 2", expectedShortCaption: "Cond. 2");
			AssertPropertyCaption(testedObject.P0V_Condition2ValueInfo, "Condition 2 Value", expectedShortCaption: "Cond.2 Value ");
			AssertPropertyCaption(testedObject.P0V_GC_CompanyInfo, "Company", expectedShortCaption: "Company");
			AssertPropertyCaption(testedObject.P0V_MessageInfo, "Validation Message", expectedShortCaption: "Val. Msg.");
			AssertPropertyCaption(testedObject.P0V_SeverityInfo, "Severity", expectedShortCaption: "Sev.");
			AssertPropertyCaption(testedObject.P0V_ValidationRuleInfo, "Validation Rule", expectedShortCaption: "Val. Rule");
			AssertPropertyCaption(testedObject.P0V_FieldToDisplayValidationInfo, "Display Validation On", expectedShortCaption: "Disp. Val.");
			AssertPropertyCaption(testedObject.P0V_LogValidationFailEventInfo, "Create Event On Failure");
		});

		static void AssertPropertyCaption(ZPropertyInfo propertyInfo, string expectedCaption, string expectedShortCaption = "")
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertEquals($"{propertyInfo.Name} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
		}

		public void TestP0V_Condition2AndP0V_Condition2Value() => CombineAssertions(() =>
		{
			ProcessTemplateValidation.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			Assert("Condition2Value should be not Readonly", !ProcessTemplateValidation.P0V_Condition2ValueInfo.ReadOnly);
			ProcessTemplateValidation.P0V_Condition2Value = "Condition2Value";

			ProcessTemplateValidation.P0V_Condition2 = "UNK";
			Assert("Condition2Value should be Readonly", ProcessTemplateValidation.P0V_Condition2ValueInfo.ReadOnly);
			Assert("Condition2Value should be Empty", ProcessTemplateValidation.P0V_Condition2Value.IsEmpty);

			ProcessTemplateValidation.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			Assert("Condition2Value should be not Readonly", !ProcessTemplateValidation.P0V_Condition2ValueInfo.ReadOnly);
			AssertEquals("Condition2Value should be 'Condition2Value'", "Condition2Value", ProcessTemplateValidation.P0V_Condition2Value);
		});

		public void TestIAntlrMacroContextProvider() => CombineAssertions(() =>
		{
			var shipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var auDeclarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			var baseDeclarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			template.P0_ProcessType = "BRK";

			var contextProvider = (IAntlrMacroContextProvider)templateValidation;

			template.P0_GC = CargoWise.Types.ZGuid.Empty;
			AssertAntlrMacroContextProvider("BRK, P0_GC is Empty", baseDeclarationType);

			template.P0_GC = GlbCompany.CurrentCompany.PK;
			AssertAntlrMacroContextProvider("BRK, P0_GC is AU", auDeclarationType);

			template.P0_ProcessType = "SHP";
			AssertAntlrMacroContextProvider("SHP", shipmentType);

			templateValidation.P0V_Condition1 = "BRK";
			AssertAntlrMacroContextProvider("SHP with BRK, P0V_GC_Company is empty", auDeclarationType);

			templateValidation.P0V_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertAntlrMacroContextProvider("SHP with BRK, P0V_GC_Company is AU", auDeclarationType);

			return;

			void AssertAntlrMacroContextProvider(string message, Type expectedParentType)
			{
				var antlrMacroContext = contextProvider.GetSampleContext();
				AssertEquals($"{message}, ParentType", expectedParentType, antlrMacroContext.ParentType);
				AssertEquals($"{message}, Parent", true, ((BusinessObject)antlrMacroContext.Parent).IsNull);
			}
		});

		public void TestLookups()
		{
			AssertType<ProcessTemplateValidationLookups>(ProcessTemplateValidation.Lookups);
		}

		public void TestValidation()
		{
			AssertType<ProcessTemplateValidationValidation>(ProcessTemplateValidation.Validation);
		}

		public void TestProcessTemplateValidationActions()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			AssertEquals(true, templateValidation.IsRegisteredEditableChildObject(templateValidation.ProcessTemplateValidationActions));
		}

		public void TestDelete() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var templateValidation = template.ProcessTemplateValidations.AddNew();
			var templateValidationAction1 = templateValidation.ProcessTemplateValidationActions.AddNew();
			var templateValidationAction2 = templateValidation.ProcessTemplateValidationActions.AddNew();

			templateValidation.Delete();

			AssertEquals("Validation", true, templateValidation.IsDeleted);
			AssertEquals("Action 1", true, templateValidationAction1.IsDeleted);
			AssertEquals("Action 2", true, templateValidationAction2.IsDeleted);
		});

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var processTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
			var result = processTaskTemplate.ProcessTemplateValidations.AddNew();
			result.P0V_Description = "Description";
			return result;
		}

		ProcessTemplateValidation processTemplateValidation;
		ProcessTemplateValidation ProcessTemplateValidation => processTemplateValidation ??= Factory.New<ProcessTemplateValidation>();
	}
}

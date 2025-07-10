using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTemplateValidationCollection))]
	sealed class ProcessTemplateValidationCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTemplateValidationCollection>
	{
		public void TestJobApplied()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_Name = "T1";
			template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template1.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateValidation11 = template1.ProcessTemplateValidations.AddNew();
			var processTemplateValidationAction11 = templateValidation11.ProcessTemplateValidationActions.AddNew();
			processTemplateValidationAction11.P0A_ActionSource = "SAV";
			templateValidation11.P0V_Description = "D1";
			templateValidation11.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			templateValidation11.P0V_Condition2Value = "\"<Z0_BitTrue>\" == \"Y\"";
			templateValidation11.P0V_Severity = "ERR";
			templateValidation11.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
			templateValidation11.P0V_FieldToDisplayValidation = "Z0_BitTrue";
			templateValidation11.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

			var templateValidation12 = template1.ProcessTemplateValidations.AddNew();
			var processTemplateValidationAction12 = templateValidation12.ProcessTemplateValidationActions.AddNew();
			processTemplateValidationAction12.P0A_ActionSource = "SAV";
			templateValidation12.P0V_Description = "D2";
			templateValidation12.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			templateValidation12.P0V_Condition2Value = "\"<Z0_BitTrue>\" == \"N\"";
			templateValidation12.P0V_Severity = "WRN";
			templateValidation12.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
			templateValidation12.P0V_FieldToDisplayValidation = "Z0_BitTrue";
			templateValidation12.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"N\"";

			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_Name = "T2";
			template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template2.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateValidation21 = template2.ProcessTemplateValidations.AddNew();
			var processTemplateValidationAction21 = templateValidation21.ProcessTemplateValidationActions.AddNew();
			processTemplateValidationAction21.P0A_ActionSource = "SAV";
			templateValidation21.P0V_Description = "D3";
			templateValidation21.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			templateValidation21.P0V_Condition2Value = "\"<Z0_BitTrue>\" == \"Y\"";
			templateValidation21.P0V_Severity = "ERR";
			templateValidation21.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
			templateValidation21.P0V_FieldToDisplayValidation = "Z0_BitTrue";
			templateValidation21.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

			var templateValidation22 = template2.ProcessTemplateValidations.AddNew();
			var processTemplateValidationAction22 = templateValidation22.ProcessTemplateValidationActions.AddNew();
			processTemplateValidationAction22.P0A_ActionSource = "SAV";
			templateValidation22.P0V_Description = "D4";
			templateValidation22.P0V_Condition2 = ProcessTasksLookups.MacroCondition;
			templateValidation22.P0V_Condition2Value = "\"<Z0_BitTrue>\" == \"N\"";
			templateValidation22.P0V_Severity = "WRN";
			templateValidation22.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
			templateValidation22.P0V_FieldToDisplayValidation = "Z0_BitTrue";
			templateValidation22.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"N\"";

			Factory.Save();

			var dummyObject = Factory.New<DummyWithWorkflow>();
			dummyObject.Z0_BitTrue = ZBool.True;
			var jobApplied = new ProcessTemplateValidationCollection(dummyObject);
			AssertContainsExactElementsInAnyOrder(new[] { templateValidation11, templateValidation21 }, jobApplied);
		}

		protected override ProcessTemplateValidationCollection GetCollectionToTest()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			return new ProcessTemplateValidationCollection(template);
		}
	}
}

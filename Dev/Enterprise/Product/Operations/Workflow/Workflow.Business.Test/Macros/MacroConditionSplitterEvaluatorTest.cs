using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class MacroConditionSplitterEvaluatorTest : TestCaseWithFactory
	{
		public void TestSimpleConditions()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Import";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "1 == 1 && 2 == 2";

			Assert("Condition is met", dummy.WorkflowItems.IsCondition2Met(milestone));
		}

		public void TestDataFieldConditions()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Import";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "Z0_Code == \"ABC\" && Z0_Description == \"123\"";
			dummy.Z0_Code = "ABC";
			dummy.Z0_Description = "123";

			Assert("Condition is met", dummy.WorkflowItems.IsCondition2Met(milestone));
		}

		public void TestMacroFunctionConditions()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Import";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "Sum([1, 2, 3]) == Sum([3, 2, 1]) && EndsWith(\"ABC\", \"C\")";

			Assert("Condition is met", dummy.WorkflowItems.IsCondition2Met(milestone));
		}
	}
}

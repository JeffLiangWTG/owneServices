using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class ReapplyWorkflowTemplatesOptionsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateReapplyWorkflowAndTasksOptions()
		{
			var obj = new ReapplyWorkflowTemplateUserOptions(new ReapplyWorkflowTemplateInGUIConfiguration());
			obj.ReapplyWorkflowAndTasksOptions = "InValide";
			AssertHasErrors(obj.ReapplyWorkflowAndTasksOptionsInfo);

			obj.ReapplyWorkflowAndTasksOptions = ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply;
			AssertNoErrors(obj.ReapplyWorkflowAndTasksOptionsInfo);
		}

		public void TestValidateReapplyMilestonesOptions()
		{
			var obj = new ReapplyWorkflowTemplateUserOptions(new ReapplyWorkflowTemplateInGUIConfiguration());
			obj.ReapplyMilestonesOptions = "InValide";
			AssertHasErrors(obj.ReapplyMilestonesOptionsInfo);

			obj.ReapplyMilestonesOptions = ReapplyMilestonesOptionsList.Codes.Exclude;
			AssertNoErrors(obj.ReapplyMilestonesOptionsInfo);
		}

		public void TestValidateReapplyTriggersOptions()
		{
			var obj = new ReapplyWorkflowTemplateUserOptions(new ReapplyWorkflowTemplateInGUIConfiguration());
			obj.ReapplyTriggersOptions = "InValide";
			AssertHasErrors(obj.ReapplyTriggersOptionsInfo);

			obj.ReapplyTriggersOptions = ReapplyTriggersOptionsList.Codes.Exclude;
			AssertNoErrors(obj.ReapplyTriggersOptionsInfo);
		}
	}
}

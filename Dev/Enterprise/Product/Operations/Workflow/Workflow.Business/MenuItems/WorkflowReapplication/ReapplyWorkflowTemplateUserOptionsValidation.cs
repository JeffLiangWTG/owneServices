using System;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class ReapplyWorkflowTemplateUserOptionsValidation : ZValidation
	{
		readonly ReapplyWorkflowTemplateUserOptions parent;

		public ReapplyWorkflowTemplateUserOptionsValidation(ReapplyWorkflowTemplateUserOptions parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override Type AutoValidationType => typeof(ReapplyWorkflowTemplateUserOptions);

		public override void ValidateAll()
		{
			ValidateReapplyWorkflowAndTasksOptions();
			ValidateReapplyMilestonesOptions();
			ValidateReapplyTriggersOptions();
		}

		public void ValidateReapplyWorkflowAndTasksOptions()
		{
			ValidateCalculatedProperty(parent.ReapplyWorkflowAndTasksOptionsInfo);
		}

		protected void CheckReapplyWorkflowAndTasksOptions()
		{
			ListValidation.ErrorIfInvalidCode(parent.ReapplyWorkflowAndTasksOptionsInfo);
		}

		public void ValidateReapplyMilestonesOptions()
		{
			ValidateCalculatedProperty(parent.ReapplyMilestonesOptionsInfo);
		}

		protected void CheckReapplyMilestonesOptions()
		{
			ListValidation.ErrorIfInvalidCode(parent.ReapplyMilestonesOptionsInfo);
		}

		public void ValidateReapplyTriggersOptions()
		{
			ValidateCalculatedProperty(parent.ReapplyTriggersOptionsInfo);
		}

		protected void CheckReapplyTriggersOptions()
		{
			ListValidation.ErrorIfInvalidCode(parent.ReapplyTriggersOptionsInfo);
		}
	}
}

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowMilestonesController))]
	sealed class WorkflowMilestonesControllerTest : WorkflowControllerTestBase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WorkflowMilestones;
		}

		protected override ProcessTask AddWorkflowItemToParent(IWorkflowProvider parent)
		{
			return parent.WorkflowItems.Milestones.AddNew();
		}
	}
}

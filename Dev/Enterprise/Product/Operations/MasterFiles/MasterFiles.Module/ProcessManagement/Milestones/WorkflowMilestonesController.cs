using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowMilestonesController : WorkflowControllerBase
	{
		public WorkflowMilestonesController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WorkflowMilestones;
		public override ControllerID ID => ControllerIDs.WorkflowMilestones;
	}
}

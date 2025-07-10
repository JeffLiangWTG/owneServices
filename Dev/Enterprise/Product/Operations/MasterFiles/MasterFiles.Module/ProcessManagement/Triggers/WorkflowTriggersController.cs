using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowTriggersController : WorkflowControllerBase
	{
		public WorkflowTriggersController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WorkflowTriggers;
		public override ControllerID ID => ControllerIDs.WorkflowTriggers;
	}
}

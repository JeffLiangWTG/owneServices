using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowExceptionsController : WorkflowControllerBase
	{
		public WorkflowExceptionsController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WorkflowExceptions;
		public override ControllerID ID => ControllerIDs.WorkflowExceptions;
	}
}

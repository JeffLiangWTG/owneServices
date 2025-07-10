using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TaskTrialAssignerBWFTest : TaskTrialAssignerTestCase
	{
		protected override string WorkflowManagementMode => WorkflowManagementModes.Codes.BasicWorkflow;

		// the only supported restriction scope for WorkflowManagementModes.Codes.BasicWorkflow is Job
		protected override ZString RestrictionScope => ScopeList.Codes.Job;
	}
}

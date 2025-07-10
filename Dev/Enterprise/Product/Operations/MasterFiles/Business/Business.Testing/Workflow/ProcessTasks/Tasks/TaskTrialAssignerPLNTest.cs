using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TaskTrialAssignerPLNTest : TaskTrialAssignerTestCase
	{
		protected override string WorkflowManagementMode => WorkflowManagementModes.Codes.PlanningManagement;

		protected override ZString RestrictionScope => ScopeList.Codes.Workflow;
	}
}

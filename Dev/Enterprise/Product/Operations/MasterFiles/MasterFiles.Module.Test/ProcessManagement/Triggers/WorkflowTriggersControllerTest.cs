using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowTriggersController))]
	sealed class WorkflowTriggersControllerTest : WorkflowControllerTestBase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WorkflowTriggers;
		}

		protected override ProcessTask AddWorkflowItemToParent(IWorkflowProvider parent)
		{
			return parent.WorkflowItems.Triggers.AddNew();
		}
	}
}

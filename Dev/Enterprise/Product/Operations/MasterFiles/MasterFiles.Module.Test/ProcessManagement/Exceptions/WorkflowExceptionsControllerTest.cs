using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowExceptionsController))]
	sealed class WorkflowExceptionsControllerTest : WorkflowControllerTestBase
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WorkflowExceptions;
		}

		protected override ProcessTask AddWorkflowItemToParent(IWorkflowProvider parent)
		{
			return parent.WorkflowItems.Exceptions.AddNew();
		}
	}
}

using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Workflow.Triggers;

namespace Enterprise.MasterFiles.Business
{
	public class CanTriggerActionRunAgain : ICanTriggerActionRunAgain
	{
		public ZBool CanRunAgain(ITriggerAction action)
		{
			switch (action.ActionType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange:
					return true;
				default:
					return false;
			}
		}
	}
}

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowTriggerActionValidator
	{
		bool ShouldValidate(IBaseTrigger trigger, ITriggerAction action, IQueuedLog log, IStmALog @event, IWorkflowDescriptor workflowDescriptor);

		bool IsValid(IBaseTrigger trigger, ITriggerAction action, IQueuedLog log, IStmALog @event, IWorkflowDescriptor workflowDescriptor, BusinessObject bizo, INotifications notifications);
	}
}

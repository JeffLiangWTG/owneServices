using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer
{
	public class XudTriggerActionValidator : IWorkflowTriggerActionValidator
	{
		public bool IsValid(IBaseTrigger trigger, ITriggerAction action, IQueuedLog log, IStmALog @event, IWorkflowDescriptor workflowDescriptor, BusinessObject bizo, INotifications notifications)
		{
			if (!(bizo is IDocManagerSupport docManagerSupport))
			{
				notifications.AddWarning(Res.GetString("cefc9e28-dbc3-46d9-9af0-dc481a426579", "Ignoring invalid XUD trigger action: Business object does not support eDocs", bizo.GetType().Name));
				return false;
			}

			if (@event == null)
			{
				notifications.AddWarning(Res.GetString("0F655DC7-F4C5-4E9D-A95E-6CBD25F9C1F6", "Ignoring invalid XUD trigger action: Could not load triggering event", bizo.GetType().Name));
				return false;
			}

			var storageDocsPK = StmALog.GetGuid(@event.SL_Reference);
			if (!storageDocsPK.IsValid || EventDataObjectWriter.FindEDocs(docManagerSupport, storageDocsPK.ToGuid()) == null)
			{
				notifications.AddWarning(Res.GetString("85e50cce-dc07-473f-8f42-873575ebb1b3", "Ignoring invalid XUD trigger action: Business object has no matching eDocs", bizo.GetType().Name));
				return false;
			}

			return true;
		}

		public bool ShouldValidate(IBaseTrigger trigger, ITriggerAction action, IQueuedLog log, IStmALog @event, IWorkflowDescriptor workflowDescriptor)
		{
			return action.ActionType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
		}
	}
}

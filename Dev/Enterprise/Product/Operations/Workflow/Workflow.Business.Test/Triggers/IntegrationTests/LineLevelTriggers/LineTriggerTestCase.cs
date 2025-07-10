using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business.Test.Triggers.LineTriggers.IntegrationTests
{
	abstract class LineTriggerTestCase : TriggerTestCase
	{
		protected void SetupFieldNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			notification.PQ_EmailAddr = "d@d.com";
			notification.PQ_FieldName = $"<{FieldNameToUpdate}>";
			notification.PQ_FieldValue = "TES";
		}

		protected abstract string FieldNameToUpdate { get; }
	}
}

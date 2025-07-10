using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business.Test.Triggers
{
	abstract class TriggerTestCase : WorkflowTestCase
	{
		protected ProcessTask CreateTriggerInNewFactory(string processType, string lineTriggerType, string eventCode)
		{
			var newFactory = new BusinessObjectFactory();
			var lineTriggerTemplate = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			lineTriggerTemplate.P0_ProcessType = processType;
			var templateTask = lineTriggerTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.P9_LineTriggerType = lineTriggerType;
			templateTask.TriggerConditions.TriggerEventCode = eventCode;
			return templateTask;
		}

		protected void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "bung@bung.bung";
			notification.Factory.Save();
		}
	}
}

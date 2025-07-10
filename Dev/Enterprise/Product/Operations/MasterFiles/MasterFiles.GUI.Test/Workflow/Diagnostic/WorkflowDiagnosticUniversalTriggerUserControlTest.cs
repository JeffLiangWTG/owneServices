using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(WorkflowDiagnosticForm))]
	sealed class WorkflowDiagnosticUniversalTriggerUserControlTest : WorkflowDiagnosticUserControlTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(DummyWorkflowDescriptor.Instance);

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.IsWorkflowTrigger = true;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "test@test.com";
		}
	}
}

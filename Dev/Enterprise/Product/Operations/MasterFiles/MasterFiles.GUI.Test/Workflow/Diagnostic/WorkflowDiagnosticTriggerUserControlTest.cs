using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(WorkflowDiagnosticForm))]
	sealed class WorkflowDiagnosticTriggerUserControlTest : WorkflowDiagnosticUserControlTest
	{
		protected override DummyWithWorkflow GetDummy()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM");
			universalTemplate.P0_Description = "Bob";
			var trigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			trigger.Description = "Trigger 1";
			trigger.TriggerEventCode = Events.AuthorisedCode;

			var triggerAction = (ProcessTaskNotification)trigger.TriggerActions.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "test@test.com";
			Factory.Save();

			return base.GetDummy();
		}

		public void TestPrecondition()
		{
			AssertEquals(1, Dummy.WorkflowItems.TriggersIncludingRelated.Count);
		}
	}
}

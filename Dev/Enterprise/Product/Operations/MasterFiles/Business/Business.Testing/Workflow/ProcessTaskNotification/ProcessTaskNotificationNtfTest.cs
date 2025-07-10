using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTask_ProcessTaskNotificationNtfTest : TestCaseWithFactory
	{
		public void TestNotificationSentToMultipleRecipients()
		{
			void TestCase(string notificationType)
			{
				var dummy = Factory.New<DummyWithWorkflow>();

				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "TestDesc";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = notificationType;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				notification.PQ_EmailAddr = "hey@you.com,out@there.in,  the@cold.com		";
				notification.PQ_EmailText = "Test";

				dummy.Logs.AddNew(Events.CustomisableEvent00);

				AssertNoErrors(notification.PQ_EmailAddrInfo);

				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				AssertEquals(1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(3, email.Recipients.Count);
				AssertCollectionContains("hey@you.com", email.Recipients);
				AssertCollectionContains("out@there.in", email.Recipients);
				AssertCollectionContains("the@cold.com", email.Recipients);
				Environment.Env.ClearAllEmailsCreated();
			}

			TestCase(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);
			TestCase(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail);
		}

		public void TestNotificationBodyEmailSendsTextOnly()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TestDesc";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "hey@you.com";
			notification.PQ_EmailText = "Test";

			dummy.Logs.AddNew(Events.CustomisableEvent00);

			AssertNoErrors(notification.PQ_EmailAddrInfo);

			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			AssertEquals(1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Recipients.Count);
			AssertEquals(0, email.Attachments.Count);
			AssertNotContains("Banner.jpg", email.Body);
			AssertNotContains("Footer.jpg", email.Body);
		}
	}
}

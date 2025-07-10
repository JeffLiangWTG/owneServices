using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonWorkflowDescriptor))]
	sealed class GlbPersonWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbPersonWorkflowDescriptor>
	{
		public void TestSupportedSendDocumentMessageRecipientParties()
		{
			var sendDocumentTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendDocument, null, null);
			AssertEquals(6, sendDocumentTriggerParties.Count);
			AssertEquals("Email", sendDocumentTriggerParties[0].Description);
			AssertEquals("Print", sendDocumentTriggerParties[1].Description);
			AssertEquals("Auto-Deliver using Document Config", sendDocumentTriggerParties[2].Description);
			AssertEquals("Personal Email", sendDocumentTriggerParties[3].Description);
			AssertEquals("Person Primary Work Email", sendDocumentTriggerParties[4].Description);
			AssertEquals("Personal, Fallback Primary Work Email", sendDocumentTriggerParties[5].Description);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Email;
			}
		}
		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.GlbPersonDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Person Intelligence", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.GlbPerson, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestEmailSubject()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();

			var trigger = person.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Complete CCO";
			trigger.TriggerConditions.TriggerEventCode = Events.AccreditationAttemptCompletedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test";

			trigger.Parent.Logs.AddNew(Events.AccreditationAttemptCompleted, "CCO - Certified Operator");

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Complete CCO", email.Subject);
		}

		#region Implementation
		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			GlbPerson glbPerson = Factory.NewWithValidTestData<GlbPerson>();
			return new IWorkflowProvider[] { glbPerson };
		}
		#endregion
	}
}

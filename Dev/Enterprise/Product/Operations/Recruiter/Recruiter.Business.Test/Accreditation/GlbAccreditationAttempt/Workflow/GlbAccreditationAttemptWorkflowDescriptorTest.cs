using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptWorkflowDescriptor))]
	sealed class GlbAccreditationAttemptWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbAccreditationAttemptWorkflowDescriptor>
	{
		public void TestSupportedMessageRecipientPartiesForSpecificAction()
		{
			var sendAccreditationDocumentTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument, null, null);
			AssertEquals(3, sendAccreditationDocumentTriggerParties.Count);
			AssertEquals("Email", sendAccreditationDocumentTriggerParties[0].Description);
			AssertEquals("Person Primary Work Email", sendAccreditationDocumentTriggerParties[1].Description);
			AssertEquals("Personal, Fallback Primary Work Email", sendAccreditationDocumentTriggerParties[2].Description);
		}

		public void TestSupportedSendDocumentMessageRecipientParties()
		{
			var sendDocumentTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendDocument, null, null);
			AssertEquals(5, sendDocumentTriggerParties.Count);
			AssertEquals("Email", sendDocumentTriggerParties[0].Description);
			AssertEquals("Print", sendDocumentTriggerParties[1].Description);
			AssertEquals("Auto-Deliver using Document Config", sendDocumentTriggerParties[2].Description);
			AssertEquals("Person Primary Work Email", sendDocumentTriggerParties[3].Description);
			AssertEquals("Personal, Fallback Primary Work Email", sendDocumentTriggerParties[4].Description);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email;

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Accreditation Attempt", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);
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
			AssertEquals(BusinessContext.AccreditationAttempt, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				result.AddPair(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument, GlbAccreditationAttemptWorkflowTriggerActionTypeList.Descriptions.SendAccreditationDocument);
				return result.Cast<CodeDescriptionPair>().ToArray();
			}
		}

		public void TestTrggerActionType_AccreditationDocument()
		{
			Assert(WorkflowDescriptor.IsDocumentTriggerAction(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument));
			Assert(WorkflowDescriptor.IsEmailSendNotificationTriggerAction(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument));
			Assert(WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument));
		}

		public void TestGetWorkFlowTriggerActionForAccreditationDocument()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			var trigger = attempt.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompleted.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = GlbAccreditationAttemptWorkflowTriggerActionTypeList.Codes.SendAccreditationDocument;
			Factory.Save();

			var triggerLog = trigger.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted);
			Factory.Save();

			var workFlowDescriptor = new GlbAccreditationAttemptWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IGlbAccreditationAttemptDocumentSender", result as GlbAccreditationAttemptDocumentSender);
		}

		public void TestEmailSubject()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();

			var trigger = attempt.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Complete CCO";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "abc@email.com";
			action.PQ_EmailText = "Test";

			trigger.Parent.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var email = Environment.Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Complete CCO", email.Subject);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var attempt = Factory.NewWithValidTestData<GlbAccreditationAttempt>();
			return new IWorkflowProvider[] { attempt };
		}
	}
}

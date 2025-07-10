using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestWorkflowDescriptor))]
	class WorkRequestWorkflowDescriptorTest : WorkflowDescriptorTestCase<WorkRequestWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Customer Service Ticket", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("CST", WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public void TestPortName()
		{
			AssertEquals("Country/Region", WorkflowDescriptor.Port1Name);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		[ExpectNoExceptions]
		public void TestAddEConversationMessageTriggerActionNoFactorySaveAlterIssue()
		{
			MasterFilesTestHelper.EnableFactorySaveAlerterInTesting = true;
			MasterFilesTestHelper.ClearWorkflowTables();

			var workflowProvider = GetParentsWithConfiguredOrganisationPartiesForTest().First();
			var bizo = (BusinessObject)workflowProvider;
			var trigger = MasterFilesTestHelper.CreateTrigger(workflowProvider, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, emailText: "Test AddEConversationMessageTriggerAction");
			Factory.Save();
			bizo.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		public override void TestSubTypes()
		{
			var matchingBranch = Factory.NewWithValidTestData<GlbBranch>();
			var matchingDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var nonMatchingBranch = Factory.NewWithValidTestData<GlbBranch>();
			var nonMatchingDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var template1 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template1", FallbackTypeList.Codes.AlwaysFallback);
			var template2 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template2", FallbackTypeList.Codes.AlwaysFallback, "AAA");
			var template3 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template3", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA");
			var template4 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template4", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA");
			var template5 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template5", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA", "AAA");
			var template6 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template6", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA", "AAA", "AAA");
			var template7 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template7", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch);
			var template8 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template8", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch, matchingDepartment);
			var template9 = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template9", FallbackTypeList.Codes.AlwaysFallback, "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch, matchingDepartment, "AU");

			ProcessMgmtTestHelper.CreateTemplateTask(template1, "template1 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template2, "template2 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template3, "template3 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template4, "template4 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template5, "template5 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template6, "template6 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template7, "template7 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template8, "template8 task");
			ProcessMgmtTestHelper.CreateTemplateTask(template9, "template9 task");

			Factory.Save();

			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request1");
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request2", "AAA");
			var request3 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request3", "AAA", "AAA");
			var request4 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request4", "AAA", "AAA", "AAA");
			var request5 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request5", "AAA", "AAA", "AAA", "AAA");
			var request6 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request6", "AAA", "AAA", "AAA", "AAA", "AAA");
			var request7 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request7", "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch);
			var request8 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request8", "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch, matchingDepartment);
			var request9 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "request9", "AAA", "AAA", "AAA", "AAA", "AAA", matchingBranch, matchingDepartment, "AU");

			var nonMatchingRequest1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest1", "BBB");
			var nonMatchingRequest2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest2", "BBB", "BBB");
			var nonMatchingRequest3 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest3", "BBB", "BBB", "BBB");
			var nonMatchingRequest4 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest4", "BBB", "BBB", "BBB", "BBB");
			var nonMatchingRequest5 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest5", "BBB", "BBB", "BBB", "BBB", "BBB");
			var nonMatchingRequest6 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest6", "BBB", "BBB", "BBB", "BBB", "BBB", nonMatchingBranch);
			var nonMatchingRequest7 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest7", "BBB", "BBB", "BBB", "BBB", "BBB", matchingBranch, nonMatchingDepartment);
			var nonMatchingRequest8 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "nonMatchingRequest8", "BBB", "BBB", "BBB", "BBB", "BBB", matchingBranch, matchingDepartment, "NZ");

			Factory.Save();

			AssertHasTasksByDescription(request1, "template1 task");
			AssertHasTasksByDescription(request2, "template1 task", "template2 task");
			AssertHasTasksByDescription(request3, "template1 task", "template2 task", "template3 task");
			AssertHasTasksByDescription(request4, "template1 task", "template2 task", "template3 task", "template4 task");
			AssertHasTasksByDescription(request5, "template1 task", "template2 task", "template3 task", "template4 task", "template5 task");
			AssertHasTasksByDescription(request6, "template1 task", "template2 task", "template3 task", "template4 task", "template5 task", "template6 task");
			AssertHasTasksByDescription(request7, "template1 task", "template2 task", "template3 task", "template4 task", "template5 task", "template6 task", "template7 task");
			AssertHasTasksByDescription(request8, "template1 task", "template2 task", "template3 task", "template4 task", "template5 task", "template6 task", "template7 task", "template8 task");
			AssertHasTasksByDescription(request9, "template1 task", "template2 task", "template3 task", "template4 task", "template5 task", "template6 task", "template7 task", "template8 task", "template9 task");

			AssertHasTasksByDescription(nonMatchingRequest1, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest2, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest3, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest4, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest5, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest6, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest7, "template1 task");
			AssertHasTasksByDescription(nonMatchingRequest8, "template1 task");
		}

		public void TestSubTypeInformation()
		{
			ProcessMgmtTestHelper.SetDummySelectionCriteriaValues();

			AssertEquals("Selection Criterion 1", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Selection Criterion 2", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("Selection Criterion 3", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("Selection Criterion 4", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("Selection Criterion 5", WorkflowDescriptor.SubTypeInformation[4].Description);

			ProcessManagementRegistry.Instance.SelectionCriterion1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 1");
			ProcessManagementRegistry.Instance.SelectionCriterion2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 2");
			ProcessManagementRegistry.Instance.SelectionCriterion3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 3");
			ProcessManagementRegistry.Instance.SelectionCriterion4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 4");
			ProcessManagementRegistry.Instance.SelectionCriterion5Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 5");

			AssertEquals("We are number 1", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("We are number 2", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals("We are number 3", WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertEquals("We are number 4", WorkflowDescriptor.SubTypeInformation[3].Description);
			AssertEquals("We are number 5", WorkflowDescriptor.SubTypeInformation[4].Description);

			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents(WorkflowDescriptor.SubTypeInformation[0].List, Tuple.Create("ENT", "CargoWise Two"), Tuple.Create("GLW", "GLOW"));
			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents(WorkflowDescriptor.SubTypeInformation[1].List, Tuple.Create("PAV", "Productivity Acceleration and Visualisation Engine"), Tuple.Create("PER", "Performance and Deployment"));
			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents(WorkflowDescriptor.SubTypeInformation[2].List, Tuple.Create("BUF", "Buffer Management"), Tuple.Create("APP", "Application Deployment"));
			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents(WorkflowDescriptor.SubTypeInformation[3].List, Tuple.Create("PRD", "Product Enhancement"), Tuple.Create("FIX", "Defect Fix"));
			ProcessMgmtTestHelper.AssertCodeDescriptionPairListContents(WorkflowDescriptor.SubTypeInformation[4].List, Tuple.Create("ALP", "Alpha"), Tuple.Create("GPR", "GPR or is GP1?? There's no way of knowing."));
		}

		static void AssertHasTasksByDescription(WorkRequest request, params string[] taskDescriptions)
		{
			AssertSequencesEqual(taskDescriptions, request.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_Description.ToString()).OrderBy(s => s));
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new[] { ProcessMgmtTestHelper.CreateWorkRequest(Factory) };
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns => WorkRequestSchema.All.Except(new SchemaColumn[]
		{
			WorkRequestSchema.PK,
			WorkRequestSchema.WKR_RequestNumber,
			WorkRequestSchema.WKR_SystemCreateTimeUtc,
			WorkRequestSchema.WKR_SystemCreateUser,
			WorkRequestSchema.WKR_SystemCreateBranch,
			WorkRequestSchema.WKR_SystemCreateDepartment,
			WorkRequestSchema.WKR_SystemLastEditTimeUtc,
			WorkRequestSchema.WKR_SystemLastEditUser,
		}).ToArray();

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties =>
			MessageRecipientPartyType.Client
			| MessageRecipientPartyType.Email
			| MessageRecipientPartyType.NotificationGroup
			| MessageRecipientPartyType.JobLevelWorkflowGroup
			| MessageRecipientPartyType.LastCompletedTaskResource;

		protected override void SetNotificationEmailAddress(BusinessObject job, ProcessTaskNotification notification, string emailAddress)
		{
			if (notification.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Client)
			{
				((WorkRequest)notification.Parent.GetParentBusinessObject()).Client.OC_Email = emailAddress;
			}
			else
			{
				base.SetNotificationEmailAddress(job, notification, emailAddress);
			}
		}

		public void TestExpectedSupportedMessageRecipientParties_WhenBufferManagementDisabled_ShouldIncludeJobLevelWorkflowGroupRecipient()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();

			BMSTestHelper.DisableBMSInRegistry();
			workRequestSystem.Delete();
			Factory.Save();

			AssertEquals(
				MessageRecipientPartyType.Client
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.NotificationGroup
				| MessageRecipientPartyType.JobLevelWorkflowGroup
				| MessageRecipientPartyType.LastCompletedTaskResource,
				WorkflowDescriptor.SupportedMessageRecipientParties(trigger, dummy));

			BMSTestHelper.EnableBMSInRegistry();

			AssertEquals("The registry is enabled, but we haven't yet created a buffer management system for the relevant workflow type",
				MessageRecipientPartyType.Client
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.NotificationGroup
				| MessageRecipientPartyType.JobLevelWorkflowGroup
				| MessageRecipientPartyType.LastCompletedTaskResource,
				WorkflowDescriptor.SupportedMessageRecipientParties(trigger, dummy));

			workRequestSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);
			Factory.Save();

			AssertEquals(
				MessageRecipientPartyType.Client
				| MessageRecipientPartyType.Email
				| MessageRecipientPartyType.NotificationGroup
				| MessageRecipientPartyType.JobLevelWorkflowGroup
				| MessageRecipientPartyType.LastCompletedTaskResource,
				WorkflowDescriptor.SupportedMessageRecipientParties(trigger, dummy));
		}

		public void TestGetMessageRecipientParty_ForClient_ShouldAddContactEmail()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var parties = WorkflowDescriptor.GetMessageRecipientParty(ticket, MessageRecipientPartyTypeList.Codes.Client).ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "JosefJanssen@hunrath.com" }, parties.Select(x => x.FallbackEmail));
		}

		public void TestSupportsWorkflowTriggerActionUniversalActivityXML()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalActivityXML);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return base.ExpectedAdditionalWorkflowTriggerActionTypes.Concat(new[]
				{
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, WorkflowTriggerActionTypeConstants.Descriptions.AddEConversationMessage),
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, WorkflowTriggerActionTypeConstants.Descriptions.AddInternalEConversationMessage),
				}).ToArray();
			}
		}

		protected override SchemaStringColumn TextPropertyWhichCanBeRepresentedByAMacro => WorkRequestSchema.WKR_Summary;

		protected override IEnumerable<string> GetAdditionalEConversationParticipantEmailAddressesAddedByDefault(IWorkflowProvider workflowProvider, bool isForInternalEConversationMessage)
		{
			return isForInternalEConversationMessage
				? Enumerable.Empty<string>()
				: new[] { ((WorkRequest)workflowProvider).Client.OC_Email.ToString() };
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			workRequestSystem = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			ProcessMgmtTestHelper.CreateAndSetRegistryNotificationGroup(Factory);
		}

		IBMSystem workRequestSystem;

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();
	}

	class WorkRequestWorkflowDescriptorNonTransactionedTest : WorkflowDescriptorNonTransactionedTestCase<WorkRequestWorkflowDescriptor>
	{
		protected override IWorkflowProvider GetJobForTest()
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ProcessMgmtTestHelper.CreateAndSetRegistryNotificationGroup(Factory);
		}
	}
}

using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupWorkflowDescriptor))]
	public class GlbGroupWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbGroupWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "GRP", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Group", WorkflowDescriptor.Description);
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

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsUniversalTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsUniversalTemplates);
		}

		public void TestShouldHideTasksTabOnJobs()
		{
			AssertEquals(true, WorkflowDescriptor.ShouldHideTasksTabOnJobs);
		}

		public void TestSupportsBufferManagement()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				Factory.NewWithValidTestData<GlbGroup>(),
			};
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties =>
			base.ExpectedSupportedMessageRecipientParties
			| MessageRecipientPartyType.Email
			| MessageRecipientPartyType.CurrentUser
			| MessageRecipientPartyType.GroupOwners;

		#region Email Notification Triggers

		public void TestNotificationTriggerRecipient_SpecificEmail()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemoved);
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.Email, emailAddress: "ned@FlancrestEnterprises.net", emailText: "Stupid sexy Flanders");

			Factory.Save();

			group.Logs.AddNew(Events.TagWasAddedOrRemoved);
			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailsSent("Stupid sexy Flanders", new[] { "ned@FlancrestEnterprises.net" });
		}

		public void TestNotificationTriggerRecipient_CurrentUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "ned@FlancrestEnterprises.net";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemoved);
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.CurrentUser, emailText: "Stupid sexy Flanders");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				group.Logs.AddNew(Events.TagWasAddedOrRemoved);
				Factory.Save();
			}

			AssertEquals(0, Env.AllEmailsCreated.Count());
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailsSent("Stupid sexy Flanders", new[] { "ned@FlancrestEnterprises.net" });
		}

		public void TestNotificationTriggerRecipient_GroupOwner()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "ned@FlancrestEnterprises.net";
			MasterFilesTestHelper.MakeStaffOwnerOfGroup(group, staff1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "homer@CompuGlobalHyperMega.net";
			MasterFilesTestHelper.MakeStaffOwnerOfGroup(group, staff2);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "dave@cut.co";

			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemoved);
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.GroupOwners, emailText: "Stupid sexy Flanders");

			Factory.Save();

			group.Logs.AddNew(Events.TagWasAddedOrRemoved);
			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());
			MasterFilesTestHelper.RunLogWalker();

			AssertEmailsSent("Stupid sexy Flanders", new[] { "ned@FlancrestEnterprises.net", "homer@CompuGlobalHyperMega.net" });
		}

		static void AssertEmailsSent(string expectedEmailBody, string[] recipientEmailAddresses)
		{
			var emails = Env.AllEmailsCreated.ToArray();

			CombineAssertions("Details regarding the emails that have been sent", () =>
			{
				AssertEquals("Number of emails sent", recipientEmailAddresses.Length, emails.Length);

				foreach (var email in emails)
				{
					AssertContains("Email body text", expectedEmailBody, email.Body);
				}

				AssertContainsExactElementsInAnyOrder("Email recipients", recipientEmailAddresses, emails.SelectMany(e => e.Recipients.Cast<RecipientDef>().Select(r => r.Email)));
			});
		}

		protected override void SetNotificationEmailAddress(BusinessObject job, ProcessTaskNotification notification, string emailAddress)
		{
			switch (notification.PQ_TriggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.CurrentUser:
					GlbStaff.GetCurrentUser(notification.Factory).GS_EmailAddress = emailAddress;
					break;

				case MessageRecipientPartyTypeList.Codes.GroupOwners:
					var group = (GlbGroup)job;
					var staff = notification.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_EmailAddress, emailAddress));

					if (staff == null)
					{
						staff = notification.Factory.NewWithValidTestData<GlbStaff>();
						staff.GS_EmailAddress = emailAddress;
					}

					if (group.GroupOwnersForGroupView.Count == 0)
					{
						MasterFilesTestHelper.MakeStaffOwnerOfGroup(group, staff);
						group.GroupOwnersForGroupView.Reload(true);
					}

					break;

				default:
					base.SetNotificationEmailAddress(job, notification, emailAddress);
					break;
			}
		}

		#endregion

		#region Send Document Triggers

		public void TestDocumentContext()
		{
			AssertSequencesEqual(new[] { BusinessContext.GlbGroup }, WorkflowDescriptor.DocumentBusinessContext);
		}

		public void TestDocumentTriggerActionRecipients()
		{
			var group = Factory.New<GlbGroup>();
			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemoved);
			var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SendDocument);

			AssertSequencesEqual(new[]
			{
				MessageRecipientPartyTypeList.Codes.Email,
				MessageRecipientPartyTypeList.Codes.Print,
				MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery,
				MessageRecipientPartyTypeList.Codes.CurrentUser,
				MessageRecipientPartyTypeList.Codes.GroupOwners,
			}, triggerAction.Lookups.MessagingTriggerPartiesList.GetAllCodes());
		}

		public void TestDocumentTriggerRecipient_CurrentUser()
		{
			var documentMenuItem = CreateDummyDocumentMenuItem();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "ned@FlancrestEnterprises.net";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemovedCode, description: "Stupid sexy Flanders");
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SendDocument, MessageRecipientPartyTypeList.Codes.CurrentUser, documentMenuItem: documentMenuItem);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				group.Logs.AddNew(Events.TagWasAddedOrRemoved);
				Factory.Save();
			}

			MasterFilesTestHelper.RunLogWalker();

			AssertDocumentsSent(group, "Nothing at all!", new[] { "ned@FlancrestEnterprises.net" });
		}

		public void TestDocumentTriggerRecipient_GroupOwner()
		{
			var documentMenuItem = CreateDummyDocumentMenuItem();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "ned@FlancrestEnterprises.net";
			staff1.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner; // Oh my...
			staff1.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "homer@CompuGlobalHyperMega.net";
			staff2.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner; // Oh my...
			staff2.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "dave@cut.co";

			var trigger = MasterFilesTestHelper.CreateTrigger(group, Events.TagWasAddedOrRemovedCode, description: "Stupid sexy Flanders");
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SendDocument, MessageRecipientPartyTypeList.Codes.GroupOwners, documentMenuItem: documentMenuItem);

			Factory.Save();

			group.Logs.AddNew(Events.TagWasAddedOrRemoved);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			AssertDocumentsSent(group, "Nothing at all!", new[] { "ned@FlancrestEnterprises.net", "homer@CompuGlobalHyperMega.net" });
		}

		StmMenuItem CreateDummyDocumentMenuItem()
		{
			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.GlbGroup);
			command.SU_MenuName = "Nothing at all!";
			command.SU_PreventAutoDelivery = false;
			command.SU_IsPublished = true;
			command.SU_ContactType = ContactType.Consignee.Code;

			var docTemplate = Factory.New<StmTemplate>();
			docTemplate.SO_Name = "WhatEver";
			docTemplate.SO_Template = DocumentEngineTestHelperBase.CreateTemplateFromString(
				$@"{{A}}-[#Config]
{{A}}-[Name=Nothing at all!]
{{A}}-[#SectionBody]
{{B}}-[<Now>]
{{A}}-[#EndOfReport]
");
			docTemplate.SO_DataContext = ".GlbGroup";

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = command.PK;
			pivot.SI_SO = docTemplate.PK;

			return command;
		}

		static void AssertDocumentsSent(GlbGroup group, string expectedEmailSubject, string[] recipientEmailAddresses)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_ParentGuid, group.PK).AddToFilter(StmPrintJobSchema.SP_EmailSubjectLine, SQLComparisonOperator.Contains, expectedEmailSubject);
			var printJobs = group.Factory.Load<StmPrintJob>(query);

			AssertContainsExactElementsInAnyOrder("Email recipients", recipientEmailAddresses, printJobs.SelectMany(job => job.EmailToRecipients.Select(e => e.SPR_EmailAddress)));
		}

		#endregion
	}
}

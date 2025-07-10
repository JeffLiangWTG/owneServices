using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHolidayWorkflowDescriptor))]
	public class GlbStaffHolidayWorkflowDescriptorTest : WorkflowDescriptorTestCase<GlbStaffHolidayWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals(WorkflowDescriptors.GlbStaffHolidayDescriptorCode, WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("Staff Holiday", WorkflowDescriptor.Description);

		public override void TestSubTypes()
		{
			AssertEquals(2, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Leave Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Approval Status", WorkflowDescriptor.SubTypeInformation[1].Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient() => AssertEquals(false, WorkflowDescriptor.RequiresClient);

		public override void TestRequiresBranch() => AssertEquals(true, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresDepartment() => AssertEquals(true, WorkflowDescriptor.RequiresDepartment);

		public override void TestSupportsEventTracking() => AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);

		public override void TestSupportsWorkflowTemplates() => AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);

		public override void TestSupportsScreenLayout() => AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties => MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.FirstApprovalTask;

		protected override bool ExpectingTasksToBeCompanySpecific => true;

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var holiday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			var manager = Factory.NewWithValidTestData<GlbStaffManager>();
			manager.Manager.GS_EmailAddress = "dummy@test.com";
			manager.GSM_ManagerType = "DRM";
			holiday.Staff.Managers.Add(manager);
			return new IWorkflowProvider[] { holiday };
		}

		public void IsDirectEmail()
		{
			AssertEquals(true, WorkflowDescriptor.IsDirectEmailRecipient(MessageRecipientPartyTypeList.Codes.FirstApprovalTask));
			AssertEquals(true, WorkflowDescriptor.IsDirectEmailRecipient(MessageRecipientPartyTypeList.Codes.Email));
			AssertEquals(true, WorkflowDescriptor.IsDirectEmailRecipient(MessageRecipientPartyTypeList.Codes.OrgProxy));
		}

		public void TestNotificationTriggerRecipient_SpecificEmail()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				Factory.Save();

				AssertEquals(0, Env.AllEmailsCreated.Count());
				MasterFilesTestHelper.RunLogWalker();

				AssertEmailsSent("the message", new[] { "recipient@email.com" });
			},
			MessageRecipientPartyTypeList.Codes.Email);
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTaskWithNoTaskFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { "recipient@email.com" });
			});
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTaskWithNoApprovalTaskFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var someOtherTask1 = AddTask(holiday, SomeOtherTaskCode, user3.GS_Code);

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { "recipient@email.com" });
			});
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTask_WithApprovalTaskClosedFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var approvalTask = AddTask(holiday, ApprovalTaskCode, user3.GS_Code);
				approvalTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { user3.GS_EmailAddress.ToString() });
			});
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTask_WithApprovalTaskCanceledFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var approvalTask = AddTask(holiday, ApprovalTaskCode, user3.GS_Code);
				approvalTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { user3.GS_EmailAddress.ToString() });
			});
		}
		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTask_WithApprovalTaskOpenFallsbackToActionEmailAddress()
		{
			NotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskNotAssignedStatusFallsbackToActionEmailAddress(ProcessTaskStatusCodeList.Codes.Open);
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTask_WithApprovalTaskWorkingFallsbackToActionEmailAddress()
		{
			NotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskNotAssignedStatusFallsbackToActionEmailAddress(ProcessTaskStatusCodeList.Codes.Working);
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTask_WithApprovalTaskSuspendedFallsbackToActionEmailAddress()
		{
			NotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskNotAssignedStatusFallsbackToActionEmailAddress(ProcessTaskStatusCodeList.Codes.Suspended);
		}

		void NotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskNotAssignedStatusFallsbackToActionEmailAddress(string status)
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var approvalTask = AddTask(holiday, ApprovalTaskCode, user3.GS_Code);
				approvalTask.P9_Status = status;

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { "recipient@email.com" });
			});
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskNotAssignedFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var approvalTask = AddTask(holiday, ApprovalTaskCode, string.Empty);

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { "recipient@email.com" });
			});
		}

		public void TestNotificationTriggerRecipient_SpecificFirstApprovalTaskWithApprovalTaskAssignedFallsbackToActionEmailAddress()
		{
			RecipientTestCode((user1, user2, user3, staff, holiday) =>
			{
				var someTask = AddTask(holiday, SomeTaskCode, user1.GS_Code);
				var someOtherTask = AddTask(holiday, SomeOtherTaskCode, user2.GS_Code);
				var approvalTask = AddTask(holiday, ApprovalTaskCode, user3.GS_Code);

				Factory.Save();
				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();
				AssertEmailsSent("the message", new[] { "user3@email.com" });
			});
		}

		GlbStaffHolidayProcessTask AddTask(GlbStaffHoliday holiday, string type, string userCode)
		{
			var task = holiday.WorkflowItems.AddNew();
			task.P9_Type = type;
			task.P9_GS_NKAssignedStaffMember = userCode;
			return task;
		}

		void RecipientTestCode(Action<GlbStaff, GlbStaff, GlbStaff, GlbStaff, GlbStaffHoliday> test, string recipientType = MessageRecipientPartyTypeList.Codes.FirstApprovalTask)
		{
			SetupRegistryItems();
			var user1 = Factory.NewWithValidTestData<GlbStaff>();
			user1.GS_EmailAddress = "user1@email.com";
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			user2.GS_EmailAddress = "user2@email.com";
			var user3 = Factory.NewWithValidTestData<GlbStaff>();
			user3.GS_EmailAddress = "user3@email.com";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var holiday = Factory.NewWithValidTestData<GlbStaffHoliday>();
			staff.Holidays.Add(holiday);

			var trigger = MasterFilesTestHelper.CreateTrigger(holiday, Events.AddedARecordToTheSystem);

			MasterFilesTestHelper.CreateTriggerAction(
				trigger,
				WorkflowTriggerActionTypeConstants.Codes.NotificationEmail,
				recipientType,
				emailAddress: "recipient@email.com",
				emailText: "the message");

			test(user1, user2, user3, staff, holiday);
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

		void SetupRegistryItems()
		{
			var registryItem = new WorkflowTaskTypesRegistryItem("ProcessManagerTaskTypes",
				RawDataRegistry.Categories.WorkflowManager,
				null,
				null,
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
			var collection = registryItem.Value;
			var workflowTaskTypes = collection.GetTaskTypesFromWorkflowCode(GlbStaffHolidayWorkflowDescriptor.WorkflowDescriptorGlbStaffHolidayDescriptorCode);
			var existing = workflowTaskTypes.Cast<WorkflowTaskType>().Select(s => s.Code).ToArray();
			if (!existing.Contains(SomeTaskCode))
			{
				var someTask = workflowTaskTypes.AddNew();
				someTask.Code = SomeTaskCode;
			}

			if (!existing.Contains(ApprovalTaskCode))
			{
				var approvalTask = workflowTaskTypes.AddNew();
				approvalTask.Code = ApprovalTaskCode;
				approvalTask.IsApprovalTask = true;
			}
			else
			{
				workflowTaskTypes.Cast<WorkflowTaskType>().First(s => s.Code == ApprovalTaskCode).IsApprovalTask = true;
			}

			if (!existing.Contains(SomeOtherTaskCode))
			{
				var someOtherTask = workflowTaskTypes.AddNew();
				someOtherTask.Code = SomeOtherTaskCode;
			}
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		const string ApprovalTaskCode = "APT";
		const string SomeTaskCode = "SOT";
		const string SomeOtherTaskCode = "STT";
	}
}

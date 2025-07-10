using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	class NotificationRecipientCalculatorTest : TestCaseWithFactory
	{
		#region Trigger Actions

		public void TestRecipientDetermination_ForNotificationGroupRecipient_ShouldConsiderRegistryFallbacks()
		{
			var group_systemLevel = Factory.NewWithValidTestData<GlbGroup>();
			var group_branch1Level = Factory.NewWithValidTestData<GlbGroup>();
			var group_branch2Level = Factory.NewWithValidTestData<GlbGroup>();
			var group_department1Level = Factory.NewWithValidTestData<GlbGroup>();
			var group_department2Level = Factory.NewWithValidTestData<GlbGroup>();

			var resource1_1 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_1@vanjie.com", new[] { group_systemLevel });
			var resource1_2 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_2@vanjie.com", new[] { group_systemLevel });
			var resource2_1 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_3@vanjie.com", new[] { group_branch1Level });
			var resource2_2 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_4@vanjie.com", new[] { group_branch1Level });
			var resource3_1 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_5@vanjie.com", new[] { group_branch2Level });
			var resource3_2 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_6@vanjie.com", new[] { group_branch2Level });
			var resource4_1 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_7@vanjie.com", new[] { group_department1Level });
			var resource4_2 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_8@vanjie.com", new[] { group_department1Level });
			var resource5_1 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_9@vanjie.com", new[] { group_department2Level });
			var resource5_2 = ProcessMgmtTestHelper.CreateStaff(Factory, "email_10@vanjie.com", new[] { group_department2Level });

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.AddedARecordToTheSystemCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.NotificationGroup);

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group_systemLevel.PK.ToGuid());

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, group_branch1Level.PK.ToGuid());
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, group_branch2Level.PK.ToGuid());

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, branch.PK.ToGuid(), department.PK.ToGuid(), group_department1Level.PK.ToGuid());
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, branch.PK.ToGuid(), Env.CurrentDepartmentPK, group_department2Level.PK.ToGuid());

			AssertNull(request.Branch);
			AssertNull(request.Department);

			AssertContainsExactElementsInAnyOrder("No branch or department was specified on the job, so use the system-level config",
				new[] { "email_1@vanjie.com", "email_2@vanjie.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));

			request.WKR_GB_Branch = branch.PK;
			AssertContainsExactElementsInAnyOrder("A branch was specified on the job, so use the branch-level config",
				new[] { "email_3@vanjie.com", "email_4@vanjie.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));

			request.WKR_GE_Department = department.PK;
			AssertContainsExactElementsInAnyOrder("A branch AND department were specified on the job, so use the branch + department level config",
				new[] { "email_7@vanjie.com", "email_8@vanjie.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));

			request.WKR_GB_Branch = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("A department was specified on the job without a branch, so use the system-level config",
				new[] { "email_1@vanjie.com", "email_2@vanjie.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));
		}

		[TestDate(2017, 8, 10)]
		public void TestRecipientDetermination_ForNotificationGroupRecipient_ShouldConsiderEditLogs()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.AddedARecordToTheSystemCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.NotificationGroup);

			var systemAccount = ProcessMgmtTestHelper.CreateStaff(Factory, "twenty");
			var nonSystemAccount1 = ProcessMgmtTestHelper.CreateStaff(Factory, "five");
			var nonSystemAccount2 = ProcessMgmtTestHelper.CreateStaff(Factory, "shmeckels");

			systemAccount.GS_IsSystemAccount = true;

			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			using (Env.SetTemporaryUserContext(nonSystemAccount1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				request.WKR_Summary = "Mr";
				Factory.Save();
			}

			TestDateAttribute.AddMinutes(1);

			using (Env.SetTemporaryUserContext(nonSystemAccount2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				request.WKR_Summary = "Booby";
				Factory.Save();
			}

			TestDateAttribute.AddMinutes(1);

			using (Env.SetTemporaryUserContext(systemAccount.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				request.WKR_Summary = "Buyer";
				Factory.Save();
			}

			AssertContainsExactElementsInAnyOrder("Should have used the most recent edit by a non-system user account", new[] { "shmeckels" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));
		}

		public void TestRecipientDetermination_ForJobLevelWorkflowGroupRecipient_WhenBufferManagementDisabled_ShouldFallbackToNotificationGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "miss.vanjie@mateo.com", new[] { group });

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.AddedARecordToTheSystemCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup);

			AssertContainsExactElementsInAnyOrder(new[] { "miss.vanjie@mateo.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));

			ErrorReporter.Clear();

			BMSTestHelper.EnableBMSInRegistry();

			AssertContainsExactElementsInAnyOrder(new[] { "miss.vanjie@mateo.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));

			ErrorReporter.Clear();

			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			AssertContainsExactElementsInAnyOrder(new[] { "miss.vanjie@mateo.com" }, NotificationRecipientCalculator.GetEmailAddressesForTriggerActionNotification(triggerAction, request));
		}

		#endregion

		#region EConversation

		public void TestGetRecipientsForEConversationEmails_WhenStaffSubscriberExists_ShouldNotReturnAnyStaff()
		{
			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var notificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "AntsInMyEyes@johnson.com", groups: new[] { notificationGroup });
			var participantStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "Jon@Gazorpazorp.com");

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Gazorpazorpfield@Gazorpazorp.com");

			Factory.Save();

			ticket.Conversation.Participants.AddNewParticipant(participantStaff);

			AssertContainsExactElementsInAnyOrder(new[] { "Jon@Gazorpazorp.com" }, GetAllConversationParticipantEmailAddresses(ticket));
		}

		public void TestGetRecipientsForEConversationEmails_WhenNoStaffSubscriberExists_ShouldConsiderAllRegistryFallbacks()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			var lastCompletedTaskStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "tophat@jones.com");

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "regular@legs.com", groups: new[] { releaseGroup });

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var notificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "baby@legs.com", groups: new[] { notificationGroup });

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Gazorpazorpfield@Gazorpazorp.com");
			var jobHeader = ProcessJobHeaderProvider.GetForParent(ticket, Factory);
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;

			var task = MasterFilesTestHelper.CreateTask(ticket, assignedStaff: lastCompletedTaskStaff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the last closed task user by default", new[] { "tophat@jones.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			lastCompletedTaskStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the staff belonging to the job-level workflow's release group next", new[] { "regular@legs.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			releaseGroupStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the staff belonging to the notification group next", new[] { "baby@legs.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			notificationGroupStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Here we give up on finding a staff email address because all fallbacks have been exhausted", Array.Empty<string>(), NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));
		}

		public void TestGetRecipientsForEConversationEmails_WhenNoStaffSubscriberExists_ShouldConsiderConfiguredRegistryFallbacks()
		{
			ProcessMgmtTestHelper.SetFallbackSequenceForNonSubscribedCSTicketNotifications(RecipientSourceTypeList.Codes.NotificationGroup, RecipientSourceTypeList.Codes.JobLevelWorkflowReleaseGroup, RecipientSourceTypeList.Codes.LastCompletedTaskResource);

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			var lastCompletedTaskStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "tophat@jones.com");

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "regular@legs.com", groups: new[] { releaseGroup });

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var notificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "baby@legs.com", groups: new[] { notificationGroup });

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Gazorpazorpfield@Gazorpazorp.com");
			var jobHeader = ProcessJobHeaderProvider.GetForParent(ticket, Factory);
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;

			var task = MasterFilesTestHelper.CreateTask(ticket, assignedStaff: lastCompletedTaskStaff.GS_Code);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the staff belonging to the notification group by default", new[] { "baby@legs.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			notificationGroupStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the staff belonging to the job-level workflow's release group next", new[] { "regular@legs.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			releaseGroupStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should fallback to the last closed task user next", new[] { "tophat@jones.com" }, NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));

			lastCompletedTaskStaff.GS_EmailAddress = "";
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Give up on finding a staff email address because all fallbacks have been exhausted", Array.Empty<string>(), NotificationRecipientCalculator.GetStaffForEConversationNotifications(ticket).Select(s => s.GS_EmailAddress));
		}

		public void TestGetRecipientsForEConversationEmails_WhenStaffAreInactiveAndStaffGroupSubscribed_ShouldSendEmailToStaffGroupOnly()
		{
			var subscriberStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberStaff@PersonalSpace.com");

			var subscriberGroup = Factory.NewWithValidTestData<GlbGroup>();
			var subscriberGroupMember1 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember1@PersonalSpace.com", new[] { subscriberGroup });
			var subscriberGroupMember2 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember2@PersonalSpace.com", new[] { subscriberGroup });

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupMember1 = ProcessMgmtTestHelper.CreateStaff(Factory, "NotificationGroupMember1@PersonalSpace.com", new[] { registryNotificationGroup });
			var registryNotificationGroupMember2 = ProcessMgmtTestHelper.CreateStaff(Factory, "NotificationGroupMember2@PersonalSpace.com", new[] { registryNotificationGroup });

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@PersonalSpace.com");
			Factory.Save();

			var subscriberStaffParticipant = ticket.Conversation.Participants.AddNewParticipant(subscriberStaff);
			var subscriberGroupParticipant = ticket.Conversation.Participants.AddNewParticipant(subscriberGroup);

			subscriberStaff.GS_IsActive = false;
			subscriberStaffParticipant.JCP_IsSubscribed = true;
			var staffMessagePK1 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK1 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			Factory.Save();

			CombineAssertions("When subscriberStaffParticipant is subscribed but inactive, should include the client and group members only.", () =>
			{
				AssertContainsExactElementsInAnyOrder("Message sent from staff.", new[]
				{
					"Client@PersonalSpace.com",
					"SubscriberGroupMember1@PersonalSpace.com",
					"SubscriberGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK1));

				AssertContainsExactElementsInAnyOrder("Message sent from client.", new[]
				{
					"Client@PersonalSpace.com",
					"SubscriberGroupMember1@PersonalSpace.com",
					"SubscriberGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK1));
			});

			var staffMessagePK2 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK2 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			subscriberStaff.GS_IsActive = true;
			subscriberStaffParticipant.JCP_IsSubscribed = false;
			Factory.Save();

			CombineAssertions("When subscriberStaffParticipant is not subscribed but active, should include the client and group members only.", () =>
			{
				AssertContainsExactElementsInAnyOrder("Message sent from staff.", new[]
				{
					"Client@PersonalSpace.com",
					"SubscriberGroupMember1@PersonalSpace.com",
					"SubscriberGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK2));

				AssertContainsExactElementsInAnyOrder("Message sent from client.", new[]
				{
					"Client@PersonalSpace.com",
					"SubscriberGroupMember1@PersonalSpace.com",
					"SubscriberGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK2));
			});

			var staffMessagePK3 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK3 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			subscriberGroupParticipant.JCP_IsSubscribed = false;
			Factory.Save();

			CombineAssertions("All people are now unsubscribed, should send to the client and fall back to the registry notification group only if sent from the client.", () =>
			{
				AssertContainsExactElementsInAnyOrder("Message sent from staff.", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK3));

				AssertContainsExactElementsInAnyOrder("Message sent from client.", new[]
				{
					"Client@PersonalSpace.com",
					"NotificationGroupMember1@PersonalSpace.com",
					"NotificationGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK3));
			});
		}

		public void TestGetRecipientsForEConversationEmails_WhenNoSubscribersWithEmailsExists_ShouldConsiderRegistryFallbacks()
		{
			var subscriberStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberStaff@PersonalSpace.com");

			var subscriberGroup = Factory.NewWithValidTestData<GlbGroup>();
			var subscriberGroupMember1 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember1@PersonalSpace.com", new[] { subscriberGroup });
			var subscriberGroupMember2 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember2@PersonalSpace.com", new[] { subscriberGroup });

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupMember1 = ProcessMgmtTestHelper.CreateStaff(Factory, "NotificationGroupMember1@PersonalSpace.com", new[] { registryNotificationGroup });
			var registryNotificationGroupMember2 = ProcessMgmtTestHelper.CreateStaff(Factory, "NotificationGroupMember2@PersonalSpace.com", new[] { registryNotificationGroup });

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@PersonalSpace.com");
			Factory.Save();

			ticket.Conversation.Participants.AddNewParticipant(subscriberStaff);
			ticket.Conversation.Participants.AddNewParticipant(subscriberGroup);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include all subscribers with valid email addresses", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberStaff@PersonalSpace.com",
				"SubscriberGroupMember1@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberGroupMember1.GS_EmailAddress = ZString.Empty;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include all subscribers with valid email addresses", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberStaff@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberStaff.GS_EmailAddress = ZString.Empty;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include all subscribers with valid email addresses", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberGroupMember2.GS_EmailAddress = ZString.Empty;
			var staffMessagePK1 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK1 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			Factory.Save();

			CombineAssertions("WHEN setting subscriberGroupMember2 email to empty", () =>
			{
				AssertContainsExactElementsInAnyOrder("Staff sends a message, should NOT include all notification group members.", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK1));

				AssertContainsExactElementsInAnyOrder("Client sends a message, should include notification group members with valid email.", new[]
				{
					"Client@PersonalSpace.com",
					"NotificationGroupMember1@PersonalSpace.com",
					"NotificationGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK1));
			});

			registryNotificationGroupMember1.GS_EmailAddress = ZString.Empty;
			var staffMessagePK2 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK2 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			Factory.Save();

			CombineAssertions("GIVEN subscribers has no valid email WHEN setting notificationGroupMember1.GS_EmailAddress to empty", () =>
			{
				AssertContainsExactElementsInAnyOrder("Staff sends a message, should NOT include all notification group members.", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK2));

				AssertContainsExactElementsInAnyOrder("Client sends a message, should include notification group members with valid email", new[]
				{
					"Client@PersonalSpace.com",
					"NotificationGroupMember2@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK2));
			});
		}

		public void TestGetRecipientsForEConversationEmails_WhenSubscriberHasUnsubscribed_ShouldNotFallBackToThem()
		{
			var subscriberStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberStaff@PersonalSpace.com");

			var subscriberGroup = Factory.NewWithValidTestData<GlbGroup>();
			var subscriberGroupMember1 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember1@PersonalSpace.com", new[] { subscriberGroup });
			var subscriberGroupMember2 = ProcessMgmtTestHelper.CreateStaff(Factory, "SubscriberGroupMember2@PersonalSpace.com", new[] { subscriberGroup });

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupMember = ProcessMgmtTestHelper.CreateStaff(Factory, "NotificationGroupMember@PersonalSpace.com", new[] { registryNotificationGroup });

			Factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@PersonalSpace.com");
			Factory.Save();

			var subscriberStaffParticipant1 = ticket.Conversation.Participants.AddNewParticipant(subscriberStaff);
			var subscriberStaffParticipant2 = ticket.Conversation.Participants.AddNewParticipant(subscriberGroup);
			var subscriberStaffParticipant3 = ticket.Conversation.Participants.AddNewParticipant(subscriberGroupMember1);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include all participants", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberStaff@PersonalSpace.com",
				"SubscriberGroupMember1@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberStaffParticipant1.JCP_IsSubscribed = false;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should include all participants which still have the IsSubscribed flag set", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberGroupMember1@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberStaffParticipant3.JCP_IsSubscribed = false;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("A member of the subscribed group now has un-checked the IsSubscribed flag for their staff participation record. So we don't include them because they've opted out. Should still include the other group member.", new[]
			{
				"Client@PersonalSpace.com",
				"SubscriberGroupMember2@PersonalSpace.com",
			}, GetAllConversationParticipantEmailAddresses(ticket));

			subscriberStaffParticipant2.JCP_IsSubscribed = false;
			var staffMessagePK1 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK1 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			Factory.Save();

			CombineAssertions("WHEN JCP_IsSubscribed flag is not set on any staff...", () =>
			{
				AssertContainsExactElementsInAnyOrder("When staff sends an email (should NOT use the registry fallback config to find recipients).", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK1));

				AssertContainsExactElementsInAnyOrder("When client sends an email (should use the registry fallback config to find recipients).", new[]
				{
					"Client@PersonalSpace.com",
					"NotificationGroupMember@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK1));
			});

			var participant4 = ticket.Conversation.Participants.AddNewParticipant(registryNotificationGroupMember);
			participant4.JCP_IsSubscribed = false;
			var staffMessagePK2 = ticket.Conversation.AddMessageFromCurrentUser("Message from Staff.", isInternal: false).PK;
			var externalMessagePK2 = ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "External Message From Client.");
			Factory.Save();

			CombineAssertions("WHEN JCP_IsSubscribed flag is not set on any staff AND the notification group member is part of the unsubscribed staff...", () =>
			{
				AssertContainsExactElementsInAnyOrder("When staff sends an email.", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, staffMessagePK2));

				AssertContainsExactElementsInAnyOrder("When client sends an email.", new[]
				{
					"Client@PersonalSpace.com",
				}, GetAllConversationParticipantEmailAddresses(ticket, externalMessagePK2));
			});
		}

		#endregion

		#region Implementation

		static IEnumerable<string> GetAllConversationParticipantEmailAddresses(WorkRequest workRequest, ZGuid? newMessagePK = null)
		{
			var newMessage = newMessagePK.HasValue ? workRequest.Factory.Load<JobConversationMessage>(newMessagePK.Value) : null;

			return EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(workRequest.Conversation, staffOnly: false, emailsToExclude: null, newMessage?.Sender).Select(p => p.Email.ToString());
		}

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();

		#endregion
	}
}

using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequest))]
	class WorkRequestWorkflowProviderTest : WorkflowProviderTest<WorkRequest, WorkRequestProcessTaskCollection>
	{
		#region Field Change Triggers

		[TestDate(2015, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWorkflowFieldChangeTriggersWork()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Hi, I'm Mr. Booby Buyer");
			var trigger = MasterFilesTestHelper.CreateTrigger(request, eventCode: null, triggerFieldName: WorkRequestSchema.Constants.WKR_Summary);

			Factory.Save();
			MasterFilesTestHelper.RunFieldChangeTriggerProcessorServiceTask();

			request.WKR_Summary = "I'll buy those boobies for 25 shmeckels.";
			Factory.Save();

			TestDateAttribute.AddHours(1); // Makes the edit log in range of what the service task will consider
			MasterFilesTestHelper.RunFieldChangeTriggerProcessorServiceTask();

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			trigger.Reload();

			AssertEquals(ZDateTime.Now.AddHours(-1), trigger.P9_ActualDate);
		}

		#endregion

		#region Email Notifications

		[TestDate(2017, 8, 10)]
		public void TestNotificationTriggerAction_ClientRecipient_ShouldSendEmailToClientEmail()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "rickandmorty@hundredyears.com");
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.ArrivalCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Client, "Fracken heck");

			Factory.Save();

			request.GetLogs().AddNew(AutoEvents.Arrival);
			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should have sent a notification email when the trigger was fired", 1, Env.AllEmailsCreated.Count());

			var email = Env.AllEmailsCreated.Single();

			AssertEquals("rickandmorty@hundredyears.com", email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertContains("Fracken heck", email.Body);
		}

		[TestDate(2017, 8, 10)]
		public void TestNotificationTriggerAction_EmailRecipient_ShouldSendEmailToSpecifiedAddress()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.ArrivalCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Email, "Fracken heck", emailAddress: "rickandmorty@hundredyears.com");

			Factory.Save();

			request.GetLogs().AddNew(AutoEvents.Arrival);
			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should have sent a notification email when the trigger was fired", 1, Env.AllEmailsCreated.Count());

			var email = Env.AllEmailsCreated.Single();

			AssertEquals("rickandmorty@hundredyears.com", email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertContains("Fracken heck", email.Body);
		}

		[TestDate(2017, 8, 10)]
		public void TestNotificationTriggerAction_NotificationGroupRecipient_ShouldUseCorrectEmailFallbackProcedure()
		{
			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var resourcePartOfNotificationGroup1 = Factory.NewWithValidTestData<GlbStaff>();
			var resourcePartOfNotificationGroup2 = Factory.NewWithValidTestData<GlbStaff>();
			var resourcePartOfNotificationGroup3 = Factory.NewWithValidTestData<GlbStaff>();
			resourcePartOfNotificationGroup1.GS_EmailAddress = "missvanjie@vanjie.com";
			resourcePartOfNotificationGroup2.GS_EmailAddress = "Aquaria@MizCracker.com";
			resourcePartOfNotificationGroup3.GS_EmailAddress = ""; // Should not send an email to an empty address

			notificationGroup.Staff.Add(resourcePartOfNotificationGroup1);
			notificationGroup.Staff.Add(resourcePartOfNotificationGroup2);
			notificationGroup.Staff.Add(resourcePartOfNotificationGroup3);

			var editingResource = Factory.NewWithValidTestData<GlbStaff>();
			editingResource.GS_EmailAddress = "miss...vanjie@mateo.com";

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(request, AutoEvents.MiscellaneousEventCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.NotificationGroup, "Miss Vanjie. Miss Vaaaanjie. Miss.. Vanjiie.");

			Factory.Save();

			request.GetLogs().AddNew(AutoEvents.MiscellaneousEvent);
			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should have sent a notification email when the trigger was fired to all members of the notification group", 2, Env.AllEmailsCreated.Count());

			var emails = Env.AllEmailsCreated.ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "missvanjie@vanjie.com", "Aquaria@MizCracker.com" }, emails.Select(e => e.Recipients.Cast<RecipientDef>().Single().Email));
			AssertContains("Miss Vanjie. Miss Vaaaanjie. Miss.. Vanjiie.", emails[0].Body);
			AssertContains("Miss Vanjie. Miss Vaaaanjie. Miss.. Vanjiie.", emails[1].Body);

			// Clear email address from notification group members
			resourcePartOfNotificationGroup1.GS_EmailAddress = ZString.Empty;
			resourcePartOfNotificationGroup2.GS_EmailAddress = ZString.Empty;

			Factory.Save();
			Env.ClearAllEmailsCreated();

			TestDateAttribute.AddMinutes(1);

			request.GetLogs().AddNew(AutoEvents.MiscellaneousEvent);
			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			AssertMatch(new Regex("Warning.*No address found for Trigger Party .NGP."), MasterFilesTestHelper.RunLogWalker());

			AssertEquals("Should not have sent any notification email when the trigger was fired since no one in the notification group has an email address", 0, Env.AllEmailsCreated.Count());

			TestDateAttribute.AddMinutes(1);

			using (Env.SetTemporaryUserContext(editingResource.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				request.WKR_Summary = "BAM!";
				Factory.Save();
			}

			request.GetLogs().AddNew(AutoEvents.MiscellaneousEvent);
			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Should have sent a notification email when the trigger was fired to the last editing non-system user", 1, Env.AllEmailsCreated.Count());
			var email = Env.AllEmailsCreated.Single();

			AssertEquals("miss...vanjie@mateo.com", email.Recipients.Cast<RecipientDef>().Single().Email);
			AssertContains("Miss Vanjie. Miss Vaaaanjie. Miss.. Vanjiie.", email.Body);
		}

		public void TestNotificationTriggerAction_JobLevelWorkflowGroupRecipient_ShouldUseCorrectEmailFallbackProcedure()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInNotificationGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "YouJustSortof@TrailedOff.there", new[] { notificationGroup });
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInReleaseGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "", new[] { releaseGroup }); // Empty email address to start off with, proving that we need valid addresses or we'll still fall back to the notification group.
			BMSTestHelper.MarkAsReleaseGroupWithinSystem(system, releaseGroup);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(workRequest, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup, "Yeah I know Milhouse");

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workRequest, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(ZGuid.Empty, jobHeader.FH_GG_ReleaseGroup);

			Factory.Save();

			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have fallen back to the Notification Group member's email address because the job-level workflow has no release group",
				"YouJustSortof@TrailedOff.there", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Set the release group of the job-level workflow, thereby making it considered before falling back to the registry config. It falls back because there aren't any valid email addresses in that group, so it won't be used. Yet.
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have used the notification group member's email address because the job-level workflow's release group member has no email address",
				"YouJustSortof@TrailedOff.there", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Set a valid email address, so now the release group should be used rather than the registry fallback.
			staffInNotificationGroup.GS_EmailAddress = "YeahIGuessIDid@TrailOff...";
			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have used the release group member as the recipient because there is a valid email address there now and PAVE is enabled",
				"YeahIGuessIDid@TrailOff...", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);
		}

		public void TestNotificationTriggerAction_JobLevelWorkflowGroupRecipient_WhenBufferManagementDisabled_ShouldFallBackToNotificationGroup()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInNotificationGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "YouJustSortof@TrailedOff.there", new[] { notificationGroup });
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInReleaseGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "YeahIGuessIDid@TrailOff...", new[] { releaseGroup });
			BMSTestHelper.MarkAsReleaseGroupWithinSystem(system, releaseGroup);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(workRequest, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup, "Yeah I know Milhouse");
			AssertNoNotifications(triggerAction.PQ_Calc_TriggerPartyInfo);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workRequest, Factory);
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;

			// Disable Buffer Management, meaning that the existing trigger on't fire at all, because the recipient party is no longer valid.
			BMSTestHelper.DisableBMSInRegistry();

			Factory.Save();

			const string expectedWarning = "Buffer Management is not enabled for this job type. The Job-level Workflow Group option is only valid for jobs associated with a Buffer Management System. Fallback logic will be used to determine an alternative recipient.";
			triggerAction.Validation.ValidateAll();
			AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, expectedWarning);

			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have used the notification group member's email address because PAVE is disabled",
				"YouJustSortof@TrailedOff.there", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Enable PAVE and re-fire just to prove the test is setup correctly
			BMSTestHelper.EnableBMSInRegistry();
			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have used the release group member's email address because PAVE is now enabled",
				"YeahIGuessIDid@TrailOff...", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			triggerAction.Validation.ValidateAll();
			AssertNoNotifications(triggerAction.PQ_Calc_TriggerPartyInfo);

			system.Delete();
			Factory.Save();
			triggerAction.Validation.ValidateAll();
			AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, expectedWarning);

			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have used the notification group member's email address because the workflow type isn't associated with a Buffer Management System. SAD!",
				"YouJustSortof@TrailedOff.there", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestNotificationTriggerAction_LastCompletedTaskResourceRecipient_ShouldUseCorrectEmailFallbackProcedure()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInNotificationGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "tommy's.father@froopy.land", new[] { notificationGroup });
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var staffInReleaseGroup = ProcessMgmtTestHelper.CreateStaff(Factory, "tommy@froopy.land", new[] { releaseGroup });
			BMSTestHelper.MarkAsReleaseGroupWithinSystem(system, releaseGroup);

			var lastEditingStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "rick@froopy.land");
			var lastClosedTaskStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "beth@froopy.land");

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Shall we resume stabbing?");
			var trigger = MasterFilesTestHelper.CreateTrigger(workRequest, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource, "Pink sentient switchblade");
			var task1 = MasterFilesTestHelper.CreateTask(workRequest, lastClosedTaskStaff.GS_Code);
			var task2 = MasterFilesTestHelper.CreateTask(workRequest, lastEditingStaff.GS_Code);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workRequest, Factory);
			jobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(lastEditingStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				task2.P9_CompletedTimeUtc = ZDateTime.UtcNow.AddDays(1); // Completed time is later than task1 but it isn't actually closed.

				workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
				workRequest.WKR_Summary += "Breathable water";

				Factory.Save();
			}

			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have sent the notification to the staff whose task was most recently closed", "beth@froopy.land", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Clear last editing staff's email address - should fall back to release group
			lastClosedTaskStaff.GS_EmailAddress = "";
			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have fallen back to using the release group member's email", "tommy@froopy.land", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Clear release group member's email address - should fall back to notification group
			staffInReleaseGroup.GS_EmailAddress = "";
			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have fallen back to using the notification group member's email", "tommy's.father@froopy.land", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			// Clear notification group member's email address - should fall back to last edit user
			staffInNotificationGroup.GS_EmailAddress = "";
			workRequest.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			Env.ClearAllEmailsCreated();
			MasterFilesTestHelper.RunLogWalker();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertEquals("Should have fallen back to using the last edit staff's email", "rick@froopy.land", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);
		}

		[TestDate(2018, 5, 18)]
		public void TestNotificationTriggerAction_ShouldSendToEmailRecipient()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var trigger = MasterFilesTestHelper.CreateTrigger(ticket, AutoEvents.AddedARecordToTheSystem);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Email, "Annatar, Lord Sauron, whatever!", emailAddress: "partypoopers@wisetechglobal.com");

			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Log Walker should be able to process ProductivityWise job types, even if the registry item is not enabled. This is because in production we'll most likely be just using the command line exe argument rather than the registry item. The registry will likely be just for testing or as an override for production systems since we have limited control over startup arguments on WiseCloud.",
				1, Env.AllEmailsCreated.Count());
			AssertContains("Sauron", Env.AllEmailsCreated.Single().Body);
		}

		[TestDate(2018, 5, 18)]
		public void TestNotificationTriggerAction_ForNonProductivityWiseJobType_ShouldSendToEmailRecipient_EvenWhenProductivityWiseRegistryItemEnabled()
		{
			var shipment = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			var trigger = MasterFilesTestHelper.CreateTrigger(shipment, AutoEvents.AddedARecordToTheSystem);
			var triggerAction = MasterFilesTestHelper.CreateEmailNotificationTriggerAction(trigger, MessageRecipientPartyTypeList.Codes.Email, "Annatar, Lord Sauron, whatever!", emailAddress: "partypoopers@wisetechglobal.com");

			Factory.Save();

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Log Walker should still be able to process non-ProductivityWise job types, otherwise if someone turns on the registry item then existing events will stop being processed.", 1, Env.AllEmailsCreated.Count());
			AssertContains("Sauron", Env.AllEmailsCreated.Single().Body);
		}

		#endregion

		#region Event Propagation

		[TestDate(2015, 7, 14)]
		public void TestEventPropagation_ShouldPublishUpdateFromWorkItemToWorkRequest()
		{
			var workRequest1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workRequest2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "A whip that forces people to like you");
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Invisibility cuffs");
			var workItem3 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "Night vision googly eye glasses");
			var workItem4 = ProcessMgmtTestHelper.CreateWorkItem(Factory, "A pink sentient switchblade");

			var task1 = MasterFilesTestHelper.CreateTask(workItem1);
			var task2 = MasterFilesTestHelper.CreateTask(workItem2);
			var task3 = MasterFilesTestHelper.CreateTask(workItem3);
			var task4 = MasterFilesTestHelper.CreateTask(workItem4);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem2);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest1, workItem3);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem3);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest2, workItem4);

			var trigger1 = MasterFilesTestHelper.CreateTrigger(workRequest1, AutoEvents.JobCloseCode, triggerCondition: EventReferenceConditionList.Codes.EventReferenceWithWildcards, triggerConditionValue: "Propagated*");
			var trigger2 = MasterFilesTestHelper.CreateTrigger(workRequest2, AutoEvents.JobCloseCode, triggerCondition: EventReferenceConditionList.Codes.EventReferenceWithWildcards, triggerConditionValue: "Propagated*");

			Factory.Save();

			AssertEquals(ZDateTime.Empty, trigger1.P9_ActualDate);
			AssertEquals(ZDateTime.Empty, trigger2.P9_ActualDate);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem3.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem4.WKI_Status);

			MasterFilesTestHelper.RunLogWalker();
			MasterFilesTestHelper.AssertNoEventRaised(workRequest1, AutoEvents.JobCloseCode);
			MasterFilesTestHelper.AssertEventRaised(workRequest2, AutoEvents.JobCloseCode, "Propagated: All Work Items");

			trigger1.Reload();
			trigger2.Reload();

			AssertEquals("Event has not fired on all attached work items yet, so should not propagate to the 'parent' work request.", ZDateTime.Empty, trigger1.P9_ActualDate);
			AssertEquals("Event should have propagated to the 'parent' work request since all attached work items have been completed.", ZDateTime.Now, trigger2.P9_ActualDate);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem1.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem2.WKI_Status);

			MasterFilesTestHelper.RunLogWalker();
			MasterFilesTestHelper.AssertEventRaised(workRequest1, AutoEvents.JobCloseCode, "Propagated: All Work Items");
			MasterFilesTestHelper.AssertEventRaised(workRequest2, AutoEvents.JobCloseCode, "Propagated: All Work Items");
			MasterFilesTestHelper.AssertNoEventRaised("There's no point propagating the JOP event because this event only matters when any single WI was opened, not when 'all' are opened", workRequest1, AutoEvents.JobOpenCode);
			MasterFilesTestHelper.AssertNoEventRaised("There's no point propagating the JOP event because this event only matters when any single WI was opened, not when 'all' are opened", workRequest2, AutoEvents.JobOpenCode);

			trigger1.Reload();
			trigger2.Reload();

			AssertEquals("Event should have now propagated to the 'parent' work request since all attached work items have been completed.", ZDateTime.Now, trigger1.P9_ActualDate);
		}

		#endregion

		#region Parent/Child Events

		[TestDate(2015, 7, 14)]
		public void TestEventsRaisedOnWorkItem_ShouldFireTriggerOnWorkRequest_SimpleExample()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem);

			var trigger = MasterFilesTestHelper.CreateTrigger(workRequest, AutoEvents.TagWasAddedOrRemovedCode);

			Factory.Save();

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			workItem.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestEventsRaisedOnWorkItem_ShouldFireTriggerOnWorkRequest_RealWorldRepresentativeExample()
		{
			var workItemUniversalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var workRequestUniversalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);
			var trigger1_workScheduled = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.AttachedCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "Source.HasIncompleteWorkItems");
			var trigger2_workCompleted = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.JobCloseCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "!TriggerSource.HasAttachedWorkItemsAndAllAreCancelled && Event.Reference.Contains(\"Propagated\")");
			var trigger2a_completedWorkItemAttached = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.AttachedCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "TriggerSource.HasAttachedWorkItemsAndAllAreCompleted && Event.Source.Contains(\"Customer Service Ticket\")");
			var trigger3_workCancelled = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.JobCloseCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "TriggerSource.HasAttachedWorkItemsAndAllAreCancelled && Event.Reference.Contains(\"Propagated\")");
			var trigger3a_cancelledWorkItemAttached = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.AttachedCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "TriggerSource.HasAttachedWorkItemsAndAllAreCancelled && Event.Source.Contains(\"Customer Service Ticket\")");
			var trigger4_workReOpened = MasterFilesTestHelper.CreateTemplateTrigger(workRequestUniversalTemplate, AutoEvents.JobOpenCode, triggerCondition: EventReferenceConditionList.Codes.ConditionWithMacros, triggerConditionValue: "TriggerSource.WereAllWorkItemsPreviouslyCompleted && Event.Source.Contains(\"WI\")");

			var workItemTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "AAA");
			MasterFilesTestHelper.CreateTask(workItemTemplate);

			var workRequestTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode);
			MasterFilesTestHelper.CreateTask(workRequestTemplate);

			Factory.Save();

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory, workItemType: "AAA");
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory, workItemType: "AAA");

			var alreadyClosedWorkItem = ProcessMgmtTestHelper.CreateWorkItem(Factory, summary: "Already closed");
			var alreadyCancelledWorkItem = ProcessMgmtTestHelper.CreateWorkItem(Factory, summary: "Already cancelled");

			MasterFilesTestHelper.CreateTask(alreadyClosedWorkItem, status: ProcessTaskStatusCodeList.Codes.Closed);
			MasterFilesTestHelper.CreateTask(alreadyCancelledWorkItem, status: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem1.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem2.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, alreadyClosedWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, alreadyCancelledWorkItem.WKI_Status);

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: false);

			TestDateAttribute.AddDays(1);
			workRequest.RelatedItems.Add(workItem1);
			workRequest.RelatedItems.Add(workItem2);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: false);

			TestDateAttribute.AddDays(1);
			workItem1.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			workItem2.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 16));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: false);

			TestDateAttribute.AddDays(1);
			workItem1.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 16));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 17));

			TestDateAttribute.AddDays(1);
			workItem1.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled);
			workItem2.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 16));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 18));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 17));

			TestDateAttribute.AddDays(1);
			workRequest.RelatedItems.RemoveAll();
			workRequest.RelatedItems.Add(alreadyClosedWorkItem);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 16));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 19));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 18));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: false);
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 17));

			TestDateAttribute.AddDays(1);
			workRequest.RelatedItems.RemoveAll();
			workRequest.RelatedItems.Add(alreadyCancelledWorkItem);
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger1_workScheduled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 15));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2_workCompleted, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 16));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger2a_completedWorkItemAttached, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 19));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3_workCancelled, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 18));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger3a_cancelledWorkItemAttached, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 20));
			MasterFilesTestHelper.AssertUniversalTriggerFired(trigger4_workReOpened, workRequest, expectingTriggerToHaveFired: true, expectedFiringTime: new ZDateTime(2015, 7, 17));
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode;

		protected override WorkRequest GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(factory);
		}

		IBMTestHelper BMSTestHelper { get; } = ObjectFactory.Get<IBMTestHelper>();

		#endregion
	}
}

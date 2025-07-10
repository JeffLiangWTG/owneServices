using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggerFiringEventsTest : WorkflowTestCase
	{
		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_WhenUniversalTemplatesDisabledInRegistry()
		{
			WorkflowDataRegistry.Instance.EnableUniversalTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			AssertHasWarning(template.P0_IsUniversalInfo, "Universal Templates are disabled in the registry. Triggers defined on this template will neither appear on jobs nor be fired by matching events. See registry item [Workflow Manager/Workflow Templates -> Enable Universal Templates].");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals("Should not ghost trigger onto job when Universal Templates are disabled in the registry.", 0, job.WorkflowItems.Triggers.Count);

			var log = job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();

			AssertNull(jobTrigger);
		}

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_AndCancelEvent_ShouldCreateJobTriggerLink()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).Single();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(new ZDateTime(2015, 7, 14), ((IWorkflowTrigger)jobTrigger).LastFiredTime.ToDateTime());
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single().P9_ActualDate.ToZDateTime());

			log.Cancel();
			Factory.Save();

			AssertEquals(false, jobTrigger.IsDeleted);
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single().P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_ShouldCreateJobTriggerLink_AndFireTriggerAction()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).Single();
			var wteEvent = jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).SingleOrDefault();

			AssertNotNull(wteEvent);
			AssertEquals(new ZDateTime(2015, 7, 14), ((IWorkflowTrigger)jobTrigger).LastFiredTime.ToDateTime());
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			RunLogWalker();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTriggerFiredByManuallyFiringMilestone()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			CreateTrigger(template, AutoEvents.CustomisableEvent00Code);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
			//I made a mistake so I need to change the date
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddMonths(-10);
			var updatedDate = milestone.P9_ActualDate;

			Factory.Save();

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).Single();
			var triggerWTE = jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).SingleOrDefault();
			milestone = new BusinessObjectFactory().Load<ProcessTask>(milestone.PK);
			var milestoneWTE = milestone.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).SingleOrDefault();

			CombineAssertions("The milestone way manually fired and then changed before save. The updated date should be reflected in the LastFiredTime and WTE events", () =>
			{
				AssertNotNull(triggerWTE);
				AssertEquals(updatedDate, ((IWorkflowTrigger)jobTrigger).LastFiredTime.ToDateTime());
				AssertEquals(updatedDate, triggerWTE.SL_EventTime);

				AssertNotNull(milestoneWTE);
				AssertEquals(updatedDate, ((IWorkflowTrigger)milestone).LastFiredTime.ToDateTime());
				AssertEquals(updatedDate, milestoneWTE.SL_EventTime);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_WhenUniversalTriggerDefinedOnNonMatchingTemplate_ShouldNotFire()
		{
			var template1 = CreateTemplate(Factory, "DUM", isUniversal: true);
			template1.P0_SubType2 = "NOP";
			var trigger1 = CreateTrigger(template1, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction1 = CreateTriggerAction(trigger1, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			var template2 = CreateTemplate(Factory, "DUM", isUniversal: true);
			template2.P0_SubType2 = "YEP";
			var trigger2 = CreateTrigger(template2, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
			var triggerAction2 = CreateTriggerAction(trigger2, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "that@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_Code = "YEP";
			Factory.Save();

			var log = job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			var jobTrigger = Factory.CreateNewFactory().Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();

			AssertNull("Should not fire for triggers whose template does not match the job's selection criteria", jobTrigger);
		}

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_WhenUniversalTriggerDefinedOnNonMatchingTemplate_ForEventsDeferredToLogWalker_ShouldNotFire()
		{
			var template1 = CreateTemplate(Factory, "DUM", isUniversal: true);
			template1.P0_SubType2 = "NOP";
			var trigger1 = CreateTrigger(template1, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction1 = CreateTriggerAction(trigger1, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			var template2 = CreateTemplate(Factory, "DUM", isUniversal: true);
			template2.P0_SubType2 = "YEP";
			var trigger2 = CreateTrigger(template2, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
			var triggerAction2 = CreateTriggerAction(trigger2, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "that@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_Code = "YEP";
			Factory.Save();

			var log = job.Logs.AddNew(new EventValue(AutoEvents.TagWasAddedOrRemoved, deferFiringWorkflow: true));
			Factory.Save();

			RunLogWalker(); // Once to create a WTE from the delay-fired event.
			RunLogWalker(); // And again to process the WTE.

			var jobTrigger = Factory.CreateNewFactory().Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();

			AssertNull("Should not fire for triggers whose template does not match the job's selection criteria", jobTrigger);
		}

		[TestDate(2015, 7, 14)]
		public void TestFireEventOnJob_WhenDelayFiringTrigger_ShouldCreateJobTriggerLink_AndFireTriggerAction()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = job.Logs.AddNew(new EventValue(AutoEvents.TagWasAddedOrRemoved, deferFiringWorkflow: true));

			Factory.Save();

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();

			AssertNull(jobTrigger);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			RunLogWalker();

			jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();

			AssertNotNull(jobTrigger);
			AssertEquals(new ZDateTime(2015, 7, 14), ((IWorkflowTrigger)jobTrigger).LastFiredTime.ToDateTime());
			AssertEquals("Runner recurs and just does this immediately.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 14)]
		public void TestEventFired_GhostedTriggerShouldIncludeEventTime()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobTrigger = CreateJobTriggerLink(trigger, job);

			Factory.Save();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			var ghostedTrigger = (ProcessTask)job.WorkflowItems.TriggersIncludingRelated.Single();

			var log = job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 14), ghostedTrigger.P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestActivateUniversalTrigger_WhenEventAlreadyExistsOnJob_ShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			trigger.P9T_IsActive = false;

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			trigger.P9T_IsActive = true;
			Factory.Save();

			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());

			var log = job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestActivateUniversalTemplate_WhenEventAlreadyExistsOnJob_ShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			template.P0_IsActive = false;

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			template.P0_IsActive = true;
			Factory.Save();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());

			var log = job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTemplate_WithApplyStartDate_WhenEventAlreadyExistsOnJob_ShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			template.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());

			var log = job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 15), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTemplate_WithEndDate_ShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			template.P0_EffectiveEndDateUtc = ZDateTime.UtcNow.AddDays(-1);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1);

			job.ApplyWorkflowTemplates();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());

			var log = job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 13), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTemplate_WhenProcessJobTriggerLinkTriggerFiredCountdownIsZero_ThenTriggerActionShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			var jobTriggerLink = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();
			AssertNotNull("Precondition: Expected adding log to job would create a job version of the trigger (ProcessJobTriggerLink) from the universal template", jobTriggerLink);

			var workflowTriggerEvents = jobTriggerLink.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));

			Factory.Save();
			RunLogWalker();

			CombineAssertions("Expected trigger to have fired when TriggerFiredCountdown is by default above zero", () =>
			{
				AssertNotNull("WorkflowTriggerEvent:", workflowTriggerEvents.SingleOrDefault());
				AssertEquals("Expected only a single WorkflowTriggerEvent", 1, workflowTriggerEvents.Length);
				AssertEquals("Expected trigger action of sending an email, to have fired only once", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			});

			((IWorkflowTrigger)jobTriggerLink).TriggerFiredCountdown = 0;
			job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();
			RunLogWalker();

			jobTriggerLink = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();
			workflowTriggerEvents = jobTriggerLink.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));

			CombineAssertions("Expected trigger not to have fired when TriggerFiredCountdown is zero", () =>
			{
				AssertEquals("Expected only a single WorkflowTriggerEvent", 1, workflowTriggerEvents.Length);
				AssertEquals("Expected ProjectJobTriggerLink trigger fired countdown to be zero", (ZShort)0, jobTriggerLink.P9L_TriggerFiredCountdown);
				AssertEquals("Expected trigger fired only once", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTemplate_WhenProcessJobTriggerLinkTriggerFiredCountdownReachesZero_ThenTriggerActionShouldNotFire()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			trigger.P9T_TriggerFiredCountdown = 150;
			CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			for (int i = 200; i > 0; i--)
			{
				job.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
			}

			Factory.Save();

			var jobTriggerLink = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).SingleOrDefault();
			AssertNotNull("Precondition: Expected adding log to job would create a job version of the trigger (ProcessJobTriggerLink) from the universal template", jobTriggerLink);

			var workflowTriggerEvents = jobTriggerLink.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));

			Factory.Save();
			RunLogWalker();

			AssertEquals("Expected 150 events in WorkflowTriggerEvents", 150, workflowTriggerEvents.Length);
			AssertEquals("Expected ProjectJobTriggerLink trigger fired countdown to be zero", (ZShort)0, jobTriggerLink.P9L_TriggerFiredCountdown);
			AssertEquals("Expected ProcessTemplateTrigger trigger fired countdown to be unmodified", (ZShort)150, trigger.P9T_TriggerFiredCountdown);
			AssertEquals("Expected trigger action of sending an email, to have fired 150x", 150, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTemplate_WhenMultipleProcessJobTriggerLink_ThenEachProcessTriggerLinkHasItsOwnTriggerCountdown()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			trigger.P9T_TriggerFiredCountdown = 150;
			CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "this@dave.east.com");

			Factory.Save();

			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var job3 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			for (int i = 25; i > 0; i--)
			{
				job1.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
			}

			for (int i = 50; i > 0; i--)
			{
				job2.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
			}

			for (int i = 75; i > 0; i--)
			{
				job3.Logs.AddNew(AutoEvents.TagWasAddedOrRemoved);
			}

			Factory.Save();

			var jobTriggerLink1 = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job1.PK)).SingleOrDefault();
			var jobTriggerLink2 = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job2.PK)).SingleOrDefault();
			var jobTriggerLink3 = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job3.PK)).SingleOrDefault();
			AssertNotNull("Precondition: Expected adding log to job would create a job version of the trigger (ProcessJobTriggerLink) since there's a universal template", jobTriggerLink1);
			AssertNotNull("Precondition: Expected adding log to job would create a job version of the trigger (ProcessJobTriggerLink) since there's a universal template", jobTriggerLink2);

			var workflowTriggerEvents1 = jobTriggerLink1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));
			var workflowTriggerEvents2 = jobTriggerLink2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));
			var workflowTriggerEvents3 = jobTriggerLink3.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode));

			Factory.Save();
			RunLogWalker();

			CombineAssertions("Expected ProcessJobTriggerLink to have decremented the number of times it fired. They're countdown before was 150, created from the universal template.", () =>
			{
				AssertEquals("jobTriggerLink1:", (ZShort)125, jobTriggerLink1.P9L_TriggerFiredCountdown);
				AssertEquals("jobTriggerLink2:", (ZShort)100, jobTriggerLink2.P9L_TriggerFiredCountdown);
				AssertEquals("jobTriggerLink3:", (ZShort)75, jobTriggerLink3.P9L_TriggerFiredCountdown);
			});

			CombineAssertions("Expected workflow to have 25x TagWasAddedOrRemoved events on them each.", () =>
			{
				AssertEquals("workflowTriggerEvents1:", 25, workflowTriggerEvents1.Length);
				AssertEquals("workflowTriggerEvents2:", 50, workflowTriggerEvents2.Length);
				AssertEquals("workflowTriggerEvents2:", 75, workflowTriggerEvents3.Length);
			});

			AssertEquals("Expected trigger action fire 150x", 150, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Expected universal countdown to be 150. Universal template trigger countdowns are not modified when ProcessJobTriggerLinks are fired.", 150, (int)trigger.P9T_TriggerFiredCountdown);
		}

		[TestDate(2015, 7, 14)]
		public void TestCreateNewUniversalTrigger_WhenEventAlreadyExistsOnJob_ShouldNotFire()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			Factory.Save();

			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);

			Factory.Save();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());

			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		public void TestMCR_Firing_LastEvent()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);
			trigger.TriggerConditions.TriggerCondition = "MCR";
			trigger.TriggerConditions.TriggerConditionValue = @"Variance(Event.EventTime, Trigger.PreviousEventDate).TotalMinutes>100.0";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			CreateJobTriggerLink(trigger, job);

			Factory.Save();

			var ghostedTrigger = (ProcessTask)job.WorkflowItems.TriggersIncludingRelated.Single();

			var log1 = job.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
			AssertEquals(log1.SL_EventTimeOffset, ghostedTrigger.P9_ActualDateForBinding);
			job.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
			AssertEquals(log1.SL_EventTimeOffset, ghostedTrigger.P9_ActualDateForBinding);
		}

		[TestDate(2015, 7, 14)]
		public void TestDelayFiringTriggerUntilAfterEverythingElseIsDoneOnSave()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"<Z0_Description>\"==\"West Virginia\"";

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithOnSaving>();
			dummy.WorkflowItems.TriggersIncludingRelated.Rebuild();

			Factory.Save();

			dummy.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, dummy.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Now, dummy.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate.ToZDateTime());
		}

		class DummyWithOnSaving : DummyWithWorkflow
		{
			public DummyWithOnSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				Logs.AddNew(AutoEvents.CustomisableEvent00, "Country rooooaaaadd");
				Z0_Description = "West Virginia";
				base.OnSaving();
			}
		}

		#region Template Selection Criteria and Fallbacks

		public void TestFireEvent_WhenTriggerDefinedOnTemplateWithMoreSpecificSelectionCriteria_NeverFallBack()
		{
			var baseTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.NeverFallback);
			var moreSpecificTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "CCR", triggerFallbackMethod: FallbackTypeList.Codes.NeverFallback);

			var baseTrigger = CreateTrigger(baseTemplate, AutoEvents.TagWasAddedOrRemovedCode);

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry1.O1_EnquiryType = "INQ";
			enquiry2.O1_EnquiryType = "CCR";

			Factory.Save();

			enquiry1.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry2.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);

			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry1, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));
			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry2, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));

			AssertUniversalTriggerFired(enquiry1, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired("TAG event should not have fired because the more-specific template takes precedence, even though it has no triggers", enquiry2, AutoEvents.TagWasAddedOrRemovedCode, shouldHaveFired: false);
		}

		public void TestFireEvent_WhenTriggerDefinedOnTemplateWithMoreSpecificSelectionCriteria_NeverFallBack_AndTemplateConditionExcludesOneJob()
		{
			var baseTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.NeverFallback);
			var moreSpecificTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "CCR", triggerFallbackMethod: FallbackTypeList.Codes.NeverFallback);

			var baseTrigger = CreateTrigger(baseTemplate, AutoEvents.TagWasAddedOrRemovedCode);
			var moreSpecificTrigger = CreateTrigger(moreSpecificTemplate, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
			moreSpecificTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			moreSpecificTrigger.TemplateConditions.TemplateCondition2Value = @"""<O1_City>""==""FakeNews""";

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry1.O1_EnquiryType = "CCR";
			enquiry2.O1_EnquiryType = "CCR";
			enquiry2.O1_City = "FakeNews";

			Factory.Save();

			enquiry1.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry1.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);
			enquiry2.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry2.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);

			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry1, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));
			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry2, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));

			AssertUniversalTriggerFired("TAG event should not have fired because the more-specific template takes precedence, even though its trigger is excluded by a Template Condition", enquiry1, AutoEvents.TagWasAddedOrRemovedCode, shouldHaveFired: false);
			AssertUniversalTriggerFired("XFR event should not have fired because the Template Condition excludes it from this job", enquiry1, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode, shouldHaveFired: false);

			AssertUniversalTriggerFired("TAG event should not have fired because the more-specific template takes precedence", enquiry2, AutoEvents.TagWasAddedOrRemovedCode, shouldHaveFired: false);
			AssertUniversalTriggerFired(enquiry2, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
		}

		public void TestFireEvent_WhenTriggerDefinedOnTemplateWithMoreSpecificSelectionCriteria_AlwaysFallBack()
		{
			var baseTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback);
			var moreSpecificTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "CCR", triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback);

			var baseTrigger = CreateTrigger(baseTemplate, AutoEvents.TagWasAddedOrRemovedCode);
			var moreSpecificTrigger = CreateTrigger(moreSpecificTemplate, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry1.O1_EnquiryType = "INQ";
			enquiry2.O1_EnquiryType = "CCR";

			Factory.Save();

			enquiry1.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry1.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);
			enquiry2.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry2.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);

			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry1, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));
			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry2, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));

			AssertUniversalTriggerFired(enquiry1, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired("XFR event should not have fired because this template is excluded from the job by the SubType1", enquiry1, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode, shouldHaveFired: false);

			AssertUniversalTriggerFired(enquiry2, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired(enquiry2, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
		}

		public void TestFireEvent_WhenTriggerDefinedOnTemplateWithMoreSpecificSelectionCriteria_AlwaysFallBack_AndTemplateConditionExcludesOneJob()
		{
			var baseTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback);
			var moreSpecificTemplate = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "CCR", triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback);

			var baseTrigger = CreateTrigger(baseTemplate, AutoEvents.TagWasAddedOrRemovedCode);
			var moreSpecificTrigger = CreateTrigger(moreSpecificTemplate, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
			moreSpecificTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			moreSpecificTrigger.TemplateConditions.TemplateCondition2Value = @"""<O1_City>""==""FakeNews""";

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry1.O1_EnquiryType = "CCR";
			enquiry2.O1_EnquiryType = "CCR";
			enquiry2.O1_City = "FakeNews";

			Factory.Save();

			enquiry1.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry1.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);
			enquiry2.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			enquiry2.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);

			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry1, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));
			AssertContainsExactElementsInAnyOrder(new[] { baseTemplate, moreSpecificTemplate }, new ProcessTaskTemplate.Loader(Factory).FindMatches(enquiry2, includeUniversalTemplates: true, includeOnlyUniversalTemplates: true));

			AssertUniversalTriggerFired(enquiry1, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired("XFR event should not have fired because the Template Condition excludes it from this job", enquiry1, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode, shouldHaveFired: false);

			AssertUniversalTriggerFired(enquiry2, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired(enquiry2, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
		}

		#endregion
	}
}

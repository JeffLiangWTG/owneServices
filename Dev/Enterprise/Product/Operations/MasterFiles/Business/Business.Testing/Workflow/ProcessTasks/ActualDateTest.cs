using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestDate(2016, 11, 11)]
	abstract class ActualDateTest : TemplateApplicationTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyDummyWithWorkflow = new Lazy<DummyWithWorkflow>(() => Factory.NewWithValidTestData<DummyWithWorkflow>());

			Template = CreateDummyTemplate();
		}

		Lazy<DummyWithWorkflow> LazyDummyWithWorkflow { get; set; }
		protected DummyWithWorkflow Dummy => LazyDummyWithWorkflow.Value;
		protected ProcessTaskTemplate Template { get; private set; }

		protected ProcessTaskTemplate CreateDummyTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			Factory.Save();
			return template;
		}

		int taskNameCounter;
		string GenTaskName(string code) => code + taskNameCounter++;

		protected ProcessTask AddTemplateTriggerable(Event evnt, bool hasNotification = true, ProcessTaskTemplate template = null)
		{
			var triggerable = AddTemplateTriggerable(template ?? Template);
			triggerable.P9_Description = GenTaskName(evnt.Code);
			triggerable.TriggerConditions.TriggerEventCode = evnt.Code;

			if (hasNotification)
			{
				AddNotification(triggerable);
			}

			return triggerable;
		}

		protected ProcessTaskNotification AddNotification(ProcessTask task)
		{
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailText = "HAGBAGHAGBAG";
			notification.PQ_EmailAddr = "bigbadcheese@kodo.com";

			return notification;
		}

		protected abstract ProcessTask AddTemplateTriggerable(ProcessTaskTemplate template);

		protected abstract IEnumerable<ProcessTask> GetTriggerables(IWorkflowProvider provider);

		protected abstract bool IsAllowedToFireMultipleTimes { get; }

		protected abstract bool SupportsActualDateUpdateType { get; }

		#endregion

		public void TestHasTemplate()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();

			var trigger = GetTriggerables(Dummy).Single();
			AssertEquals(Events.CustomisableEvent00.Code, trigger.TriggerConditions.TriggerEventCode);
			AssertEquals(template.PK, trigger.P9_ParentTemplateID);
		}

		public void TestCreateEventFromMilestone_InvalidEventSyntax()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			if (trigger.IsWorkflowTrigger)
			{
				// Only milestones can be set via actual date so, this test is skipped for triggers
				Assert(true);
				return;
			}

			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "|SNOT=GOO|SNOT=FLEM";
			AssertNoExceptionThrown(() => trigger.SetMilestoneActualDateForTest(ZDateTime.Now));
			AssertProcessTaskFired("The trigger should fire as the new event is created", trigger);
		}

		public void TestCreateEventAfterCreatingTrigger()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();

			var trigger = GetTriggerables(Dummy).Single();
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertProcessTaskFired("The trigger should fire. This is the normal use case.", trigger);
		}

		public void TestCreateEventBeforeCreatingTrigger_UnsavedEvent()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();

			var trigger = GetTriggerables(Dummy).Single();
			AssertProcessTaskFired("The trigger should fire because the event was not in the database", trigger);
		}

		public void TestCreateEventBeforeCreatingTrigger_SavedEvent()
		{
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();

			var trigger = GetTriggerables(Dummy).Single();
			if (trigger.IsMilestone)
			{
				AssertProcessTaskFired("Milestones are supposed to fire though right?", trigger);
			}
			else
			{
				AssertProcessTaskNotFired("The default system behaviour is not to fire the trigger.", trigger);
			}
		}

		public void TestRecursiveDDAEventHackery_DontExplode()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocatedCode;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			Dummy.Logs.AddNew(Events.DocumentAllocated);
			AssertEquals(1, trigger.GetWTELogs().Length);
		}

		[TestDate(2016, 12, 20)]
		public void TestCreateRecreateOrUpdate_NoSave()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);
			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(1));
			AssertEquals(ZDateTime.UtcNow.AddMinutes(1), trigger.P9_ActualDate);
			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(2));
			AssertEquals(ZDateTime.UtcNow.AddMinutes(2), trigger.P9_ActualDate);
			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(3));
			AssertEquals(ZDateTime.UtcNow.AddMinutes(3), trigger.P9_ActualDate);

			AssertProcessTaskFired("And only fire once.", trigger);
		}

		[TestDate(2016, 12, 20)]
		public void TestCreateRecreateOrUpdate_WithSave()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(1));
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow.AddMinutes(1), trigger.P9_ActualDate);

			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(2));
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow.AddMinutes(IsAllowedToFireMultipleTimes ? 2 : 1), trigger.P9_ActualDate);

			Dummy.Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Actual, ZDateTimeOffset.UtcNow.AddMinutes(3));
			Factory.Save();
			AssertEquals(ZDateTime.UtcNow.AddMinutes(IsAllowedToFireMultipleTimes ? 3 : 1), trigger.P9_ActualDate);

			AssertEquals(IsAllowedToFireMultipleTimes ? 3 : 1, trigger.GetWTELogs().Length);
		}

		public void TestTemplateFallback_UDFConditionsFallbackCorrectly()
		{
			AssertUDFFallback(true);
		}

		public void TestTemplateFallback_Baseline()
		{
			AssertUDFFallback(false);
		}

		public void AssertUDFFallback(bool shouldApplySequentially)
		{
			Template.P0_SubType1 = "BOB";
			Template.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var templateTrigger = AddTemplateTriggerable(Events.CustomisableEvent00);
			templateTrigger.P9_Description = "firstTask";
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "1==1";
			Dummy.SubType1 = "BOB";

			if (shouldApplySequentially)
			{
				Dummy.ApplyWorkflowTemplates();
				AssertNotNull(GetTriggerables(Dummy).Single());
			}

			var template2 = CreateDummyTemplate();
			var templateTrigger2 = AddTemplateTriggerable(Events.CustomisableEvent01, template: template2);
			templateTrigger2.P9_Description = "secondTask";
			templateTrigger2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger2.TemplateConditions.TemplateCondition2Value = "1==1";

			Dummy.ApplyWorkflowTemplates();
			var tasks = GetTriggerables(Dummy).Select(t => (string)t.P9_Description);
			AssertCollectionContains("firstTask", tasks);
			AssertCollectionContains("secondTask", tasks);
		}

		public void TestCancelIncreasesCountdown()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			var log1 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
			log1.Cancel();
			AssertEquals(100, (int)trigger.P9_TriggerFiredCountdown);
		}

		public void TestTriggerTwice_CancelFirst()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			var log1 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			log1.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestWhenTriggerFiredCountdownIsZero_DoNotFireTrigger()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerFiredCountdown = 0;
			Dummy.Logs.AddNew(Events.CustomisableEvent00);

			AssertProcessTaskNotFired("Expected trigger not to fire when TriggerFiredCountdown is zero.", trigger);
		}

		public void TestWhenTriggerFiredCountdownIsAboveZero_FireTrigger()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerFiredCountdown = 1;
			Dummy.Logs.AddNew(Events.CustomisableEvent00);

			AssertProcessTaskFired("Expected trigger to fire when TriggerFiredCountdown is above zero.", trigger);
		}

		public void TestTriggerTwice_CancelIncreasesCountdown()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			var log1 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals(98, (int)trigger.P9_TriggerFiredCountdown);
			log1.Cancel();
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		public void TestTriggerTwice_CancelSecond()
		{
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			log2.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestParentWorkflowProviders()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			dummy2.Logs.AddNew(Events.CustomisableEvent00);

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestTriggerTwice_TwoJobs_JobWithTriggerFirst_CancelFirst()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			var log = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy2.Logs.AddNew(Events.CustomisableEvent00);
			log.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestDeleteTwoEventsInTheMiddleOfSave()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			var log1 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			var log3 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			using (Factory.OnSaveDelayer.TemporaryChangeToDelayStrategy())
			{
				log1.Delete();
				log2.Delete();
			}
			Factory.OnSaveDelayer.RunAllDelayed();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestTriggerTwice_TwoJobs_JobWithTriggerFirst_CancelSecond()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = dummy2.Logs.AddNew(Events.CustomisableEvent00);
			log2.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestTriggerTwice_TwoJobs_ParentProviderFirst_CancelFirst()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			dummy2.Logs.AddNew(Events.CustomisableEvent00);
			var log2 = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			log2.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestTriggerTwice_TwoJobs_ParentProviderFirst_CancelSecond()
		{
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ParentWorkflowProviders = new[] { Dummy };
			var trigger = MakeTrigger(Dummy, Events.CustomisableEvent00Code);
			var log = dummy2.Logs.AddNew(Events.CustomisableEvent00);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			log.Cancel();

			AssertProcessTaskFired("Should still have fired.", trigger);
		}

		public void TestProcessTaskUnfiredIfCountdownIs0()
		{
			var milestone = MakeMilestone(Dummy, Events.CustomisableEvent00Code);
			milestone.TriggerConditions.TriggerFiredCountdown = 1;
			Factory.Save();
			var log = Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertProcessTaskFired("Milestone should be fired", milestone);
			log.Cancel();
			AssertProcessTaskNotFired("Milestone should be unfired", milestone);
			AssertEquals("Countdown should be reset", (short)1, milestone.P9_TriggerFiredCountdown);
		}

		protected static void AssertProcessTaskFired(string message, ProcessTask task)
		{
			CombineAssertions(message, () =>
			{
				Assert("The task should have an actual date.", !task.P9_ActualDate.IsEmpty);
				AssertEquals("The task should have WTE events.", 1, task.GetWTELogs().Length);
			});
		}

		protected static void AssertProcessTaskNotFired(string message, ProcessTask task)
		{
			CombineAssertions(message, () =>
			{
				Assert("The task should have no actual date.", task.P9_ActualDate.IsEmpty);
				AssertEquals("The task should have no WTE events.", 0, task.GetWTELogs().Length);
			});
		}

		#region ActualDateUpdateType

		[TestDate(2016, 12, 20)]
		public void TestActualDateUpdateType_AD1()
		{
			if (!SupportsActualDateUpdateType)
			{
				Assert(true);
				return;
			}

			AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			trigger.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.AD1;
			trigger.TriggerConditions.TriggerFiredCountdown = 100;

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			CombineAssertions("The trigger should only fire once and update actual date once", () =>
			{
				var date = ZDateTime.UtcNow;
				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.ToOffset());
				Factory.Save();
				AssertEquals(date, trigger.P9_ActualDate);
				AssertEquals(1, trigger.GetWTELogs().Length);
				AssertEquals((ZShort)99, trigger.TriggerConditions.TriggerFiredCountdown);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(1).ToOffset());
				Factory.Save();
				AssertEquals(date, trigger.P9_ActualDate);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(2).ToOffset());
				Factory.Save();
				AssertEquals(date, trigger.P9_ActualDate);

				AssertEquals(1, trigger.GetWTELogs().Length);
				AssertEquals((ZShort)99, trigger.TriggerConditions.TriggerFiredCountdown);
			});
		}

		[TestDate(2016, 12, 20)]
		public void TestActualDateUpdateType_ALW()
		{
			if (!SupportsActualDateUpdateType)
			{
				Assert(true);
				return;
			}

			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			trigger.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.ALW;
			trigger.TriggerConditions.TriggerFiredCountdown = 100;

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			CombineAssertions("The trigger should fire for every matching event and update actual date each time", () =>
			{
				var date = ZDateTime.UtcNow;
				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.ToOffset());
				Factory.Save();
				AssertEquals(date, trigger.P9_ActualDate);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(1).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(1), trigger.P9_ActualDate);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(2).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(2), trigger.P9_ActualDate);

				AssertEquals(3, trigger.GetWTELogs().Length);
				AssertEquals((ZShort)97, trigger.TriggerConditions.TriggerFiredCountdown);

				trigger.TriggerConditions.TriggerFiredCountdown = 0;
				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(3).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(3), trigger.P9_ActualDate);
				AssertEquals(3, trigger.GetWTELogs().Length);
			});
		}

		[TestDate(2016, 12, 20)]
		public void TestActualDateUpdateType_UAD()
		{
			if (!SupportsActualDateUpdateType)
			{
				Assert(true);
				return;
			}

			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			trigger.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.UAD;
			trigger.TriggerConditions.TriggerFiredCountdown = 100;

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			CombineAssertions("The trigger should only fire for the first matching but actual date should be updated for each event", () =>
			{
				var date = ZDateTime.UtcNow;
				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.ToOffset());
				Factory.Save();
				AssertEquals(date, trigger.P9_ActualDate);
				AssertEquals(1, trigger.GetWTELogs().Length);
				AssertEquals((ZShort)99, trigger.TriggerConditions.TriggerFiredCountdown);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(1).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(1), trigger.P9_ActualDate);

				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(2).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(2), trigger.P9_ActualDate);

				AssertEquals((ZShort)99, trigger.TriggerConditions.TriggerFiredCountdown);

				trigger.TriggerConditions.TriggerFiredCountdown = 0;
				Dummy.Logs.AddNew(Events.CustomisableEvent00, date.AddMinutes(3).ToOffset());
				Factory.Save();
				AssertEquals(date.AddMinutes(3), trigger.P9_ActualDate);
				AssertEquals(1, trigger.GetWTELogs().Length);
			});
		}

		#endregion
	}

	class MilestoneActualDateTest : ActualDateTest
	{
		protected override bool IsAllowedToFireMultipleTimes => false;

		protected override bool SupportsActualDateUpdateType => true;

		protected override ProcessTask AddTemplateTriggerable(ProcessTaskTemplate template)
		{
			return template.WorkflowItems.Milestones.AddNew();
		}

		protected override IEnumerable<ProcessTask> GetTriggerables(IWorkflowProvider provider)
		{
			return provider.WorkflowItems.Milestones.Cast<ProcessTask>();
		}

		public void TestForBindingPropertyRegardsTimeZone()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var utcNow = new ZDateTime(ZDateTime.UtcNow, DateTimeKind.Unspecified);
				var trigger = MakeMilestone(Dummy);
				trigger.P9_ActualDateForBinding = new ZDateTimeOffset(utcNow.AddHours(2), TimeSpan.FromHours(2));

				AssertEquals(trigger.P9_ActualDate, utcNow.AddHours(2));
				AssertEquals(trigger.P9_ActualDateUtc, utcNow);
			}
		}

		public void TestImpossibleDateRange()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var trigger = MakeMilestone(Dummy);
				trigger.P9_ActualDateForBinding = new ZDateTimeOffset(21, 07, 03);
				Assert(!string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				ErrorReporter.Clear();
			}
		}

		public void TestDontErrorReportInvalidDateSetFromGUI()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var trigger = MakeMilestone(Dummy);
				trigger.P9_ActualDateForBinding = new ZDateTimeOffset(1754, 07, 03);
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				ErrorReporter.Clear();
			}
		}

		public void TestPastDateTimeEnteredByUser_ForMilestone_NewEventIsCreated()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();
			var trigger = GetTriggerables(Dummy).Single();
			AssertNoExceptionThrown(() => trigger.SetMilestoneActualDateForTest(ZDateTime.Now.AddMinutes(-1)));
			AssertProcessTaskFired("The trigger should fire as the new event is created", trigger);
		}

		public void TestMilestoneDoesntFireTwice_AD1()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_ActualDateUpdateType = ActualDateUpdateTypeCodeList.Codes.AD1;
			milestone.TriggerConditions.TriggerFiredCountdown = 100;
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);

			AssertEquals(1, milestone.GetWTELogs().Length);
			AssertEquals((short)99, milestone.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestEstimateDateEvent_EscapeAllRegexCharacters()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "*|OLD*";
			AddNotification(trigger);

			Dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now, isEstimate: true);

			AssertNotNull("Should be an EST event", Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.EstimatedDateChangedCode).Single());
			AssertProcessTaskNotFired("Don't fire the trigger since the event should not match the reference", trigger);
		}

		public void TestEstimateDateEventDoNotMatchEmpty()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "";
			AddNotification(trigger);

			Dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now, isEstimate: true);

			AssertNotNull("Should be an EST event", Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.EstimatedDateChangedCode).Single());
			AssertProcessTaskNotFired("Don't fire the trigger since the event should not match the reference", trigger);
		}

		public void TestCreateEvent_WorkflowTriggerEventCanFindEventSource()
		{
			var template = AddTemplateTriggerable(Events.CustomisableEvent00);
			Dummy.ApplyWorkflowTemplates();

			var trigger = GetTriggerables(Dummy).Single();
			trigger.P9_ActualDateForBinding = ZDateTimeOffset.Now;
			var log = Dummy.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code);
			var wteLog = trigger.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);

			AssertContains("Expect the log pk to be in the sl_reference of the WTE log.", log.PK.ToString(), wteLog.SL_Reference.ToString());
		}

		public void TestCreateEvent_WithRFW_ThenCancelEvent()
		{
			var condition = "Breathe*?Out";
			var milestone = MakeMilestone(Dummy.WorkflowItems, Events.CustomisableEvent00Code);
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			milestone.TriggerConditions.TriggerConditionValue = condition;
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
			var log = Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Single();
			AssertEquals(condition, log.SL_Reference);
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Empty;
			AssertEquals(true, log.IsDeleted);
		}

		public void TestCreateEvent_WithRFW_ThenSave_ThenCancelEvent()
		{
			var condition = "Breathe*?Out";
			var milestone = MakeMilestone(Dummy.WorkflowItems, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			milestone.TriggerConditions.TriggerConditionValue = condition;
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
			var log = Dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent00Code).Single();
			AssertEquals(condition, log.SL_Reference);

			Factory.Save();
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Empty;
			AssertEquals(true, log.IsCancelled);
		}
	}

	class TriggerActualDateTest : ActualDateTest
	{
		public void TestCreateMultipleEvent_WithNoSaveBetween_FireWorkflowEachTime()
		{
			var trigger = MakeTrigger(Dummy.WorkflowItems, Events.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerFiredCountdown = 100;
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertMilestoneFireCount(1, trigger);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertMilestoneFireCount(2, trigger);
			AssertEquals((short)98, trigger.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestCreateMultipleEvent_WithSaveBetween_FireWorkflowEachTime()
		{
			var trigger = MakeTrigger(Dummy.WorkflowItems, Events.CustomisableEvent00Code);
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertMilestoneFireCount(1, trigger);
			Factory.Save();
			Dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertMilestoneFireCount(2, trigger);
		}

		protected override bool IsAllowedToFireMultipleTimes => true;

		protected override bool SupportsActualDateUpdateType => false;

		protected override ProcessTask AddTemplateTriggerable(ProcessTaskTemplate template)
		{
			return template.WorkflowItems.Triggers.AddNew();
		}

		protected override IEnumerable<ProcessTask> GetTriggerables(IWorkflowProvider provider)
		{
			return provider.WorkflowItems.Triggers.Cast<ProcessTask>();
		}
	}
}

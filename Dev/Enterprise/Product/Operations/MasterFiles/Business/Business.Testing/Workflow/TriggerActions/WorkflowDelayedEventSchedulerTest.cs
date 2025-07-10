using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowDelayedEventSchedulerTest : TestCaseWithFactory
	{
		public void TestShouldQueueDelayedActionEveryTime()
		{
			AssertEquals("Precondition: GMT +10 hours", "AUBNE", Env.CurrentBranch.NKUNLOCO);
			const int timeZoneOffset = 10;

			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";
			var templateTrigger = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger.Description = "Test trigger";
			templateTrigger.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action = MasterFilesTestHelper.CreateTriggerAction(templateTrigger, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);

			var zeroOffset = new ZDateTime(2022, 1, 1);
			action.PQ_Offset = zeroOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var jobTriggerBO = dummyBO.WorkflowItems.TriggersIncludingRelated.SingleOrDefault();
			AssertNotNull(jobTriggerBO);
			var jobTrigger = (ProcessTask)jobTriggerBO;
			var jobAction = jobTrigger.ProcessTaskNotifications[0];
			Assert("Precondition: representation of a universal trigger", jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var localEventTime1 = new ZDateTime(2022, 6, 7);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(localEventTime1, TimeSpan.FromHours(timeZoneOffset)), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("DLY", actionScheduleProvider.LastScheduledActionCode);
			AssertEquals("UTC time based on the current branch", localEventTime1.AddHours(-timeZoneOffset), actionScheduleProvider.LastScheduledExecutionDateTimeUtc);
			AssertEquals(dummyBO.PK, actionScheduleProvider.LastScheduledTargetPk);
			AssertEquals("Z0", actionScheduleProvider.LastScheduledTargetTableCode);
			AssertEquals("|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var localEventTime2 = new ZDateTime(2022, 6, 8);
			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(localEventTime2, TimeSpan.FromHours(timeZoneOffset)), isEstimate: true);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("DLY", actionScheduleProvider.LastScheduledActionCode);
			AssertEquals("UTC time based on the current branch", localEventTime2.AddHours(-timeZoneOffset), actionScheduleProvider.LastScheduledExecutionDateTimeUtc);
			AssertEquals(dummyBO.PK, actionScheduleProvider.LastScheduledTargetPk);
			AssertEquals("Z0", actionScheduleProvider.LastScheduledTargetTableCode);
			AssertEquals("|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestScheduleTimeShouldBeUtcTime_BasedOnLocalEventTime_BranchTimeZome_AndActionOffset()
		{
			AssertEquals("Precondition: GMT +10 hours", "AUBNE", Env.CurrentBranch.NKUNLOCO);
			const int timeZoneOffset = 10;

			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var positiveOffset = new ZDateTime(2022, 1, 1).AddHours(1);
			action2.PQ_Offset = positiveOffset;

			var templateTrigger3 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent02Code;
			templateTrigger3.Description = "Test trigger 3";
			templateTrigger3.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action3 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger3, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var negativeOffset = new ZDateTime(2022, 1, 1).AddHours(-1);
			action3.PQ_Offset = negativeOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(3, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger3 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];
			var jobAction3 = jobTrigger3.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var localEventTime1 = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(localEventTime1, TimeSpan.FromHours(timeZoneOffset)), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("DLY", actionScheduleProvider.LastScheduledActionCode);
			AssertEquals("eventTime1 + zero offset - time zone offset", localEventTime1.AddHours(-timeZoneOffset), actionScheduleProvider.LastScheduledExecutionDateTimeUtc);
			AssertEquals(dummyBO.PK, actionScheduleProvider.LastScheduledTargetPk);
			AssertEquals("Z0", actionScheduleProvider.LastScheduledTargetTableCode);
			AssertEquals("|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var eventTime2 = new ZDateTime(2022, 6, 9);
			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime2, TimeSpan.FromHours(timeZoneOffset)), isEstimate: true);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("DLY", actionScheduleProvider.LastScheduledActionCode);
			AssertEquals("eventTime2 + positive offset - time zone offset", eventTime2.AddHours(1).AddHours(-timeZoneOffset), actionScheduleProvider.LastScheduledExecutionDateTimeUtc);
			AssertEquals(dummyBO.PK, actionScheduleProvider.LastScheduledTargetPk);
			AssertEquals("Z0", actionScheduleProvider.LastScheduledTargetTableCode);
			AssertEquals("|ACT=|EVT=Z01|OFF=001:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var eventTime3 = new ZDateTime(2022, 6, 10);
			var sourceLog3 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent02,
				new ZDateTimeOffset(eventTime3, TimeSpan.FromHours(timeZoneOffset)), isEstimate: true);
			var wteLog3 = new TestWTEQueuedLog(Factory, sourceLog3);

			scheduler = new WorkflowDelayedEventScheduler(jobAction3, dummyBO, wteLog3);
			scheduler.Process(null);

			AssertEquals(3, actionScheduleProvider.ScheduledCount);
			AssertEquals("DLY", actionScheduleProvider.LastScheduledActionCode);
			AssertEquals("eventTime3 + negative offset - time zone offset", eventTime3.AddHours(-1).AddHours(-timeZoneOffset), actionScheduleProvider.LastScheduledExecutionDateTimeUtc);
			AssertEquals(dummyBO.PK, actionScheduleProvider.LastScheduledTargetPk);
			AssertEquals("Z0", actionScheduleProvider.LastScheduledTargetTableCode);
			AssertEquals("|ACT=|EVT=Z02|OFF=-001:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeEventCodeIntoJsonParameter()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(2, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("Event Z00", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("Event Z01", "|ACT=|EVT=Z01|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeIsEstimateIntoJsonParameter()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(false);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(2, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("Estimate=Yes", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: false);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("Estimate=No", "|ACT=|EVT=Z01|OFF=000:00|EST=N|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeOffsetIntoJsonParameter()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var positiveOffset = new ZDateTime(2022, 1, 1).AddHours(1);
			action2.PQ_Offset = positiveOffset;

			var templateTrigger3 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent02Code;
			templateTrigger3.Description = "Test trigger 3";
			templateTrigger3.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action3 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger3, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var negativeOffset = new ZDateTime(2022, 1, 1).AddHours(-1);
			action3.PQ_Offset = negativeOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(3, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger3 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];
			var jobAction3 = jobTrigger3.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("Zero offset", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("Positive offset", "|ACT=|EVT=Z01|OFF=001:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog3 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent02,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog3 = new TestWTEQueuedLog(Factory, sourceLog3);

			scheduler = new WorkflowDelayedEventScheduler(jobAction3, dummyBO, wteLog3);
			scheduler.Process(null);

			AssertEquals(3, actionScheduleProvider.ScheduledCount);
			AssertEquals("Negative offset", "|ACT=|EVT=Z02|OFF=-001:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeActionReferenceIntoJsonParameter()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;
			action1.PQ_ActionReference = string.Empty;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;
			action2.PQ_ActionReference = "Success is ability to move from failure to failure without losing enthusiasm";

			var templateTrigger3 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent02Code;
			templateTrigger3.Description = "Test trigger 3";
			templateTrigger3.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action3 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger3, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action3.PQ_Offset = zeroOffset;
			action3.PQ_ActionReference = "A journey of a thousand miles begins with a single step";

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(3, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger3 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger3.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];
			var jobAction3 = jobTrigger3.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("Empty reference", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("Reference 1", "|ACT=Success is ability to move from failure to failure without losing enthusiasm|EVT=Z01|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog3 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent02,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog3 = new TestWTEQueuedLog(Factory, sourceLog3);

			scheduler = new WorkflowDelayedEventScheduler(jobAction3, dummyBO, wteLog3);
			scheduler.Process(null);

			AssertEquals(3, actionScheduleProvider.ScheduledCount);
			AssertEquals("Negative offset", "|ACT=A journey of a thousand miles begins with a single step|EVT=Z02|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeUserIntoJsonParameter()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(2, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("Test user", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			sourceLog2.SL_GS_NKUser = "US1";
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("User 1", "|ACT=|EVT=Z01|OFF=000:00|EST=Y|USR=US1|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
		}

		public void TestShouldIncludeBranchIntoJsonParameter_AndStoreInBranchColumn()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			Factory.Save();

			AssertEquals("Precondition", "BNE", Env.CurrentBranch.Code);

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(2, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("BNE branch in json parameter", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
			AssertEquals("BNE branch as execution branch", Env.CurrentBranchPK, actionScheduleProvider.LastScheduledExecutionBranch);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			sourceLog2.SL_GB_NKBranch = branch1.GB_Code;
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("BR1 branch in json parameter", "|ACT=|EVT=Z01|OFF=000:00|EST=Y|USR=E|BRN=BR1|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
			AssertEquals("BR1 branch as execution branch", branch1.PK, actionScheduleProvider.LastScheduledExecutionBranch);
		}

		public void TestShouldIncludeDepartmentIntoJsonParameter_AndStoreInDepartmentColumn()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "UniversalTemplate";

			var templateTrigger1 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger1.Description = "Test trigger 1";
			templateTrigger1.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action1 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger1, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			var zeroOffset = new ZDateTime(2022, 1, 1);
			action1.PQ_Offset = zeroOffset;

			var templateTrigger2 = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01Code;
			templateTrigger2.Description = "Test trigger 2";
			templateTrigger2.SetShouldTriggerOnEstimateEvents_ForTests(true);

			var action2 = MasterFilesTestHelper.CreateTriggerAction(templateTrigger2, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);
			action2.PQ_Offset = zeroOffset;

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";

			Factory.Save();

			AssertEquals("Precondition", "BRN", Env.CurrentDepartment.Code);

			var dummyBO = Factory.New<DummyWithWorkflow>();
			AssertEquals(2, dummyBO.WorkflowItems.TriggersIncludingRelated.Count);
			var jobTrigger1 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobTrigger2 = dummyBO.WorkflowItems.TriggersIncludingRelated.Find(new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode)).First() as ProcessTask;
			var jobAction1 = jobTrigger1.ProcessTaskNotifications[0];
			var jobAction2 = jobTrigger2.ProcessTaskNotifications[0];

			var actionScheduleProvider = new ActionScheduleProviderForTest();
			ObjectFactory.Substitute<IActionScheduleProvider>(actionScheduleProvider);

			var eventTime = new ZDateTime(2022, 6, 8);
			var sourceLog1 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent00,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			var wteLog1 = new TestWTEQueuedLog(Factory, sourceLog1);

			var scheduler = new WorkflowDelayedEventScheduler(jobAction1, dummyBO, wteLog1);
			scheduler.Process(null);

			AssertEquals(1, actionScheduleProvider.ScheduledCount);
			AssertEquals("BRN department in json parameter", "|ACT=|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=BRN", actionScheduleProvider.LastScheduledJsonParameter);
			AssertEquals("BRN branch as execution department", Env.CurrentDepartmentPK, actionScheduleProvider.LastScheduledExecutionDepartment);

			var sourceLog2 = dummyBO.GetLogs().AddNew(Events.CustomisableEvent01,
				new ZDateTimeOffset(eventTime), isEstimate: true);
			sourceLog2.SL_GE_NKDepartment = "DP1";
			var wteLog2 = new TestWTEQueuedLog(Factory, sourceLog2);

			scheduler = new WorkflowDelayedEventScheduler(jobAction2, dummyBO, wteLog2);
			scheduler.Process(null);

			AssertEquals(2, actionScheduleProvider.ScheduledCount);
			AssertEquals("DP1 department in json parameter", "|ACT=|EVT=Z01|OFF=000:00|EST=Y|USR=E|BRN=BNE|DEP=DP1", actionScheduleProvider.LastScheduledJsonParameter);
			AssertEquals("DP1 branch as execution department", department1.PK, actionScheduleProvider.LastScheduledExecutionDepartment);
		}
	}

	#region Test Classes

	class TestWTEQueuedLog : IQueuedLog
	{
		public TestWTEQueuedLog(BusinessObjectFactory factory, StmALog sourceLog)
		{
			Factory = factory;
			SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			SJ_EventTime = new ZDateTime(2022, 6, 1); // though posted time and event time for source events and wte events are the same, let's artificially use another wte event time to ensure it does not affect anything
			SJ_IsEstimate = false;
			SJ_Reference = $"{sourceLog.PK}|{sourceLog.SL_GB_NKBranch}|{sourceLog.SL_GE_NKDepartment}";
			SJ_GB_NKBranch = sourceLog.SL_GB_NKBranch;
			SJ_GE_NKDepartment = sourceLog.SL_GE_NKDepartment;
		}

		public BusinessObjectFactory Factory { get; private set; }

		public ZString SJ_SE_NKEvent { get; private set; }

		public ZDateTime SJ_EventTime { get; private set; }
		public ZDateTime SJ_EventTimeUtc { get; private set; }

		public ZBool SJ_IsEstimate { get; private set; }

		public ZString SJ_Reference { get; private set; }

		public ZGuid PK => throw new NotImplementedException();

		public ZGuid SJ_ParentID => throw new NotImplementedException();

		public ZGuid SJ_ALogReference => throw new NotImplementedException();

		public ZGuid SJ_TargetID => throw new NotImplementedException();

		public ZString SJ_GS_NKUser => throw new NotImplementedException();

		public ZString SJ_GB_NKBranch { get; private set; }

		public ZString SJ_GE_NKDepartment { get; private set; }

		public ZString SJ_ParentTableCode => throw new NotImplementedException();

		public bool IsRetry => throw new NotImplementedException();

		public ZBool SJ_IsDelayFired => throw new NotImplementedException();

		public ZDateTime SJ_PostedTimeUtc => throw new NotImplementedException();

		public ZByte SJ_RetryCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public ZString SJ_Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public IEnumerable<IStmChangeLog> ChangeLogs => throw new NotImplementedException();

		public ZString StaffCode => throw new NotImplementedException();

		public ZString UserCode => throw new NotImplementedException();

		public ZGuid Identifier => throw new NotImplementedException();

		public ZGuid ParentID => throw new NotImplementedException();

		public ZDateTime EventTime => throw new NotImplementedException();

		public ZDateTime EventTimeUtc => throw new NotImplementedException();

		public ZDateTimeOffset EventTimeOffset => StmALog.ToDateTimeOffset(Factory, SJ_EventTime, SJ_EventTimeUtc, SJ_GB_NKBranch);

		public ZDateTime PostedTimeUtc => throw new NotImplementedException();

		public ZString FriendlyTableName => throw new NotImplementedException();

		public ZString Source => throw new NotImplementedException();

		public ZString Reference => throw new NotImplementedException();

		public ZString SourceType => throw new NotImplementedException();

		public ZString DepartmentCode => SJ_GE_NKDepartment;

		public ZString BranchCode => SJ_GB_NKBranch;

		public ZString CompanyCode => throw new NotImplementedException();

		public ZBool IsEstimate => throw new NotImplementedException();

		public IPropagationSettings PropagationSettings => throw new NotImplementedException();

		public ZBool IsCancelled => throw new NotImplementedException();
	}

	class ActionScheduleProviderForTest : IActionScheduleProvider
	{
		public int ScheduledCount { get; private set; }
		public string LastScheduledActionCode { get; private set; }
		public ZDateTime LastScheduledExecutionDateTimeUtc { get; private set; }
		public ZGuid LastScheduledTargetPk { get; private set; }
		public string LastScheduledTargetTableCode { get; private set; }
		public string LastScheduledJsonParameter { get; private set; }
		public ZGuid? LastScheduledExecutionBranch { get; private set; }
		public ZGuid? LastScheduledExecutionDepartment { get; private set; }

		public IActionSchedule ScheduleAction(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranch = null,
			ZGuid? executionDepartment = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null)
		{
			ScheduledCount++;
			LastScheduledActionCode = actionCode;
			LastScheduledExecutionDateTimeUtc = executionDateTimeUtc;
			LastScheduledTargetPk = targetPk;
			LastScheduledTargetTableCode = targetTableCode;
			LastScheduledJsonParameter = jsonParameter;
			LastScheduledExecutionBranch = executionBranch;
			LastScheduledExecutionDepartment = executionDepartment;

			return null;
		}

		public void ChangeState(IActionSchedule schedule)
		{
			throw new NotImplementedException();
		}

		public ZQuery GetRunnableSchedulesQuery()
		{
			throw new NotImplementedException();
		}

		public IReadOnlyCollection<IActionSchedule> GetSchedules(BusinessObjectFactory factory, string actionCode, ZGuid targetPk, string targetTableCode, string jsonParameter = null, ZDateTime? scheduledLaterThanDateTimeUtc = null, int? batchSize = null)
		{
			throw new NotImplementedException();
		}

		public IActionSchedule ScheduleOrRescheduleAction(string actionCode, ZDateTime executionDateTimeUtc, ZGuid targetPk, string targetTableCode, string jsonParameter = null, ZGuid? executionBranchPk = null, ZGuid? executionDepartmentPk = null, string token = null, bool scheduleSuspended = false, BusinessObjectFactory factory = null)
		{
			throw new NotImplementedException();
		}

		public IActionSchedule ScheduleActionIfNotScheduled(string actionCode, ZDateTime executionDateTimeUtc, ZGuid targetPk, string targetTableCode, string jsonParameter = null, ZGuid? executionBranchPk = null, ZGuid? executionDepartmentPk = null, string token = null, bool scheduleSuspended = false, BusinessObjectFactory factory = null)
		{
			throw new NotImplementedException();
		}

		public void ChangeStatusByToken(string token, string newStatus)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}

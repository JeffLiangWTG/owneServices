using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	internal class ProcessTemplateTriggerJobRepresentationTest : WorkflowTestCase
	{
		public void TestUniversalTriggersJobRepresentation()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var universalTrigger1 = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);
			var universalTrigger2 = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent01Code);

			universalTrigger1.P9T_Description = "Universal trigger 1";
			universalTrigger1.P9T_Sequence = 10;
			universalTrigger1.P9T_IsEstimate = false;
			universalTrigger1.P9T_DelayDurationSeconds = 100;
			universalTrigger1.P9T_SuppressDuplicates = true;

			universalTrigger2.P9T_Description = "Universal trigger 2";
			universalTrigger2.P9T_Sequence = 20;
			universalTrigger2.P9T_IsEstimate = true;
			universalTrigger2.P9T_DelayDurationSeconds = 0;
			universalTrigger2.P9T_SuppressDuplicates = false;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			var workflowProvider = (IWorkflowProvider)bizo;
			var jobTrigger = workflowProvider.WorkflowItems.AddNew();
			jobTrigger.P9_Description = "Job trigger";
			jobTrigger.P9_Sequence = 5;
			jobTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			jobTrigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent02Code;
			jobTrigger.P9_DelayDurationSeconds = 0;
			jobTrigger.P9_SuppressDuplicates = false;

			Factory.Save();

			var triggers = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().ToArray();
			AssertEquals(3, triggers.Length);

			var trigger1 = triggers.Single(t => t.P9_Description == "Universal trigger 1");
			var trigger2 = triggers.Single(t => t.P9_Description == "Universal trigger 2");
			var trigger3 = triggers.Single(t => t.P9_Description == "Job trigger");

			AssertEquals(true, trigger1.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(10, trigger1.P9_Sequence);
			AssertEquals(false, trigger1.ShouldTriggerOnEstimateEvents);
			AssertEquals(100, trigger1.P9_DelayDurationSeconds);
			AssertEquals(true, trigger1.P9_SuppressDuplicates);

			AssertEquals(true, trigger2.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(20, trigger2.P9_Sequence);
			AssertEquals(true, trigger2.ShouldTriggerOnEstimateEvents);
			AssertEquals(0, trigger2.P9_DelayDurationSeconds);
			AssertEquals(false, trigger2.P9_SuppressDuplicates);

			AssertEquals(false, trigger3.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(5, trigger3.P9_Sequence);
			AssertEquals("Job triggers should have ShouldTriggerOnEstimateEvents set to false", false, trigger3.ShouldTriggerOnEstimateEvents);
			AssertEquals(0, trigger3.P9_DelayDurationSeconds);
			AssertEquals(false, trigger3.P9_SuppressDuplicates);
		}

		public void TestUniversalTriggersJobRepresentation_ShouldTakeTriggerFiredCountdownFromProcessJobTriggerLink_ForBothActualAndEstimateTriggers()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var actualUniversalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);
			var estimateUniversalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent01Code);

			actualUniversalTrigger.P9T_Description = "Actual universal trigger";
			actualUniversalTrigger.P9T_Sequence = 10;
			actualUniversalTrigger.P9T_IsEstimate = false;

			estimateUniversalTrigger.P9T_Description = "Estimate universal trigger";
			estimateUniversalTrigger.P9T_Sequence = 20;
			estimateUniversalTrigger.P9T_IsEstimate = true;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			Factory.Save();

			var workflowProvider = (IWorkflowProvider)bizo;
			var triggers = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().ToArray();

			var jobActualTrigger = triggers.Single(t => t.P9_Description == "Actual universal trigger");
			var jobEstimateTrigger = triggers.Single(t => t.P9_Description == "Estimate universal trigger");
			AssertNotNull(jobActualTrigger);
			AssertNotNull(jobEstimateTrigger);

			AssertEquals(true, jobActualTrigger.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(false, jobActualTrigger.ShouldTriggerOnEstimateEvents);
			AssertEquals(false, jobActualTrigger.HasProcessJobTriggerLink);
			AssertEquals("Precondition", (ZShort)100, ((ITriggerConditions)jobActualTrigger).TriggerFiredCountdown);

			AssertEquals(true, jobEstimateTrigger.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(true, jobEstimateTrigger.ShouldTriggerOnEstimateEvents);
			AssertEquals(false, jobEstimateTrigger.HasProcessJobTriggerLink);
			AssertEquals("Precondition", (ZShort)100, ((ITriggerConditions)jobEstimateTrigger).TriggerFiredCountdown);

			bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent00);
			bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent01, isEstimate: true);

			AssertEquals(true, jobActualTrigger.HasProcessJobTriggerLink);
			AssertEquals((ZShort)99, ((ITriggerConditions)jobActualTrigger).TriggerFiredCountdown);
			AssertEquals(true, jobEstimateTrigger.HasProcessJobTriggerLink);
			AssertEquals((ZShort)99, ((ITriggerConditions)jobEstimateTrigger).TriggerFiredCountdown);
		}

		public void TestActualUniversalTrigger_ShouldFireOnActualEvent()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var universalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);

			universalTrigger.P9T_Description = "Universal trigger";
			universalTrigger.P9T_IsEstimate = false;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			var log = bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: false);
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)bizo;

			var jobTrigger = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().SingleOrDefault(t => t.P9_ParentTemplateID == universalTrigger.PK);
			AssertNotNull(jobTrigger);
			Assert(jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler((IStmALogParent)bizo, log).Fire();
			}

			AssertEquals("Should change trigger's actual time", log.SL_EventTime, jobTrigger.P9_ActualDate.ToZDateTime());
		}

		public void TestActualUniversalTrigger_ShouldNotFireOnEstimateEvent()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var universalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);

			universalTrigger.P9T_Description = "Universal trigger";
			universalTrigger.P9T_IsEstimate = false;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			var log = bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: true);
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)bizo;

			var jobTrigger = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().SingleOrDefault(t => t.P9_ParentTemplateID == universalTrigger.PK);
			AssertNotNull(jobTrigger);
			Assert(jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler((IStmALogParent)bizo, log).Fire();
			}

			AssertNotEquals("Should not change trigger's actual time", log.SL_EventTime, jobTrigger.P9_ActualDate.ToZDateTime());
		}

		public void TestEstimateUniversalTrigger_ShouldNotFireOnActualEvent()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var universalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);

			universalTrigger.P9T_Description = "Universal trigger";
			universalTrigger.P9T_IsEstimate = true;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			var log = bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: false);
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)bizo;

			var jobTrigger = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().SingleOrDefault(t => t.P9_ParentTemplateID == universalTrigger.PK);
			AssertNotNull(jobTrigger);
			Assert(jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler((IStmALogParent)bizo, log).Fire();
			}

			AssertNotEquals("Should not change trigger's actual time", log.SL_EventTime, jobTrigger.P9_ActualDate.ToZDateTime());
		}

		public void TestEstimateUniversalTrigger_ShouldFireOnEstimateEvent()
		{
			var shipmentTransportMode = "AIR";

			var universalTemplate = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true, triggerFallbackMethod: FallbackTypeList.Codes.AlwaysFallback, subType1: shipmentTransportMode);
			var universalTrigger = CreateTrigger(universalTemplate, AutoEvents.CustomisableEvent00Code);

			universalTrigger.P9T_Description = "Universal trigger";
			universalTrigger.P9T_IsEstimate = true;

			Factory.Save();

			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			var shipment = (Forwarding.IForwardingShipment)bizo;
			shipment.JS_TransportMode = shipmentTransportMode;

			var log = bizo.GetLogs().AddNew(AutoEvents.CustomisableEvent00, dateTime: new ZDateTimeOffset(2022, 05, 23), isEstimate: true);
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)bizo;

			var jobTrigger = workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().SingleOrDefault(t => t.P9_ParentTemplateID == universalTrigger.PK);
			AssertNotNull(jobTrigger);
			Assert(jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);

			using (EventRecursionHandler.WithEventRecursionDetection())
			{
				ProcessTaskHandlerProvider.GetHandler((IStmALogParent)bizo, log).Fire();
			}

			AssertEquals("Should change trigger's actual time", log.SL_EventTime, jobTrigger.P9_ActualDate.ToZDateTime());
		}

		[TestDate(2030, 1, 1)]
		public void TestUniversalTrigger_CanRunDelayedAndSuppressDuplicates()
		{
			var templateTrigger = GetDelayedTemplateTriggerForTest(true);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Description = "Dummy";
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy);
			Factory.Save();

			var jobTriggers = dummy.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().ToArray();
			AssertEquals("Dummy has one job trigger added from template", 1, jobTriggers.Length);
			var trigger = jobTriggers[0];

			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			var mostRecentLog = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			((INeedRow)mostRecentLog).Row[StmALogSchema.SL_PostedTimeUtc.Name] = mostRecentLog.SL_PostedTimeUtc.AddSeconds(10);
			mostRecentLog.HasChanges = true;
			Factory.Save();
			AssertEquals("Trigger should not have fired yet", 0, trigger.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());

			var triggerLink = FindJobTriggerLink(dummy, templateTrigger, Factory);
			var schedules = ObjectFactory.Get<IActionScheduleProvider>().GetSchedules(Factory, WorkflowDelayedTriggerProcessor.Code, triggerLink.PK, ProcessJobTriggerLinkSchema.Constants.Prefix, scheduledLaterThanDateTimeUtc: ZDateTime.Now.AddDays(-1));
			AssertEquals("Scheduled actions for each event", 2, schedules.Count);

			var workflowDelayedTriggerProcessor = new WorkflowDelayedTriggerProcessor();

			var schedule = schedules.First();
			var result = workflowDelayedTriggerProcessor.Execute(new CancellationToken(), Factory, new DummyLogger(), schedule.TargetPK, schedule.TargetTableCode, schedule.JsonParameter, schedule.SystemCreateTimeUtc);
			AssertContains("Trigger should fire", "Fired delayed trigger", result);

			schedule = schedules.ElementAt(1);
			result = workflowDelayedTriggerProcessor.Execute(new CancellationToken(), Factory, new DummyLogger(), schedule.TargetPK, schedule.TargetTableCode, schedule.JsonParameter, schedule.SystemCreateTimeUtc);
			AssertContains("Duplicate should be suppressed", "already fired", result);

			var wteEvents = triggerLink.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode);
			AssertEquals("Trigger should fire once", 1, wteEvents.Count());
			var wteReference = wteEvents.First().SL_Reference;
			AssertContains("Trigger should be fired by the most recent event", mostRecentLog.PK.ToString(), wteReference);
		}

		[TestDate(2030, 1, 1)]
		public void TestUniversalTrigger_CanRunDelayedWithoutSuppressDuplicates()
		{
			var templateTrigger = GetDelayedTemplateTriggerForTest(false);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Description = "Dummy";
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy);
			Factory.Save();

			var jobTriggers = dummy.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().ToArray();
			AssertEquals("Dummy has one job trigger added from template", 1, jobTriggers.Length);
			var trigger = jobTriggers[0];

			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			AssertEquals("Trigger should not have fired yet", 0, trigger.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).Count());
			var triggerLink = FindJobTriggerLink(dummy, templateTrigger, Factory);
			var schedules = ObjectFactory.Get<IActionScheduleProvider>().GetSchedules(Factory, WorkflowDelayedTriggerProcessor.Code, triggerLink.PK, ProcessJobTriggerLinkSchema.Constants.Prefix, scheduledLaterThanDateTimeUtc: ZDateTime.Now.AddDays(-1));
			AssertEquals("Scheduled actions for each event", 2, schedules.Count);

			var workflowDelayedTriggerProcessor = new WorkflowDelayedTriggerProcessor();

			foreach (var schedule in schedules)
			{
				var result = workflowDelayedTriggerProcessor.Execute(new CancellationToken(), Factory, new DummyLogger(), schedule.TargetPK, schedule.TargetTableCode, schedule.JsonParameter, schedule.SystemCreateTimeUtc);
				AssertContains("Trigger should fire", "Fired delayed trigger", result);
			}

			var wteEvents = triggerLink.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode);
			AssertEquals("Two WTE events", 2, wteEvents.Count());
		}

		ProcessTemplateTrigger GetDelayedTemplateTriggerForTest(bool suppressDuplicates)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsUniversal = true;
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var templateTrigger = (ProcessTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			templateTrigger.P9T_SuppressDuplicates = suppressDuplicates;
			templateTrigger.P9T_DelayDuration = TimeSpan.FromMinutes(10);
			templateTrigger.P9T_Description = "Universal trigger";

			Factory.Save();
			return templateTrigger;
		}
	}
}

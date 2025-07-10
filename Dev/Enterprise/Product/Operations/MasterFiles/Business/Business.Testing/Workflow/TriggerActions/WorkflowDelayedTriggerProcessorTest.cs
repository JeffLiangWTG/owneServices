using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WorkflowDelayedTriggerProcessorTest : TestCaseWithFactory
	{
		string ProcessActionSchedule(IActionSchedule schedule)
		{
			var action = new WorkflowDelayedTriggerProcessor();
			var logger = new DummyLogger();
			return action.Execute(new CancellationToken(), Factory, logger, schedule.TargetPK, schedule.TargetTableCode, schedule.JsonParameter, schedule.SystemCreateTimeUtc);
		}

		IReadOnlyCollection<IActionSchedule> GetSchedules(ProcessTask trigger)
		{
			var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			return actionScheduleProvider.GetSchedules(factory, WorkflowDelayedTriggerProcessor.Code, trigger.PK, trigger.TablePrefix, scheduledLaterThanDateTimeUtc: ZDateTime.Now.AddDays(-1));
		}

		[TestDate(2030, 1, 1)]
		public void TestDelayedTriggerNotNegative()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			trigger.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown");
			trigger.P9_TriggerFiredCountdown = 2;
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			AssertEquals("Trigger has not fired yet", 0, trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());

			var schedules = GetSchedules(trigger);
			AssertEquals(4, schedules.Count);

			var result = "";
			foreach (var schedule in schedules)
			{
				result = ProcessActionSchedule(schedule);
			}
			AssertEquals("Trigger has fired twice", 2, trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
			AssertEquals((ZShort)0, trigger.P9_TriggerFiredCountdown);
			AssertEquals("Negative countdown", WorkflowDelayedTriggerProcessorLogger.TriggerCannotBeNegative(trigger), result);
		}

		[TestDate(2030, 1, 1)]
		public void TestDelayedTriggerAndSuppressDuplicates()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			trigger.P9_SuppressDuplicates = true;
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			var mostRecentLog = dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			((INeedRow)mostRecentLog).Row[StmALogSchema.SL_PostedTimeUtc.Name] = mostRecentLog.SL_PostedTimeUtc.AddSeconds(10);
			mostRecentLog.HasChanges = true;
			Factory.Save();
			AssertEquals("Trigger should not fired yet", 0, trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());

			var schedules = GetSchedules(trigger);
			AssertEquals("Schedule 2 actions incase the first event is cancelled, the second action can run", 2, schedules.Count);

			var schedule = schedules.First();
			var result = ProcessActionSchedule(schedule);
			AssertEquals("ExecutionDateTimeUtc should be 10 minutes in the future", schedule.SystemCreateTimeUtc.Add(trigger.P9_DelayDuration.TimeOfDay), schedule.ExecutionDateTimeUtc);
			AssertEquals("Trigger should fire", WorkflowDelayedTriggerProcessorLogger.TriggerFiredLog(trigger), result);

			result = ProcessActionSchedule(schedules.ElementAt(1));
			AssertEquals("Duplicate should be suppressed", WorkflowDelayedTriggerProcessorLogger.TriggerAlreadyFiredLog(trigger), result);

			var wteEvents = trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("Trigger should fire once", 1, wteEvents.Count());
			var wteReference = wteEvents.First().SL_Reference;
			AssertContains("Trigger should be fired by the most revent event", mostRecentLog.PK.ToString(), wteReference);
		}

		public void TestDelayedTriggerWithoutSuppressDuplicates()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			AssertEquals(false, trigger.P9_SuppressDuplicates);
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			AssertEquals("Trigger has not fired yet", 0, trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());

			var schedules = GetSchedules(trigger);
			AssertEquals(2, schedules.Count);

			foreach (var schedule in schedules)
			{
				ProcessActionSchedule(schedule);
			}
			AssertEquals("Trigger has fired twice", 2, trigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
		}

		public void TestSourceLogCancelledBeforeActionSchedule()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			log.Cancel();
			Factory.Save();

			var schedules = GetSchedules(trigger);
			AssertEquals(1, schedules.Count);
			var schedule = schedules.First();
			var result = ProcessActionSchedule(schedule);
			AssertEquals("Cancelled log", WorkflowDelayedTriggerProcessorLogger.SourceEventCancelledLog(trigger), result);
		}

		public void TestWTEReference()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var delayedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			delayedTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			delayedTrigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			delayedTrigger.P9_SuppressDuplicates = true;
			var normalTrigger = dummy.WorkflowItems.Triggers.AddNew();
			normalTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			dummy.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			AssertEquals("Trigger has not fired yet", 0,
				delayedTrigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
			var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var schedules = actionScheduleProvider.GetSchedules(factory, WorkflowDelayedTriggerProcessor.Code,
				delayedTrigger.PK, delayedTrigger.TablePrefix,
				scheduledLaterThanDateTimeUtc: ZDateTime.Now.AddDays(-1));
			var schedule = schedules.First();
			ProcessActionSchedule(schedule);

			var delayedWTE = delayedTrigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var normalWTE = normalTrigger.Logs.Find(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			AssertEquals(normalWTE.SL_Reference, delayedWTE.SL_Reference);
		}

		public void TestDeletedTrigger()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			trigger.Delete();
			Factory.Save();

			var schedules = GetSchedules(trigger);
			AssertEquals(1, schedules.Count);
			var schedule = schedules.First();
			var result = ProcessActionSchedule(schedule);
			AssertEquals("Cancelled log", WorkflowDelayedTriggerProcessorLogger.TriggerDeletedLog(), result);
		}

		public void TestJsonParamsAreBackwardCompatible()
		{
			var jsonParam = "{\"WTEData\":{\"TriggeringLogPK\":\"fd2b189d-e183-4b6b-a4e1-ade261df2147\",\"TriggeringSourceCode\":\"\",\"TriggeringLogParentPK\":\"fd2b189d-e183-4b6b-a4e1-ade261df2147\",\"TriggeringBranchCode\":\"BNE\",\"TriggeringDepartmentCode\":\"BRN\",\"ContextStaffCode\":\"E\",\"ContextBranchCode\":\"BNE\",\"ContextDepartmentCode\":\"BRN\",\"Version\":1},\"TriggerFiredCountdown\":\"100\"}";

			var paramObject = JsonConvert.DeserializeObject<DelayedTriggerParameters>(jsonParam);

			CombineAssertions("Don't change the API of this object for backward compatibility", () =>
			{
				AssertEquals((ZShort)100, paramObject.TriggerFiredCountdown);
				AssertNotNull(paramObject.WTEData);
				AssertEquals("fd2b189d-e183-4b6b-a4e1-ade261df2147", paramObject.WTEData.TriggeringLogPK.ToString());
				AssertEquals("fd2b189d-e183-4b6b-a4e1-ade261df2147", paramObject.WTEData.TriggeringLogParentPK.ToString());
				AssertEquals("BRN", paramObject.WTEData.TriggeringDepartmentCode);
				AssertEquals("BNE", paramObject.WTEData.TriggeringBranchCode);
			});
		}

		public void TestDelayedTriggersWorkWithoutSourceEvent()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			DummyWithWorkflow.AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLoggedToQueueOnly;
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;
			trigger.P9_DelayDuration = TimeSpan.FromMinutes(10);
			Factory.Save();
			var schedules = GetSchedules(trigger);
			var schedule = schedules.First();
			var result = ProcessActionSchedule(schedule);
			AssertEquals($"Fired delayed trigger [{trigger.HumanReadableName}].", result);
		}
	}
}

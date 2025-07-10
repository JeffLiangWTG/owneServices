using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	static class WorkflowDefaultDateProvider
	{
		internal static (StmALog estimateEvent, StmALog actualEvent) DefaultDatesFromMilestoneEvent(IMilestoneDateDefaultable milestoneDateDefaultable, BusinessObject job, bool clearNonEmptyDate, bool forceRefreshActualDate)
		{
			var eventType = Events.All[milestoneDateDefaultable.TriggerEventCode];
			if (IsTriggerableValidForDateDefaulting(milestoneDateDefaultable, eventType))
			{
				var estimateEvent = DefaultMilestoneEstimateFromEvent(milestoneDateDefaultable, eventType);
				var actualEvent = DefaultMilestoneActualFromEvent(milestoneDateDefaultable, job, clearNonEmptyDate, forceRefreshActualDate, eventType);
				return (estimateEvent, actualEvent);
			}
			return default;
		}

		internal static bool IsTriggerableValidForDateDefaulting(IMilestoneDateDefaultable milestoneDateDefaultable)
		{
			var eventType = Events.All[milestoneDateDefaultable.TriggerEventCode];
			return eventType != null && IsTriggerableValidForDateDefaulting(milestoneDateDefaultable, eventType);
		}

		internal static bool IsTriggerableValidForDateDefaulting(IMilestoneDateDefaultable milestoneDateDefaultable, Event eventType)
		{
			return !milestoneDateDefaultable.IsLineTrigger &&
					milestoneDateDefaultable.Parent != null &&
					eventType != null;
		}

		static StmALog DefaultMilestoneEstimateFromEvent(IMilestoneDateDefaultable trigger, Event eventType)
		{
			var parent = trigger.Parent;
			var estimatedLog = parent.GetLogs().MostRecentLogIncludingChildrenByEventTime(eventType, GetQueryForDefaults(trigger, true));

			if (estimatedLog != null && IsDateWithin10Years(estimatedLog.SL_EventTimeOffset))
			{
				trigger.ScheduledDate = estimatedLog.SL_EventTimeOffset;
			}

			var estimateDateProperties = EventDatePropertyAttribute.FindPropertyInfos(parent, eventType, EstimateActual.MilestoneEstimateOnly);
			foreach (var estimateDateProperty in estimateDateProperties)
			{
				var eventTime = new ZDateTimeOffset((ZDateTime)estimateDateProperty.Property.Value);
				if (IsDateWithin10Years(eventTime))
				{
					trigger.ScheduledDate = eventTime;
				}
			}
			return estimatedLog;
		}

		static bool IsDateWithin10Years(ZDateTimeOffset eventTime)
		{
			return eventTime > ZDateTimeOffset.Now.AddYears(-10);
		}

		static StmALog DefaultMilestoneActualFromEvent(IMilestoneDateDefaultable milestoneDateDefaultable, BusinessObject job, bool clearNonEmptyDate, bool forceRefreshActualDate, Event eventType)
		{
			var actualLog = GetLogForDefaultMilestoneEvent(milestoneDateDefaultable, eventType);
			if (actualLog != null)
			{
				if (ShouldUpdateActualDate(milestoneDateDefaultable, actualLog, forceRefreshActualDate))
				{
					milestoneDateDefaultable.TrySetActualDateForEvent(actualLog, job, actualLog.SL_EventTimeOffset);
				}
			}
			else if (clearNonEmptyDate)
			{
				milestoneDateDefaultable.ActualDate = ZDateTimeOffset.Empty;
			}
			return actualLog;
		}

		static StmALog GetLogForDefaultMilestoneEvent(IMilestoneDateDefaultable trigger, Event eventType)
		{
			var actualLogsFilter = GetQueryForDefaults(trigger, trigger.IsEstimateTrigger);

			if (eventType == Events.EditedARecord)
			{
				var tempFilter = actualLogsFilter;
				actualLogsFilter = log => tempFilter(log) && !GlbStaff.IsBatchProcessor(log.SL_GS_NKUser);
			}

			var allLogs = trigger.Parent.GetLogs();
			var actualLog = trigger.IsWorkflowTrigger
					? allLogs.MostRecentLogIncludingChildrenByEventTime(eventType, actualLogsFilter)
					: allLogs.EarliestLogIncludingChildrenByEventTime(eventType, actualLogsFilter);
			return actualLog;
		}

		internal static bool ShouldUpdateActualDate(IMilestoneDateDefaultable trigger, StmALog actualLog, bool forceRefreshActualDate)
		{
			return !actualLog.SL_FireWorkflow &&
					!actualLog.SL_IsCancelled &&
				(
					!trigger.ActualDate.IsValid
					|| forceRefreshActualDate
					|| trigger.ActualDate < actualLog.SL_EventTimeOffset.AddSeconds(-(actualLog.SL_EventTime.Second + 1))
				)
				&&
				(
					WorkflowDataRegistry.Instance.AllowTriggersToFireForExistingEvents.Value
					|| !TemplateTriggerCreatedAfterEvent(trigger, actualLog)
				);
		}

		static bool TemplateTriggerCreatedAfterEvent(IMilestoneDateDefaultable trigger, StmALog actualLog)
		{
			if (trigger.IsWorkflowTrigger
				&& !trigger.IsInDatabase
				&& actualLog.IsInDatabase)
			{
				var templateCreateTime = trigger.TemplateCreateTimeUtc;
				return trigger.HasTemplate && (templateCreateTime.IsEmpty || templateCreateTime > actualLog.SL_PostedTimeUtc);
			}
			else
			{
				return false;
			}
		}

		public static Func<StmALog, bool> GetQueryForDefaults(IMilestoneDateDefaultable triggerable, bool isEstimate, string oldTriggerConditionValue = null)
		{
			var triggerCondition = triggerable.TriggerCondition;
			var triggerConditionValue = oldTriggerConditionValue != null ? new ZString(oldTriggerConditionValue) : triggerable.TriggerConditionValue;
			return log =>
			{
				if (log.SL_IsEstimate != isEstimate)
				{
					return false;
				}

				var result = TriggerConditionEvaluator.AreTriggerConditionsMet(triggerable, triggerable.Parent, log, triggerConditionValue);

				result &= triggerable.GetLogIsValidForDateDefaulting(log);

				return result;
			};
		}
	}
}

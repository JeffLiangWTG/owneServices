using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

[assembly:
SchedulerAction(
		WorkflowDelayedTriggerProcessor.Code,
		WorkflowDelayedTriggerProcessor.Description,
		typeof(WorkflowDelayedTriggerProcessor))]
namespace Enterprise.MasterFiles.Business
{
	public class WorkflowDelayedTriggerProcessor : ISchedulerAction
	{
		public const string Code = "WTE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "Workflow Delayed Trigger Processor";

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			var job = factory.Load(targetCode, targetPk);

			if (!(job is IBaseTrigger trigger) || job.IsDeleted)
			{
				return GetResult(logger, WorkflowDelayedTriggerProcessorLogger.TriggerDeletedLog());
			}

			if (trigger.TriggerFiredCountdown < 1)
			{
				return GetResult(logger, WorkflowDelayedTriggerProcessorLogger.TriggerCannotBeNegative(job));
			}

			StmALog sourceEvent = null;
			DelayedTriggerParameters delayedTriggerParameters = null;

			if (trigger.SuppressDuplicates)
			{
				var queuedSchedules = GetSchedules(factory, job.PK, job.TablePrefix);
				if (queuedSchedules.Length == 0)
				{
					return GetResult(logger, WorkflowDelayedTriggerProcessorLogger.TriggerAlreadyFiredLog(job));
				}

				foreach (var scheduleGroup in queuedSchedules
					.GroupBy(s => s.SystemCreateTimeUtc) //non deterministic order since CreateTimeUtc is to nearest minute
					.OrderByDescending(s => s.Key))
				{
					foreach (var queuedSchedule in scheduleGroup)
					{
						var currentParameters = JsonConvert.DeserializeObject<DelayedTriggerParameters>(queuedSchedule.JsonParameter);
						if (currentParameters.WTEData.TriggeringLogPK == Guid.Empty)
						{
							delayedTriggerParameters = currentParameters;
							break;
						}

						if (TryLoadTriggeringEvent(trigger, job, currentParameters,  out var currentEvent))
						{
							if (sourceEvent == null || sourceEvent.SL_PostedTimeUtc < currentEvent.SL_PostedTimeUtc)
							{
								sourceEvent = currentEvent;
								delayedTriggerParameters = currentParameters;
							}
						}
					}

					if (sourceEvent != null)
					{
						break;
					}
				}

				MarkSchedulesAsClosed(logger, queuedSchedules);
			}
			else
			{
				delayedTriggerParameters = JsonConvert.DeserializeObject<DelayedTriggerParameters>(parameter);
				TryLoadTriggeringEvent(trigger, job, delayedTriggerParameters, out sourceEvent);
			}

			if (sourceEvent?.IsCancelled ?? false)
			{
				return GetResult(logger, WorkflowDelayedTriggerProcessorLogger.SourceEventCancelledLog(job));
			}

			((IStmALogParent)job).Logs.AddNew(Events.WorkflowTriggerEvent, delayedTriggerParameters.WTEData.ToReference(), ZDateTimeOffset.Now);

			if (trigger.TriggerFiredCountdown > short.MinValue)
			{
				trigger.TriggerFiredCountdown--;
			}

			return GetResult(logger, WorkflowDelayedTriggerProcessorLogger.TriggerFiredLog(job));
		}

		public static void QueueScheduleActionForDelayedTrigger(IBaseTrigger trigger, WorkflowTriggerEventData wteData)
		{
			var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
			var jsonParams = JsonConvert.SerializeObject(new DelayedTriggerParameters(wteData, trigger.TriggerFiredCountdown));

			actionScheduleProvider.ScheduleAction(Code,
				ZDateTime.UtcNow.AddSeconds(trigger.DelayDurationSeconds),
				targetPk: ((BusinessObject)trigger).PK,
				targetTableCode: ((BusinessObject)trigger).TablePrefix,
				jsonParams,
				GlbBranch.CurrentBranch.PK,
				GlbDepartment.CurrentDepartment.PK,
				factory: trigger.Factory);
		}

		static IActionSchedule[] GetSchedules(BusinessObjectFactory factory, ZGuid targetPk, string targetTableCode)
		{
			var filter = new ZQuery(TimeActionScheduleSchema.TAS_ExecutionStatus, Constants.TimeActionScheduleStatus.Scheduled);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_ActionCode, Code);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_TargetPK, targetPk);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_TargetTableCode, targetTableCode);
			return factory.Load<IActionSchedule>(filter);
		}

		static bool TryLoadTriggeringEvent(IBaseTrigger trigger, BusinessObject job, DelayedTriggerParameters delayedTriggerParameters, out StmALog log)
		{
			bool logFound = TriggeringLogFinder.TryFindLog(delayedTriggerParameters.WTEData, trigger, job, out log, true);
			return logFound && log != null && !log.SL_IsCancelled;
		}

		static void MarkSchedulesAsClosed(ILogger logger, IActionSchedule[] queuedSchedules)
		{
			if (queuedSchedules.Length > 1)
			{
				logger.Information(WorkflowDelayedTriggerProcessorLogger.DuplicatesSuppressed(queuedSchedules.Length - 1));
			}
			queuedSchedules.ForEach(s => s.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed);
		}

		static string GetResult(ILogger logger, string log)
		{
			logger.Information(log);
			return log;
		}
	}

	class DelayedTriggerParameters
	{
		public DelayedTriggerParameters()
		{
		}

		public DelayedTriggerParameters(WorkflowTriggerEventData wteData, ZShort triggerFiredCountdown)
		{
			WTEData = wteData;
			TriggerFiredCountdown = triggerFiredCountdown;
		}

		public WorkflowTriggerEventData WTEData { get; set; }
		public ZShort TriggerFiredCountdown { get; set; }
	}
}

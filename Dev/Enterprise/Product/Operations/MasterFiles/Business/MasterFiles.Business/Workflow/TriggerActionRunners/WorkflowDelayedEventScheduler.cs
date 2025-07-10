using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.TimeEngineScheduler.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowDelayedEventScheduler : IProcessor
	{
		public WorkflowDelayedEventScheduler(ProcessTaskNotification action, BusinessObject job, IQueuedLog queuedLog)
		{
			this.action = action;
			this.job = job;
			this.queuedLog = queuedLog;
		}

		readonly ProcessTaskNotification action;
		readonly BusinessObject job;
		readonly IQueuedLog queuedLog;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var sourceEventInfo = new EventInfoProvider(queuedLog, action.Parent, job);

			if (sourceEventInfo.FoundEvent)
			{
				if (string.IsNullOrEmpty(sourceEventInfo.Event.SL_SE_NKEvent))
				{
					ErrorReporter.ReportOnce(
						"EmptyEventCodeInDelayedEventScheduler",
						$"Event code (SL_SE_NKEvent) is null or empty for job {job.PK}. This will cause failures in WorkflowDelayedEventProcessor.");
					return;
				}

				var actionOffset = action.PQ_Offset.TimeSpan6MonthsFromStartOfYear;
				var executionDateTimeUtc = sourceEventInfo.Event.EventTimeOffset.ToUtcDateTime() + actionOffset;

				var jsonParameter = string.Format(CultureInfo.InvariantCulture, "|ACT={0}|EVT={1}|OFF={2}|EST={3}|USR={4}|BRN={5}|DEP={6}",
					action.PQ_ActionReference, // ACT may be empty therefore placed the first (otherwise the reference may be parsed incorrectly)
					sourceEventInfo.Event.SL_SE_NKEvent,
					ZDateTimeGeneralHelper.GetTextFromTimeOffset(actionOffset, maximumHours: 999, allowNegative: true),
					sourceEventInfo.Event.SL_IsEstimate ? "Y" : "N",
					sourceEventInfo.Event.SL_GS_NKUser,
					sourceEventInfo.Event.SL_GB_NKBranch,
					sourceEventInfo.Event.SL_GE_NKDepartment);

				var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
				actionScheduleProvider.ScheduleAction(WorkflowDelayedEventProcessor.Code,
					executionDateTimeUtc,
					targetPk: job.PK,
					targetTableCode: job.TablePrefix,
					jsonParameter,
					sourceEventInfo.EventBranch.PK,
					sourceEventInfo.EventDepartment.PK);
			}
		}
	}
}

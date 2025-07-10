using System;
using System.Collections.Generic;
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

[assembly: SchedulerAction(
		WorkflowDelayedEventProcessor.Code,
		WorkflowDelayedEventProcessor.Description,
		typeof(WorkflowDelayedEventProcessor))]
namespace Enterprise.MasterFiles.Business
{
	public class WorkflowDelayedEventProcessor : ISchedulerAction
	{
		public const string Code = "DLY";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "Workflow Scheduled Action Runner";

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			Argument.NotNull(targetCode, nameof(targetCode));
			Argument.NotNull(parameter, nameof(parameter));

			var referenceParameters = StmALog.GetParametersFromReference(parameter);
			var evt = GetParameter("EVT", referenceParameters);
			var off = GetParameter("OFF", referenceParameters);
			var act = GetParameter("ACT", referenceParameters);
			var est = GetParameter("EST", referenceParameters);

			if (evt == null || off == null || est == null)
			{
				ErrorReporter.ReportOnce("WorkflowDelayedEventProcessor_InvalidAction", FormattableString.Invariant($"Invalid Workflow Scheduled Action\r\ntargetPk:{targetPk}\r\ntargetCode:{targetCode}\r\nparameter:{parameter}\r\nACT:{act}\r\nEVT:{evt}\r\nOFF:{off}\r\nEST:{est}"));
				return Res.GetString("F7C7A4AE-D9F0-4C37-B2A3-DFA332E692FB", "Canceled: the DLY action has invalid parameters. ACT:{0} EVT:{1} OFF:{2} EST:{3}", act, evt, off, est);
			}

			var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
			var schedulesFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var moreRecentSchedules = actionScheduleProvider.GetSchedules(schedulesFactory, Code, targetPk, targetCode, scheduledLaterThanDateTimeUtc: systemCreateTimeUtc)
				.Where(s => s.ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled)
				.Where(s => HasExactParameters(s.JsonParameter, evt, off, act, est))
				.OrderByDescending(s => s.ExecutionDateTimeUtc);
			var mostRecentSchedule = moreRecentSchedules.FirstOrDefault();

			if (mostRecentSchedule != null)
			{
				return Res.GetString("B010588D-E6B8-40C9-95F9-52955D37A5FE", "No action needed: there is a more recent DLY action scheduled for {0} on {1}.",
					mostRecentSchedule.ExecutionDateTimeUtc, mostRecentSchedule.SystemCreateTimeUtc);
			}

			var job = factory.Load(targetCode, targetPk);

			if (job == null || job.IsDeleted)
			{
				return Res.GetString("4B2C7251-9F2A-4A10-98BF-338B209A3241", "Canceled: the job has been deleted.");
			}

			var matchingLogs = job.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DelayedEventCode)
				.Where(l => HasExactParameters(l.SL_Reference, evt, off, act, est))
				.OrderByDescending(l => l.SL_PostedTimeUtc);
			var latestLog = matchingLogs.FirstOrDefault();

			if (latestLog != null)
			{
				return Res.GetString("3E3C36D1-6596-4BAD-A013-4977F93B8379", "No action needed: a DLY event with the same reference parameters was already generated at {0} ({1}).",
					latestLog.SL_PostedTimeUtc, latestLog.SL_Reference);
			}

			var log = job.GetLogs().AddNew(Events.DelayedEvent, reference: parameter);

			return Res.GetString("CFE54A1C-706C-4627-B3B2-EFDCAC0C2747", "Successfully created a DLY event.");
		}

		string GetParameter(string paramName, IDictionary<string, string> dictionary)
		{
			return dictionary.TryGetValue(paramName, out string paramValue) ? paramValue : null;
		}

		bool HasExactParameters(string reference, string evt, string off, string act, string est)
		{
			var parameters = StmALog.GetParametersFromReference(reference);
			return GetParameter("EVT", parameters) == evt
				&& GetParameter("OFF", parameters) == off
				&& GetParameter("ACT", parameters) == act
				&& GetParameter("EST", parameters) == est;
		}
	}
}

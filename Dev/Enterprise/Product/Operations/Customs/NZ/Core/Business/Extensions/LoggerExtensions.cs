using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business
{
	public static class LoggerExtensions
	{
		public static void LogCustomsEventsIfRequired(this IStmALogProvider logProvider, ZString currentStatus, ZString newStatus, ZString agencyMsgProcessing, ZString entryType, ZString bioStatus, ZBool isExport, ZString clearanceEventReference)
		{
			LogImpedimentChangeIfRequired(logProvider, currentStatus, newStatus, agencyMsgProcessing);
			LogCustomsCompletedIfRequired(logProvider, currentStatus, newStatus, entryType, bioStatus, isExport, clearanceEventReference);
		}

		#region TSW Logging

		static void LogImpedimentChangeIfRequired(IStmALogProvider logProvider, ZString currentStatus, ZString newStatus, ZString agencyMsgProcessing)
		{
			if (IsImpedimentStatusChanging(currentStatus, newStatus))
			{
				if (IsImpedimentCleared(currentStatus, newStatus))
				{
					logProvider.Logs.AddNew(Events.CustomsImpedimentReceived, "CLR - " + agencyMsgProcessing, ZDateTimeOffset.Now);
				}
				else
				{
					logProvider.Logs.AddNew(Events.CustomsImpedimentReceived, "HLD - " + agencyMsgProcessing, ZDateTimeOffset.Now);
				}
			}
		}

		static void LogCustomsCompletedIfRequired(IStmALogProvider logProvider, ZString currentStatus, ZString newStatus, ZString entryType, ZString bioStatus, ZBool isExport, ZString clearanceEventReference)
		{
			// The Cleared event must be logged against the Shipment if there is one for Workflow purposes.

			if (entryType != MessageTypeList.Codes.IPI && IsStatusChangingToCompleted(currentStatus, newStatus, entryType, bioStatus))
			{
				var clearanceEventDate = ZDateTimeOffset.Now;

				var parent = (logProvider as IStmALogParentProvider)?.LogParent ?? logProvider;
				var clearedEvent = isExport ? Events.ExportCustomsCleared : Events.CustomsCleared;
				LogEventIfRequired(parent, clearedEvent, clearanceEventReference, clearanceEventDate);

				var completedEventType = IsWriteOffEntry(entryType) ? LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff : FormalEntryStatusList.Codes.DeliveryOrderReceived;
				LogEventIfRequired(parent, Events.CustomsEntryStatus, completedEventType, clearanceEventDate);
			}
		}

		static bool IsImpedimentStatusChanging(string originalStatus, string newStatus)
		{
			var impedimentStatusHasChanged = false;
			var safeLength = Math.Min(originalStatus.Length, newStatus.Length);
			for (var i = 0; i < safeLength; i++)
			{
				var currentAgencyStatus = originalStatus[i].ToString();
				var newAgencyStatus = newStatus[i].ToString();

				impedimentStatusHasChanged = (!TSWEntryStatusList.IsImpedimentStatus(currentAgencyStatus) && TSWEntryStatusList.IsImpedimentStatus(newAgencyStatus) ||
											TSWEntryStatusList.IsImpedimentStatus(currentAgencyStatus) && !TSWEntryStatusList.IsImpedimentStatus(newAgencyStatus));

				if (impedimentStatusHasChanged)
				{
					break;
				}
			}

			return impedimentStatusHasChanged;
		}

		static bool IsImpedimentCleared(string originalStatus, string newStatus)
		{
			var impedimentHasCleared = false;
			var safeLength = Math.Min(originalStatus.Length, newStatus.Length);
			for (var i = 0; i < safeLength; i++)
			{
				impedimentHasCleared = originalStatus[i] == '1' && newStatus[i] == '0';
				if (impedimentHasCleared)
				{
					break;
				}
			}

			return impedimentHasCleared;
		}

		static bool IsStatusChangingToCompleted(ZString originalStatus, ZString newStatus, ZString entryType, ZString bioStatus)
		{
			return !TSWEntryStatusList.IsCompletedStatus(originalStatus, entryType, ZString.Empty) && TSWEntryStatusList.IsCompletedStatus(newStatus, entryType, bioStatus);
		}

		static bool IsWriteOffEntry(ZString entryType)
		{
			return entryType == TSWEntryStatusList.EntryTypes.WriteOff;
		}

		static void LogEventIfRequired(IStmALogProvider logProvider, Event eventType, ZString reference, ZDateTimeOffset dateTime)
		{
			var logs = logProvider.Logs;

			var shouldAddANewLog = !logs.LogsNotInDB.Any(x => x.SL_SE_NKEvent == eventType.Code && !x.SL_IsCancelled && x.SL_Reference == reference);
			if (shouldAddANewLog && ((logProvider as BusinessObject)?.IsInDatabase ?? false))
			{
				var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code).AddToFilter(StmALogSchema.SL_IsCancelled, false);
				eventQuery.AddToFilter(StmALogSchema.SL_Reference, reference);
				shouldAddANewLog = !logProvider.Logs.DatabaseHasLogs(eventQuery);
			}

			if (shouldAddANewLog)
			{
				logs.AddNew(eventType, reference, dateTime);
			}
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class AirBookingHelper
	{
		#region IsAirEBookingMessage

		public const string AirBookingDataStoreName = "AirBooking"; // programmatic constant

		public static bool IsAirEBookingMessage(this ITopLevelDataObject dataObject) => dataObject?.DataContext.IsAirEBookingMessage() ?? false;

		static bool IsAirEBookingMessage(this IDataContextDataObject dataContext)
		{
			var documentName = dataContext
				?.DocumentaryOverride
				?.DocumentName;

			return documentName.HasValue
				&& string.Equals(documentName.Value, AirBookingDataStoreName, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region GetSubmissionVersion

		public static int? GetSubmissionVersion(this ITopLevelDataObject dataObject) => dataObject?.DataContext.GetSubmissionVersion();
		static int? GetSubmissionVersion(this IDataContextDataObject dataContext) => dataContext?.DocumentaryOverride?.SubmissionVersion;

		#endregion

		#region GetAirBookingLastImportedLogSubmissionVersion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static int? GetLastImportedAirBookingLogSubmissionVersion(this IStmALogParent logParent, string eventCode, IDictionary<string, string> parameters)
		{
			if (logParent == null
				|| string.IsNullOrWhiteSpace(eventCode))
			{
				return null;
			}

			const string airBookingMessageType = "Air Booking";

			var logMatchingKey = parameters.GetAirBookingLogMatchingKey();

			bool IsAirBookingLog(StmALog log)
			{
				return log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, out var messageType)
					&& string.Equals(messageType, airBookingMessageType, StringComparison.OrdinalIgnoreCase);
			}

			bool IsApplicable(StmALog log)
			{
				return string.Equals(log.SL_SE_NKEvent, eventCode, StringComparison.OrdinalIgnoreCase)
					&& IsAirBookingLog(log)
					&& string.Equals(logMatchingKey, log.Parameters.GetAirBookingLogMatchingKey(), StringComparison.OrdinalIgnoreCase)
					&& log.RelatedEDIMessage != null;
			}

			var logs = logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.Where(IsApplicable);

			foreach (var log in logs)
			{
				var submissionVersion = log.RelatedEDIMessage.DataContext.GetSubmissionVersion();

				if (submissionVersion.HasValue)
				{
					return submissionVersion.Value;
				}
			}

			return null;
		}

		static string GetAirBookingLogMatchingKey(this IDictionary<string, string> parameters)
		{
			if (parameters == null
				|| parameters.Count == 0)
			{
				return string.Empty;
			}

			var p = new List<string>(parameters.Count);

			var orderedKeys = parameters.Keys.OrderBy(k => k);

			foreach (var key in orderedKeys)
			{
				p.Add(FormattableString.Invariant($"{key}={parameters[key]}")); // programmatic constant
			}

			return string.Join("|", p);
		}

		#endregion

		#region GetAirBookingLastImportedShipmentSubmissionVersion

		public static int? GetLastImportedAirBookingShipmentSubmissionVersion(this IStmALogParent logParent)
		{
			if (logParent == null)
			{
				return null;
			}

			bool IsApplicable(StmALog log)
			{
				return log.SL_SE_NKEvent == Events.DataImportCode
					&& (log.RelatedEDIMessage?.DataContext.IsAirEBookingMessage() ?? false);
			}

			var dataImportLogs = logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.OrderByDescending(l => l.SL_PostedTimeUtc)
				.Where(IsApplicable);

			foreach (var log in dataImportLogs)
			{
				var submissionVersion = log.RelatedEDIMessage.DataContext.GetSubmissionVersion();

				if (submissionVersion.HasValue)
				{
					return submissionVersion.Value;
				}
			}

			return null;
		}

		#endregion
	}
}

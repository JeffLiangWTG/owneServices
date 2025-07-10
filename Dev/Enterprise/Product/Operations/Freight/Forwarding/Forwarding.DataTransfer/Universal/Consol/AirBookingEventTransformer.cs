using System;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class AirBookingEventTransformer : IEventTransformer
	{
		public EventValue Transform(EventValue sourceEventValue, UniversalDataBuss.DataObjects.Universal.Event sourceUniversalEvent, IStmALogParent logParent)
		{
			if (sourceEventValue == null
				|| !sourceEventValue.EventTime.IsValid)
			{
				return sourceEventValue;
			}

			var submissionVersion = sourceUniversalEvent.GetSubmissionVersion() ?? 0;
			var lastImportedSubmissionVersion = logParent.GetLastImportedAirBookingLogSubmissionVersion(sourceEventValue.EventType.Code, sourceEventValue.Parameters);

			if (lastImportedSubmissionVersion.HasValue
				&& lastImportedSubmissionVersion.Value >= submissionVersion)
			{
				throw new MessageProcessingBusinessFailureException(FormattableString.Invariant($"This Universal Event submission version is: {submissionVersion}. A Universal Event with the same or newer submission versions ({lastImportedSubmissionVersion}) has already been imported."));
			}

			if (TryConvertUtcDateToLocal(sourceEventValue.EventTime, out var localDateTime))
			{
				return new EventValue(
					sourceEventValue.EventType,
					sourceEventValue.IsEstimate,
					sourceEventValue.DeferFiringWorkflow,
					localDateTime,
					sourceEventValue.Reference,
					sourceEventValue.Parameters);
			}

			return sourceEventValue;
		}

		bool TryConvertUtcDateToLocal(ZDateTimeOffset utcDateTime, out ZDateTimeOffset localDateTime)
		{
			if (!utcDateTime.IsValid)
			{
				localDateTime = ZDateTimeOffset.Empty;
				return false;
			}

			var convertedDate = DateTime.SpecifyKind(
				utcDateTime.ToDateTime(),
				DateTimeKind.Utc);

			localDateTime = new ZDateTime(convertedDate.ToLocalTime()).ToOffset();
			return true;
		}
	}
}

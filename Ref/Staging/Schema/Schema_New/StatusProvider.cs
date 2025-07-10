using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	enum Status
	{
		Queued,
		Processed,
		Error,
		Merged,
		Final,
		FinalWithErrors,
		Duplicated,
		Purged,
	}

	public static class StatusProvider
	{
		static string GetStatusCode(Status status)
		{
			return status switch
			{
				Status.Queued => "QUE",
				Status.Processed => "PRS",
				Status.Error => "ERR",
				Status.Merged => "MER",
				Status.Final => "FIN",
				Status.FinalWithErrors => "FIE",
				Status.Duplicated => "DUP",
				Status.Purged => "PUR",
				_ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
			};
		}

		#region Shared Statuses

		public static string GetERRStatus() => GetStatusCode(Status.Error);
		public static string GetQUEStatus() => GetStatusCode(Status.Queued);
		public static string GetPRSStatus() => GetStatusCode(Status.Processed);
		public static string GetMERStatus() => GetStatusCode(Status.Merged);
		public static string GetFINStatus() => GetStatusCode(Status.Final);
		public static string GetFIEStatus() => GetStatusCode(Status.FinalWithErrors);
		public static string GetDUPStatus() => GetStatusCode(Status.Duplicated);
		public static string GetPURStatus() => GetStatusCode(Status.Purged);

		#endregion

		#region SourceData

		public static IEnumerable<string> GetSourceDataMergedStatuses() => [GetMERStatus(), GetFINStatus(), GetDUPStatus()];

		public static IEnumerable<string> GetSourceDataErrorStatuses() => [GetERRStatus(), GetFIEStatus()];

		public static IEnumerable<string> GetSourceDataFinalStatuses() => [GetFINStatus(), GetFIEStatus()];

		public static IEnumerable<string> GetSourceDataProcessedStatuses() => [GetPRSStatus()];

		public static IEnumerable<string> GetSourceDataToBeFinalisedStatuses() => [GetMERStatus(), GetERRStatus()];

		#endregion

		#region ProcessorStatus

		public static IEnumerable<string> GetProcessorStatusErrorStatuses() => [GetERRStatus()];

		#endregion

		#region DataProcessingInformation

		public static IEnumerable<string> GetDataProcessingInformationErrorStatuses() => [GetERRStatus()];
		public static IEnumerable<string> GetDataProcessingInformationProcessedStatuses() => [GetPRSStatus()];

		#endregion

		#region DataChangeCapture

		public static IEnumerable<string> GetDataChangeCaptureErrorStatuses() => [GetERRStatus(), GetFIEStatus()];

		#endregion
	}
}

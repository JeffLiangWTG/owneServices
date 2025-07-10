using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public partial class DrawbackSummaryStatusList : IStatusList
	{
		#region IStatusList Members

		IReadOnlyList<string> IStatusList.RejectStatusInterested
		{
			get { return System.Array.Empty<string>(); }
		}

		IReadOnlyList<string> IStatusList.AcceptedStatusToCancelRejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ClearDrawbackSummaryOriginal,
					Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings,
					Codes.ClearDrawbackSummaryReplacement,
					Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings,
					Codes.ClearDrawbackSummaryDelete
				};
			}
		}

		bool IStatusList.IsStatusClear(string status)
		{
			return status == Codes.ClearDrawbackSummaryOriginal
				|| status == Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings
				|| status == Codes.ClearDrawbackSummaryReplacement
				|| status == Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings
				|| status == Codes.ClearDrawbackSummaryDelete;
		}

		bool IStatusList.IsWaitingForResponse(string status)
		{
			return status == Codes.AwaitingDrawbackSummaryOriginal
				|| status == Codes.AwaitingDrawbackSummaryReplacement
				|| status == Codes.AwaitingDrawbackSummaryDelete;
		}

		bool IStatusList.IsWithdrawnStatus(string status)
		{
			return status == Codes.ClearDrawbackSummaryDelete;
		}

		bool IStatusList.IsPartialStatus(string status)
		{
			return false;
		}

		bool IStatusList.IsArrivalExportBTATransmissionStatus(string status)
		{
			return false;
		}

		IReadOnlyList<string> IStatusList.GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType)
		{
			return new string[] { Codes.ClearDrawbackSummaryOriginal, Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings };
		}

		#endregion

		public static bool IsACEDrawbackClearedEntryStatus(ZString status)
		{
			return status == Codes.ClearDrawbackSummaryOriginal
				|| status == Codes.ClearDrawbackSummaryReplacement
				|| status == Codes.DrawbackSummaryOriginalAcceptedWithCensusWarnings
				|| status == Codes.DrawbackSummaryReplacementAcceptedWithCensusWarnings;
		}

		public static bool IsACEDrawbackAwaitingForResponse(ZString status)
		{
			return status == Codes.AwaitingDrawbackSummaryOriginal
				|| status == Codes.AwaitingDrawbackSummaryReplacement;
		}
	}
}

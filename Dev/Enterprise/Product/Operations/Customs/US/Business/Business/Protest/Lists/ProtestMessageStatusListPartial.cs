using System.Collections.Generic;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	partial class ProtestMessageStatusList : IStatusList
	{
		#region IStatusList Members

		public IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ClearInitialFiling,
					Codes.ClearInitialFilingWithWarnings,
					Codes.ClearAmendment,
					Codes.ClearAddenda
				};
			}
		}

		public IReadOnlyList<string> GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType)
		{
			return new string[] { Codes.ClearInitialFiling, Codes.ClearInitialFilingWithWarnings, Codes.ClearAmendment, Codes.ClearAddenda };
		}

		public bool IsArrivalExportBTATransmissionStatus(string status)
		{
			return false;
		}

		public bool IsPartialStatus(string status)
		{
			return false;
		}

		public bool IsStatusClear(string status)
		{
			return status == Codes.ClearInitialFiling ||
					status == Codes.ClearInitialFilingWithWarnings ||
					status == Codes.ClearAmendment ||
					status == Codes.ClearAddenda;
		}

		public bool IsWaitingForResponse(string status)
		{
			return
				status == Codes.AwaitingInitialFiling ||
				status == Codes.AwaitingAmendment ||
				status == Codes.AwaitingAddenda;
		}

		public IReadOnlyList<string> RejectStatusInterested
		{
			get { return System.Array.Empty<string>(); }
		}

		bool IStatusList.IsWithdrawnStatus(string status)
		{
			return false;
		}

		#endregion
	}
}

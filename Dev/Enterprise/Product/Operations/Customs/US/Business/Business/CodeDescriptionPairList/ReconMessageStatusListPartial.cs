using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	partial class ReconMessageStatusList : IStatusList
	{
		public static ReconMessageStatusList GetCachedReconMessageStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ReconMessageStatusList>();
		}

		#region IStatusList Members

		public IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ClearReconDelete,
					Codes.ClearReconOriginal,
					Codes.ClearReconReplace
				};
			}
		}

		public IReadOnlyList<string> GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType)
		{
			return new string[] { Codes.ClearReconOriginal, Codes.ReconOriginalAcceptedWarnings };
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
			return status == Codes.ClearReconOriginal ||
					status == Codes.ClearReconReplace ||
					status == Codes.ClearReconDelete ||
					status == Codes.ReconOriginalAcceptedWarnings ||
					status == Codes.ReconReplaceAcceptedWarnings;
		}

		public bool IsWaitingForResponse(string status)
		{
			return
				status == Codes.AwaitingReconDelete ||
				status == Codes.AwaitingReconOriginal ||
				status == Codes.AwaitingReconReplace;
		}

		public bool IsWithdrawnStatus(string status)
		{
			return status == Codes.ClearReconDelete;
		}

		public IReadOnlyList<string> RejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ErrorReconDelete,
					Codes.ErrorReconOriginal,
					Codes.ErrorReconReplace
				};
			}
		}

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class CargoReleaseProcessingResultList
	{
		public static bool AIIRecordsMayBeRequired(string code)
		{
			return code == Codes.CondReleaseSpecDocReview ||
				code == Codes.FurtherDocReviewRequired ||
				code == Codes.ReleaseRemovedFurtherDocReviewRequired;
		}

		/// <summary>
		/// entry as opposed to entry summary
		/// </summary>
		public static bool IsEntryDeletionDispositionCode(string code)
		{
			return code == CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP ||
				   code == CargoReleaseProcessingResultList.Codes.EntryCancelled ||
				   code == CargoReleaseProcessingResultList.Codes.CancellationRequestRejected;
		}

		public static bool IsDocRequiredDispositionCode(string code)
		{
			return code == CargoReleaseProcessingResultList.Codes.DocRequiredForCorrectionRequest ||
				code == CargoReleaseProcessingResultList.Codes.DocRequiredForCancellationRequest ||
				code == CargoReleaseProcessingResultList.Codes.DocumentRequired;
		}

		public static bool IsHold(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.ManifestHoldCBP ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.CBPHold;
		}

		public static bool IsExam(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.PendingIntenstiveReview ||
				   dispositionCode == CargoReleaseProcessingResultList.Codes.OverrideToIntensive;
		}

		public static bool IsHoldExamOrHoldExamRemove(string transportMode, BusinessObjectFactory factory, string code)
		{
			return IsHold(code) ||
				   IsHoldRemoved(code) ||
				   DispositionCodeListLoader.IsExam(transportMode, factory, code) ||
				   DispositionCodeListLoader.IsHold(transportMode, factory, code) ||
				   DispositionCodeListLoader.IsHoldExamRemoved(transportMode, factory, code);
		}

		public static bool IsDeleted(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP;
		}

		public static bool IsNotReleasedCancellationPending(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.EntryCancelled ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.EntryCancellationUnset ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.EntryWillBeCancelledIn7Days;
		}

		public static bool IsEntryDeletedOrCancelled(ZString dispositionCode)
		{
			return dispositionCode == Codes.EntryCancelled
				|| dispositionCode == Codes.EntryDeletedByCBP;
		}

		public static CodeDescriptionPairList GetListForQuotaStatus(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CargoReleaseProcessingResultList_QuotaStatus", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.QuotaPending, Descriptions.QuotaPending);
				result.AddPair(Codes.QuotaRejected, Descriptions.QuotaRejected);
				result.AddPair(Codes.QuotaReserved, Descriptions.QuotaReserved);
				result.AddPair(Codes.QuotaAccepted, Descriptions.QuotaAccepted);
				return result;
			});
		}

		public static bool IsCancelled(ZString dispositionCode)
		{
			return dispositionCode == Codes.EntryCancelled;
		}

		public static bool IsReleased(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.Released;
		}

		public static bool IsHoldRemoved(ZString billStatus, ZString dispositionCode)
		{
			return IsHold(billStatus) && IsHoldRemoved(dispositionCode);
		}

		static bool IsHoldRemoved(ZString dispositionCode)
		{
			return dispositionCode == CargoReleaseProcessingResultList.Codes.CBPManifestHoldRemoved ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.AgricultureManifestHoldRemoved ||
				dispositionCode == CargoReleaseProcessingResultList.Codes.CBPHoldRemoved;
		}

		internal static ZString[] GetDispositionCodesRelatedToReleaseStatus()
		{
			return new ZString[]
			{
				CargoReleaseProcessingResultList.Codes.PendingIntenstiveReview,
				CargoReleaseProcessingResultList.Codes.OverrideToIntensive,
				CargoReleaseProcessingResultList.Codes.ManifestHoldCBP,
				CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture,
				CargoReleaseProcessingResultList.Codes.CBPHold,
				CargoReleaseProcessingResultList.Codes.DocRequiredForCorrectionRequest,
				CargoReleaseProcessingResultList.Codes.DocRequiredForCancellationRequest,
				CargoReleaseProcessingResultList.Codes.DocumentRequired,
				CargoReleaseProcessingResultList.Codes.UnderCBPReview,
				CargoReleaseProcessingResultList.Codes.Admissible,
				CargoReleaseProcessingResultList.Codes.EntryDeletedByCBP,
				CargoReleaseProcessingResultList.Codes.EntryCancelled,
				CargoReleaseProcessingResultList.Codes.EntryCancellationUnset,
				CargoReleaseProcessingResultList.Codes.EntryWillBeCancelledIn7Days,
				CargoReleaseProcessingResultList.Codes.Released,
				CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate,
				CargoReleaseProcessingResultList.Codes.ReleaseSuspended,
				CargoReleaseProcessingResultList.Codes.NotRelease
			};
		}

		public static ZBool IsDispositionCodeRelatedToRleaseStatus(DispositionData disposition)
		{
			return GetDispositionCodesRelatedToReleaseStatus().Contains(disposition.US_Code);
		}
	}
}

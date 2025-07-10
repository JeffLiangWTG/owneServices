using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class DispositionDataCollectionExtensionMethods
	{
		public static IEnumerable<DispositionData> GetLatestDispositions(this IEnumerable<DispositionData> dispositionDatas, ICodeDescriptionPairList list = null)
		{
			var result = new List<DispositionData>();
			var dispDate = ZDateTime.Empty;
			foreach (var data in dispositionDatas.OrderBy(x => x, new DispositionDataByDispDateAndMsgDateComparer()))
			{
				if (list == null || list.ContainsCode(data.US_Code))
				{
					if (dispDate.IsEmpty || dispDate == data.US_DispositionDate)
					{
						result.Add(data);
						dispDate = data.US_DispositionDate;
					}
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZString GetReleaseStatus(this DispositionData latestDisposition)
		{
			var result = ZString.Empty;
			var dispositionCode = latestDisposition.US_Code;

			if (CargoReleaseProcessingResultList.IsExam(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.EXM;
			}
			else if (CargoReleaseProcessingResultList.IsHold(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.HLD;
			}
			else if (CargoReleaseProcessingResultList.IsDocRequiredDispositionCode(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.DOC;
			}
			else if (dispositionCode == CargoReleaseProcessingResultList.Codes.UnderCBPReview)
			{
				result = CRLReleaseStatusList.Codes.RVW;
			}
			else if (dispositionCode == CargoReleaseProcessingResultList.Codes.Admissible)
			{
				result = CRLReleaseStatusList.Codes.ADM;
			}
			else if (CargoReleaseProcessingResultList.IsDeleted(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.DEL;
			}
			else if (CargoReleaseProcessingResultList.IsCancelled(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.CAN;
			}
			else if (DispositionHasReleased(latestDisposition))
			{
				result = CRLReleaseStatusList.Codes.REL;
			}
			else if (CargoReleaseProcessingResultList.IsNotReleasedCancellationPending(dispositionCode))
			{
				result = CRLReleaseStatusList.Codes.NRC;
			}
			else if (dispositionCode == CargoReleaseProcessingResultList.Codes.ReleaseSuspended
				|| dispositionCode == CargoReleaseProcessingResultList.Codes.NotRelease
				|| (CargoReleaseProcessingResultList.IsReleased(dispositionCode) && (latestDisposition.US_ReleaseDate.IsEmpty || ReleaseOriginCodeList.ShouldRemoveReleaseDate(latestDisposition.US_ReleaseOrigin))))
			{
				result = CRLReleaseStatusList.Codes.NRL;
			}
			return result;
		}

		static ZBool DispositionHasReleased(DispositionData disposition)
		{
			return CargoReleaseProcessingResultList.IsReleased(disposition.US_Code) && !disposition.US_ReleaseDate.IsEmpty && !ReleaseOriginCodeList.ShouldRemoveReleaseDate(disposition.US_ReleaseOrigin);
		}

		public static ZDateTime GetReleaseDateFromLatestDispositions(this IEnumerable<DispositionData> latestDispositions)
		{
			var dispositionHasReleased = latestDispositions.FirstOrDefault(x => DispositionHasReleased(x));
			return dispositionHasReleased?.US_ReleaseDate ?? ZDateTime.Empty;
		}
	}
}

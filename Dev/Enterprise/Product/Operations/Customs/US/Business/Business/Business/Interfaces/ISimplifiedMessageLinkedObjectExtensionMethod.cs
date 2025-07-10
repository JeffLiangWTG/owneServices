using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Customs.US.Business
{
	public static class ISimplifiedMessageLinkedObjectExtensionMethod
	{
		/// <param name="previousReleaseStatus">A response for Entry Query will add a log only if it differs from the current status. Otherwise system will add whenver a new message is processed.</param>
		public static void UpdateAndLogStatusDetails(this ISimplifiedMessageLinkedObject messageLinkedObject, ZString messageType, bool aIIRecordsRequired, ZString? previousReleaseStatus = null)
		{
			if (messageLinkedObject != null)
			{
				var messageLinkedParentBO = messageLinkedObject.ParentBusinessObject;
				if (aIIRecordsRequired)
				{
					messageLinkedParentBO.MarkAIIRequested();
				}

				var newReleaseStatus = CalculateReleaseStatus(messageLinkedParentBO.DispositionCodes, messageLinkedParentBO);
				using (messageLinkedParentBO.ReleaseStatusChangingSuspender)
				{
					messageLinkedParentBO.ReleaseStatus = newReleaseStatus;

					if (newReleaseStatus == CRLReleaseStatusList.Codes.REL)
					{
						messageLinkedParentBO.ReleaseDateTime = CalculateReleaseDate(messageLinkedParentBO.DispositionCodes);
						messageLinkedObject.AutoSendEntrySummaryQueryIfEligible();
					}
					else if (!messageLinkedParentBO.ReleaseDateTime.IsEmpty)
					{
						messageLinkedParentBO.ReleaseDateTime = ZDateTime.Empty;
						((EnterpriseBusinessObject)messageLinkedParentBO)?.Logs.AddNew(Events.AuthorisationWithdrawn, messageType + " - " + newReleaseStatus);
					}
				}

				string previousReleaseStatusToCompare = null;

				if (previousReleaseStatus.HasValue)
				{
					previousReleaseStatusToCompare = previousReleaseStatus.Value.IsEmpty ? CRLReleaseStatusList.Codes.NRL : previousReleaseStatus.Value.ToString();
				}

				var currentReleaseStatusToCompare = messageLinkedParentBO.ReleaseStatus.IsEmpty ? CRLReleaseStatusList.Codes.NRL : messageLinkedParentBO.ReleaseStatus.ToString();
				if (string.IsNullOrEmpty(previousReleaseStatusToCompare) || previousReleaseStatusToCompare != currentReleaseStatusToCompare)
				{
					if (messageLinkedObject.IsLVSCargoRelease)
					{
						var parameters = messageLinkedObject.GetMSCEventParameters().ToList();
						parameters.Add(new KeyValuePair<string, string>(Params.New, newReleaseStatus));
						parameters.Add(new KeyValuePair<string, string>(Params.Type, messageType));
						if (messageLinkedObject.UseCodeIsHVL)
						{
							parameters.Add(new KeyValuePair<string, string>(Params.Service, CustomsStatusLogSubscriber.PublishCustomsStatusChangedEventService));
						}
						LogMessageStatusChangeEventAgainstTopLevelBusinessObject_LVS(messageLinkedObject, parameters.ToArray());
					}
					else
					{
						var releaseStatusReference = " - " + currentReleaseStatusToCompare;
						LogMessageStatusChangeEventAgainstTopLevelBusinessObject(messageLinkedObject, messageType + releaseStatusReference);
					}
				}
			}
		}

		public static void LogMessageStatusChangeEventAgainstTopLevelBusinessObject(IMessageAttachee entry, ZString reference)
		{
			var logs = entry.TopLevelBusinessObjectLogs;
			if (logs != null)
			{
				logs.AddNew(Events.MessageStatusChange, reference, dateTime: DateTimeParser.GetFromJobBranchCurrentTime(entry.Branch), null);
			}
		}

		public static void LogMessageStatusChangeEventAgainstTopLevelBusinessObject_LVS(ISimplifiedMessageLinkedObject entry, KeyValuePair<string, string>[] parameters)
		{
			var logs = entry.TopLevelBusinessObjectLogs;
			if (logs != null)
			{
				logs.AddNew(Events.MessageStatusChange, reference: string.Empty, dateTime: DateTimeParser.GetFromJobBranchCurrentTime(entry.Branch), parameters);
			}
		}

		public static ZBool HasSpecifiedDispositionCodes(this ISimplifiedMessageLinkedObject messageLinkedObject, string[] specifiedDispositionCodeList)
		{
			if (messageLinkedObject != null && messageLinkedObject.ParentBusinessObject != null)
			{
				return HasSpecifiedDispositionCodesInLatestDispositions(messageLinkedObject.ParentBusinessObject.DispositionCodes.OfType<DispositionData>(), specifiedDispositionCodeList);
			}

			return false;
		}

		static ZBool HasSpecifiedDispositionCodesInLatestDispositions(IEnumerable<DispositionData> dispositionCodes, string[] specifiedDispositionCodeList)
		{
			return HasSpecifiedDispositionCodes(dispositionCodes.GetLatestDispositions(), specifiedDispositionCodeList);
		}

		static ZBool HasSpecifiedDispositionCodes(IEnumerable<DispositionData> dispositionCodes, string[] specifiedDispositionCodeList)
		{
			var result = false;
			if (dispositionCodes != null && dispositionCodes.Any() && specifiedDispositionCodeList.Length > 0)
			{
				return dispositionCodes.Any(x => specifiedDispositionCodeList.Any(y => x.US_Code == y));
			}
			return result;
		}

		public static ZString CalculateReleaseStatus(DispositionDataCollection dispositionCodes, IEntryHeaderParentBusinessObject messageLinkedParentBO)
		{
			var latestEffectiveDispositions = GetLatestEffectiveDispositionsExcludeRequestRejected(dispositionCodes.OfType<DispositionData>(), ZDateTime.Empty);
			var lastEffectiveDisposition = latestEffectiveDispositions.OrderBy(x => x.US_Order).LastOrDefault();
			var result = lastEffectiveDisposition?.GetReleaseStatus() ?? ZString.Empty;

			if (result.IsEmpty)
			{
				if (messageLinkedParentBO.ReleaseStatus.IsEmpty)
				{
					result = CRLReleaseStatusList.Codes.NRL;
				}
				else
				{
					result = messageLinkedParentBO.ReleaseStatus;
				}
			}

			return result;
		}

		static ZDateTime CalculateReleaseDate(DispositionDataCollection dispositionCodes)
		{
			var latestEffectiveDispositions = GetLatestEffectiveDispositionsExcludeRequestRejected(dispositionCodes.OfType<DispositionData>(), ZDateTime.Empty);
			return latestEffectiveDispositions.GetReleaseDateFromLatestDispositions();
		}

		static IEnumerable<DispositionData> GetLatestEffectiveDispositionsExcludeRequestRejected(IEnumerable<DispositionData> dispositionCodes, ZDateTime currentDispositionDateTime)
		{
			var dispositionCodesWithSameDateTime = dispositionCodes.Where(x => x.US_DispositionDate == currentDispositionDateTime);
			var dispositionCodesExcludeSameDateTime = dispositionCodes.Except(dispositionCodesWithSameDateTime);
			var latestDispositions = dispositionCodesExcludeSameDateTime.GetLatestDispositions();

			if (HasSpecifiedDispositionCodes(latestDispositions, new[] { CargoReleaseProcessingResultList.Codes.EntryCancelled }))
			{
				return latestDispositions.Where(x => x.US_Code == CargoReleaseProcessingResultList.Codes.EntryCancelled);
			}
			else if (HasSpecifiedDispositionCodes(latestDispositions, new[] { CargoReleaseProcessingResultList.Codes.CorrectionRequestRejected, CargoReleaseProcessingResultList.Codes.CancellationRequestRejected }))
			{
				if (HasSpecifiedDispositionCodes(latestDispositions, new[] { CargoReleaseProcessingResultList.Codes.Released }))
				{
					return latestDispositions.Where(x => x.US_Code == CargoReleaseProcessingResultList.Codes.Released);
				}

				var latestDisposition = latestDispositions.FirstOrDefault();
				if (latestDisposition != null && latestDisposition.US_DispositionDate.IsValid)
				{
					return GetLatestEffectiveDispositionsExcludeRequestRejected(dispositionCodesExcludeSameDateTime, latestDisposition.US_DispositionDate);
				}
			}
			else
			{
				return dispositionCodesExcludeSameDateTime.Where(x => CargoReleaseProcessingResultList.IsDispositionCodeRelatedToRleaseStatus(x)).GetLatestDispositions();
			}

			return null;
		}
	}
}

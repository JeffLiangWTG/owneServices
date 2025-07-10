using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	internal static class IDispositionDetailProviderExtensionMethods
	{
		public static DispositionData AddDispositionData(this IDispositionDetailProvider dispositionDetail, DispositionDataCollection dispositionCodes, string source = "")
		{
			var dispositionDateTime = dispositionDetail.DispositionDateTime;

			var codeAndDate = dispositionCodes.AddNewIfNotExist(dispositionDetail.DispositionCode, dispositionDateTime, source);

			codeAndDate.US_ReleaseDate = dispositionDetail.ReleaseDateTime;
			codeAndDate.US_ReleaseOrigin = dispositionDetail.ReleaseOrigin;
			codeAndDate.US_DocumentType = dispositionDetail.DocumentType;
			return codeAndDate;
		}

		public static void UpdateMessageLinkedParentBO(this IDispositionDetailProvider releaseDetailBlock, ISimplifiedMessageLinkedObject messageLinkedObject, ZString messageType)
		{
			var messageLinkedParentBO = messageLinkedObject.ParentBusinessObject;
			var codeAndDate = releaseDetailBlock.AddDispositionData(messageLinkedParentBO.DispositionCodes);
			bool isEntryCancelled = CargoReleaseProcessingResultList.IsEntryDeletionDispositionCode(codeAndDate.US_Code);
			if (isEntryCancelled)
			{
				ISimplifiedMessageLinkedObject cargoReleaseEntry = null;

				if (messageLinkedObject.IsFormalEntry)
				{
					cargoReleaseEntry = messageLinkedParentBO.CargoReleaseEntry ?? messageLinkedParentBO.SimplifiedEntry;
				}
				else
				{
					cargoReleaseEntry = messageLinkedObject;
				}

				if (cargoReleaseEntry != null)
				{
					var status = ZString.Empty;
					if (cargoReleaseEntry.IsBorderCargoRelease)
					{
						status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete;
					}
					else if (cargoReleaseEntry.IsCargoRelease)
					{
						status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;
					}
					else if (cargoReleaseEntry.IsACECargoRelease)
					{
						if (releaseDetailBlock.DispositionCode == CargoReleaseProcessingResultList.Codes.CancellationRequestRejected)
						{
							status = ImportMessageStatusList.Codes.CancellationRequestRejected;
						}
						else if (CargoReleaseProcessingResultList.IsEntryDeletedOrCancelled(releaseDetailBlock.DispositionCode))
						{
							status = ImportMessageStatusList.Codes.ClearACECargoReleaseDelete;
						}
					}

					if (!status.IsEmpty)
					{
						cargoReleaseEntry.MessageStatus = status;
					}
				}

				messageLinkedObject.ClearCRLCertStatusFromAllHeaders();

				var clearDeleteStatus = new ZString[] { ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete, ImportMessageStatusList.Codes.ClearCargoReleaseDelete };
				if (cargoReleaseEntry != null)
				{
					messageLinkedObject.DeactiveStatementLineIfRequired(clearDeleteStatus, cargoReleaseEntry.MessageStatus);
				}
			}

			if (releaseDetailBlock.DispositionCode == CargoReleaseProcessingResultList.Codes.CondReleaseGenExam)
			{
				var reference = messageType + " - " + OneUSG;
				if (messageLinkedParentBO is EnterpriseBusinessObject parentBO)
				{
					parentBO.Logs.AddNew(Events.MessageStatusChange, reference, DateTimeParser.GetFromJobBranchCurrentTime(messageLinkedObject.Branch), null);
				}
			}

			if (CargoReleaseProcessingResultList.GetListForQuotaStatus(messageLinkedObject.Factory).ContainsCode(codeAndDate.US_Code))
			{
				messageLinkedParentBO.UpdateQuotaStatus(codeAndDate.US_Code);
			}
		}

		public static string GetUnableToDeactivateStatementLineRemarkIfNecessary(this JobDeclaration declaration)
		{
			string result = string.Empty;

			var statement = declaration.RelatedStatement;
			var cargoReleaseEntry = declaration.ActiveEntryHeaders.SimplifiedEntry ?? declaration.ActiveEntryHeaders.CargoReleaseEntry;
			var clearDeleteStatus = new ZString[] { ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete, ImportMessageStatusList.Codes.ClearCargoReleaseDelete };

			if (statement != null && !statement.CanDeactivateStatementLine && cargoReleaseEntry != null && cargoReleaseEntry.CH_StatusInfo.HasChanges && clearDeleteStatus.Contains(cargoReleaseEntry.CH_Status))
			{
				result = "This entry has just been canceled, but it is on a statement, '" + statement.B2_StatementNumber + "' which is paid or its ACH Authorization is in progress. System has not adjusted any records. Please follow it up with CBP.";
			}
			return result;
		}
		const string OneUSG = "1USG";

		public static bool HasEarlierReleaseDispositionDateTime(this IDispositionDetailProvider dispositionBlock, IEnumerable<MQEDIMessage> existingMessages)
		{
			var existingLatestDispositionTime = (from MQEDIMessage message in existingMessages
												 let existingReleaseBlock = message.GetFirstReleaseDetailBlock()
												 where existingReleaseBlock != null
												 orderby existingReleaseBlock.DispositionDateTime descending
												 select existingReleaseBlock.DispositionDateTime).FirstOrDefault();

			return existingLatestDispositionTime.IsValid && dispositionBlock.DispositionDateTime < existingLatestDispositionTime;
		}

		public static IEnumerable<MQEDIMessage> GetReleaseDispositionMessages(this ISimplifiedMessageLinkedObject messageLinkedObject, EDIMessage messageToExclude)
		{
			IEnumerable<MQEDIMessage> result = null;
			if (messageLinkedObject != null)
			{
				var messages = messageLinkedObject.Messages.OfType<MQEDIMessage>();
				var messageLinkedParentBO = messageLinkedObject.ParentBusinessObject;
				if (messageLinkedParentBO != null)
				{
					if (messageLinkedParentBO.LinkedObject != messageLinkedObject.LinkedObject)
					{
						messages = messages.Concat(messageLinkedParentBO.Messages.Cast<MQEDIMessage>());
					}

					if (messageLinkedParentBO.SimplifiedEntry != null && messageLinkedObject.LinkedObject != messageLinkedParentBO.SimplifiedEntry.LinkedObject)
					{
						messages = messages.Concat(messageLinkedParentBO.SimplifiedEntry.Messages.Cast<MQEDIMessage>());
					}
				}

				result = messages.Where(x => x != messageToExclude && MayContainReleaseDetails(x.EM_MessageType));
			}
			return result;
		}

		internal static bool MayContainReleaseDetails(string messageType)
		{
			return messageType == ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse
				|| messageType == ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults
				|| messageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus
				|| messageType == ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
		}

		public static IDispositionDetailProvider GetFirstReleaseDetailBlock(this MQEDIMessage message)
		{
			return message.MessageBlock.MessageBlocks.OfType<IDispositionDetailProvider>().FirstOrDefault(x => HasReleaseDetails(x));
		}

		public static bool HasReleaseDetails(this IDispositionDetailProvider dispositionBlock)
		{
			return dispositionBlock.ReleaseDateTime.IsValid || ReleaseOriginCodeList.ShouldRemoveReleaseDate(dispositionBlock.ReleaseOrigin);
		}
	}
}

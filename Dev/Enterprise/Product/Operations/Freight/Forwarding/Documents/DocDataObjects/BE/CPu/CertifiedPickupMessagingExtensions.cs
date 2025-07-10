using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class CertifiedPickupMessagingExtensions : BaseMessagingExtensions, ICustomMessageWithdrawalSupporter
	{
		public CertifiedPickupMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			certifiedPickup = document?.Data.Value as CertifiedPickup;
			this.consol = consol;

			Argument.NotNull(certifiedPickup, nameof(certifiedPickup));
		}

		readonly CertifiedPickup certifiedPickup;
		readonly ForwardingConsol consol;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (certifiedPickup.IsTransferMode && certifiedPickup.SelectedContainers.All(c => c.CurrentStatus == CertifiedPickupConstants.Status.TransferSentAwaitingResponse))
			{
				var message = Res.GetString("7fc849b5-6ed8-4c1e-82d7-e2e69b536d9d", "No message will be sent. Please select any container that has not been transferred.");
				var information = Res.GetString("c18f3e57-2e5e-4875-acdd-3278435cac31", "Information");
				notifications?.ShowMessage(message, information);

				return false;
			}

			return true;
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => false;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => false;

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => true;

		public override string GetMessageStatus()
		{
			var documentDataLoader = CargoWise.Application.ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.BECertifiedPickup) as IStmALogParent;

			return documentData?.Logs.GetAllLogs().Cast<StmALog>()
				.Where(log => MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString()) && CheckIsMessageStatusLog(log))
				.OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault()?.DisplayEventReference.ToString() ?? Res.GetString("b847b711-ed10-4651-a488-08131c27dff5", "No Certified Pick up Messages Have Been Sent.");
		}

		public override bool? GetRequireMessageAmendmentReason() => false;

		public override bool? IsSendingAmendment() => false;

		public override string MessagePurposeCodeOverride => certifiedPickup.IsRevokeMode ? MessagePurposes.Codes.Withdrawal : null;

		#region Implementation

		string FormMode => certifiedPickup.FormMode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Search string")]
		bool CheckIsMessageStatusLog(StmALog log)
		{
			var messageTypeToFind = CertifiedPickupContainerEventHelper.GetMessageTypeByFormMode(FormMode);

			if (!string.IsNullOrEmpty(messageTypeToFind))
			{
				return log != null && MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString())
					&& !log.SL_Reference.StartsWith("Propagated: ")
					&& !log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.EquipmentReferenceNumber)
					&& log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.MessageType, out var messageType)
					&& string.Compare(messageType, messageTypeToFind, StringComparison.OrdinalIgnoreCase) == 0;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region ICustomMessageWithdrawalSupporter members

		const string WithdrawalReasonForRevokeMessage = "";

		object ICustomMessageWithdrawalSupporter.GetMessageWithdrawalReason() => WithdrawalReasonForRevokeMessage;

		bool ICustomMessageWithdrawalSupporter.PopulateMessageWithdrawalReason(IDataObject dataObject, object reasonForSending)
		{
			return true;
		}

		#endregion
	}
}

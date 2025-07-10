using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Shared
{
	public abstract class BookingMessagingValidation
	{
		#region ContinueWithSendingMessage

		public virtual bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (ShouldResetToOriginal)
			{
				notifications.ShowMessage(
					Res.GetString("17116ccf-8a55-41e3-97c1-ca84636e9135",
						"An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option."),
					ConfirmationMessage);
				return false;
			}

			if (Carrier != null && EnableOceanCarrierMessagingConnectionValidation)
			{
				if (CarrierCanNotReceiveThisMessage)
				{
					notifications.ShowMessage(Res.GetString("6074384c-6d74-44e7-bb23-81c4a85c8688",
						@"You are trying to send the {0} message to {1} that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", DocumentName,
						Carrier.OH_FullName), ConfirmationMessage);
					return false;
				}
			}

			return null;
		}

		#endregion

		#region ContinueWithSendingMessageAmendment

		public virtual bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			if (CarrierBookingReferenceIsEmpty)
			{
				notifications.ShowMessage(
					Res.GetString("2d8b9f45-c0e6-4788-afbc-0bed7dce86a0",
						"You are trying to send an amendment message, carrier booking reference is mandatory to send an amendment message."),
					ConfirmationMessage);
				return false;
			}

			if (CarrierChangedSinceLastSent)
			{
				notifications.ShowMessage(CarrierChangeErrorMessageForAmendment, ConfirmationMessage);
				return false;
			}

			return CheckMessageStatusForAmendment(notifications);
		}

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		public virtual bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (CarrierChangedSinceLastSent)
			{
				var carrierChangedErrorMessage = Res.GetString("adc57212-1096-481a-acc5-6b646418008c",
					"You are trying to send a {0} withdrawal message to a new carrier. Sending {0} withdrawal message to a new carrier is not allowed.",
					DocumentName);
				notifications.ShowMessage(carrierChangedErrorMessage, ConfirmationMessage);
				return false;
			}

			return null;
		}

		#endregion

		#region Abstract Properties

		protected abstract BusinessObject BizO { get; }

		protected abstract string DocumentName { get; }
		protected abstract string DocumentDataStoreName { get; }

		protected abstract OrgHeader Carrier { get; }

		protected abstract bool CarrierCanNotReceiveThisMessage { get; }

		protected abstract bool CarrierBookingReferenceIsEmpty { get; }

		protected abstract string CarrierCode { get; }

		#endregion

		#region Implementation

		protected virtual bool ShouldResetToOriginal
		{
			get
			{
				if (Context.ResponseCode == Events.MessageWithdrawCancelAcceptedCode)
				{
					return true;
				}

				if (Context.TransmissionCode == Events.MessageWithdrawCancelRequestCode &&
					MessageEventCodes.AcceptanceEventCodes.Contains(Context.ResponseCode))
				{
					return true;
				}

				return false;
			}
		}

		protected MessageContext Context => context ?? (context = new MessageContext(BizO, DocumentName, DocumentDataStoreName));
		MessageContext context;

		protected virtual bool? CheckMessageStatusForAmendment(IUserNotifications notifications)
		{
			if (Context.TransmissionCode == Events.MessageWithdrawCancelRequestCode && string.IsNullOrEmpty(Context.ResponseCode))
			{
				var messagePrefix = Res.GetString("e656b634-3679-46a9-a873-ca5e43797e08", "A withdraw/cancellation message has been sent");
				return notifications.ShowConfirmation(GetMessage(messagePrefix), ConfirmationMessage);
			}

			if (Context.TransmissionCode == Events.MessageSentCode && NoReplyForOriginalMessage)
			{
				var messagePrefix = Context.DocumentDataVersion == 2
					? Res.GetString("2f12004d-e474-4b4e-930b-78402a197751", "An original message has been sent")
					: Res.GetString("fa0d9c90-0005-4c4c-9a2d-c2a88bd1574a", "An amendment message has been sent");
				return notifications.ShowConfirmation(GetMessage(messagePrefix), ConfirmationMessage);
			}

			return null;
		}

		bool NoReplyForOriginalMessage
		{
			get
			{
				var applicableResponseCode = new[]
				{
					Events.MessageRejectedCode, Events.MessageAcceptedCode, Events.InterchangeRejectedCode
				};
				return !applicableResponseCode.Contains(Context.ResponseCode);
			}
		}

		protected virtual bool CarrierChangedSinceLastSent
		{
			get
			{
				var lastSentCarrierCode = Context.LastSentCarrierCode;
				return lastSentCarrierCode.HasValue && lastSentCarrierCode.Value != CarrierCode;
			}
		}

		protected virtual string CarrierChangeErrorMessageForAmendment => Res.GetString(
			"ed05201d-6e63-4777-ae39-5396a168d3d1",
			"You are trying to send a {0} amendment message to a new carrier. Please verify Carrier (or Co-Load With) and Reset to Original if you are sending {0} to new carrier.",
			DocumentName);

		static string GetMessage(string prefixMessage)
		{
			var recipient = Res.GetString("2a6e2216-70a0-4b16-b5ab-ab77997c00e5", "recipient");

			var userOptionMessage = Res.GetString("76ecca45-6e82-49f2-b771-0c62ed0e1966",
				"You may want to reset the message to original first (using \"Reset to Original\" option) if the {0} did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?",
				recipient);

			var confirmationTemplate = Res.GetString("4ae5c33b-1189-41a7-825a-2809b08b0b06",
				@"{0}, and there is no reply received from the {1}. Resending the message may cause errors and possibly revert to a manual process.",
				prefixMessage, recipient);

			return confirmationTemplate + System.Environment.NewLine + System.Environment.NewLine + userOptionMessage;
		}

		protected bool EnableOceanCarrierMessagingConnectionValidation =>
			FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.Value;

		protected string ConfirmationMessage => Res.GetString("743483ca-3007-422c-8e00-38a9e664793e", "Confirmation");

		protected sealed class MessageContext
		{
			public MessageContext(BusinessObject bo, string documentName, string documentDataStoreName)
			{
				this.bo = bo;
				this.documentName = documentName;
				this.documentDataStoreName = documentDataStoreName;
			}

			readonly BusinessObject bo;
			readonly string documentName;
			readonly string documentDataStoreName;

			public string ResponseCode => LastDialog?.ResponseCode;
			public string TransmissionCode => LastDialog?.TransmissionCode;

			IDialog LastDialog => lastDialog ?? (lastDialog = Dialogs?.LastOrDefault());
			IDialog lastDialog;

			IEnumerable<IDialog> Dialogs =>
				dialogs ?? (dialogs = (DocumentData as IStmALogParent)?.GetDialogs(documentName, false));

			IEnumerable<IDialog> dialogs;

			IVisualizerDocumentData DocumentData => documentData ?? (documentData = GetDocumentData());
			IVisualizerDocumentData documentData;

			IVisualizerDocumentData GetDocumentData()
			{
				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentDataArray = documentDataLoader.Load(bo);
				return documentDataArray.FirstOrDefault(x => x.Name == documentDataStoreName);
			}

			public string DocumentName => documentName;

			public int DocumentDataVersion => DocumentData?.CalculateDataVersion(documentName, false) ?? 0;

			public ZString? LastSentCarrierCode
			{
				get
				{
					var applicableEventCodes = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
					var sentMessageOrStatusUpdateLog =
						LastDialog?.Logs.FirstOrDefault(log => applicableEventCodes.Contains(log.SL_SE_NKEvent));
					if (sentMessageOrStatusUpdateLog != null &&
						sentMessageOrStatusUpdateLog.Parameters.TryGetValue(
							Constants.EventReferenceParameters.Codes.Company,
							out var lastCarrierCode))
					{
						return new ZString(lastCarrierCode);
					}

					return null;
				}
			}
		}

		#endregion
	}
}

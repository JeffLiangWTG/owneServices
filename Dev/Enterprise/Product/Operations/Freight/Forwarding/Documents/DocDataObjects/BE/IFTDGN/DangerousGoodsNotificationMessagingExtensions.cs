using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UnversalNote = Enterprise.UniversalDataBuss.DataObjects.Universal.Note;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DangerousGoodsNotificationMessagingExtensions : BaseMessagingExtensions, ICustomMessageAmendmentSupporter, ICustomMessageWithdrawalSupporter
	{
		public DangerousGoodsNotificationMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			this.document = document;
			this.consol = consol;
			dangerousGoodsNotification = document?.Data.Value as DangerousGoodsNotification;
			Argument.NotNull(dangerousGoodsNotification, nameof(dangerousGoodsNotification));
			isAwaitingResponse = GetIsAwaitingReplyOnMessageSent();
		}

		readonly DangerousGoodsNotification dangerousGoodsNotification;
		readonly IDocument document;
		readonly ForwardingConsol consol;
		readonly bool isAwaitingResponse;

		bool GetIsAwaitingReplyOnMessageSent()
		{
			bool messageHasBeenSend = false;

			foreach (var log in GetEventLogsInDescendingOrder())
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
					case Events.InterchangeSentCode:
						messageHasBeenSend = true;
						break;

					default:
						break;
				}
				break;
			}

			return messageHasBeenSend;
		}
		IEnumerable<StmALog> GetEventLogsInDescendingOrder()
		{
			foreach (var log in consol.Logs.GetAllLogs().OfType<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc))
			{
				var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);
				var logMessageType = string.Compare(dangerousGoodsNotification.HandlingInstruction, BelgianPortsConstants.HandlingInstructions.Discharge, System.StringComparison.OrdinalIgnoreCase) == 0 ? BelgianPortsConstants.DocumentNames.IFTDGNImport : BelgianPortsConstants.DocumentNames.IFTDGNExport;

				if (string.Compare(messageType, logMessageType, System.StringComparison.OrdinalIgnoreCase) == 0)
				{
					yield return log;
				}
			}
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (isAwaitingResponse)
			{
				var message = Res.GetString("2DE30A27-A7B1-49DD-A9F8-026C32DD0F93", "A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.");
				var information = Res.GetString("7A539DD1-6B9A-403A-B5E5-764326CA305C", "Information");
				notifications?.ShowMessage(message, information);
				return false;
			}

			return true;
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => true;

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (isAwaitingResponse)
			{
				return null;
			}
			return !dangerousGoodsNotification.DgnSecurityNumber.IsEmpty;
		}

		#region GetRequireMessageAmendmentReason

		public override bool? GetRequireMessageAmendmentReason()
		{
			return !dangerousGoodsNotification.DgnSecurityNumber.IsEmpty;
		}

		#endregion

		#region ICustomMessageWithdrawalSupporter members

		public object GetMessageWithdrawalReason()
		{
			var optionsList = GetWithdrawalOptions();

			var message = Res.GetString("f0bee0a5-909a-49ac-9b60-1ce9343f6039", "You are sending a {0} Cancellation message. Please enter a reason for cancellation:", document.Name);
			var caption = Res.GetString("1944a264-0b52-484f-b8a7-b861aa237e37", "Cancellation Reason");

			var selector = ObjectFactory.Get<IDocumentActionReasonSelector>();
			var selectedReason = selector.SelectDocumentActionReason(message, caption, optionsList);

			return selectedReason;
		}

		public bool PopulateMessageWithdrawalReason(IDataObject dataObject, object reasonForSending)
		{
			if (reasonForSending is DocumentActionReasonModel reason
				&& dataObject is UniversalShipment shipment)
			{
				return PopulateWithdrawalNotesCollection(shipment, reason);
			}

			return true;
		}

		#region GetWithdrawalOptions

		public override ICodeDescriptionPairList GetWithdrawalOptions()
		{
			return new DangerousGoodsNotificationMessageWithdrawalOptions();
		}

		#endregion

		#endregion

		#region ICustomMessageAmendmentSupporter members

		public object GetMessageAmendmentReason()
		{
			var optionsList = GetAmendmentOptions();

			var message = Res.GetString("5aa9b94b-2593-430b-a7b4-3fc1c07639c9", "You are re-sending the {0} so this message acts as a replacement. Please enter a reason for this replacement:", document.Name);
			var caption = Res.GetString("d0b88f77-0218-4733-a72f-3ff7ace36d56", "Replacement Reason");

			var selector = ObjectFactory.Get<IDocumentActionReasonSelector>();
			var selectedReason = selector.SelectDocumentActionReason(message, caption, optionsList);

			return selectedReason;
		}

		public bool PopulateMessageAmendmentReason(IDataObject dataObject, object reasonForSending)
		{
			if (reasonForSending is DocumentActionReasonModel reason
				&& dataObject is UniversalShipment shipment)
			{
				return PopulateAmendmentNotesCollection(shipment, reason);
			}

			return false;
		}

		#region GetAmendmentOptions

		public override ICodeDescriptionPairList GetAmendmentOptions()
		{
			return new DangerousGoodsNotificationMessageAmendmentOptions();
		}

		#endregion

		#endregion

		#region IsSendingAmendment

		public override bool? IsSendingAmendment()
		{
			return !dangerousGoodsNotification.DgnSecurityNumber.IsEmpty;
		}

		#endregion

		#region PopulateNoteCollection

		bool PopulateWithdrawalNotesCollection(UniversalShipment shipment, DocumentActionReasonModel reasonForSending)
		{
			return PopulateReasonNotesCollection(
				shipment,
				reasonForSending,
				"ReasonForMessageCancellation",          // programmatic constant
				"ReasonForMessageCancellationFreeText"); // programmatic constant
		}

		bool PopulateAmendmentNotesCollection(UniversalShipment shipment, DocumentActionReasonModel reasonForSending)
		{
			return PopulateReasonNotesCollection(
				shipment,
				reasonForSending,
				"ReasonForMessageAmendment",          // programmatic constant
				"ReasonForMessageAmendmentFreeText"); // programmatic constant
		}

		bool PopulateReasonNotesCollection(UniversalShipment shipment, DocumentActionReasonModel reasonForSending, string noteReasonCodeDescription, string noteReasonTextDescription)
		{
			if (shipment == null
				|| reasonForSending == null)
			{
				return false;
			}

			var noteReasonCode = new UnversalNote
			{
				Description = noteReasonCodeDescription,
				IsCustomDescription = true,
				NoteText = reasonForSending.ReasonCode
			};

			var noteReasonText = new UnversalNote
			{
				Description = noteReasonTextDescription,
				IsCustomDescription = true,
				NoteText = reasonForSending.ReasonText
			};

			if (shipment.NoteCollection == null)
			{
				shipment.SetNoteCollection(() => new UniversalDataBuss.DataObjects.Core.DataObjectList<UnversalNote>
				{
					noteReasonCode,
					noteReasonText
				});
			}
			else
			{
				void RemoveNote(string description)
				{
					var note = shipment.NoteCollection.FirstOrDefault(n => n.Description.HasValue && n.Description.Value == description);

					if (note != null)
					{
						shipment.NoteCollection.Remove(note);
					}
				}

				RemoveNote(noteReasonCodeDescription);
				RemoveNote(noteReasonTextDescription);

				shipment.NoteCollection.Add(noteReasonCode);
				shipment.NoteCollection.Add(noteReasonText);
			}

			return true;
		}

		#endregion
	}
}

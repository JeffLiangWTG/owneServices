using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class SeaShipmentBookingRequestMessagingExtensions : IMessagingExtensions
	{
		public SeaShipmentBookingRequestMessagingExtensions(ForwardingShipment shipment)
		{
			this.shipment = shipment;
			bookingRequestMessagingValidation = new SeaShipmentBookingRequestMessagingValidation(shipment);
		}

		readonly ForwardingShipment shipment;
		readonly SeaShipmentBookingRequestMessagingValidation bookingRequestMessagingValidation;

		#region IMessagingExtensions

		public string MessagePurposeCodeOverride => null;

		public bool? ContinueWithResetToOriginal(IUserNotifications notifications) => null;

		public bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			return bookingRequestMessagingValidation.ContinueWithSendingMessage(notifications);
		}

		public bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			return bookingRequestMessagingValidation.ContinueWithSendingMessageAmendment(notifications);
		}

		public bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			return bookingRequestMessagingValidation.ContinueWithSendingMessageWithdrawal(notifications);
		}

		public KeyValuePair<string, string>[] GetAdditionalParametersForEvent()
		{
			var result = new List<KeyValuePair<string, string>>();

			var carrierSCACCode = shipment.GetCarrierCodeWithFallback();
			if (!carrierSCACCode.IsEmpty)
			{
				result.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Company, carrierSCACCode));
			}

			return result.ToArray();
		}
		public ICodeDescriptionPairList GetAmendmentOptions() => null;

		public string GetMessageStatus() => null;

		public bool? GetRequireMessageAmendmentReason() => null;

		public ICodeDescriptionPairList GetWithdrawalOptions() => null;

		public string GetXmlNamespace() => null;

		public string GetDocumentaryOverrideDocumentName() => null;

		public bool? IsSendingAmendment() => null;

		public bool? ShowEvents() => null;

		public bool? ShowLastEventDetails() => null;

		#endregion
	}
}

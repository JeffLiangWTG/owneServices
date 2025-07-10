using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class ShippingOrderMessagingExtensions : BaseMessagingExtensions
	{
		public ShippingOrderMessagingExtensions(IDocument document, ForwardingConsol consol, IMessageInstructions messageInstructions)
		{
			this.document = document;
			this.messageInstructions = messageInstructions;
			this.bookingValidation = new OceanBookingMessagingValidation(ConsolDocumentNames.ShippingOrder, consol);
			this.consol = consol;
		}

		readonly OceanBookingMessagingValidation bookingValidation;
		readonly IDocument document;
		readonly IMessageInstructions messageInstructions;
		readonly ForwardingConsol consol;

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			return bookingValidation.ContinueWithResetToOriginal(notifications);
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			return bookingValidation.ContinueWithSendingMessage(notifications, document, messageInstructions);
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			return bookingValidation.ContinueWithSendingMessageAmendment(notifications, document, messageInstructions);
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			return bookingValidation.ContinueWithSendingMessageWithdrawal(notifications, document, messageInstructions);
		}

		public override bool? GetRequireMessageAmendmentReason()
		{
			return bookingValidation.GetRequireMessageAmendmentReason();
		}

		public override bool? IsSendingAmendment()
		{
			return bookingValidation.IsSendingAmendment();
		}

		public override KeyValuePair<string, string>[] GetAdditionalParametersForEvent()
		{
			var result = new List<KeyValuePair<string, string>>();

			var carrierSCACCode = consol.GetCarrierCodeWithFallback();
			if (!carrierSCACCode.IsEmpty)
			{
				result.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Company, carrierSCACCode));
			}

			return result.ToArray();
		}
	}
}

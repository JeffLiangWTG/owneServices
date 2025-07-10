using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Shared;
using Enterprise.MasterFiles.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	class SeaShipmentBookingRequestMessagingValidation : BookingMessagingValidation
	{
		public SeaShipmentBookingRequestMessagingValidation(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		#region Implementation

		#region ContinueWithSendingMessageWithdrawal

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (CarrierBookingReferenceIsEmpty)
			{
				var needPopulateBookingRequestErrorMessage = Res.GetString("722f65f0-1174-4a2f-877b-6194d75c6f33",
					"You are trying to send a Withdrawal/Cancellation message, carrier booking reference is mandatory to send a Withdrawal/Cancellation message.");
				notifications.ShowMessage(needPopulateBookingRequestErrorMessage, ConfirmationMessage);
				return false;
			}

			return base.ContinueWithSendingMessageWithdrawal(notifications);
		}

		#endregion

		#region Override

		protected override BusinessObject BizO => shipment;

		protected override string DocumentName => ShipmentDocumentNames.BookingRequest;
		protected override string DocumentDataStoreName => ShipmentDocumentDataStoreNames.BookingRequest;

		protected override OrgHeader Carrier => shipment.GetCarrierWithFallback();

		protected override bool CarrierCanNotReceiveThisMessage => !Carrier.ShippingLine?.RSL_BookingRequestAvailable ?? true;

		protected override bool CarrierBookingReferenceIsEmpty => shipment.GetCarrierBookingReference().IsEmpty;

		protected override string CarrierCode => shipment.GetCarrierCodeWithFallback();

		#region Error Message

		protected override string CarrierChangeErrorMessageForAmendment => Res.GetString(
			"87fcea49-5a69-4907-891d-71481dccd072",
			"You are trying to send a {0} amendment message to a new carrier. Please verify Co-Load With (or Planned Carrier) and Reset to Original if you are sending {0} to new carrier.",
			DocumentName);

		#endregion

		#endregion

		#endregion
	}
}

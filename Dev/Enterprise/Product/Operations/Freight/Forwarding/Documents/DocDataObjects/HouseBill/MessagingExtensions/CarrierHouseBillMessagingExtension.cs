using CargoWise.Application;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class CarrierHouseBillMessagingExtension : BaseMessagingExtensions
	{
		public CarrierHouseBillMessagingExtension(ForwardingShipment shipment, IMessageInstructions messageInstructions)
		{
			this.shipment = shipment;
			this.messageInstructions = messageInstructions;
		}

		readonly ForwardingShipment shipment;
		readonly IMessageInstructions messageInstructions;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (!shipment.IsElectronicShippingInstructionReceived)
			{
				var message = Res.GetString("df47cf13-33dc-4849-b34a-5d60ceac9721", "The Draft Bill of Lading cannot be sent until Shipping Instruction received from Booking Party.");
				var information = Res.GetString("7A539DD1-6B9A-403A-B5E5-764326CA305C", "Information");
				notifications?.ShowMessage(message, information);
				return false;
			}

			return null;
		}

		public override bool? IsSendingAmendment() => false;

		public override string GetMessageStatus()
		{
			var documentData = GetDocumentData() as IStmALogParent;
			return documentData?.GetCurrentMessageStatusFromLogs(messageInstructions.DocumentName, messageInstructions);
		}

		IVisualizerDocumentData GetDocumentData()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load(shipment, ShipmentDocumentDataStoreNames.CarrierBillOfLading);
		}

		public override string GetXmlNamespace() => shipment.IsEditingElectronicBOL ? DocDataConstants.XmlNamespaces.BLData : null;
		public override string GetDocumentaryOverrideDocumentName() => shipment.IsEditingElectronicBOL ? ShipmentDocumentNames.DraftBill : null;
	}
}

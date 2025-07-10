using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public sealed class CargoReceiptAdviceWithoutUIMessageSender : DocDataObjectWithoutUIMessageSender
	{
		protected override IMessageInstructions GetMessageInstructions(BusinessObjectFactory factory) => factory.GetCachedValue(nameof(CargoReceiptAdviceMessageInstructions), () => new CargoReceiptAdviceMessageInstructions());

		protected override DocDataObject GetDocDataObject(BusinessObject bizObj)
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.English))
			{
				return new CargoReceiptAdviceBuilder(bizObj as ForwardingShipment).Build();
			}
		}

		protected override bool IsValidBeforeSending(BusinessObject bizObj, INotifications notifications)
		{
			if (bizObj is ForwardingShipment shipment)
			{
				if (shipment.OuterPackLines.Count == 0)
				{
					notifications.AddMessageError((NoResString)"The Shipment must have at least one valid packing line to send Cargo Receipt Advice to Booking Party."); // ErrorMessage
					return false;
				}

				var masterShipment = shipment.CoLoadMasterShipment as ForwardingShipment;
				if (masterShipment != null && masterShipment.IsElectronicShippingInstructionReceived)
				{
					notifications.AddMessageError((NoResString)"Shipping Instruction is received against this Shipment; Cargo Receipt Advice is not allowed to be sent to Booking Party once Shipping Instruction is received."); // ErrorMessage
					return false;
				}

				return true;
			}

			notifications.AddMessageError((NoResString)"Can not load Shipment correctly."); // ErrorMessage
			return false;
		}

		protected override ZString DataStoreName => ShipmentDocumentDataStoreNames.CargoReceiptAdvice;
	}
}

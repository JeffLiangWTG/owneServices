using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CarrierHouseBillMessageEventProcessor : IMessageEventsProcessor
	{
		public CarrierHouseBillMessageEventProcessor(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public void OnMessageSent()
		{
			var factory = new BusinessObjectFactory();
			var shipmentInAnotherFactory = factory.Load<ForwardingShipment>(shipment.PK);

			using (shipmentInAnotherFactory.SuspendShipmentStatusUpdateActions())
			{
				shipmentInAnotherFactory.JS_ShipmentStatus = Integration.ShipmentStatusList.Codes.Confirmed;
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
		}

		public void OnMessageWithdrawalSent() { }

		public void OnResetToOriginal() { }
	}
}

using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ShipmentCargoImpPhase2MessageDelivery : CargoImpPhase2MessageDelivery
	{
		public ShipmentCargoImpPhase2MessageDelivery(ForwardingShipment shipment, ProcessTaskNotification action, IQueuedLog queuedLog)
			: base(action, queuedLog)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		protected override void ProcessCore(ZString messageType, ZString cargoIMPEvent, INotifications notifications)
		{
			CreateMessage(this.shipment, messageType, cargoIMPEvent, notifications);
		}
	}
}

using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolCargoImpPhase2MessageDelivery : CargoImpPhase2MessageDelivery
	{
		public ConsolCargoImpPhase2MessageDelivery(ForwardingConsol consol, ProcessTaskNotification action, IQueuedLog queuedLog)
			: base(action, queuedLog)
		{
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		protected override void ProcessCore(ZString messageType, ZString cargoIMPEvent, INotifications notifications)
		{
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				CreateMessage(shipment, messageType, cargoIMPEvent, notifications);
			}
		}
	}
}

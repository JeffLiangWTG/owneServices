
namespace Enterprise.Freight.Forwarding.Business
{
	using Enterprise.Freight.Business;

	public class ForwardingShipmentEventDataModel : ShipmentEventDataModel<ForwardingShipment>
	{
		public ForwardingShipmentEventDataModel(ForwardingShipment shipment)
			: base(shipment)
		{
		}
	}
}

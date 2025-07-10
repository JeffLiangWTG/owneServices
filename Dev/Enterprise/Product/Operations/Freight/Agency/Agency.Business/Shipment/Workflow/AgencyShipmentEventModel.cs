
namespace Enterprise.Freight.Agency.Business
{
	using Enterprise.Freight.Business;

	public class AgencyShipmentEventDataModel : ShipmentEventDataModel<AgencyShipment>
	{
		public AgencyShipmentEventDataModel(AgencyShipment shipment)
			: base(shipment)
		{
		}
	}
}

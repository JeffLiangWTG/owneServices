using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentCollection : ActiveBusinessObjectCollection<AgencyShipment>
	{
		public AgencyShipmentCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public AgencyShipmentCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter) { }
	}
}

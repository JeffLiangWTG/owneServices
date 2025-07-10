using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentEventDataModelTest : ShipmentEventDataModelTest
	{
		protected override CommonShipment GetShipmentInstance()
		{
			return Factory.New<AgencyShipment>();
		}
	}
}

using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentEventDataModelTest : ShipmentEventDataModelTest
	{
		protected override CommonShipment GetShipmentInstance()
		{
			return Factory.New<ForwardingShipment>();
		}
	}
}

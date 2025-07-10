using Enterprise.Warehouse.Transit.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment))]
	sealed class ForwardingShipmentITransitWarehouseParentTestCase : ITransitWarehouseParentForShipmentTestCase<ForwardingShipment>
	{
		protected override ForwardingShipment GetNewParent(string jobNumber)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = jobNumber;
			return shipment;
		}

		protected override string GetParentTableCode()
		{
			return JobShipmentSchema.Constants.Prefix;
		}
	}
}

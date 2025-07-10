using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentNumberEntry))]
	sealed class ShipmentNumberEntryBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new ShipmentNumberEntry(shipment);
		}
	}
}

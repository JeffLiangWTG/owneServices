using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentPrepareForDispatchInstructionCollection))]
	sealed class ShipmentPrepareForDispatchInstructionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentPrepareForDispatchInstructionCollection>
	{
		protected override ShipmentPrepareForDispatchInstructionCollection GetCollectionToTest()
		{
			var parentConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			parentConsol.Shipments.AddNew();
			parentConsol.Shipments.AddNew();
			parentConsol.Shipments.AddNew();

			return new ShipmentPrepareForDispatchInstructionCollection(parentConsol, TransitWarehouseInstructionHelper.Direction.Pickup);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipmentPrepareForDispatchInstruction(Factory.NewWithValidTestData<ForwardingShipment>(), Factory.NewWithValidTestData<OrgAddress>());
		}
	}
}

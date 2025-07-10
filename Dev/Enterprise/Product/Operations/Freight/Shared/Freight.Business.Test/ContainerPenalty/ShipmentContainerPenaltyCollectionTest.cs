using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentContainerPenaltyCollection))]
	sealed class ShipmentContainerPenaltyCollectionTest : ActiveBusinessObjectCollectionTestCase<ShipmentContainerPenaltyCollection>
	{
		public void TestProcessType()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();
			var deliveryPenalty = shipment.DeliveryPenalties.AddNew();
			var pickupPenalty = shipment.PickupPenalties.AddNew();
			AssertEquals("DLV", deliveryPenalty.CPY_ProcessType);
			AssertEquals("PIC", pickupPenalty.CPY_ProcessType);
		}

		protected override ShipmentContainerPenaltyCollection GetCollectionToTest()
		{
			return Factory.NewWithValidTestData<CommonShipment>().PickupPenalties;
		}
	}
}

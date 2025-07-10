using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing.DtbConsignmentLeg
{
	[TestedType(typeof(DtbConsignmentLegCollection))]
	public class DtbConsignmentLegCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentLegCollection>
	{
		public void TestCollectionHasObject()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var pickupAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var pickupAction = Helper.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			var dlvAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			var consignmentLeg = helper.CreateConsignmentLeg(consignment, pickupAction, dlvAction);

			var pickupConsignmentLegs = new DtbConsignmentLegCollection(pickupAction, DtbConsignmentLegSchema.LTG_LTA_Pickup);
			var deliveryConsignmentLegs = new DtbConsignmentLegCollection(dlvAction, DtbConsignmentLegSchema.LTG_LTA_Delivery);

			AssertEquals("Collection should have 1 objects.", 1, pickupConsignmentLegs.Count);
			AssertContainsExactElementsInAnyOrder("Should contain consignment Leg", pickupConsignmentLegs, new[] { consignmentLeg });

			AssertEquals("Collection should have 1 objects.", 1, deliveryConsignmentLegs.Count);
			AssertContainsExactElementsInAnyOrder("Should contain consignment Leg", deliveryConsignmentLegs, new[] { consignmentLeg });
		}

		protected override DtbConsignmentLegCollection GetCollectionToTest()
		{
			return new DtbConsignmentLegCollection(Helper.CreateConsignment());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}

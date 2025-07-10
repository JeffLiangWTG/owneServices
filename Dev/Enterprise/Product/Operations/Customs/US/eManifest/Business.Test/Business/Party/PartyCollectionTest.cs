using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(PartyCollection))]
	sealed class PartyCollectionTest : ActiveBusinessObjectCollectionTestCase<PartyCollection>
	{
		public void TestPartyCollection()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			var consignee = shipment.Consignee;
			var shipper = shipment.Shipper;
			var carrier = shipment.Parties.AddNew();
			carrier.E2_AddressType = PartyTypes.Codes.Carrier;
			AssertEquals("Parties.Count", 1, shipment.Parties.Count);
			AssertEquals("Consignee should not be included into the parties collection", false, shipment.Parties.Contains(consignee));
			AssertEquals("Shipper should not be included into the parties collection", false, shipment.Parties.Contains(shipper));
		}

		protected override PartyCollection GetCollectionToTest() => new PartyCollection(Factory.New<Shipment>());
	}
}

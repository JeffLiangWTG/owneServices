using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(ShipmentActionCollection))]
	sealed class ShipmentActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentActionCollection>
	{
		public void TestLoad()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_ReleaseStatus = EntryStatusList.Codes.Cancelled;
			var shipment2 = trip.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			AssertEquals("ShipmentsActions.Count", 2, trip.ShipmentsActions.Count);
			AssertEquals("Action1.Shipment", shipment1, trip.ShipmentsActions[0].Shipment);
			AssertEquals("Action1.B0_ActionCode", MessageActionCodes.Codes.Original, trip.ShipmentsActions[0].B0_ActionCode);
			AssertEquals("Action2.Shipment", shipment2, trip.ShipmentsActions[1].Shipment);
			AssertEquals("Action2.B0_ActionCode", MessageActionCodes.Codes.Change, trip.ShipmentsActions[1].B0_ActionCode);
			var shipment3 = trip.Shipments.AddNew();
			shipment3.B0_ReleaseStatus = EntryStatusList.Codes.Error;
			AssertEquals("ShipmentsActions.Count", 3, trip.ShipmentsActions.Count);
			AssertEquals("Action3.Shipment", shipment3, trip.ShipmentsActions[2].Shipment);
			AssertEquals("Action3.B0_ActionCode", MessageActionCodes.Codes.Original, trip.ShipmentsActions[2].B0_ActionCode);
		}

		protected override ShipmentActionCollection GetCollectionToTest() => new ShipmentActionCollection(Factory.New<Trip>(), MessageTypes.Codes.eManifest);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ShipmentAction(Factory.New<Shipment>(), MessageTypes.Codes.eManifest);
	}
}

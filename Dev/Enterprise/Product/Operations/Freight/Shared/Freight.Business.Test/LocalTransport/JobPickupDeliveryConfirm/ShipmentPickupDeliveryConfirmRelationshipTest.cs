using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonPickupDeliveryConfirmCollection))]
	sealed class ShipmentPickupDeliveryConfirmRelationshipTest : ActiveBusinessObjectCollectionTestCase<CommonPickupDeliveryConfirmCollection>
	{
		public void TestRefreshByDataRefreshBus()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();

			var confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);

			Factory.Save();

			var newFactory = NewFactory();

			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);

			var confirmsInNewFactory = shipmentInNewFactory.PickupConfirms;

			var newConfirm = confirmsInNewFactory.AddNew();
			newConfirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 30);

			AssertEquals(2, confirmsInNewFactory.Count);
			AssertEquals(1, shipment.PickupConfirms.Count);

			newFactory.Save();

			AssertEquals("Should be refreshed by the data refresh bus.", 2, shipment.PickupConfirms.Count);

			var expectedPks = new[] { confirm.PK, newConfirm.PK };
			var actualPks = shipment.PickupConfirms.GetPKs();

			AssertContainsExactElementsInAnyOrder(expectedPks, actualPks);
		}

		public void TestLoadBusinessObjectsDBHits()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.PickupConfirms.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<CommonShipment>(shipment.PK);
			var relationship = new ShipmentPickupDeliveryConfirmRelationship(shipment, Constants.PickupDeliveryConfirmTypes.OriginPickup);
			var initialDBHitCount = newFactory.DatabaseLoadCount;
			relationship.LoadBusinessObjects(newFactory, new ZQuery());
			var dbHitCountAfterFirstLoad = newFactory.DatabaseLoadCount;
			Assert(dbHitCountAfterFirstLoad - initialDBHitCount > 0);

			relationship = new ShipmentPickupDeliveryConfirmRelationship(shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			relationship.LoadBusinessObjects(newFactory, new ZQuery());
			AssertEquals(dbHitCountAfterFirstLoad, newFactory.DatabaseLoadCount);
		}

		public void TestHasChangesIncludingRelationship_MixedConfirmTypes()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();

			consol.JK_ConsolMode = "FCL";
			shipment.JS_PackingMode = "FCL";

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 56;
			line1.Containers.RemoveAll();

			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 23;

			var confirm1 = shipment.PickupConfirms.AddNew();
			var confirm2 = container.OriginConfirm;

			Assert("Packline and container divots don't share a confirm (1)", confirm1.Divots[0].J8_EU_PickupDeliverConfirm != confirm2.PK);
			Assert("Packline and container divots don't share a confirm (2)", confirm1.Divots[1].J8_EU_PickupDeliverConfirm != confirm2.PK);

			confirm1.Divots[0].J8_EU_PickupDeliverConfirm = confirm2.PK;

			Factory.Save();

			Assert("Expect Exception to be reported due to manually moving divot from one confirm to another", ErrorReporter.LastMessageReported.Contains("Shipment PackLine count: 2 (2) differs from Divots count: 1 (1)"));
			ErrorReporter.Clear();

			var relationship = new ShipmentPickupDeliveryConfirmRelationship(shipment, Constants.PickupDeliveryConfirmTypes.OriginPickup);

			Assert("Even if they did (legacy data) then it wouldn't break HasChangesIncludingRelationship (1)", !relationship.HasChangesIncludingRelationship(confirm1));
			Assert("Even if they did (legacy data) then it wouldn't break HasChangesIncludingRelationship (2)", !relationship.HasChangesIncludingRelationship(confirm2));
		}

		public void TestBuildRelationshipFilterDBHits()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.PickupConfirms.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<CommonShipment>(shipment.PK);
			var relationship = new ShipmentPickupDeliveryConfirmRelationship(shipment, Constants.PickupDeliveryConfirmTypes.OriginPickup);
			var initialDBHitCount = newFactory.DatabaseLoadCount;
			shipment.OuterPackLines.AddNew();
			var dbHitCountAfterFirstLoad = newFactory.DatabaseLoadCount;
			Assert(dbHitCountAfterFirstLoad - initialDBHitCount > 0);

			relationship = new ShipmentPickupDeliveryConfirmRelationship(shipment, Constants.PickupDeliveryConfirmTypes.OriginPickup);
			shipment.OuterPackLines.AddNew();
			AssertEquals(dbHitCountAfterFirstLoad, newFactory.DatabaseLoadCount);
		}

		#region Implementation

		protected override CommonPickupDeliveryConfirmCollection GetCollectionToTest()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			return new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Does not currently support remove, only delete", true);
		}

		#endregion
	}
}

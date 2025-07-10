using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLocationForDocumentCollectionTest : TestCaseWithFactory
	{
		public void TestShipmentWithNoPackLines()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Shipment.OuterPackLines.RemoveAll();

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, Shipment.JS_OuterPacks, Shipment.JS_F3_NKPackType, "TST : A-3-1-1");

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, Shipment.JS_OuterPacks, Shipment.JS_F3_NKPackType, "Warehouse1");
		}

		public void TestShipmentWithPackLine()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Shipment.OuterPackLines.RemoveAll();

			PackLine line = SetupPackLine(Shipment, 10, "BAG");

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, line.JL_PackageCount, line.JL_F3_NKPackType, "TST : A-3-1-1");

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, line.JL_PackageCount, line.JL_F3_NKPackType, "Warehouse1");
		}

		public void TestShipmentWithPackCountsNotMatching()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Shipment.OuterPackLines.RemoveAll();

			PackLine line = SetupPackLine(Shipment, 7, "BAG");

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 2, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, 3, Shipment.JS_F3_NKPackType, "TST : A-3-1-1");
			AssertPackLocationInCollection(collection, line.JL_PackageCount, line.JL_F3_NKPackType, "TST : A-3-1-1");

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertPackLocationInCollection(collection, 3, Shipment.JS_F3_NKPackType, "Warehouse1");
			AssertPackLocationInCollection(collection, line.JL_PackageCount, line.JL_F3_NKPackType, "Warehouse1");
		}

		public void TestShipmentWithPackLocationsOnPackLines()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Shipment.OuterPackLines.RemoveAll();

			PackLine line = SetupPackLine(Shipment, 10, "BAG");
			PackLocation location = SetupPackLocation(line, 10, WhsLoc2, "Warehouse2");

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, location.JQ_NoPackages, line.JL_F3_NKPackType, "TST : A-4-3-2");

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, location.JQ_NoPackages, line.JL_F3_NKPackType, "Warehouse2");
		}

		public void TestShipmentWithPackLocationsButDifferingTotals()
		{
			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Shipment.OuterPackLines.RemoveAll();

			PackLine line = SetupPackLine(Shipment, 7, "BAG");
			PackLocation location1 = SetupPackLocation(line, 4, WhsLoc2, "Warehouse2");
			PackLocation location2 = SetupPackLocation(line, 2, WhsLoc3, "Warehouse3");

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 4, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, 3, Shipment.JS_F3_NKPackType, "TST : A-3-1-1");
			AssertPackLocationInCollection(collection, 1, line.JL_F3_NKPackType, "TST : A-3-1-1");
			AssertPackLocationInCollection(collection, location1.JQ_NoPackages, line.JL_F3_NKPackType, "TST : A-4-3-2");
			AssertPackLocationInCollection(collection, location2.JQ_NoPackages, line.JL_F3_NKPackType, "TST : A-1-2-2");

			Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 4, collection.PackLocations.Count);
			AssertPackLocationInCollection(collection, 3, Shipment.JS_F3_NKPackType, "Warehouse1");
			AssertPackLocationInCollection(collection, 1, line.JL_F3_NKPackType, "Warehouse1");
			AssertPackLocationInCollection(collection, location1.JQ_NoPackages, line.JL_F3_NKPackType, "Warehouse2");
			AssertPackLocationInCollection(collection, location2.JQ_NoPackages, line.JL_F3_NKPackType, "Warehouse3");
		}

		public void TestAvailableDate()
		{
			//ContainerLeg Leg1 = Shipment.DeliveryLegs.AddNew();
			//ContainerLeg Leg2 = Shipment.DeliveryLegs.AddNew();
			//ContainerLeg Leg3 = Shipment.DeliveryLegs.AddNew();
			//ContainerLeg Leg4 = Shipment.DeliveryLegs.AddNew();
			//ContainerLeg Leg5 = Shipment.DeliveryLegs.AddNew();

			//Leg1.JU_Leg = "FCD";
			//Leg1.JU_DeliverTimeIn = ZDateTime.Now.AddDays(3);
			//Leg2.JU_Leg = "COL";
			//Leg2.JU_DeliverTimeIn = ZDateTime.Now;
			//Leg3.JU_Leg = "FCL";
			//Leg3.JU_DeliverTimeIn = ZDateTime.Now.AddDays(2);
			//Leg4.JU_Leg = "COL";
			//Leg4.JU_DeliverTimeIn = ZDateTime.Now.AddDays(2);
			//Leg5.JU_Leg = "COL";
			//Leg5.JU_DeliverTimeIn = ZDateTime.Now.AddDays(1);

			PackLocationForDocumentCollection collection = new PackLocationForDocumentCollection(Shipment, Factory);
			AssertEquals("Collection.PackLocations.Count", 1, collection.PackLocations.Count);
		}

		void AssertPackLocationInCollection(PackLocationForDocumentCollection collection, ZInt count, ZString type, ZString location)
		{
			bool result = false;
			foreach (PackLocationForDocument packLocation in collection.PackLocations)
			{
				if (packLocation.PackCount == count
				&& packLocation.PackType == type
				&& packLocation.WhsLocation == location)
				{
					result = true;
				}
			}
			Assert("Pack location is in collection", result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupWarehouses();
			Shipment = SetupShipment(10, "PKG", WhsLoc1, "Warehouse1");
		}

		CommonShipment SetupShipment(ZInt packCount, ZString packType, IWhsLocation trueLocation, ZString locationString)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = packCount;
			shipment.JS_F3_NKPackType = packType;
			shipment.JS_WL = trueLocation.PK;
			shipment.JS_WarehouseLocation = locationString;
			return shipment;
		}

		PackLine SetupPackLine(CommonShipment shipment, ZInt packCount, ZString packType)
		{
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = packCount;
			line.JL_F3_NKPackType = packType;
			return line;
		}

		PackLocation SetupPackLocation(PackLine line, ZInt packCount, IWhsLocation trueLocation, ZString locationString)
		{
			PackLocation location = line.PackLocations.AddNew();
			location.JQ_NoPackages = packCount;
			location.JQ_WarehouseLocation = locationString;
			location.JQ_WL = trueLocation.PK;
			return location;
		}

		void SetupWarehouses()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			Warehouse = (IWhsWarehouse)helper.CreateWarehouse("Test warehouse");
			Warehouse.WW_WarehouseCode = "TST";
			var row = helper.CreateRowAndGenerateLocations(Warehouse, "A", 4, 3, 2);

			Factory.Save();

			WhsLoc1 = (IWhsLocation)row.Locations[12];
			WhsLoc2 = (IWhsLocation)row.Locations[23];
			WhsLoc3 = (IWhsLocation)row.Locations[3];
		}

		CommonShipment Shipment;
		IWhsWarehouse Warehouse;
		IWhsLocation WhsLoc1;
		IWhsLocation WhsLoc2;
		IWhsLocation WhsLoc3;
	}
}

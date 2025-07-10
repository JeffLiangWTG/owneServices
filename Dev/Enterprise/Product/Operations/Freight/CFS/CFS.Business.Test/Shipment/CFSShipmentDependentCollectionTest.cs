using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal class CFSShipmentDependentCollectionTest : TestCaseWithFactory
	{
		public void TestDefaultContainerForAdds()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSShipmentDependentCollection collection = new CFSShipmentDependentCollection(Factory, container);
			CFSShipment shipment = collection.AddNew();

			AssertEquals("Container count", 1, shipment.DefaultPackLine.Containers.Count);
			AssertEquals("Container PK", container.PK, shipment.DefaultPackLine.Containers[0].PK);
		}

		public void TestDefaultContainerForLoads()
		{
			CFSContainer newContainer = Factory.New<CFSContainer>();
			CFSShipmentDependentCollection collectionForAdding = new CFSShipmentDependentCollection(Factory, newContainer);
			CFSShipment shipmentAdded = collectionForAdding.AddNew();
			AssertEquals("1 added", 1, collectionForAdding.Count);

			CFSShipmentDependentCollection collectionForLoading = new CFSShipmentDependentCollection(Factory, newContainer);
			collectionForLoading.Load();
			AssertEquals("1 loaded", 1, collectionForLoading.Count);
			CFSShipment shipmentLoaded = collectionForLoading[0];

			AssertEquals("Container count", 1, shipmentLoaded.DefaultPackLine.Containers.Count);
			AssertEquals("Container PK", newContainer.PK, shipmentLoaded.DefaultPackLine.Containers[0].PK);
		}

		public void TestParentContainerForAdds()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSShipmentDependentCollection collection = new CFSShipmentDependentCollection(Factory, container);
			CFSShipment shipment = collection.AddNew();

			AssertEquals("parent set", container, shipment.ParentContainerRegistration);
		}

		public void TestParentContainerForLoads()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSShipmentDependentCollection collectionForAdding = new CFSShipmentDependentCollection(Factory, container);
			CFSShipment shipmentAdded = collectionForAdding.AddNew();
			shipmentAdded.ParentContainerRegistration = null;
			AssertEquals("linked to container", container.PK, shipmentAdded.DefaultPackLine.Containers[0].PK);

			CFSShipmentDependentCollection collectionForLoading = new CFSShipmentDependentCollection(Factory, container);
			collectionForLoading.Load();
			AssertEquals("1 loaded", 1, collectionForLoading.Count);
			CFSShipment shipmentLoaded = collectionForLoading[0];

			AssertEquals("parent set", container, shipmentLoaded.ParentContainerRegistration);
			//AssertEquals("parent set", 1, ShipmentLoaded.OuterPackLinesViewedByParentContainer.Count);
		}

		public void TestTotalsOnCoLoadForCoLoads()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSShipmentDependentCollection testCollection = new CFSShipmentDependentCollection(Factory, container);
			CFSShipment coLoadMaster = Factory.New<CFSShipment>();
			testCollection.Add(coLoadMaster);
			coLoadMaster.JS_ActualWeight = 4251.124m;
			coLoadMaster.JS_ActualVolume = 4.5m;
			coLoadMaster.JS_OuterPacks = 20;
			AssertEquals("Total Package Count", 20, testCollection.TotalPacks);
			AssertEquals("Total Volume", 4.5m, testCollection.TotalVolume);
			AssertEquals("Total Weight", 4251.124m, testCollection.TotalWeight);
			coLoadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CFSShipment subHouse1 = Factory.New<CFSShipment>();
			coLoadMaster.CoLoadShipments.Add(subHouse1);
			subHouse1.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			subHouse1.JS_ActualWeight = 2251.004m;
			subHouse1.JS_ActualVolume = 2.25m;
			subHouse1.JS_OuterPacks = 12;
			CFSShipment subHouse2 = Factory.New<CFSShipment>();
			coLoadMaster.CoLoadShipments.Add(subHouse2);
			subHouse2.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			subHouse2.JS_ActualWeight = 2000.12m;
			subHouse2.JS_ActualVolume = 2.25m;
			subHouse2.JS_OuterPacks = 8;
			testCollection.Add(subHouse1);
			testCollection.Add(subHouse2);
			AssertEquals("Total Package Count", 20, testCollection.TotalPacks);
			AssertEquals("Total Volume", 4.5m, testCollection.TotalVolume);
			AssertEquals("Total Weight", 4251.124m, testCollection.TotalWeight);
		}

		public void TestTotalsOnCoLoadForBlindCoLoads()
		{
			var container = Factory.New<CFSContainer>();
			var collection = new CFSShipmentDependentCollection(Factory, container);
			var shipment = Factory.New<CFSShipment>();
			collection.Add(shipment);

			shipment.JS_ActualWeight = 1234.987m;
			shipment.JS_ActualVolume = 6m;
			shipment.JS_OuterPacks = 10;

			AssertEquals("Total Package Count", 10, collection.TotalPacks);
			AssertEquals("Total Volume", 6m, collection.TotalVolume);
			AssertEquals("Total Weight", 1234.987m, collection.TotalWeight);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			var subShipment1 = shipment.CoLoadShipments.AddNew();
			subShipment1.JS_ActualWeight = 2251.004m;
			subShipment1.JS_ActualVolume = 2.25m;
			subShipment1.JS_OuterPacks = 12;

			var subShipment2 = shipment.CoLoadShipments.AddNew();
			subShipment2.JS_ActualWeight = 2000.12m;
			subShipment2.JS_ActualVolume = 2.25m;
			subShipment2.JS_OuterPacks = 8;

			collection.Add(subShipment1);
			collection.Add(subShipment2);

			AssertEquals("Total Package Count", 20, collection.TotalPacks);
			AssertEquals("Total Volume", 4.5m, collection.TotalVolume);
			AssertEquals("Total Weight", 4251.124m, collection.TotalWeight);
		}

		public void TestShipmentAddedToContainerAppearsInLoadList()
		{
			PackUnpackLoadListConsol loadList = Factory.New<PackUnpackLoadListConsol>();
			TallyContainer container = loadList.Containers.AddNew();
			AssertEquals("Precondition: LoadList should have no shipments", 0, loadList.Shipments.Count);
			container.PackUnpackShipments.AddNew();
			AssertEquals("Adding a shipment to the container should add it to the load list also", 1, loadList.Shipments.Count);
		}

		public void TestCollectionLoadDoesNotAddShipmentToLoadList()
		{
			PackUnpackLoadListConsol loadList = Factory.New<PackUnpackLoadListConsol>();
			TallyContainer container = loadList.Containers.AddNew();

			var shipment = Factory.NewWithValidTestData<PackUnpackShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.Containers.Add(container);
			AssertEquals("CommonContainerManyToManyCollection.OnAdded()", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			AssertEquals("Precondition: LoadList should have no shipments", 0, loadList.Shipments.Count);
			container.PackUnpackShipments.Load();
			AssertEquals("LoadList should have no shipments", 0, loadList.Shipments.Count);
		}
	}
}

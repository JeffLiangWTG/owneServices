using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Freight.CFS.Business
{
	internal class PackUnpackShipmentDependentCollectionTest : TestCaseWithFactory
	{
		public void TestDefaultContainerForAdds()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection collection = new PackUnpackShipmentDependentCollection(Factory, container);
			PackUnpackShipment shipment = collection.AddNew();

			AssertEquals("Container count", 1, shipment.DefaultPackLine.Containers.Count);
			AssertEquals("Container PK", container.PK, shipment.DefaultPackLine.Containers[0].PK);
		}

		public void TestDefaultContainerForLoads()
		{
			TallyContainer newContainer = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection collectionForAdding = new PackUnpackShipmentDependentCollection(Factory, newContainer);
			PackUnpackShipment shipmentAdded = collectionForAdding.AddNew();
			AssertEquals("1 added", 1, collectionForAdding.Count);

			PackUnpackShipmentDependentCollection collectionForLoading = new PackUnpackShipmentDependentCollection(Factory, newContainer);
			collectionForLoading.Load();
			AssertEquals("1 loaded", 1, collectionForLoading.Count);
			PackUnpackShipment shipmentLoaded = collectionForLoading[0];

			AssertEquals("Container count", 1, shipmentLoaded.DefaultPackLine.Containers.Count);
			AssertEquals("Container PK", newContainer.PK, shipmentLoaded.DefaultPackLine.Containers[0].PK);
		}

		public void TestParentContainerForAdds()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection collection = new PackUnpackShipmentDependentCollection(Factory, container);
			PackUnpackShipment shipment = collection.AddNew();

			AssertEquals("parent set", container, shipment.ParentContainerRegistration);
		}

		public void TestParentContainerForLoads()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection collectionForAdding = new PackUnpackShipmentDependentCollection(Factory, container);
			PackUnpackShipment shipmentAdded = collectionForAdding.AddNew();
			shipmentAdded.ParentContainerRegistration = null;
			AssertEquals("linked to container", container.PK, shipmentAdded.DefaultPackLine.Containers[0].PK);

			PackUnpackShipmentDependentCollection collectionForLoading = new PackUnpackShipmentDependentCollection(Factory, container);
			collectionForLoading.Load();
			AssertEquals("1 loaded", 1, collectionForLoading.Count);
			PackUnpackShipment shipmentLoaded = collectionForLoading[0];

			AssertEquals("parent set", container, shipmentLoaded.ParentContainerRegistration);
			//AssertEquals("parent set", 1, ShipmentLoaded.OuterPackLinesViewedByParentContainer.Count);
		}

		public void TestTotalsOnCoLoadForCoLoads()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			PackUnpackShipmentDependentCollection testCollection = new PackUnpackShipmentDependentCollection(Factory, container);
			PackUnpackShipment coLoadMaster = Factory.New<PackUnpackShipment>();
			testCollection.Add(coLoadMaster);
			coLoadMaster.JS_ActualWeight = 4251.124m;
			coLoadMaster.JS_ActualVolume = 4.5m;
			coLoadMaster.JS_OuterPacks = 20;
			AssertEquals("Total Package Count", 20, testCollection.TotalPacks);
			AssertEquals("Total Volume", 4.5m, testCollection.TotalVolume);
			AssertEquals("Total Weight", 4251.124m, testCollection.TotalWeight);
			coLoadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			PackUnpackShipment subHouse1 = Factory.New<PackUnpackShipment>();
			coLoadMaster.CoLoadShipments.Add(subHouse1);
			subHouse1.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			subHouse1.JS_ActualWeight = 2251.004m;
			subHouse1.JS_ActualVolume = 2.25m;
			subHouse1.JS_OuterPacks = 12;
			PackUnpackShipment subHouse2 = Factory.New<PackUnpackShipment>();
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
	}
}

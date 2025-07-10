using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Facts;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CycleCountLocationTaskCreationFactLoaderTest : CycleCountLocationFactLoaderTest<ICycleCountLocationTaskCreationFactLoader, CycleCountLocationTaskCreationFactLoader>
	{
		public void TestLoadInputFacts_ExcludeVoidLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one VOID location and default DockDoor location", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's VOID", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeDockDoorLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.DDL);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one DockDoor location and default Dockdoor location", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludePackingStationLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", "Location Type 1", false, 0, LocationClasses.Codes.PST);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one PackingStation location and default Dockdoor location", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludePackingConsolidationLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var packingConsolidationlocationType = Helper.CreateLocationType("CON", "Consloidation", false, 0, LocationClasses.Codes.CON);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = packingConsolidationlocationType.PK;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one Packing Consolidation location and default Dockdoor location", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it's of LocationClass DockDoor", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeLocation_HasWCLEndTimeIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location1, "PLT02");
			Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.PalletIDOnly);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one location which has CycleCountLocation where WCL_EndTime is null", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has a cycle count location which WCL_EndTime is null", locationFact1);
		}

		public void TestLoadInputFacts_ExcludeLocation_HasOpenVariance_WCL_WL_Location()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");

			var ccLocation = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Today, ZDateTimeOffset.Today);
			ccLocation.WCL_GS_NKAssignedTo = "DDD";
			Helper.CreateWhsCycleCountLocationVariance(ccLocation, CycleCountVarianceStatus.Codes.Open, true, varianceQty: -1, expectedQty: 1);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Exclude one location which has an open variance", 7, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has an open variance by WCL_WL_Location", locationFact1);
		}

		public void TestLoadInputFacts_NotExcludeLocation_HasOpenVariance_WCL_WL_ExpectedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location2.WLV_WLT_LocationType = locationType1.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location2, "PLT02");

			var ccLocation1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Today, ZDateTimeOffset.Today);
			ccLocation1.WCL_GS_NKAssignedTo = "DDD";
			var variance = Helper.CreateWhsCycleCountLocationVariance(ccLocation1, CycleCountVarianceStatus.Codes.Open, true, varianceQty: 1, expectedQty: 1);
			variance.WCC_WL_ExpectedStockLocation = location2.PK;
			variance.WCC_PalletID = "PLT02";
			variance.WCC_IsCountingPalletsOnly = false;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("Should not exclude location2 which has open variance by WCC_WL_ExpectedStockLocation", 7, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).SingleOrDefault();
			AssertNull("location1 should be excluded because it has an open variance by WCL_WL_Location", locationFact1);

			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "NO1", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
				AssertEquals("Granularity", string.Empty, ((CycleCountLocationCoreFact)locationFact2.WrappedLocation.Fact).Granularity);
				AssertEquals("Priority", 0, ((CycleCountLocationCoreFact)locationFact2.WrappedLocation.Fact).Priority);
			});
		}
	}
}

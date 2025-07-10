using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseCycleCountTaskCreation;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class CycleCountLocationFactLoaderTest<TInterface, TClass> : WhsTestCaseWithFactory
		where TInterface : ICycleCountLocationFactLoader
		where TClass : TInterface, new()
	{
		public void TestObjectFactoryConfiguration()
		{
			var factLoader = ObjectFactory.Get<TInterface>();
			AssertType<TClass>(factLoader);
			AssertEquals("Should be a singleton.", factLoader, ObjectFactory.Get<TInterface>());
		}

		public void TestLoadInputFacts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location1.WLV_PickMethod = "CAR";
			location2.WLV_WLT_LocationType = locationType2.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;
			location2.WLV_PickMethod = "UFO";
			SetupRow(row);

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location2, "PLT02");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 8, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact.PK == part1Relation.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact.PK == part2Relation.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "NO1", locationFact1.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact1.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact1.AreaName);
				AssertEquals("RowName", "RO1", locationFact1.RowName);
				AssertEquals("LocationString", "RO1-1-1-1", locationFact1.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Held, locationFact1.LocationStatus);
				AssertEquals("PickMethod", "CAR", locationFact1.PickMethod);
				AssertEquals("Column", 1, locationFact1.Column);
				AssertEquals("Level", 1, locationFact1.Level);
				AssertEquals("Tray", 1, locationFact1.Tray);

				AssertEquals("LocationTypeCode", "NO2", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.HPL, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("PickMethod", "UFO", locationFact2.PickMethod);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
			});
		}

		public void TestLoadInputFacts_TwoWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("W01");
			var whs2 = Helper.CreateWarehouse("W02");
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var row2 = Helper.CreateRow(whs2, "RO2", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");

			Factory.Save();
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");
			var location3 = row2.Locations.Single(l => l.WLV_LocationString == "RO2-1-1-1");
			var location4 = row2.Locations.Single(l => l.WLV_LocationString == "RO2-1-1-2");
			location1.WLV_WLT_LocationType = locationType1.PK;
			location1.WLV_LocationStatus = LocationStatus.Codes.Held;
			location1.WLV_WA_PickingArea = area.PK;
			location2.WLV_WLT_LocationType = locationType2.PK;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_WA_PickingArea = area.PK;
			SetupRow(row);
			SetupRow(row2);

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location2, "PLT02");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R3", data.Part1, 11m, location3, "PLT03");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R4", data.Part2, 23m, location4, "PLT04");
			Factory.Save();

			AssertEquals("Precondition: wh1 has 9 locations", 9, whs1.Rows.Sum(r => r.Locations.Count));
			AssertEquals("Precondition: wh2 has 9 locations", 9, whs2.Rows.Sum(r => r.Locations.Count));

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 8, locationFacts.Length);
			AssertEquals("No locations from wh2", 0, locationFacts.Where(l => row2.Locations.Any(l2 => l2.PK == l.PK)).Count());
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact.PK == part1Relation.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact.PK == part2Relation.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationTypeCode", "NO1", locationFact1.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.NOR, locationFact1.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact1.AreaName);
				AssertEquals("RowName", "RO1", locationFact1.RowName);
				AssertEquals("LocationString", "RO1-1-1-1", locationFact1.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Held, locationFact1.LocationStatus);
				AssertEquals("Column", 1, locationFact1.Column);
				AssertEquals("Level", 1, locationFact1.Level);
				AssertEquals("Tray", 1, locationFact1.Tray);
				AssertEquals("Part Code", data.Part1.OP_PartNum, locationFact1.Product.Fact.Code);
				AssertEquals("StockOnHand", 10m, locationFact1.StockOnHand);

				AssertEquals("LocationTypeCode", "NO2", locationFact2.LocationTypeCode);
				AssertEquals("LocationClass", LocationClasses.Codes.HPL, locationFact2.LocationClass);
				AssertEquals("AreaName", "AE1", locationFact2.AreaName);
				AssertEquals("RowName", "RO1", locationFact2.RowName);
				AssertEquals("LocationString", "RO1-1-1-2", locationFact2.LocationString);
				AssertEquals("LocationStatus", LocationStatus.Codes.Normal, locationFact2.LocationStatus);
				AssertEquals("Column", 1, locationFact2.Column);
				AssertEquals("Level", 1, locationFact2.Level);
				AssertEquals("Tray", 2, locationFact2.Tray);
				AssertEquals("Part Code", data.Part2.OP_PartNum, locationFact2.Product.Fact.Code);
				AssertEquals("StockOnHand", 20m, locationFact2.StockOnHand);
			});
		}

		public void TestLoadInputFacts_UsesSmartParameterisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row);

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Context = "PWC";
			ruleSet.PRS_Name = "PWC-TEST";
			ruleSet.PRS_Description = "PWC-TEST";
			ruleSet.PRS_WW_Warehouse = whs1.PK;
			Factory.Save();

			var processor = GetFactLoader();

			using (TestConnection.TrackExecutedCommands())
			{
				var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
				var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

				AssertEquals("Exclude default DockDoor location", 8, locationFacts.Length);
				AssertEquals("There are 8 empty locations", 8, locationFacts.Where(l => l.Product.Fact == null).Count());

				var selectCommand = TestConnection.ExecutedCommands.SingleOrDefault(c =>
					c.Contains("CROSS APPLY dbo.WhsLocationIndexForSort(WLV_Column, WLV_Level, WLV_Tray, WR_Levels, WR_Trays) as LocationIndexForSort"));
				AssertContains("Smart parameterisation should be used.", "WLV_WW_Whs = @WhsPK_NOHISTOGRAM", selectCommand);
			}
		}

		public void TestLoadInputFacts_ABCCategory()
		{
			var date = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row);

			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");

			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB1", date.AddDays(-10), date);
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB2", date.AddDays(-10), date.AddDays(1));
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB3", date.AddDays(-10), date.AddDays(-1));
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 8, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			AssertEquals("ABCCategory should be the top 1 order by WJ_AnalysisDateTo desc", "AB2", locationFact1.Product.Fact.ABCCategoryCode);
		}

		public void TestLoadInputFacts_LocationAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var date1 = ZDateTimeOffset.Today;
			var date2 = new ZDateTimeOffset(2023, 2, 14);
			var date3 = new ZDateTimeOffset(2023, 1, 14);
			var date4 = new ZDateTimeOffset(2023, 1, 1);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row);
			var location1 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location2, "PLT02");

			location1.WLV_CycleCountPathSequence = 1;
			location1.WLV_CycleCountLastPerformed = date1;
			location1.WLV_LastInventoryChangeDate = date2;

			location2.WLV_CycleCountPathSequence = 2;
			location2.WLV_CycleCountLastPerformed = date3;
			location2.WLV_LastInventoryChangeDate = date4;
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 8, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("LocationStringSortIndex", 1, locationFact1.LocationStringSortIndex);
				AssertEquals("CycleCountPathSequence", 1, locationFact1.CycleCountPathSequence);
				AssertEquals("CycleCountLastPerformedDate", date1.ToDateTime(), locationFact1.CycleCountLastPerformedDate);
				AssertEquals("InventoryLastChangedDate", date2.ToDateTime(), locationFact1.InventoryLastChangedDate);
				AssertEquals("StockOnHand", 10m, locationFact1.StockOnHand);
				AssertEquals("HasCommittedStock", false, locationFact1.HasCommittedStock);

				AssertEquals("LocationStringSortIndex", 2, locationFact2.LocationStringSortIndex);
				AssertEquals("CycleCountPathSequence", 2, locationFact2.CycleCountPathSequence);
				AssertEquals("CycleCountLastPerformedDate", date3.ToDateTime(), locationFact2.CycleCountLastPerformedDate);
				AssertEquals("InventoryLastChangedDate", date4.ToDateTime(), locationFact2.InventoryLastChangedDate);
				AssertEquals("HasCommittedStock", false, locationFact2.HasCommittedStock);
			});
		}

		public void TestLoadInputFacts_StockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var part3 = Helper.CreateProduct("P3", data.Org1);
			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 5m, location1, "PLT02");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R3", data.Part2, 20m, location1, "PLT03");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R4", part3, 10m, location2, "PLT04");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 CCA locations", 9, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var part3Relation = part3.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == part1Relation.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == part2Relation.PK).Single();
			var locationFact3 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == part3Relation.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("StockOnHand on locationFact1", 35m, locationFact1.StockOnHand);
				AssertEquals("StockOnHand on locationFact2", 35m, locationFact2.StockOnHand);
				AssertEquals("StockOnHand on locationFact3", 10m, locationFact3.StockOnHand);
				AssertEquals("StockOnHandForThisProduct on locationFact1", 15m, locationFact1.StockOnHandForThisProduct);
				AssertEquals("StockOnHandForThisProduct on locationFact2", 20m, locationFact2.StockOnHandForThisProduct);
				AssertEquals("StockOnHandForThisProduct on locationFact3", 10m, locationFact3.StockOnHandForThisProduct);
				AssertEquals("HasCommittedStock on locationFact1", false, locationFact1.HasCommittedStock);
				AssertEquals("HasCommittedStock on locationFact2", false, locationFact2.HasCommittedStock);
				AssertEquals("HasCommittedStock on locationFact3", false, locationFact3.HasCommittedStock);
			});

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, data.Part2, 3m);
			var pick = Helper.CreatePickNew(new[] { order });
			Factory.Save();

			inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 CCA locations", 9, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == part1Relation.PK).Single();
			locationFact2 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == part2Relation.PK).Single();
			locationFact3 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == part3Relation.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals("StockOnHand on locationFact1", 35m, locationFact1.StockOnHand);
				AssertEquals("StockOnHand on locationFact2", 35m, locationFact2.StockOnHand);
				AssertEquals("StockOnHand on locationFact3", 10m, locationFact3.StockOnHand);
				AssertEquals("StockOnHandForThisProduct on locationFact1", 15m, locationFact1.StockOnHandForThisProduct);
				AssertEquals("StockOnHandForThisProduct on locationFact2", 20m, locationFact2.StockOnHandForThisProduct);
				AssertEquals("StockOnHandForThisProduct on locationFact3", 10m, locationFact3.StockOnHandForThisProduct);
				AssertEquals("HasCommittedStock on locationFact1", true, locationFact1.HasCommittedStock);
				AssertEquals("HasCommittedStock on locationFact2", true, locationFact2.HasCommittedStock);
				AssertEquals("HasCommittedStock on locationFact3", false, locationFact3.HasCommittedStock);
			});
		}

		public void TestLoadInputFacts_PartiallyPickedProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			Factory.Save();

			SetupRow(row1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("There are 8 locations", 8, locationFacts.Length);

			var locationWithStockOnHandFact = locationFacts.Single(l => l.Product.Fact != null);
			AssertEquals("StockOnHand", 5m, locationWithStockOnHandFact.StockOnHand);
			AssertEquals("HasCommittedStock", false, locationWithStockOnHandFact.HasCommittedStock);
			AssertEquals("Product", part1Relation.PK, locationWithStockOnHandFact.Product.Fact.PK);
			AssertEquals("Client", data.Org1.PK, locationWithStockOnHandFact.Client.Fact.PK);

			var emptyLocationFacts = locationFacts.Where(l => l.Product.Fact == null).ToArray();
			AssertEquals("There are 7 empty locations", 7, emptyLocationFacts.Length);

			foreach (var locationFact in emptyLocationFacts)
			{
				AssertEquals("StockOnHand", 0m, locationFact.StockOnHand);
				AssertEquals("HasCommittedStock", false, locationFact.HasCommittedStock);
				AssertNull("Product", locationFact.Product.Fact);
				AssertNull("Client", locationFact.Client.Fact);
			}
		}

		public void TestLoadInputFacts_FullyPickedProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			Factory.Save();

			SetupRow(row1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("There are 8 locations", 8, locationFacts.Length);

			foreach (var locationFact in locationFacts)
			{
				AssertEquals("StockOnHand", 0m, locationFact.StockOnHand);
				AssertEquals("HasCommittedStock", false, locationFact.HasCommittedStock);
				AssertNull("Product", locationFact.Product.Fact);
				AssertNull("Client", locationFact.Client.Fact);
			}
		}

		public void TestLoadInputFacts_ZeroProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			Factory.Save();

			SetupRow(row1);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("There are 8 locations", 8, locationFacts.Length);

			foreach (var locationFact in locationFacts)
			{
				AssertEquals("StockOnHand", 0m, locationFact.StockOnHand);
				AssertEquals("HasCommittedStock", false, locationFact.HasCommittedStock);
				AssertNull("Product", locationFact.Product.Fact);
				AssertNull("Client", locationFact.Client.Fact);
			}
		}

		public void TestLoadInputFacts_OneProduct_OneClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			data.Part1.OP_RH_NKCommodityCode = commodity.RH_Code;
			var category1 = Helper.CreateProductCategory(data.Org1, data.Part1, "Cat1");
			category1.OPC_CategoryDescription = "Cat1";
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB1", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			part1Relation.OU_UnitPrice = 200m;
			part1Relation.OU_RX_NKUnitPriceCurrency = "CNY";

			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var locationType2 = Helper.CreateLocationType("NO2", LocationClasses.Codes.HPL);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 8 locations", 8, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact1.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact1.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);

				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 10m, locationFact1.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), false, locationFact1.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 10m, locationFact1.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact1.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category1.OPC_CategoryCode, locationFact1.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact1.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB1", locationFact1.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 200m, locationFact1.Product.Fact.UnitPrice.Value);
			});
		}

		public void TestLoadInputFacts_OneProduct_TwoClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var client2 = Helper.CreateClient("Org2");

			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			data.Part1.OP_RH_NKCommodityCode = commodity.RH_Code;
			var category1 = Helper.CreateProductCategory(data.Org1, data.Part1, "Cat1");
			category1.OPC_CategoryDescription = "Cat1";
			var category2 = Helper.CreateProductCategory(client2, data.Part1, "Cat2");
			category2.OPC_CategoryDescription = "Cat2";
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB1", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));
			Helper.CreateABCCategory(data.Part1, client2, whs1, "AB2", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));

			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_OPC_Category = category1.PK;
			part1Relation.OU_UnitPrice = 200m;
			part1Relation.OU_RX_NKUnitPriceCurrency = "CNY";
			var part1Relation2 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(client2, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation2.OU_OPC_Category = category2.PK;
			part1Relation2.OU_UnitPrice = 199m;
			part1Relation2.OU_RX_NKUnitPriceCurrency = "CNY";

			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R2", data.Part1, 20m, location1, "PLT02");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 CCA locations", 9, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.Client.Fact?.PK == data.Org1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.Client.Fact?.PK == client2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact1.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact1.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 30m, locationFact1.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), false, locationFact1.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 10m, locationFact1.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact1.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category1.OPC_CategoryCode, locationFact1.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact1.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB1", locationFact1.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 200m, locationFact1.Product.Fact.UnitPrice.Value);

				AssertEquals(nameof(IOrganisationFact.PK), client2.PK, locationFact2.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), client2.OH_Code, locationFact2.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact2.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact2.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 30m, locationFact2.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), false, locationFact2.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 20m, locationFact2.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact2.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category2.OPC_CategoryCode, locationFact2.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact2.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB2", locationFact2.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 199m, locationFact2.Product.Fact.UnitPrice.Value);
			});
		}

		public void TestLoadInputFacts_TwoProducts_OneClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");

			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			data.Part1.OP_RH_NKCommodityCode = commodity1.RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity2.RH_Code;
			var category1 = Helper.CreateProductCategory(data.Org1, data.Part1, "Cat1");
			category1.OPC_CategoryDescription = "Cat1";
			var category2 = Helper.CreateProductCategory(data.Org1, data.Part2, "Cat2");
			category2.OPC_CategoryDescription = "Cat2";
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB1", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));
			Helper.CreateABCCategory(data.Part2, data.Org1, whs1, "AB2", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));

			var partRelation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_OPC_Category = category1.PK;
			partRelation1.OU_UnitPrice = 200m;
			partRelation1.OU_RX_NKUnitPriceCurrency = "CNY";
			var partRelation2 = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_OPC_Category = category2.PK;
			partRelation2.OU_UnitPrice = 199m;
			partRelation2.OU_RX_NKUnitPriceCurrency = "CNY";

			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 20m, location1, "PLT02");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 9 CCA locations", 9, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation2.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact1.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact1.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 30m, locationFact1.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), false, locationFact1.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 10m, locationFact1.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact1.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category1.OPC_CategoryCode, locationFact1.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact1.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB1", locationFact1.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 200m, locationFact1.Product.Fact.UnitPrice.Value);

				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact2.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact2.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact2.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact2.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 30m, locationFact2.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), false, locationFact2.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 20m, locationFact2.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part2.OP_PartNum, locationFact2.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category2.OPC_CategoryCode, locationFact2.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part2.CommodityCode.RH_Code, locationFact2.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB2", locationFact2.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 199m, locationFact2.Product.Fact.UnitPrice.Value);
			});
		}

		public void TestLoadInputFacts_TwoProducts_OneClient_FullyPickedProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var part2Relation = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var whs1 = Helper.CreateWarehouse("whs0");
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations[0];
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 10m, location1, "2");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();

			AssertEquals("There are 8 locations", 8, locationFacts.Length);

			var locationWithStockOnHandFact = locationFacts.Single(l => l.LocationPK == location1.PK);
			AssertEquals("StockOnHand", 10m, locationWithStockOnHandFact.StockOnHand);
			AssertEquals("HasCommittedStock", false, locationWithStockOnHandFact.HasCommittedStock);
			AssertEquals("Product", part2Relation.PK, locationWithStockOnHandFact.Product.Fact.PK);
			AssertEquals("Client", data.Org1.PK, locationWithStockOnHandFact.Client.Fact.PK);

			var emptyLocationFacts = locationFacts.Where(l => l.Product.Fact == null).ToArray();
			AssertEquals("There are 7 empty locations", 7, emptyLocationFacts.Length);

			foreach (var locationFact in emptyLocationFacts)
			{
				AssertEquals("StockOnHand", 0m, locationFact.StockOnHand);
				AssertEquals("HasCommittedStock", false, locationFact.HasCommittedStock);
				AssertNull("Product", locationFact.Product.Fact);
				AssertNull("Client", locationFact.Client.Fact);
			}
		}

		public void TestLoadInputFacts_TwoProducts_TwoClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var client2 = Helper.CreateClient("Org2");

			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			data.Part1.OP_RH_NKCommodityCode = commodity1.RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity2.RH_Code;
			var category1 = Helper.CreateProductCategory(data.Org1, data.Part1, "Cat1");
			category1.OPC_CategoryDescription = "Cat1";
			var category2 = Helper.CreateProductCategory(data.Org1, data.Part2, "Cat2");
			category2.OPC_CategoryDescription = "Cat2";
			var category3 = Helper.CreateProductCategory(client2, data.Part1, "Cat3");
			category3.OPC_CategoryDescription = "Cat3";
			var category4 = Helper.CreateProductCategory(client2, data.Part2, "Cat4");
			category4.OPC_CategoryDescription = "Cat4";
			Helper.CreateABCCategory(data.Part1, data.Org1, whs1, "AB1", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));
			Helper.CreateABCCategory(data.Part2, data.Org1, whs1, "AB2", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));
			Helper.CreateABCCategory(data.Part1, client2, whs1, "AB3", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));
			Helper.CreateABCCategory(data.Part2, client2, whs1, "AB4", ZDateTimeOffset.Today.AddDays(-10), ZDateTimeOffset.Today.AddDays(10));

			var partRelation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_OPC_Category = category1.PK;
			partRelation1.OU_UnitPrice = 200m;
			partRelation1.OU_RX_NKUnitPriceCurrency = "CNY";
			var partRelation2 = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_OPC_Category = category2.PK;
			partRelation2.OU_UnitPrice = 199m;
			partRelation2.OU_RX_NKUnitPriceCurrency = "CNY";
			var partRelation3 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(client2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation3.OU_OPC_Category = category3.PK;
			partRelation3.OU_UnitPrice = 198m;
			partRelation3.OU_RX_NKUnitPriceCurrency = "CNY";
			var partRelation4 = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(client2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation4.OU_OPC_Category = category4.PK;
			partRelation4.OU_UnitPrice = 197m;
			partRelation4.OU_RX_NKUnitPriceCurrency = "CNY";

			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 11m, location1, "PLT02");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R3", data.Part2, 20m, location1, "PLT03");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R4", data.Part2, 22m, location1, "PLT04");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R5", data.Part1, 30m, location1, "PLT05");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R6", data.Part1, 33m, location1, "PLT06");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R7", data.Part2, 40m, location1, "PLT07");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R8", data.Part2, 44m, location1, "PLT08");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, data.Part2, 1m);
			var pick = Helper.CreatePickNew(new[] { order });
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 11 CCA locations", 11, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation2.PK).Single();
			var locationFact3 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation3.PK).Single();
			var locationFact4 = locationFacts.Where(l => l.Product.Fact?.PK == partRelation4.PK).Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact1.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact1.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 210m, locationFact1.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), true, locationFact1.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 21m, locationFact1.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact1.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category1.OPC_CategoryCode, locationFact1.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact1.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB1", locationFact1.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 200m, locationFact1.Product.Fact.UnitPrice.Value);
				AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfLines), 1, ((ITaskManagementGroupingFact)locationFact1.WrappedLocation.Fact).NumberOfLines);

				AssertEquals(nameof(IOrganisationFact.PK), data.Org1.PK, locationFact2.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, locationFact2.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact2.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact2.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 210m, locationFact2.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), true, locationFact2.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 42m, locationFact2.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part2.OP_PartNum, locationFact2.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category2.OPC_CategoryCode, locationFact2.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part2.CommodityCode.RH_Code, locationFact2.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB2", locationFact2.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 199m, locationFact2.Product.Fact.UnitPrice.Value);
				AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfLines), 1, ((ITaskManagementGroupingFact)locationFact2.WrappedLocation.Fact).NumberOfLines);

				AssertEquals(nameof(IOrganisationFact.PK), client2.PK, locationFact3.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), client2.OH_Code, locationFact3.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact3.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact3.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 210m, locationFact3.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), true, locationFact3.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 63m, locationFact3.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part1.OP_PartNum, locationFact3.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category3.OPC_CategoryCode, locationFact3.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part1.CommodityCode.RH_Code, locationFact3.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB3", locationFact3.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 198m, locationFact3.Product.Fact.UnitPrice.Value);
				AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfLines), 1, ((ITaskManagementGroupingFact)locationFact3.WrappedLocation.Fact).NumberOfLines);

				AssertEquals(nameof(IOrganisationFact.PK), client2.PK, locationFact4.Client.Fact.PK);
				AssertEquals(nameof(IOrganisationFact.Code), client2.OH_Code, locationFact4.Client.Fact.Code);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact4.Client.Fact.IsProxyOrgOfCurrentCompany);
				AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact4.Client.Fact.IsProxyOrgOfAnyCompany);
				AssertEquals(nameof(ICycleCountLocationCoreFact.StockOnHand), 210m, locationFact4.StockOnHand);
				AssertEquals(nameof(ICycleCountLocationCoreFact.HasCommittedStock), true, locationFact4.HasCommittedStock);
				AssertEquals(nameof(ICycleCountLocationFact.StockOnHandForThisProduct), 84m, locationFact4.StockOnHandForThisProduct);
				AssertEquals(nameof(IProductFact.Code), data.Part2.OP_PartNum, locationFact4.Product.Fact.Code);
				AssertEquals(nameof(IProductFact.CategoryCode), category4.OPC_CategoryCode, locationFact4.Product.Fact.CategoryCode);
				AssertEquals(nameof(IProductFact.CommodityCode), data.Part2.CommodityCode.RH_Code, locationFact4.Product.Fact.CommodityCode);
				AssertEquals(nameof(ICycleCountProductFact.ABCCategoryCode), "AB4", locationFact4.Product.Fact.ABCCategoryCode);
				AssertEquals(nameof(ICycleCountProductFact.UnitPrice), 197m, locationFact4.Product.Fact.UnitPrice.Value);
				AssertEquals(nameof(ITaskManagementGroupingFact.NumberOfLines), 1, ((ITaskManagementGroupingFact)locationFact4.WrappedLocation.Fact).NumberOfLines);
			});
		}

		public void TestLoadInputFacts_Client_Proxy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");

			var proxyOfCurrentCompany = Helper.CreateClient("Org2");
			var proxyOfCurrentBranch = Helper.CreateClient("Org3");
			var proxyOfAnotherCompany = Helper.CreateClient("Org4");
			var proxyOfAnotherBranch = Helper.CreateClient("Org5");
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;

			currentCompany.GC_OH_OrgProxy = proxyOfCurrentCompany.PK;
			anotherCompany.GC_OH_OrgProxy = proxyOfAnotherCompany.PK;
			anotherBranch.GB_OH_OrgProxy = proxyOfAnotherBranch.PK;

			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			data.Part1.RelatedOrganisations.AddOwner(proxyOfCurrentCompany);
			data.Part1.RelatedOrganisations.AddOwner(proxyOfCurrentBranch);
			data.Part1.RelatedOrganisations.AddOwner(proxyOfAnotherCompany);
			data.Part1.RelatedOrganisations.AddOwner(proxyOfAnotherBranch);
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(proxyOfCurrentCompany, whs1, "R2", data.Part1, 10m, location1, "PLT02");
			Helper.CreateWhsReceiveWithInventory(proxyOfCurrentBranch, whs1, "R3", data.Part1, 10m, location1, "PLT03");
			Helper.CreateWhsReceiveWithInventory(proxyOfAnotherCompany, whs1, "R4", data.Part1, 10m, location1, "PLT04");
			Helper.CreateWhsReceiveWithInventory(proxyOfAnotherBranch, whs1, "R5", data.Part1, 10m, location1, "PLT05");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 12 CCA locations", 12, locationFacts.Length);
			AssertEquals("There are 7 empty locations", 7, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.Client.Fact?.PK == data.Org1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfCurrentCompany.PK).Single();
			var locationFact3 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfCurrentBranch.PK).Single();
			var locationFact4 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfAnotherCompany.PK).Single();
			var locationFact5 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfAnotherBranch.PK).Single();

			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, locationFact2.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact3.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact4.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact5.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact2.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact3.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact4.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact5.Client.Fact.IsProxyOrgOfAnyCompany);

			currentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			currentBranch.GB_OH_OrgProxy = proxyOfCurrentBranch.PK;
			Factory.Save();

			inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 12 CCA locations", 12, locationFacts.Length);

			locationFact1 = locationFacts.Where(l => l.Client.Fact?.PK == data.Org1.PK).Single();
			locationFact2 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfCurrentCompany.PK).Single();
			locationFact3 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfCurrentBranch.PK).Single();
			locationFact4 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfAnotherCompany.PK).Single();
			locationFact5 = locationFacts.Where(l => l.Client.Fact?.PK == proxyOfAnotherBranch.PK).Single();

			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact1.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact2.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, locationFact3.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact4.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, locationFact5.Client.Fact.IsProxyOrgOfCurrentCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact1.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), false, locationFact2.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact3.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact4.Client.Fact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, locationFact5.Client.Fact.IsProxyOrgOfAnyCompany);
		}

		public void TestLoadInputFacts_SingleInstanceForClientAndProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = Helper.CreateWarehouse("whs0");
			var client2 = Helper.CreateClient("Org2");

			var partRelation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var partRelation2 = data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var partRelation3 = Helper.CreateProductClientRelationShip(client2, data.Part1);
			var partRelation4 = Helper.CreateProductClientRelationShip(client2, data.Part2);

			var locationType1 = Helper.CreateLocationType("NO1", LocationClasses.Codes.NOR);
			var row1 = Helper.CreateRow(whs1, "RO1", 2, 2, 2);
			var area = Helper.CreateArea(whs1, "AE1");
			Factory.Save();

			SetupRow(row1);
			var location1 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-1");
			var location2 = row1.Locations.Single(l => l.WLV_LocationString == "RO1-1-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 10m, location1, "PLT01");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part1, 11m, location2, "PLT02");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R3", data.Part2, 20m, location1, "PLT03");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R4", data.Part2, 22m, location2, "PLT04");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R5", data.Part1, 30m, location1, "PLT05");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R6", data.Part1, 33m, location2, "PLT06");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R7", data.Part2, 40m, location1, "PLT07");
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R8", data.Part2, 44m, location2, "PLT08");
			Factory.Save();

			var processor = GetFactLoader();
			var inputFacts = processor.LoadInputFacts(new ReadOnlyBusinessObjectFactory(), whs1.PK, new CancellationToken());
			var locationFacts = inputFacts.OfType<ICycleCountLocationFact>().ToArray();
			AssertEquals("There are 14 CCA locations", 14, locationFacts.Length);
			AssertEquals("There are 6 empty locations", 6, locationFacts.Where(l => l.Product.Fact == null).Count());

			var locationFact1 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == partRelation1.PK).Single();
			var locationFact2 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == partRelation2.PK).Single();
			var locationFact3 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == partRelation3.PK).Single();
			var locationFact4 = locationFacts.Where(l => l.LocationPK == location1.PK && l.Product.Fact?.PK == partRelation4.PK).Single();
			var locationFact5 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == partRelation1.PK).Single();
			var locationFact6 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == partRelation2.PK).Single();
			var locationFact7 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == partRelation3.PK).Single();
			var locationFact8 = locationFacts.Where(l => l.LocationPK == location2.PK && l.Product.Fact?.PK == partRelation4.PK).Single();

			var clientFact1 = locationFact1.Client.Fact;
			var clientFact2 = locationFact2.Client.Fact;
			var clientFact3 = locationFact3.Client.Fact;
			var clientFact4 = locationFact4.Client.Fact;
			var clientFact5 = locationFact5.Client.Fact;
			var clientFact6 = locationFact6.Client.Fact;
			var clientFact7 = locationFact7.Client.Fact;
			var clientFact8 = locationFact8.Client.Fact;

			AssertEquals("Should be same ClientFact instance", true, clientFact1 == clientFact2 && clientFact1 == clientFact5 && clientFact1 == clientFact6);
			AssertEquals("Should be same ClientFact instance", true, clientFact3 == clientFact4 && clientFact3 == clientFact7 && clientFact3 == clientFact8);

			var productFact1 = locationFact1.Product.Fact;
			var productFact2 = locationFact2.Product.Fact;
			var productFact3 = locationFact3.Product.Fact;
			var productFact4 = locationFact4.Product.Fact;
			var productFact5 = locationFact5.Product.Fact;
			var productFact6 = locationFact6.Product.Fact;
			var productFact7 = locationFact7.Product.Fact;
			var productFact8 = locationFact8.Product.Fact;

			AssertEquals("Should be same ProductFact instance", true, productFact1 == productFact5);
			AssertEquals("Should be same ProductFact instance", true, productFact2 == productFact6);
			AssertEquals("Should be same ProductFact instance", true, productFact3 == productFact7);
			AssertEquals("Should be same ProductFact instance", true, productFact4 == productFact8);
		}

		#region Implementation

		protected virtual void SetupRow(WhsRow row)
		{
		}

		protected TInterface GetFactLoader() => new TClass();

		#endregion
	}
}

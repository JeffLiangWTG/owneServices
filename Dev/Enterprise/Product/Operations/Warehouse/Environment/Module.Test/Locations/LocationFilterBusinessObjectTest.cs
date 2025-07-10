using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(LocationFilterBusinessObject))]
	class LocationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestDockDoorLocationsFilter

		public void TestDockDoorLocationsFilter()
		{
			var dockDoorLocationType1 = Helper.CreateLocationType("DD1",  "Test DDL1", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocationType2 = Helper.CreateLocationType("DD2", "Test DDL2", false, 0, LocationClasses.Codes.DDL);
			var melbourneLocations = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow").Locations;

			var dockDoorLocation1 = melbourneLocations[0];
			var dockDoorLocation2 = melbourneLocations[1];
			var dockDoorLocation3 = melbourneLocations[2];
			var nonDockDoorLocation = melbourneLocations[3];
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType1.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType1.PK;
			dockDoorLocation3.WLV_WLT_LocationType = dockDoorLocationType2.PK;

			AddLocationToScope(Asserter, dockDoorLocation1, dockDoorLocation2, dockDoorLocation3, nonDockDoorLocation);
			Factory.Save();

			var dockDoorLocationsFilter = (ModuleFlagsFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.DockDoorLocation];
			AssertEquals(FilterCategories.StatusAndFlags, dockDoorLocationsFilter.Category);

			dockDoorLocationsFilter.Property0 = true;
			Asserter.AssertMatches("Only dock door locations must be returned.", dockDoorLocationsFilter, dockDoorLocation1, dockDoorLocation2, dockDoorLocation3);

			dockDoorLocationsFilter.Property0 = false;
			Asserter.AssertMatches("Dock door locations must not be returned.", dockDoorLocationsFilter, nonDockDoorLocation);
		}

		#endregion

		#region TestDockDoorLocationsFilter

		public void TestBondedLocationsFilter()
		{
			var bondedArea = Helper.CreateArea(MelbourneWarehouse, "BONDED", AreaTypes.Codes.Bonded);
			var melbourneLocations = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow").Locations;

			var bondedLocation1 = melbourneLocations[0];
			var bondedLocation2 = melbourneLocations[1];
			var bondedLocation3 = melbourneLocations[2];
			var nonBondedLocation = melbourneLocations[3];
			bondedLocation1.WLV_WA_PickingArea = bondedArea.PK;
			bondedLocation1.WLV_WA_PutawayArea = bondedArea.PK;
			bondedLocation2.WLV_WA_PickingArea = bondedArea.PK;
			bondedLocation3.WLV_WA_PutawayArea = bondedArea.PK;

			AddLocationToScope(Asserter, bondedLocation1, bondedLocation2, bondedLocation3, nonBondedLocation);
			Factory.Save();

			var bondedLocationsFilter = (ModuleFlagsFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.BondedLocation];
			AssertEquals(FilterCategories.StatusAndFlags, bondedLocationsFilter.Category);

			bondedLocationsFilter.Property0 = true;
			Asserter.AssertMatches("Only bonded pick locations must be returned.", bondedLocationsFilter, bondedLocation1, bondedLocation2);

			bondedLocationsFilter.Property0 = false;
			Asserter.AssertMatches("Bonded pick locations must not be returned.", bondedLocationsFilter, nonBondedLocation, bondedLocation3);
		}

		#endregion

		#region TestPackingStationLocationsFilter

		public void TestPackingStationLocationsFilter()
		{
			var packingStationLocationType1 = Helper.CreateLocationType("PS1", "Test PST1", false, 0, LocationClasses.Codes.PST);
			var packingStationLocationType2 = Helper.CreateLocationType("PS2", "Test PST2", false, 0, LocationClasses.Codes.PST);
			var melbourneLocations = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow").Locations;

			var packingStationLocation1 = melbourneLocations[0];
			var packingStationLocation2 = melbourneLocations[1];
			var packingStationLocation3 = melbourneLocations[2];
			var nonPackingStationLocation = melbourneLocations[3];
			packingStationLocation1.WLV_WLT_LocationType = packingStationLocationType1.PK;
			packingStationLocation2.WLV_WLT_LocationType = packingStationLocationType1.PK;
			packingStationLocation3.WLV_WLT_LocationType = packingStationLocationType2.PK;

			AddLocationToScope(Asserter, packingStationLocation1, packingStationLocation2, packingStationLocation3, nonPackingStationLocation);
			Factory.Save();

			var filterStripBizO = GetNewFilterStripBusinessObject();
			var packingStationLocationsFilter = (ModuleFlagsFilter)filterStripBizO[WhsLocationCollection.FilterSchema.PackingStationLocation];
			AssertEquals(FilterCategories.StatusAndFlags, packingStationLocationsFilter.Category);

			packingStationLocationsFilter.Property0 = true;
			Asserter.AssertMatches("Only dock door locations must be returned.", packingStationLocationsFilter, packingStationLocation1, packingStationLocation2, packingStationLocation3);

			packingStationLocationsFilter.Property0 = false;
			Asserter.AssertMatches("Dock door locations must not be returned.", packingStationLocationsFilter, nonPackingStationLocation);
		}

		#endregion

		#region TestLocationVoidStatusFilter

		public void TestLocationVoidStatusFilter()
		{
			var melbourneLocations = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow").Locations;
			var melbourneLocation1 = melbourneLocations[0];
			var melbourneLocation2 = melbourneLocations[1];
			var melbourneLocation3 = melbourneLocations[2];
			var melbourneLocation4 = melbourneLocations[3];
			melbourneLocation1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			melbourneLocation2.WLV_LocationStatus = LocationStatus.Codes.Void;
			melbourneLocation3.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			melbourneLocation4.WLV_LocationStatus = LocationStatus.Codes.Held;

			AddLocationToScope(Asserter, melbourneLocation1, melbourneLocation2, melbourneLocation3, melbourneLocation4);
			Factory.Save();

			AssertEquals("Status filter category should be TextSearch.", LocationStatusFilter.Category, FilterCategories.TextSearch);
			AssertEquals(2, LocationStatusFilter.ComparisonOperator_List.Count);
			Assert(LocationStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.Exact));
			Assert(LocationStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));

			LocationStatusFilter.Property = LocationStatus.Codes.Normal;
			Asserter.AssertMatches("Since the filter is Normal it should return normal locations.", LocationStatusFilter, melbourneLocation1);

			LocationStatusFilter.Property = LocationStatus.Codes.Void;
			Asserter.AssertMatches("Since the filter value is Void it should return only melbourneLocation2.", LocationStatusFilter, melbourneLocation2);

			LocationStatusFilter.Property = LocationStatus.Codes.Held;
			Asserter.AssertMatches("Since the filter value is Held it should return only melbourneLocation4.", LocationStatusFilter, melbourneLocation4);

			LocationStatusFilter.Property = LocationStatus.Codes.Damaged;
			Asserter.AssertMatches("Since the filter value is Damaged it should return only melbourneLocation3.", LocationStatusFilter, melbourneLocation3);

			LocationStatusFilter.Property = LocationStatus.Codes.Normal;
			LocationStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("It should return all locations except Normal.", LocationStatusFilter, melbourneLocation2, melbourneLocation3, melbourneLocation4);

			LocationStatusFilter.Property = LocationStatus.Codes.Void;
			LocationStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("It should return all locations except Void.", LocationStatusFilter, melbourneLocation1, melbourneLocation3, melbourneLocation4);

			LocationStatusFilter.Property = LocationStatus.Codes.Damaged;
			LocationStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("It should return all locations except Damaged.", LocationStatusFilter, melbourneLocation1, melbourneLocation2, melbourneLocation4);

			LocationStatusFilter.Property = LocationStatus.Codes.Held;
			LocationStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("It should return all locations except Held.", LocationStatusFilter, melbourneLocation1, melbourneLocation2, melbourneLocation3);
		}

		FilterStripAsserter<WhsLocation> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsLocation>(Factory, (s) => s.WLV_LocationStatus.ToString()));
		FilterStripAsserter<WhsLocation> asserter;

		void AddLocationToScope(FilterStripAsserter<WhsLocation> whslocation, params WhsLocation[] locationLines)
		{
			foreach (var location in locationLines)
			{
				whslocation.AddToScope(location);
			}
		}

		ModuleTextFilter LocationStatusFilter => (ModuleTextFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.LocationStatus];

		#endregion

		#region TestLocationEmptyStatusFilter

		public void TestLocationEmptyStatusFilter()
		{
			var melbourneLocations = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow").Locations;
			var sydneyLocations = SydneyWarehouse.Rows.Single(r => r.WR_Name == "SydRow").Locations;

			WarehouseFilter.IsActive = false; // testing across warehouses

			AssertEquals(LocationFilterBusinessObject.EmptyLocationFilterOption.All, LocationEmptyStatusFilter.DefaultProperty);
			AssertEquals(LocationFilterBusinessObject.EmptyLocationFilterOption.All, LocationEmptyStatusFilter.Property);

			// fill some locations with stock.
			var nonEmptyMelbourneLocation = melbourneLocations[0];
			var nonEmptySydneyLocation = sydneyLocations[0];
			var sydneyLocationWithZeroStock = sydneyLocations[1];

			var melbReceivePK = IHelper.CreateWhsReceive(OrgPK, MelbourneWarehouse.PK, "Melb Whs Rcv", Notify);
			IHelper.CreateWhsReceiveInventoryLine(melbReceivePK, ProductPK, 2m, nonEmptyMelbourneLocation.PK);

			var sydReceivePK = IHelper.CreateWhsReceive(OrgPK, SydneyWarehouse.PK, "Syd Whs Rcv", Notify);
			IHelper.CreateWhsReceiveInventoryLine(sydReceivePK, ProductPK, 7m, nonEmptySydneyLocation.PK);
			IHelper.CreateWhsReceiveInventoryLine(sydReceivePK, ProductPK, 0m, sydneyLocationWithZeroStock.PK);

			// create inventory with no location.
			IHelper.CreateWhsReceiveInventoryLine(melbReceivePK, ProductPK, 5m, ZGuid.Empty);
			Factory.Save();

			// 3x3 = 9 Locations in Melb Whs
			// 2x2 = 4 Locations in Syd Whs
			// +2 default dock door locations
			AssertLocationsMatchingEmptyStatus(LocationFilterBusinessObject.EmptyLocationFilterOption.All, true, 9 + 4 + 2);
			AssertLocationsMatchingEmptyStatus(LocationFilterBusinessObject.EmptyLocationFilterOption.InUse, true, 2, nonEmptyMelbourneLocation, nonEmptySydneyLocation);
			AssertLocationsMatchingEmptyStatus(LocationFilterBusinessObject.EmptyLocationFilterOption.Empty, false, 9 + 4 - 2 + 2, nonEmptyMelbourneLocation, nonEmptySydneyLocation);
		}

		void AssertLocationsMatchingEmptyStatus(ZString locationStatus, bool contains, int locationCount, params WhsLocation[] locations)
		{
			LocationEmptyStatusFilter.IsActive = true;
			LocationEmptyStatusFilter.Property = locationStatus;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (var location in locations)
			{
				AssertCollectionContains(location, Collection, contains);
			}

			AssertEquals(locationCount, Collection.Count);
		}

		ModuleTextFilter LocationEmptyStatusFilter => (ModuleTextFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.LocationEmptyStatus];

		#endregion

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			AssertLocationsMatchingWarehouse(MelbourneWarehouse, 10);
			AssertLocationsMatchingWarehouse(SydneyWarehouse, 5);
			AssertEquals(FilterVisibility.AlwaysVisible, WarehouseFilter.Visibility);
		}

		void AssertLocationsMatchingWarehouse(WhsWarehouse warehouse, int locationCount)
		{
			WarehouseFilter.IsActive = true;
			WarehouseFilter.Property = warehouse.PK;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (var row in warehouse.Rows)
			{
				foreach (var location in row.Locations)
				{
					AssertCollectionContains(location, Collection);
				}
			}

			AssertEquals(locationCount, Collection.Count);
		}

		public void TestWarehouseFilter_WarehouseValidation_WithDefaultProductWarehouse()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS", "TESTROW");
			var transitWarehouse = Helper.CreateWarehouse("TRW", "TESTTRW");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var freeTradeWarehouse = Helper.CreateWarehouse("FTZ", "TESTFTZ");
			freeTradeWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			Factory.Save();

			var productWarehouseDefaultFilter = new FilterBusinessObjectDefaults();
			productWarehouseDefaultFilter.Add(new FilterBusinessObjectDefault("Warehouse", "Property",
				productWarehouse.PK));

			var productWarehouseFilterBO = new LocationFilterBusinessObject();
			productWarehouseFilterBO.SetExternalDefaults(productWarehouseDefaultFilter);
			var productWarehouseFilter = (ModuleGuidFilter)productWarehouseFilterBO[WhsLocationCollection.FilterSchema.Warehouse];
			AssertNoErrors(productWarehouseFilter.PropertyInfo);

			productWarehouseFilter.Property = freeTradeWarehouse.PK;
			AssertNoErrors(productWarehouseFilter.PropertyInfo);

			productWarehouseFilter.Property = Guid.Empty;
			AssertHasError(productWarehouseFilter.PropertyInfo, "Enter a Warehouse.");

			productWarehouseFilter.Property = transitWarehouse.PK;
			AssertHasError(productWarehouseFilter.PropertyInfo, "Enter a valid selection.");

			productWarehouseFilter.Property = productWarehouse.PK;
			AssertNoErrors(productWarehouseFilter.PropertyInfo);
		}

		public void TestWarehouseFilter_WarehouseValidation_WithDefaultTransitWarehouse()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS", "TESTROW");
			var transitWarehouse = Helper.CreateWarehouse("TRW", "TESTTRW");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var freeTradeWarehouse = Helper.CreateWarehouse("FTZ", "TESTFTZ");
			freeTradeWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			Factory.Save();

			var transitWarehouseDefaultFilter = new FilterBusinessObjectDefaults();
			transitWarehouseDefaultFilter.Add(new FilterBusinessObjectDefault("Warehouse", "Property",
				transitWarehouse.PK));

			var transitWarehouseFilterBO = new LocationFilterBusinessObject();
			transitWarehouseFilterBO.SetExternalDefaults(transitWarehouseDefaultFilter);
			var transitWarehouseFilter = (ModuleGuidFilter)transitWarehouseFilterBO[WhsLocationCollection.FilterSchema.Warehouse];
			AssertNoErrors(transitWarehouseFilter.PropertyInfo);

			transitWarehouseFilter.Property = productWarehouse.PK;
			AssertHasError(transitWarehouseFilter.PropertyInfo, "Enter a valid selection.");

			transitWarehouseFilter.Property = Guid.Empty;
			AssertHasError(transitWarehouseFilter.PropertyInfo, "Enter a Warehouse.");

			transitWarehouseFilter.Property = freeTradeWarehouse.PK;
			AssertHasError(transitWarehouseFilter.PropertyInfo, "Enter a valid selection.");

			transitWarehouseFilter.Property = transitWarehouse.PK;
			AssertNoErrors(transitWarehouseFilter.PropertyInfo);
		}

		public void TestWarehouseFilter_WarehouseValidation_WithNoDefaultWarehouse()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS", "TESTROW");
			var transitWarehouse = Helper.CreateWarehouse("TRW", "TESTTRW");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var freeTradeWarehouse = Helper.CreateWarehouse("FTZ", "TESTFTZ");
			freeTradeWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			Factory.Save();

			var warehouseFilterBO = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)warehouseFilterBO[WhsLocationCollection.FilterSchema.Warehouse];
			AssertNoErrors(warehouseFilter.PropertyInfo);

			warehouseFilter.Property = productWarehouse.PK;
			AssertNoErrors(warehouseFilter.PropertyInfo);

			warehouseFilter.Property = transitWarehouse.PK;
			AssertHasError(warehouseFilter.PropertyInfo, "Enter a valid selection.");

			warehouseFilter.Property = Guid.Empty;
			AssertHasError(warehouseFilter.PropertyInfo, "Enter a Warehouse.");

			warehouseFilter.Property = freeTradeWarehouse.PK;
			AssertNoErrors(warehouseFilter.PropertyInfo);
		}

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.Warehouse];

		#endregion

		#region TestWarehouseFilterShowsValidationErrorIfNoWarehouseSelected

		public void TestWarehouseFilterShowsValidationErrorIfNoWarehouseSelected()
		{
			var filterStripBizO = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStripBizO[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = ZGuid.Invalid; // To allow validation to be run
			warehouseFilter.Property = ZGuid.Empty;
			AssertHasError(warehouseFilter.PropertyInfo, "Enter a Warehouse.");

			var warehouse = Helper.CreateWarehouse("WHS", "TESTROW");
			Factory.Save();

			warehouseFilter.Property = warehouse.PK;
			AssertNoErrors(warehouseFilter.PropertyInfo);
		}

		#endregion

		#region TestRowFilter

		public void TestRowFilter()
		{
			// add another row+locations to be sure we are not loading all locations
			Helper.CreateRow(MelbourneWarehouse, "Row2", 2, 2);
			AssertEquals("Precondition", 3, MelbourneWarehouse.Rows.Count); // +1 for DOCKDOOR row

			var melbRow0 = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow");
			RowFilter.IsActive = true;
			RowFilter.Property = melbRow0.PK;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (var location in melbRow0.Locations)
			{
				AssertCollectionContains(location, Collection);
			}

			AssertEquals("Should have loaded all Melbourne Warehouse Row 0 locations.", 9, Collection.Count);
		}

		ModuleGuidFilter RowFilter => (ModuleGuidFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.Row];

		#endregion

		#region TestLevelFilter

		public void TestLevelFilter()
		{
			LevelFilter.IsActive = true;
			LevelFilter.Property1 = 1;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (WhsLocation location in Collection)
			{
				AssertEquals((short)1, location.WLV_Level);
			}

			AssertEquals("Should have loaded all Melbourne Warehouse Locations with Level == 1.", 4, Collection.Count);
		}

		ModuleNumberRangeFilter LevelFilter => (ModuleNumberRangeFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.Level];

		#endregion

		#region TestColumnFilter

		public void TestColumnFilter()
		{
			ColumnFilter.IsActive = true;
			ColumnFilter.Property1 = 1;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (var location in Collection)
			{
				AssertEquals((short)1, location.WLV_Column);
			}

			AssertEquals("Should have loaded all Melbourne Warehouse Locations with Column == 1.", 4, Collection.Count);
		}

		ModuleNumberRangeFilter ColumnFilter => (ModuleNumberRangeFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.Column];

		#endregion

		#region TestTrayFilter

		public void TestTrayFilter()
		{
			TrayFilter.IsActive = true;
			TrayFilter.Property1 = 1;
			Collection.AdditionalFilter = FilterStripBizO.Filter;

			foreach (var location in Collection)
			{
				AssertEquals((short)1, location.WLV_Tray);
			}

			AssertEquals("Should have loaded all Melbourne Warehouse Locations with Tray == 1.", 10, Collection.Count);
		}

		ModuleNumberRangeFilter TrayFilter => (ModuleNumberRangeFilter)FilterStripBizO[WhsLocationCollection.FilterSchema.Tray];

		#endregion

		#region TestAreaFilter

		public void TestAreaFilter()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var area1InWhs1 = Helper.CreateArea(whs1, "A1", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var area2InWhs1 = Helper.CreateArea(whs1, "A2", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);
			var area3InWhs1 = Helper.CreateArea(whs1, "A3", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);

			Helper.CreateRowAndGenerateLocations(whs1, "R1", 1, 3);

			var locationsInR1 = whs1.Rows.Single(r => r.WR_Name == "R1").Locations;
			var location1 = locationsInR1[0];
			var location2 = locationsInR1[1];
			var location3 = locationsInR1[2];
			Helper.SetLocationArea(location1, area1InWhs1, area2InWhs1);
			Helper.SetLocationArea(location2, area2InWhs1, area2InWhs1);
			Helper.SetLocationArea(location3, area1InWhs1, area3InWhs1);
			Asserter.AddToScope(location1, location2, location3);
			Factory.Save();

			// picking Area filter
			var filterStrip = new LocationFilterBusinessObject();
			var pickingAreaFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.PickingArea];
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			AssertEquals(FilterCategories.NumbersAndReferences, pickingAreaFilter.Category);
			var pickingAreaPks = pickingAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertCollectionContains("Picking only area must be returned.", area1InWhs1.PK, pickingAreaPks);
			AssertCollectionContains("Both area must be returned.", area2InWhs1.PK, pickingAreaPks);
			AssertCollectionNotContains("Putaway only area must not be returned.", area3InWhs1.PK, pickingAreaPks);

			pickingAreaFilter.IsActive = true;
			pickingAreaFilter.Property = area1InWhs1.PK;
			Asserter.AssertMatches("Only locations are in Picking Area1 must be returned.", pickingAreaFilter, location1, location3);

			pickingAreaFilter.Property = area2InWhs1.PK;
			Asserter.AssertMatches("Only locations are in Picking Area2 must be returned.", pickingAreaFilter, location2);

			pickingAreaFilter.Property = area3InWhs1.PK;
			Asserter.AssertMatches("No locations in Picking Area3 so nothing must be returned.", pickingAreaFilter);

			pickingAreaFilter.IsActive = false;

			// putaway Area filter
			var putawayAreaFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.PutawayArea];
			AssertEquals(FilterCategories.NumbersAndReferences, putawayAreaFilter.Category);
			var putawayAreaPKs = putawayAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertCollectionNotContains("Picking area must not be returned.", area1InWhs1.PK, putawayAreaPKs);
			AssertCollectionContains("Both area must be returned.", area2InWhs1.PK, putawayAreaPKs);
			AssertCollectionContains("Putaway only area must be returned.", area3InWhs1.PK, putawayAreaPKs);

			putawayAreaFilter.IsActive = true;
			putawayAreaFilter.Property = area1InWhs1.PK;
			Asserter.AssertMatches("No locations in Putaway Area1 so nothing must be returned.", putawayAreaFilter);

			putawayAreaFilter.Property = area2InWhs1.PK;
			Asserter.AssertMatches("Only locations are in Putaway Area2 must be returned.", putawayAreaFilter, location1, location2);

			putawayAreaFilter.Property = area3InWhs1.PK;
			Asserter.AssertMatches("Only locations are in Putaway Area3 must be returned.", putawayAreaFilter, location3);
		}

		public void TestPickingAreaFilterLookups()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var pickingOnlyAreaInWhs1 = Helper.CreateArea(whs1, "A11", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var bothAreaInWhs1 = Helper.CreateArea(whs1, "A12", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);
			var putawayOnlyAreaInWhs1 = Helper.CreateArea(whs1, "A13", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);

			var pickingOnlyAreaInWhs2 = Helper.CreateArea(whs2, "A21", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var bothAreaInWhs2 = Helper.CreateArea(whs2, "A22", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);
			var putawayOnlyAreaInWhs2 = Helper.CreateArea(whs2, "A23", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);

			Factory.Save();

			// picking Area filter - without warehouse
			var filterStrip = new LocationFilterBusinessObject();
			var pickingAreaFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.PickingArea];
			pickingAreaFilter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, pickingAreaFilter.Category);
			var pickingAreaPks = pickingAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertEquals("Without warehouse no areas must be returned.", 0, pickingAreaPks.Length);

			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			var pickingAreaInWhs1Pks = pickingAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertCollectionContains("Picking only area must be returned.", pickingOnlyAreaInWhs1.PK, pickingAreaInWhs1Pks);
			AssertCollectionContains("Both area must be returned.", bothAreaInWhs1.PK, pickingAreaInWhs1Pks);
			AssertCollectionNotContains("Putaway only area must not be returned.", putawayOnlyAreaInWhs1.PK, pickingAreaInWhs1Pks);
			AssertCollectionNotContains("Picking only area must not be returned since it is from a different warehouse.", pickingOnlyAreaInWhs2.PK, pickingAreaInWhs1Pks);
			AssertCollectionNotContains("Both area must not be returned since it is from a different warehouse.", bothAreaInWhs2.PK, pickingAreaInWhs1Pks);
			AssertCollectionNotContains("Putaway only area must not be returned.", putawayOnlyAreaInWhs2.PK, pickingAreaInWhs1Pks);
		}

		public void TestPutawayAreaFilterLookups()
		{
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var pickingOnlyAreaInWhs1 = Helper.CreateArea(whs1, "A11", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var bothAreaInWhs1 = Helper.CreateArea(whs1, "A12", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);
			var putawayOnlyAreaInWhs1 = Helper.CreateArea(whs1, "A13", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);

			var pickingOnlyAreaInWhs2 = Helper.CreateArea(whs2, "A21", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var bothAreaInWhs2 = Helper.CreateArea(whs2, "A22", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);
			var putawayOnlyAreaInWhs2 = Helper.CreateArea(whs2, "A23", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);

			Factory.Save();

			// putaway Area filter - without warehouse
			var filterStrip = new LocationFilterBusinessObject();
			var putawayAreaFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.PutawayArea];
			AssertEquals(FilterCategories.NumbersAndReferences, putawayAreaFilter.Category);
			var putawayAreaPKs = putawayAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertEquals("Without warehouse no areas must be returned.", 0, putawayAreaPKs.Length);

			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			var putawayAreaInWhs1Pks = putawayAreaFilter.List.Cast<WhsArea>().Select(a => a.PK).ToArray();
			AssertCollectionNotContains("Picking only area must not be returned.", pickingOnlyAreaInWhs1.PK, putawayAreaInWhs1Pks);
			AssertCollectionContains("Both area must be returned.", bothAreaInWhs1.PK, putawayAreaInWhs1Pks);
			AssertCollectionContains("Putaway only area must be returned.", putawayOnlyAreaInWhs1.PK, putawayAreaInWhs1Pks);
			AssertCollectionNotContains("Picking only area not must be returned.", pickingOnlyAreaInWhs1.PK, putawayAreaInWhs1Pks);
			AssertCollectionNotContains("Both area must not be returned since it is from a different warehouse.", bothAreaInWhs2.PK, putawayAreaInWhs1Pks);
			AssertCollectionNotContains("Putaway only area must not be returned since it is from a different warehouse.", putawayOnlyAreaInWhs2.PK, putawayAreaInWhs1Pks);
		}

		#endregion

		#region TestSetInitialCodeForSearch

		public void TestSetInitialCodeForSearch_LocationInGrid()
		{
			var melbourneRow = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow");
			// Wrong location searches
			FilterStripBizO.SetInitialCodeForSearch("MelRow-4", typeof(WhsLocation));
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			FilterStripBizO.SetInitialCodeForSearch("MelRow-4-4", typeof(WhsLocation));
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			AssertEquals(FilterVisibility.Visible, ColumnFilter.Visibility);
			AssertEquals(0m, ColumnFilter.Property1);
			AssertEquals(0m, ColumnFilter.Property2);

			FilterStripBizO.SetInitialCodeForSearch("MelRow-3-2-1", typeof(WhsLocation));
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			AssertEquals(FilterVisibility.Visible, ColumnFilter.Visibility);
			AssertEquals(0m, ColumnFilter.Property1);
			AssertEquals(0m, ColumnFilter.Property2);

			AssertEquals(FilterVisibility.Visible, LevelFilter.Visibility);
			AssertEquals(0m, LevelFilter.Property1);
			AssertEquals(0m, LevelFilter.Property2);

			AssertEquals(FilterVisibility.Visible, TrayFilter.Visibility);
			AssertEquals(0m, TrayFilter.Property1);
			AssertEquals(0m, TrayFilter.Property2);

			// Right location search
			FilterStripBizO.SetInitialCodeForSearch("MelRow-3-2", typeof(WhsLocation));
			AssertEquals(FilterVisibility.AlwaysVisible, RowFilter.Visibility);
			AssertEquals(RowFilter.Property, melbourneRow.PK);

			AssertEquals(FilterVisibility.AlwaysVisible, ColumnFilter.Visibility);
			AssertEquals(3m, ColumnFilter.Property1);
			AssertEquals(3m, ColumnFilter.Property2);

			AssertEquals(FilterVisibility.AlwaysVisible, LevelFilter.Visibility);
			AssertEquals(2m, LevelFilter.Property1);
			AssertEquals(2m, LevelFilter.Property2);

			AssertEquals(FilterVisibility.AlwaysVisible, TrayFilter.Visibility);
			AssertEquals(1m, TrayFilter.Property1);
			AssertEquals(1m, TrayFilter.Property2);
		}

		public void TestSetInitialCodeForSearch_LocationInControl()
		{
			var melbourneRow = MelbourneWarehouse.Rows.Single(r => r.WR_Name == "MelRow");
			// Wrong location searches
			FilterStripBizO.SetInitialCodeForSearch("MelRow-4", "Location");
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			FilterStripBizO.SetInitialCodeForSearch("MelRow-4-4", "Location");
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			AssertEquals(FilterVisibility.Visible, ColumnFilter.Visibility);
			AssertEquals(0m, ColumnFilter.Property1);
			AssertEquals(0m, ColumnFilter.Property2);

			FilterStripBizO.SetInitialCodeForSearch("MelRow-3-2-1", "Location");
			AssertEquals(FilterVisibility.Visible, RowFilter.Visibility);
			AssertEquals(ZGuid.Empty, RowFilter.Property);

			AssertEquals(FilterVisibility.Visible, ColumnFilter.Visibility);
			AssertEquals(0m, ColumnFilter.Property1);
			AssertEquals(0m, ColumnFilter.Property2);

			AssertEquals(FilterVisibility.Visible, LevelFilter.Visibility);
			AssertEquals(0m, LevelFilter.Property1);
			AssertEquals(0m, LevelFilter.Property2);

			AssertEquals(FilterVisibility.Visible, TrayFilter.Visibility);
			AssertEquals(0m, TrayFilter.Property1);
			AssertEquals(0m, TrayFilter.Property2);

			// Right location search
			FilterStripBizO.SetInitialCodeForSearch("MelRow-3-2", "Location");
			AssertEquals(FilterVisibility.AlwaysVisible, RowFilter.Visibility);
			AssertEquals(RowFilter.Property, melbourneRow.PK);

			AssertEquals(FilterVisibility.AlwaysVisible, ColumnFilter.Visibility);
			AssertEquals(3m, ColumnFilter.Property1);
			AssertEquals(3m, ColumnFilter.Property2);

			AssertEquals(FilterVisibility.AlwaysVisible, LevelFilter.Visibility);
			AssertEquals(2m, LevelFilter.Property1);
			AssertEquals(2m, LevelFilter.Property2);

			AssertEquals(FilterVisibility.AlwaysVisible, TrayFilter.Visibility);
			AssertEquals(1m, TrayFilter.Property1);
			AssertEquals(1m, TrayFilter.Property2);
		}

		#endregion

		#region TestFilterIsNull

		public void TestWarehouseFilterIsNull()
		{
			TestFilterIsNull(WarehouseFilter, WhsLocationCollection.FilterSchema.Warehouse);
		}

		public void TestRowFilterIsNull()
		{
			TestFilterIsNull(RowFilter, WhsLocationCollection.FilterSchema.Row);
		}

		public void TestColumnFilterIsNull()
		{
			TestFilterIsNull(ColumnFilter, WhsLocationCollection.FilterSchema.Column);
		}

		public void TestLevelFilterIsNull()
		{
			TestFilterIsNull(LevelFilter, WhsLocationCollection.FilterSchema.Level);
		}

		public void TestTrayFilterIsNull()
		{
			TestFilterIsNull(TrayFilter, WhsLocationCollection.FilterSchema.Tray);
		}

		void TestFilterIsNull(ModuleFilter filter, ZString description)
		{
			// HACK: Change category to re-add filter
			filter.CategoryChanging += (o, e) =>
			{
				((IModuleFilterForStrategyInternal)e.Filter).Description = "New";
			};
			filter.Category = FilterCategories.UserDefined;

			AssertNull("Precondition: The existing Filter has been removed", FilterStripBizO[description]);
			AssertNoExceptionThrown("Should not throw exception", () => FilterStripBizO.SetInitialCodeForSearch("MelRow-1-1", typeof(WhsLocation)));
		}

		#endregion

		#region TestFilterPickPathSequence

		public void TestFilterPickPathSequence()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 4, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA3 = whs.FindLocation("A-3");
			var locationA4 = whs.FindLocation("A-4");

			locationA1.WLV_PickPathSequence = 2;
			locationA2.WLV_PickPathSequence = 1;
			locationA3.WLV_PickPathSequence = 1;
			locationA4.WLV_PickPathSequence = 3;
			Factory.Save();

			AddLocationToScope(Asserter, locationA1, locationA2, locationA3, locationA4);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var pickPathSequenceFilter = (ModuleNumberRangeFilter)filterStrip[WhsLocationCollection.FilterSchema.PickPathSequence];
			pickPathSequenceFilter.IsActive = true;
			AssertEquals("Precondition: Default value", 0m, pickPathSequenceFilter.Property1);
			Asserter.AssertMatches("Should not find out any locations", pickPathSequenceFilter);

			pickPathSequenceFilter.Property1 = 1;
			pickPathSequenceFilter.Property2 = 1;
			Asserter.AssertMatches($"Should match Location with Pick Path Sequence is 1", pickPathSequenceFilter, locationA2, locationA3);

			pickPathSequenceFilter.Property1 = 1;
			pickPathSequenceFilter.Property2 = 2;
			Asserter.AssertMatches($"Should match Location with Pick Path Sequence is from 1 to 2", pickPathSequenceFilter, locationA1, locationA2, locationA3);

			pickPathSequenceFilter.Property1 = 2;
			pickPathSequenceFilter.Property2 = 3;
			Asserter.AssertMatches($"Should match Location with Pick Path Sequence is from 2 to 3", pickPathSequenceFilter, locationA1, locationA4);

			pickPathSequenceFilter.Property1 = 3;
			pickPathSequenceFilter.Property2 = 3;
			Asserter.AssertMatches($"Should match Location with Pick Path Sequence is from 2 to 3", pickPathSequenceFilter, locationA4);
		}

		#endregion

		#region TestFilterPutawayPathSequence

		public void TestFilterPutawayPathSequence()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 4, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA3 = whs.FindLocation("A-3");
			var locationA4 = whs.FindLocation("A-4");

			locationA1.WLV_PutawayPathSequence = 1;
			locationA2.WLV_PutawayPathSequence = 2;
			locationA3.WLV_PutawayPathSequence = 3;
			locationA4.WLV_PutawayPathSequence = 1;
			Factory.Save();

			AddLocationToScope(Asserter, locationA1, locationA2, locationA3, locationA4);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var putawayPathSequenceFilter = (ModuleNumberRangeFilter)filterStrip[WhsLocationCollection.FilterSchema.PutawayPathSequence];
			putawayPathSequenceFilter.IsActive = true;
			AssertEquals("Precondition: Default value", 0m, putawayPathSequenceFilter.Property1);
			Asserter.AssertMatches("Should not find out any locations", putawayPathSequenceFilter);

			putawayPathSequenceFilter.Property1 = 1;
			putawayPathSequenceFilter.Property2 = 1;
			Asserter.AssertMatches($"Should match Location with Putaway Path Sequence is 1", putawayPathSequenceFilter, locationA1, locationA4);

			putawayPathSequenceFilter.Property1 = 1;
			putawayPathSequenceFilter.Property2 = 2;
			Asserter.AssertMatches($"Should match Location with Putaway Path Sequence is from 1 to 2", putawayPathSequenceFilter, locationA1, locationA2, locationA4);

			putawayPathSequenceFilter.Property1 = 2;
			putawayPathSequenceFilter.Property2 = 3;
			Asserter.AssertMatches($"Should match Location with Putaway Path Sequence is from 2 to 3", putawayPathSequenceFilter, locationA2, locationA3);

			putawayPathSequenceFilter.Property1 = 3;
			putawayPathSequenceFilter.Property2 = 3;
			Asserter.AssertMatches($"Should match Location with Putaway Path Sequence is from 2 to 3", putawayPathSequenceFilter, locationA3);
		}

		#endregion

		#region TestLastInventoryChangeDateFilter

		public void TestLastInventoryChangeDateFilter()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 3, 1);
			Factory.Save();

			var today = ZDateTimeOffset.Today;
			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA3 = whs.FindLocation("A-3");

			locationA1.WLV_LastInventoryChangeDate = today;
			locationA2.WLV_LastInventoryChangeDate = today.AddDays(1);
			locationA3.WLV_LastInventoryChangeDate = today.AddDays(2);
			Factory.Save();

			AddLocationToScope(Asserter, locationA1, locationA2, locationA3);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var dateFilter = (ModuleDateTimeOffsetFilter)filterStrip[WhsLocationCollection.FilterSchema.LastInventoryChangeDate];
			dateFilter.IsActive = true;
			AssertEquals("Precondition: No date entered", true, dateFilter.Property1.IsEmpty);
			Asserter.AssertMatches("Should find all locations", dateFilter, locationA1, locationA2, locationA3);

			var todayZDateTime = today.ToLocalZDateTime();
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = todayZDateTime;
			dateFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches($"Should match Location with Last Inventory Change Date of {todayZDateTime.ToString()}", dateFilter, locationA1);

			dateFilter.Property1 = todayZDateTime;
			dateFilter.Property2 = todayZDateTime.AddDays(1);
			Asserter.AssertMatches($"Should match Location with Last Inventory Change Date of {todayZDateTime.AddDays(1).ToString()}", dateFilter, locationA1, locationA2);

			dateFilter.Property1 = todayZDateTime.AddDays(1);
			dateFilter.Property2 = todayZDateTime.AddDays(2);
			Asserter.AssertMatches($"Should match Location with Last Inventory Change Date of {todayZDateTime.AddDays(2).ToString()}", dateFilter, locationA2, locationA3);

			dateFilter.Property1 = todayZDateTime.AddDays(3);
			dateFilter.Property2 = todayZDateTime.AddDays(4);
			Asserter.AssertMatches($"Should not match any locations", dateFilter);
		}

		public void TestLastInventoryChangeDateFilter_InDifferentTimeZone()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 3, 1);
			Factory.Save();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var today = ZDateTimeOffset.Today; // e.g.: 2019-09-02 00:00:00 +10/+11
			var locationA1 = whs.FindLocation("A-1");
			locationA1.WLV_LastInventoryChangeDate = today;
			Factory.Save();

			AddLocationToScope(Asserter, locationA1);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var dateFilter = (ModuleDateTimeOffsetFilter)filterStrip[WhsLocationCollection.FilterSchema.LastInventoryChangeDate];
			dateFilter.IsActive = true;

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNNJG";
			var todayZDateTime = today.ToZDateTime(); // e.g. ZDateTime: 2019-09-02 00:00:00 +08:00, Local ZDateTime: 2019-09-01 22:00:00/21:00:00
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = todayZDateTime;
			dateFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches("Should not match Location when local time is different with LastInventoryChangeDate", dateFilter);

			dateFilter.Property1 = todayZDateTime.AddDays(-1);
			dateFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches($"Should match Location with Last Inventory Change Date of {todayZDateTime.AddDays(1).ToString()}", dateFilter, locationA1);
		}

		#endregion

		#region TestCycleCount

		public void TestCycleCountLastPerformed()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 3, 1);
			Factory.Save();

			var today = ZDateTimeOffset.Today;
			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA3 = whs.FindLocation("A-3");

			locationA1.WLV_CycleCountLastPerformed = today;
			locationA2.WLV_CycleCountLastPerformed = today.AddDays(1);
			locationA3.WLV_CycleCountLastPerformed = today.AddDays(2);
			Factory.Save();

			AddLocationToScope(Asserter, locationA1, locationA2, locationA3);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var cycleCountLastPerformedFilter = (ModuleDateTimeOffsetFilter)filterStrip[WhsLocationCollection.FilterSchema.CycleCountLastPerformed];
			cycleCountLastPerformedFilter.IsActive = true;
			AssertEquals("Precondition: No date entered", true, cycleCountLastPerformedFilter.Property1.IsEmpty);
			Asserter.AssertMatches("Should find out all locations", cycleCountLastPerformedFilter, locationA1, locationA2, locationA3);

			var todayZDateTime = today.ToZDateTime();
			cycleCountLastPerformedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			cycleCountLastPerformedFilter.Property1 = todayZDateTime;
			cycleCountLastPerformedFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches($"Should match Location with last performed date of {todayZDateTime.ToString()}", cycleCountLastPerformedFilter, locationA1);

			cycleCountLastPerformedFilter.Property1 = todayZDateTime;
			cycleCountLastPerformedFilter.Property2 = todayZDateTime.AddDays(1);
			Asserter.AssertMatches($"Should match Location with last performed date of {todayZDateTime.AddDays(1).ToString()}", cycleCountLastPerformedFilter, locationA1, locationA2);

			cycleCountLastPerformedFilter.Property1 = todayZDateTime.AddDays(1);
			cycleCountLastPerformedFilter.Property2 = todayZDateTime.AddDays(2);
			Asserter.AssertMatches($"Should match Location with last performed date of {todayZDateTime.AddDays(2).ToString()}", cycleCountLastPerformedFilter, locationA2, locationA3);
		}

		public void TestCycleCountLastPerformedCore_InDifferentTimeZone()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 3, 1);
			Factory.Save();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			var today = ZDateTimeOffset.Today; // e.x.: 2019-09-02 00:00:00 +10/+11
			var locationA1 = whs.FindLocation("A-1");
			locationA1.WLV_CycleCountLastPerformed = today;
			Factory.Save();

			AddLocationToScope(Asserter, locationA1);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var cycleCountLastPerformedFilter = (ModuleDateTimeOffsetFilter)filterStrip[WhsLocationCollection.FilterSchema.CycleCountLastPerformed];
			cycleCountLastPerformedFilter.IsActive = true;

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNNJG";
			var todayZDateTime = today.ToZDateTime(); // e.g. ZDateTime: 2019-09-02 00:00:00 +08:00, Local ZDateTime: 2019-09-01 22:00:00/21:00:00
			cycleCountLastPerformedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			cycleCountLastPerformedFilter.Property1 = todayZDateTime;
			cycleCountLastPerformedFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches("Should not match Location if local time is different with last performed date", cycleCountLastPerformedFilter);

			cycleCountLastPerformedFilter.Property1 = todayZDateTime.AddDays(-1);
			cycleCountLastPerformedFilter.Property2 = todayZDateTime;
			Asserter.AssertMatches($"Should match Location with last performed date of {todayZDateTime.AddDays(-1).ToString()}", cycleCountLastPerformedFilter, locationA1);
		}

		public void TestCycleCountPathSequence()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 3, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			var locationA2 = whs.FindLocation("A-2");
			var locationA3 = whs.FindLocation("A-3");
			var row = locationA1.Row;
			row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel;
			row.UpdatePathSequenceOnLocations();
			Factory.Save();

			AssertEquals("Precondtion: CycleCountPathSequence", 1, locationA1.WLV_CycleCountPathSequence);
			AssertEquals("Precondtion: CycleCountPathSequence", 2, locationA2.WLV_CycleCountPathSequence);
			AssertEquals("Precondtion: CycleCountPathSequence", 3, locationA3.WLV_CycleCountPathSequence);

			AddLocationToScope(Asserter, locationA1, locationA2, locationA3);

			var filterStrip = new LocationFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterStrip.ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs.PK;

			var cycleCountPathSequenceFilter = (ModuleNumberRangeFilter)filterStrip[WhsLocationCollection.FilterSchema.CycleCountPathSequence];
			cycleCountPathSequenceFilter.IsActive = true;
			AssertEquals("Precondition: Default value", 0m, cycleCountPathSequenceFilter.Property1);
			Asserter.AssertMatches("Should not find out any locations", cycleCountPathSequenceFilter);

			cycleCountPathSequenceFilter.Property1 = 1;
			cycleCountPathSequenceFilter.Property2 = 1;
			Asserter.AssertMatches($"Should match Location with Cycle Count Path Sequence is 1", cycleCountPathSequenceFilter, locationA1);

			cycleCountPathSequenceFilter.Property1 = 1;
			cycleCountPathSequenceFilter.Property2 = 2;
			Asserter.AssertMatches($"Should match Location with Cycle Count Path Sequence is from 1 to 2", cycleCountPathSequenceFilter, locationA1, locationA2);

			cycleCountPathSequenceFilter.Property1 = 2;
			cycleCountPathSequenceFilter.Property2 = 3;
			Asserter.AssertMatches($"Should match Location with Cycle Count Path Sequence is from 2 to 3", cycleCountPathSequenceFilter, locationA2, locationA3);
		}

		#endregion

		#region Implementation

		LocationCollectionForTest Collection { get; set; }
		WhsWarehouse MelbourneWarehouse { get; set; }
		WhsWarehouse SydneyWarehouse { get; set; }
		ZGuid OrgPK { get; set; }
		ZGuid ProductPK { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			Collection = new LocationCollectionForTest(Factory);
			MelbourneWarehouse = Helper.CreateWarehouse("Melbourne", "MelRow", 3, 3);
			SydneyWarehouse = Helper.CreateWarehouse("Sydney", "SydRow", 2, 2);
			OrgPK = IHelper.CreateClient("Org1");
			ProductPK = IHelper.CreateProduct(OrgPK, "P1").PK;

			// save setup() data because the mandatory Warehouse filter uses a ZDBOnlyQuery
			Factory.Save();

			// mandatory filter
			WarehouseFilter.IsActive = true;
			WarehouseFilter.Property = MelbourneWarehouse.PK;
		}

		LocationFilterBusinessObject FilterStripBizO => filterStripBizO ?? (filterStripBizO = new LocationFilterBusinessObject());

		WhsTestHelperFunctionsEnv Helper => (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory));

		IWhsTransactionTestHelper IHelper => ihelper ?? (ihelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory));

		TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LocationFilterBusinessObject();
		}

		LocationFilterBusinessObject filterStripBizO;
		WhsTestHelperFunctionsEnv helper;
		IWhsTransactionTestHelper ihelper;
		TestNotificationBuffer notify;

		class LocationCollectionForTest : ActiveBusinessObjectCollection<WhsLocation>
		{
			public LocationCollectionForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion
	}
}

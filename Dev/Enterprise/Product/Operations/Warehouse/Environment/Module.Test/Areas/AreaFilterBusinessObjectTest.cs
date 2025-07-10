using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(AreaFilterBusinessObject))]
	class AreaFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Warehouse Filter

		public void TestFilterWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");

			var area11 = Helper.CreateArea(whs1, "AREA11", CodeLists.AreaTypes.Codes.Bonded);
			var area12 = Helper.CreateArea(whs1, "AREA12", CodeLists.AreaTypes.Codes.Excise);
			var area21 = Helper.CreateArea(whs2, "AREA21", CodeLists.AreaTypes.Codes.FreeStore);

			Factory.Save();

			var areaFilter = new AreaFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)areaFilter[WhsAreaCollection.FilterConstants.Warehouse];

			var areaCollection1 = new WhsAreaCollection(Factory, whs1);
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			areaCollection1.Find(areaFilter.Filter);
			AssertCollectionContains("Area11", area11, areaCollection1);
			AssertCollectionContains("Area12", area12, areaCollection1);
			AssertCollectionNotContains("Area21", area21, areaCollection1);

			var areaCollection2 = new WhsAreaCollection(Factory, whs2);
			warehouseFilter.Property = whs2.PK;
			areaCollection2.Find(areaFilter.Filter);
			AssertCollectionNotContains("Area11", area11, areaCollection2);
			AssertCollectionNotContains("Area12", area12, areaCollection2);
			AssertCollectionContains("Area21", area21, areaCollection2);
		}

		public void TestFilterWarehouseVisibility()
		{
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			var filterBusinessObject = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBusinessObject[WhsAreaCollection.FilterConstants.Warehouse];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				filterBusinessObject = GetNewFilterStripBusinessObject();
				filter = (ModuleGuidFilter)filterBusinessObject[WhsAreaCollection.FilterConstants.Warehouse];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter.Visibility);

				Globals.IsWeb = true;
				try
				{
					filterBusinessObject = GetNewFilterStripBusinessObject();
					filter = (ModuleGuidFilter)filterBusinessObject[WhsAreaCollection.FilterConstants.Warehouse];
					AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		#endregion

		#region TestWarehouseFilter_IncludesTransitWarehouses

		public void TestWarehouseFilter_IncludesTransitWarehouses()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filterBizO[WhsAreaCollection.FilterConstants.Warehouse];
			var warehouseCollection = (WhsWarehouseCollection)warehouseFilter.List;
			warehouseCollection.Load();
			AssertContainsExactElementsInAnyOrder("Area Module should be able to see Transit Warehouses.", new[] { productWarehouse.PK, transitWarehouse.PK }, warehouseCollection.Select(w => w.PK));
		}

		#endregion

		#region TestAreaNameFilter

		public void TestAreaNameFilter()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");
			var area11 = Helper.CreateArea(whs1, "AREA11", CodeLists.AreaTypes.Codes.Bonded);
			var area12 = Helper.CreateArea(whs1, "AREA12", CodeLists.AreaTypes.Codes.Excise);
			var area21 = Helper.CreateArea(whs2, "AREA21", CodeLists.AreaTypes.Codes.FreeStore);
			Factory.Save();

			var filterStrip = new AreaFilterBusinessObject();
			var areaNameFilter = (ModuleTextFilter)filterStrip[WhsAreaCollection.FilterConstants.AreaName];
			areaNameFilter.Property = "AREA11";
			areaNameFilter.IsActive = true;

			var areas = new WhsAreaCollection(Factory, filterStrip.Filter);

			AssertCollectionContains(area11, areas);
			AssertCollectionNotContains(area12, areas);
			AssertCollectionNotContains(area21, areas);

			using (Res.TemporarilySwitchLanguage("ZH-CN"))
			using (Res.UseMockData())
			{
				filterStrip = new AreaFilterBusinessObject();
				AssertType(typeof(ModuleTextFilter), filterStrip[WhsAreaCollection.FilterConstants.AreaName]);
				AssertEquals("Area Name (English)", filterStrip[WhsAreaCollection.FilterConstants.AreaName].MultilingualDescription);
				AssertEquals(WhsAreaSchema.WA_Name, filterStrip[WhsAreaCollection.FilterConstants.AreaName].FilterColumn);

				AssertType(typeof(ModuleTranslatableTextFilter), filterStrip["Area Name_Local"]);
				AssertEquals("Area Name (Chinese - Simplified)", filterStrip["Area Name_Local"].MultilingualDescription);
				AssertEquals(WhsAreaSchema.WA_Name, filterStrip["Area Name_Local"].FilterColumn);
			}
		}

		#endregion

		#region TestIsPickingAndPutawayAreaFilters

		public void TestIsPickingAndPutawayAreaFilters()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");
			var pickingAreaInWhs1 = Helper.CreateArea(whs1, "A1", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayAreaInWhs1 = Helper.CreateArea(whs1, "A2", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			var bothAreaInWhs1 = Helper.CreateArea(whs1, "A3", CodeLists.AreaTypes.Codes.FreeStore);
			var pickingAreaInWhs2 = Helper.CreateArea(whs2, "A4", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayAreaInWhs2 = Helper.CreateArea(whs2, "A5", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			var bothAreaInWhs2 = Helper.CreateArea(whs2, "A6", CodeLists.AreaTypes.Codes.FreeStore);
			Factory.Save();
			Asserter.AddToScope(pickingAreaInWhs1, putawayAreaInWhs1, bothAreaInWhs1, pickingAreaInWhs2, putawayAreaInWhs2, bothAreaInWhs2);

			// picking area filter
			var filterStrip = new AreaFilterBusinessObject();
			var pickingAreaFilter = (ModuleFlagsFilter)filterStrip.ModuleFilters[WhsAreaCollection.FilterConstants.IsPickingArea];
			AssertEquals(FilterCategories.StatusAndFlags, pickingAreaFilter.Category);
			pickingAreaFilter.IsActive = true;
			pickingAreaFilter.Property0 = true;
			Asserter.AssertMatches("Only Picking areas must be returned.", pickingAreaFilter, pickingAreaInWhs1, pickingAreaInWhs2, bothAreaInWhs2, bothAreaInWhs1);

			pickingAreaFilter.Property0 = false;
			Asserter.AssertMatches("Since pick only filter is disabled, all areas must be returned.", pickingAreaFilter, pickingAreaInWhs1, putawayAreaInWhs1, bothAreaInWhs1, pickingAreaInWhs2, putawayAreaInWhs2, bothAreaInWhs2);

			//putaway area filter
			var putawayAreaFilter = (ModuleFlagsFilter)filterStrip.ModuleFilters[WhsAreaCollection.FilterConstants.IsPutawayArea];
			AssertEquals(FilterCategories.StatusAndFlags, putawayAreaFilter.Category);
			putawayAreaFilter.IsActive = true;
			putawayAreaFilter.Property0 = true;
			Asserter.AssertMatches("Only Putaway areas must be returned.", putawayAreaFilter, putawayAreaInWhs1, putawayAreaInWhs2, bothAreaInWhs2, bothAreaInWhs1);

			putawayAreaFilter.Property0 = false;
			Asserter.AssertMatches("Since putawa only filter is disabled, all areas must be returned.", putawayAreaFilter, pickingAreaInWhs1, putawayAreaInWhs1, bothAreaInWhs1, pickingAreaInWhs2, putawayAreaInWhs2, bothAreaInWhs2);
		}

		#endregion

		#region Properties

		public void TestWA_WW_Whs()
		{
			var whs = Factory.New<WhsWarehouse>();
			var filterBusinessObject = new AreaFilterBusinessObject();
			AssertEquals(ZGuid.Empty, filterBusinessObject.WA_WW_Whs);

			var filter = (ModuleGuidFilter)filterBusinessObject[WhsAreaCollection.FilterConstants.Warehouse];
			filter.Property = whs.PK;
			AssertEquals(whs.PK, filterBusinessObject.WA_WW_Whs);
		}

		#endregion

		#region Implementation

		FilterStripAsserter<WhsArea> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsArea>(Factory, a => a.WA_Name));
		FilterStripAsserter<WhsArea> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AreaFilterBusinessObject();
		}

		WhsTestHelperFunctionsEnv Helper => (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(RowFilterBusinessObject))]
	public class RowFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestFilterWarehouse

		public void TestFilterWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");

			var row11 = Helper.CreateRow(whs1, "ROW11");
			var row12 = Helper.CreateRow(whs1, "ROW12");
			var row21 = Helper.CreateRow(whs2, "ROW21");

			Factory.Save();

			var rowFilter = new RowFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)rowFilter["Warehouse"];

			var rowCollection1 = new WhsRowCollection(whs1, Factory);
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			rowCollection1.AdditionalFilter = rowFilter.Filter;
			AssertCollectionContains("Row11", row11, rowCollection1);
			AssertCollectionContains("Row12", row12, rowCollection1);
			AssertCollectionNotContains("Row21", row21, rowCollection1);

			var rowCollection2 = new WhsRowCollection(whs2, Factory);
			warehouseFilter.Property = whs2.PK;
			rowCollection2.AdditionalFilter = rowFilter.Filter;
			AssertCollectionNotContains("Row11", row11, rowCollection2);
			AssertCollectionNotContains("Row12", row12, rowCollection2);
			AssertCollectionContains("Row21", row21, rowCollection2);
		}

		#endregion

		#region TestAreaFilter

		public void TestAreaFilter()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var whs2 = Helper.CreateWarehouse("WHS2");

			var row11 = Helper.CreateRowAndGenerateLocations(whs1, "ROW11");
			var row12 = Helper.CreateRowAndGenerateLocations(whs1, "ROW12");
			var row13 = Helper.CreateRowAndGenerateLocations(whs1, "ROW13");
			var row14 = Helper.CreateRowAndGenerateLocations(whs1, "ROW14");
			var row21 = Helper.CreateRowAndGenerateLocations(whs2, "ROW21");

			var pickingOnlyArea = Helper.CreateArea(whs1, "A3", AreaTypes.Codes.FreeStore, true, false);
			var putawayOnlyArea = Helper.CreateArea(whs1, "A4", AreaTypes.Codes.FreeStore, false, true);
			var bothArea = Helper.CreateArea(whs1, "A5", AreaTypes.Codes.FreeStore, true, true);
			Helper.SetLocationArea(row11.Locations.Single(), pickingOnlyArea, bothArea);
			Helper.SetLocationArea(row12.Locations.Single(), bothArea, putawayOnlyArea);
			Helper.SetLocationArea(row13.Locations.Single(), bothArea, bothArea);

			Asserter.AddToScope(row11, row12, row13, row14, row21);

			Factory.Save();

			var rowFilter = new RowFilterBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)rowFilter["Warehouse"];
			Asserter.AssertMatches("If warehouse is not set rows from all warehouses must be returned.", warehouseFilter, row11, row12, row13, row14, row21);

			warehouseFilter.IsActive = true;
			warehouseFilter.Property = whs1.PK;
			Asserter.AssertMatches("Only warehouse rows must be returned.", warehouseFilter, row11, row12, row13, row14);

			var areaFilter = (ModuleGuidFilter)rowFilter["Area"];
			areaFilter.IsActive = true;
			areaFilter.Property = pickingOnlyArea.PK;
			Asserter.AssertMatches("Only rows with locations in pickingOnlyArea must be returned.", areaFilter, row11);

			areaFilter.Property = putawayOnlyArea.PK;
			Asserter.AssertMatches("Only rows with locations in putawayOnlyArea must be returned.", areaFilter, row12);

			areaFilter.Property = bothArea.PK;
			Asserter.AssertMatches("Only rows with locations in bothArea must be returned.", areaFilter, row11, row12, row13);
		}

		#endregion

		#region TestFilterWarehouseVisibility

		public void TestFilterWarehouseVisibility()
		{
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			FilterStripBusinessObject filterBusinessObject = GetNewFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBusinessObject["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				filterBusinessObject = GetNewFilterStripBusinessObject();
				filter = (ModuleGuidFilter)filterBusinessObject["Warehouse"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter.Visibility);

				Globals.IsWeb = true;
				try
				{
					filterBusinessObject = GetNewFilterStripBusinessObject();
					filter = (ModuleGuidFilter)filterBusinessObject["Warehouse"];
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

		#region TestAreaFilterLookups

		public void TestAreaFilterLookups()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var area1 = warehouse.Areas[0];
			var area2 = warehouse.Areas[1];
			var pickingOnlyArea = Helper.CreateArea(warehouse, "A3", AreaTypes.Codes.FreeStore, true, false);
			var putawayOnlyArea = Helper.CreateArea(warehouse, "A4", AreaTypes.Codes.FreeStore, false, true);
			var bothArea = Helper.CreateArea(warehouse, "A5", AreaTypes.Codes.FreeStore, true, true);
			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var areaFilter = (ModuleGuidFilter)filterBizO["Area"];
			AssertEquals("Area collection should be empty without Warehouse set.", 0, areaFilter.List.Count);

			var warehouseFilter = (ModuleGuidFilter)filterBizO["Warehouse"];
			warehouseFilter.Property = warehouse.PK;
			AssertContainsExactElementsInAnyOrder(new[] { area1.PK, area2.PK, pickingOnlyArea.PK, putawayOnlyArea.PK, bothArea.PK }, areaFilter.List.OfType<WhsArea>().Select(a => a.PK));
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
			var warehouseFilter = (ModuleGuidFilter)filterBizO["Warehouse"];
			var warehouseCollection = (WhsWarehouseCollection)warehouseFilter.List;
			warehouseCollection.Load();
			AssertContainsExactElementsInAnyOrder("Row Module should be able to see Transit Warehouses.", new[] { productWarehouse.PK, transitWarehouse.PK }, warehouseCollection.Select(w => w.PK));
		}

		#endregion

		#region TestWR_WW_Whs

		public void TestWR_WW_Whs()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			RowFilterBusinessObject filterBusinessObject = new RowFilterBusinessObject();
			AssertEquals(ZGuid.Empty, filterBusinessObject.WR_WW_Whs);

			ModuleGuidFilter filter = (ModuleGuidFilter)filterBusinessObject["Warehouse"];
			filter.Property = whs.PK;
			AssertEquals(whs.PK, filterBusinessObject.WR_WW_Whs);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RowFilterBusinessObject();
		}

		FilterStripAsserter<WhsRow> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsRow>(Factory, r => r.WR_Name)); }
		}
		FilterStripAsserter<WhsRow> asserter;

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return (helper = helper ?? new WhsTestHelperFunctionsEnv(Factory)); }
		}
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}

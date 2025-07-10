using System.Linq;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsRowLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestWarehouses

		public void TestWarehouses()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var row = Factory.New<WhsRow>();
			AssertNotNull(row.Lookups.Warehouses);
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), row.Lookups.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, row.Lookups.Warehouses.Count);

			row.Lookups.Warehouses.Load();
			AssertContainsExactElementsInAnyOrder("Warehouses Lookups on Rows should include Transit Warehouses.", new[] { productWarehouse, transitWarehouse }, row.Lookups.Warehouses);
		}

		#endregion

		#region TestPickingAndPutawayAreas

		public void TestPickingAndPutawayAreas()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "R1", 2, 2);
			var pickingArea = Helper.CreateArea(warehouse, "A", "", isPickingArea: true, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(warehouse, "B", "", isPickingArea: false, isPutawayArea: true);
			var bothArea = Helper.CreateArea(warehouse, "C", "", isPickingArea: true, isPutawayArea: true);
			var row = warehouse.Rows.Single(r => r.WR_Name == "R1");

			AssertCollectionContains("Picking only area must be returned for PickingAreas.", pickingArea, row.Lookups.PickingAreas);
			AssertCollectionContains("Area marked as both picking and putaway must be returned for PickingAreas.", bothArea, row.Lookups.PickingAreas);
			AssertCollectionNotContains("Putaway only area must not be returned for PickingAreas.", putawayArea, row.Lookups.PickingAreas);

			AssertCollectionNotContains("Picking only area must be not returned for PutawayAreas.", pickingArea, row.Lookups.PutawayAreas);
			AssertCollectionContains("Area marked as both picking and putaway must be returned for PutawayAreas.", bothArea, row.Lookups.PutawayAreas);
			AssertCollectionContains("Putaway only area must not be returned for PutawayAreas.", putawayArea, row.Lookups.PutawayAreas);
		}

		#endregion

		#region TestLocationTypes

		public void TestLocationTypes()
		{
			Helper.CreateLocationType("123");

			var lookups = Row.Lookups;
			AssertEquals(7, lookups.LocationTypes.Count);
			// user created location types
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "123");
			// system location types
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "DOC");
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "OPN");
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "RDO");
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "RNO");
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "PFC");
			lookups.LocationTypes.Single(lt => lt.WLT_Code == "DPF");
		}

		#endregion

		#region TestWeightUnitTypes

		public void TestWeightUnitTypes()
		{
			var lookups = Factory.New<WhsRow>().Lookups;
			AssertEquals(true, lookups.WeightUnitTypes.ContainsCode(Enterprise.Core.Constants.Weight.Pounds));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.WeightUnitTypes);
		}

		#endregion

		#region TestVolumeUnitTypes

		public void TestVolumeUnitTypes()
		{
			var lookups = Factory.New<WhsRow>().Lookups;
			AssertEquals(true, lookups.CubicUnitTypes.ContainsCode(Enterprise.Core.Constants.Volume.Litre));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Volume), lookups.CubicUnitTypes);
		}

		#endregion

		#region TestDimensionUnitTypes

		public void TestDimensionUnitTypes()
		{
			var lookups = Factory.New<WhsRow>().Lookups;
			AssertEquals(true, lookups.DimensionUnitTypes.ContainsCode(Enterprise.Core.Constants.Length.Metres));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Length), lookups.DimensionUnitTypes);
		}

		#endregion
		#region TestApprovedKnownStatuses

		public virtual void TestApprovedKnownStatuses()
		{
			AssertEquals(true, Row.Lookups.ApprovedKnownStatuses.ContainsCode(CodeLists.ApprovedKnownStatus.Codes.NO));
		}

		#endregion

		#region TestSortPathMethods

		public void TestSortPathMethods()
		{
			var lookups = Row.Lookups;
			AssertEquals(3, lookups.SortPathMethods.Count);
			lookups.SortPathMethods.ContainsCode(CodeLists.SortPathMethods.Codes.ColumnThenLevel);
			lookups.SortPathMethods.ContainsCode(CodeLists.SortPathMethods.Codes.LevelThenColumn);
			lookups.SortPathMethods.ContainsCode(CodeLists.SortPathMethods.Codes.UserDefined);
		}

		#endregion

		#region Implementation

		public WhsRow Row
		{
			get
			{
				if (row == null)
				{
					WhsWarehouse whs = Factory.New<WhsWarehouse>();
					row = whs.Rows.AddNew();
				}
				return row;
			}
		}

		WhsRow row;

		#endregion
	}
}

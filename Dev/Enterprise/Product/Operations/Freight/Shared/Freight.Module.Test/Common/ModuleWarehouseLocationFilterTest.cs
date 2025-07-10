using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(ModuleWarehouseLocationFilter))]
	sealed class ModuleWarehouseLocationFilterTest : ModuleFilterTestCase<ModuleWarehouseLocationFilter>
	{
		#region TestLocationValidation

		public void TestLocationValidation()
		{
			Filter.Location = "";
			AssertNoNotifications(Filter.LocationInfo);

			Filter.Location = "ROW1";
			AssertNoNotifications(Filter.LocationInfo);

			Filter.Location = "ROW1-2";
			AssertNoNotifications(Filter.LocationInfo);

			Filter.Location = "ROW1-2-3";
			AssertNoNotifications(Filter.LocationInfo);

			Filter.Location = "ROW1-2-3-4";
			AssertNoNotifications(Filter.LocationInfo);

			Filter.Location = "Invalid Value";
			AssertHasErrors(Filter.LocationInfo);
		}

		#endregion

		#region TestLocationReformat

		public void TestLocationReformat()
		{
			AssertEquals("", Filter.Location);

			Filter.Location = "ROW1";
			AssertEquals("ROW1", Filter.Location);

			Filter.Location = "ROW1 2";
			AssertEquals("ROW1-2", Filter.Location);

			Filter.Location = "ROW1 2-3";
			AssertEquals("ROW1-2-3", Filter.Location);

			Filter.Location = "ROW1,2,3.4";
			AssertEquals("ROW1-2-3-4", Filter.Location);

			Filter.Location = "Invalid";
			AssertEquals("Invalid", Filter.Location);

			Filter.Location = "";
			AssertEquals("", Filter.Location);
		}

		#endregion

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			PackLocation loc1 = NewLocation("ROW1", 1, 1, 1);
			PackLocation loc2 = NewLocation("ROW1", 1, 1, 1);

			Factory.Save();

			PackLocation[] results;

			Filter.Warehouse = ZGuid.Empty;
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionContains(loc2, results);

			Filter.Warehouse = loc1.LocationWhsGuid;
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionNotContains(loc2, results);
		}

		#endregion

		#region TestLocationFilter

		public void TestLocationFilter()
		{
			PackLocation loc1 = NewLocation("ROW1", 1, 1, 1);
			PackLocation loc2 = NewLocation("ROW2", 1, 1, 1);
			PackLocation loc3 = NewLocation("ROW1", 2, 1, 1);
			PackLocation loc4 = NewLocation("ROW1", 1, 2, 1);
			PackLocation loc5 = NewLocation("ROW1", 1, 1, 2);

			Factory.Save();

			PackLocation[] results;

			Filter.Location = "";
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionContains(loc2, results);
			AssertCollectionContains(loc3, results);
			AssertCollectionContains(loc4, results);
			AssertCollectionContains(loc5, results);

			Filter.Location = "ROW1";
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionNotContains(loc2, results);
			AssertCollectionContains(loc3, results);
			AssertCollectionContains(loc4, results);
			AssertCollectionContains(loc5, results);

			Filter.Location = "ROW1-1";
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionNotContains(loc2, results);
			AssertCollectionNotContains(loc3, results);
			AssertCollectionContains(loc4, results);
			AssertCollectionContains(loc5, results);

			Filter.Location = "ROW1-1-1";
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionNotContains(loc2, results);
			AssertCollectionNotContains(loc3, results);
			AssertCollectionNotContains(loc4, results);
			AssertCollectionContains(loc5, results);

			Filter.Location = "ROW1 1-1-1";
			results = Factory.Load<PackLocation>(Filter.Query);
			AssertCollectionContains(loc1, results);
			AssertCollectionNotContains(loc2, results);
			AssertCollectionNotContains(loc3, results);
			AssertCollectionNotContains(loc4, results);
			AssertCollectionNotContains(loc5, results);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			ModuleWarehouseLocationFilter filter = GetNewModuleFilter();
			AssertEquals("IsExpensiveQuery", false, filter.IsExpensiveQuery);
		}

		#endregion

		#region Implementation

		PackLocation NewLocation(ZString row, ZShort column, ZShort level, ZShort tray)
		{
			var address = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var warehouse = (IWhsWarehouse)Helper.CreateWarehouse("1");
			warehouse.WW_OA_WarehouseAddress = address.PK;

			var currentWarehouseBranches = Factory.Load<IWhsWarehouse>(new ZQuery()).Select(w => w.WW_GB_RelatedCompanyBranch);
			var query = new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, currentWarehouseBranches);

			var branch = Factory.LoadTop1<GlbBranch>(query);
			if (branch == null)
			{
				branch = Factory.New<GlbBranch>();
				branch.GB_Code = warehouse.WW_WarehouseCode;
				branch.GB_GC = GlbCompany.CurrentCompany.PK;
			}

			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;

			var whsRow = (IWhsRow)warehouse.Rows.AddNew();
			whsRow.WR_Name = row;
			whsRow.WR_Columns = 2;
			whsRow.WR_Levels = 2;
			whsRow.WR_Trays = 2;

			var whsArea = (IWhsArea)warehouse.Areas.AddNew();
			whsArea.WA_Name = "AREA";
			Factory.Save();

			var combinedQuery = new ZQuery();
			combinedQuery.AddToFilter(new ZQuery(WhsLocationViewSchema.WLV_WR, whsRow.PK));
			combinedQuery.AddToFilter(new ZQuery(WhsLocationViewSchema.WLV_Column, column));
			combinedQuery.AddToFilter(new ZQuery(WhsLocationViewSchema.WLV_Level, level));
			combinedQuery.AddToFilter(new ZQuery(WhsLocationViewSchema.WLV_Tray, tray));

			var location = Factory.LoadTop1<IWhsLocation>(combinedQuery);
			location.WLV_WA_PickingArea = whsArea.PK;

			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var packLocation = packLine.PackLocations.AddNew();
			packLocation.JQ_WL = location.PK;

			return packLocation;
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ModuleWarehouseLocationFilter GetNewModuleFilter()
		{
			return new ModuleWarehouseLocationFilter(ExpectedDescription, GetFilter, ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory));
		}

		ZQuery GetFilter(ZQuery warehouseLocationFilter)
		{
			var whsLocationFilter = new ZDBOnlySubQuery(typeof(IWhsLocation), JobPackLocSchema.JQ_WL);
			whsLocationFilter.AddToFilter(warehouseLocationFilter);

			var locationFilter = new ZDBOnlyQuery(typeof(PackLocation));
			locationFilter.AddSubQuery(whsLocationFilter, JoinCondition.And);

			return locationFilter;
		}

		#region WhsHelper

		IWhsTransactionTestHelper Helper => helper ?? (helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory));
		IWhsTransactionTestHelper helper;

		#endregion

		#endregion
	}
}

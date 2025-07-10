using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsWarehouseCollectionWithSecurityCheck))]
	public class WhsWarehouseCollectionWithSecurityCheckTest : WhsBusinessObjectCollectionTestCase
	{
		public void TestAllowedAccessTo()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			SetupOrgs();
			AssertEquals(true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(staffWarehouse));
			AssertEquals(true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(groupWarehouse));
			AssertEquals(false, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(badWarehouse));
		}

		public void TestFilter()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			SetupOrgs();

			WhsWarehouseCollectionWithSecurityCheck collection = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			collection.Load();

			AssertCollectionContains(staffWarehouse, collection);
			AssertCollectionNotContains(groupWarehouse, collection);
			AssertCollectionNotContains(badWarehouse, collection);
		}

		public void TestFilterWarehouseCollectionTypeParam()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			SetupOrgs();

			var collection = new WhsWarehouseCollectionWithSecurityCheck(Factory, WarehouseCollectionType.All);
			collection.Load();
			AssertCollectionContains(staffWarehouse, collection);
			AssertCollectionContains(groupWarehouse, collection);
			AssertCollectionNotContains(badWarehouse, collection);

			var collectionTransitWarehouse = new WhsWarehouseCollectionWithSecurityCheck(Factory, WarehouseCollectionType.TransitWarehouse);
			collectionTransitWarehouse.Load();
			AssertCollectionNotContains(staffWarehouse, collectionTransitWarehouse);
			AssertCollectionContains(groupWarehouse, collectionTransitWarehouse);
			AssertCollectionNotContains(badWarehouse, collectionTransitWarehouse);
		}

		public void TestAllowedAccessTo_AllowAll()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			SetupOrgs();
			AssertEquals(true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(staffWarehouse));
			AssertEquals(true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(groupWarehouse));
			AssertEquals(true, WhsWarehouseCollectionWithSecurityCheck.AllowedAccessTo(badWarehouse));
		}

		public void TestFilter_AllowAll()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			SetupOrgs();

			var collection = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			collection.Load();
			AssertCollectionContains(staffWarehouse, collection);
			AssertCollectionNotContains(groupWarehouse, collection);
			AssertCollectionContains(badWarehouse, collection);

			var collectionTransitWarehouse = new WhsWarehouseCollectionWithSecurityCheck(Factory, WarehouseCollectionType.TransitWarehouse);
			collectionTransitWarehouse.Load();
			AssertCollectionNotContains(staffWarehouse, collectionTransitWarehouse);
			AssertCollectionContains(groupWarehouse, collectionTransitWarehouse);
			AssertCollectionNotContains(badWarehouse, collectionTransitWarehouse);
		}

		WhsWarehouse staffWarehouse;
		WhsWarehouse groupWarehouse;
		WhsWarehouse badWarehouse;

		#region Implementation

		void SetupOrgs()
		{
			staffWarehouse = Helper.CreateWarehouse("STAFF", "STF", "AA");
			staffWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			groupWarehouse = Helper.CreateWarehouse("GROUP", "GRP", "BB");
			groupWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			badWarehouse = Helper.CreateWarehouse("BAD", "BAD", "CC");
			badWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.Save();

			GlbStaff staff = GlbStaff.CurrentUser;
			staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)staff).AddSecurityToAccessOrgOrWarehouse("STF");
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.AddFromDatabase(staff.PK);
			group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)group).AddSecurityToAccessOrgOrWarehouse("GRP");
			Factory.Save();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsWarehouseCollectionWithSecurityCheck(Factory);
		}

		#endregion
	}
}

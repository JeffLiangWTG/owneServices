using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WarehouseClientCollectionWithSecurityCheck))]
	sealed class WarehouseClientCollectionWithSecurityCheckBOTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowedAccessTo()
		{
			Env.Security.WhsAllowedClients.IsAllowed = false;
			SetupOrgs();
			AssertEquals(true, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(staffClient));
			AssertEquals(true, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(groupClient));
			AssertEquals(false, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(badClient));
		}

		public void TestFilter()
		{
			Env.Security.WhsAllowedClients.IsAllowed = false;
			SetupOrgs();

			WarehouseClientCollectionWithSecurityCheck collection = new WarehouseClientCollectionWithSecurityCheck(Factory);
			collection.Load();

			AssertCollectionContains(staffClient, collection);
			AssertCollectionContains(groupClient, collection);
			AssertCollectionNotContains(badClient, collection);
		}

		public void TestAllowedAccessTo_AllowAll()
		{
			Env.Security.WhsAllowedClients.IsAllowed = true;
			SetupOrgs();
			AssertEquals(true, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(staffClient));
			AssertEquals(true, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(groupClient));
			AssertEquals(true, WarehouseClientCollectionWithSecurityCheck.AllowedAccessTo(badClient));
		}

		public void TestFilter_AllowAll()
		{
			Env.Security.WhsAllowedClients.IsAllowed = true;
			SetupOrgs();

			WarehouseClientCollectionWithSecurityCheck collection = new WarehouseClientCollectionWithSecurityCheck(Factory);
			collection.Load();

			AssertCollectionContains(staffClient, collection);
			AssertCollectionContains(groupClient, collection);
			AssertCollectionContains(badClient, collection);
		}

		OrgHeader staffClient;
		OrgHeader groupClient;
		OrgHeader badClient;

		#region Implementation

		void SetupOrgs()
		{
			staffClient = Factory.NewWithValidTestData<OrgHeader>();
			staffClient.OH_Code = "STAFF";
			staffClient.OH_IsWarehouseClient = true;

			groupClient = Factory.NewWithValidTestData<OrgHeader>();
			groupClient.OH_Code = "GROUP";
			groupClient.OH_IsWarehouseClient = true;

			badClient = Factory.NewWithValidTestData<OrgHeader>();
			badClient.OH_Code = "BAD";
			badClient.OH_IsWarehouseClient = true;
			Factory.Save();

			GlbStaff staff = GlbStaff.CurrentUser;
			staff.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)staff).AddSecurityToAccessOrgOrWarehouse("STAFF");
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.AddFromDatabase(staff.PK);
			group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			((IOrgsAndWarehousesAccessProvider)group).AddSecurityToAccessOrgOrWarehouse("GROUP");
			Factory.Save();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WarehouseClientCollectionWithSecurityCheck(Factory);
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WhsWarehouseCollectionWithSecurityCheckForWeb))]
	[HttpContextEnabledTest]
	sealed class WhsWarehouseCollectionWithSecurityCheckForWebTest : WhsWarehouseCollectionWithSecurityCheckTest
	{
		#region TestAdditionalFilter_UserWebSecurityForWarehouse

		public void TestAdditionalFilter_UserWebSecurityForWarehouse()
		{
			var staffWarehouse = Helper.CreateWarehouse("STAFF", "STF", "AA");
			staffWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var groupWarehouse = Helper.CreateWarehouse("GROUP", "GRP", "BB");
			groupWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var badWarehouse = Helper.CreateWarehouse("BAD", "BAD", "CC");
			badWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var whsHepler = new WhsTestHelperFunctions(Factory);
			var client = whsHepler.CreateClient();
			var contact = client.Contacts.AddNew();
			contact.OC_Email = "123456@qq.com";
			contact.SetHashedPassword("123");
			contact.OC_WebAccessEnabled = true;

			var trackingHepler = new TestHelper(Factory);
			trackingHepler.TestSiteUser.Login(client.OH_Code, contact.OC_Email, contact.PasswordForTesting);
			AssertNotNull("Precondition: SiteUser has been set up", trackingHepler.TestSiteUser);
			Assert("WebUser should be logged in", trackingHepler.TestSiteUser.IsLoggedIn);

			whsHepler.CreateWhsOrder(client, staffWarehouse);

			whsHepler.ProhibitWarehouseAccessForOrgContact(badWarehouse, contact);

			Factory.Save();

			var collection = GetCollectionToTest();
			collection.Load();
			AssertCollectionContains(staffWarehouse, collection);
			AssertCollectionNotContains(groupWarehouse, collection);
			AssertCollectionNotContains(badWarehouse, collection);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsWarehouseCollectionWithSecurityCheckForWeb(Factory);
		}

		#endregion
	}
}

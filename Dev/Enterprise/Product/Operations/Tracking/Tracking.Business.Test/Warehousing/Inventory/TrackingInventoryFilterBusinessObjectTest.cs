using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingInventoryFilterBusinessObject))]
	sealed class TrackingInventoryFilterBusinessObjectTest : WarehouseFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TrackingInventoryFilterBusinessObject(null);
		}

		#region TestProhibitedWarehousesDisplaysError

		public void TestProhibitedWarehousesDisplaysError()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs2 = Helper.CreateWarehouse("WH2");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Helper.ProhibitWarehouseAccessForOrgContact(whs1, contact);
			Factory.Save();

			var filter = new TrackingInventoryFilterBusinessObject(null);
			var filterWarehouse = (ModuleGuidFilter)filter[InventoryFilterBusinessObject.Schema.Warehouse];

			filter.LoggedInUser = contact;
			filterWarehouse.IsActive = true;
			filterWarehouse.Property = whs2.PK;
			AssertEquals("WH2 is not prohibited, so there should be no errors.", false, filter.HasErrors);

			filterWarehouse.Property = whs1.PK;
			AssertEquals("WH1 is prohibited, so there should be an error.", true, filter.HasErrors);
		}

		#endregion

		#region TestLoggedInUser

		public void TestLoggedInUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var filter = new TrackingInventoryFilterBusinessObject(null);
			AssertNull(filter.LoggedInUser);

			filter.LoggedInUser = contact;
			AssertEquals(contact, filter.LoggedInUser);

			filter.LoggedInUser = null;
			AssertNull(filter.LoggedInUser);
		}

		#endregion

		#region Helpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}

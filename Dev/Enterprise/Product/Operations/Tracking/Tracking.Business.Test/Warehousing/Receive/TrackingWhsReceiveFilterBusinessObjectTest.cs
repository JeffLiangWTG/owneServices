using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceiveFilterBusinessObject))]
	sealed class TrackingWhsReceiveFilterBusinessObjectTest : WarehouseFilterBusinessObjectTest
	{
		#region TestProhibitedWarehousesDisplaysError

		public void TestProhibitedWarehousesDisplaysError()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var whs2 = Helper.CreateWarehouse("WH2");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Helper.ProhibitWarehouseAccessForOrgContact(whs1, contact);
			Factory.Save();

			var filter = new TrackingWhsReceiveFilterBusinessObject();
			filter.LoggedInUser = contact;
			var moduleFilter = filter["Warehouse"];
			((ModuleGuidFilter)moduleFilter).Property = whs2.PK;
			AssertEquals("WH2 is not prohibited, so there should be no error.", false, moduleFilter.HasErrors);

			((ModuleGuidFilter)moduleFilter).Property = whs1.PK;
			AssertEquals("WH1 is prohibited, so there should be error.", true, moduleFilter.HasErrors);
		}

		#endregion

		#region TestLoggedInUser

		public void TestLoggedInUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var filter = new TrackingWhsReceiveFilterBusinessObject();
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

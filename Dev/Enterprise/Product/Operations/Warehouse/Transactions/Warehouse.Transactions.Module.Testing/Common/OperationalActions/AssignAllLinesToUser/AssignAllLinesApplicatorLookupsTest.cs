using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class AssignAllLinesApplicatorLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestUsersToSelect

		public void TestUsersToSelect()
		{
			var user1 = Helper.CreateGlbStaff("T1", "T1");
			var user2 = Helper.CreateGlbStaff("T2", "T2");

			var lookups = new AssignAllLinesApplicatorLookups(new DummyApplicatorActionMethodApplicator(Factory));
			AssertCollectionContains(user1, lookups.UsersToSelect);
			AssertCollectionContains(user2, lookups.UsersToSelect);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}

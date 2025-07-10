using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WarehouseClientLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestWarehouses

		public void TestWarehouses()
		{
			AssertType(ObjectFactory.GetType<IWhsWarehouseCollection>(), GetNewLookups().Warehouses);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			AssertType<OrgHeaderCollection>(GetNewLookups().Clients);
		}

		#endregion

		#region Implementation

		WarehouseClientLookups GetNewLookups()
		{
			return new WarehouseClientLookups(new UpdateExpiryNotificationPeriodMethodApplicator("test", Factory));
		}

		#endregion
	}
}

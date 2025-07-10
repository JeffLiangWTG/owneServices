using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryHeldCodeLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		#region TestClients

		public void TestClients()
		{
			var holdCode = Factory.New<WhsInventoryHeldCode>();
			var lookups = new WhsInventoryHeldCodeLookups(holdCode);
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;
			var clients = lookups.Clients;
			AssertNotNull(clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), clients.GetType());
			AssertEquals("Should not be loaded", 0, clients.Count);
			AssertEquals("Should be cached", clients, lookups.Clients);
		}

		#endregion
	}
}

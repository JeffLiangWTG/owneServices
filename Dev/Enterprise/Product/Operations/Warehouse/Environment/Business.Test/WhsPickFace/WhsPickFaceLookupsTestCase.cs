namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsPickFaceLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		public void TestGetClientsWithDeletedPickface()
		{
			var pickface = Factory.New<WhsPickFace>();
			var lookup = new WhsPickFaceLookups(pickface);
			pickface.Delete();
			var clients = lookup.Clients;
			AssertEquals(0, clients.Count);
		}

		#region TestLocations

		public void TestLocations()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 1, 1);
			Factory.Save();

			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, data.Whs1.FindLocation("A-1"));
			var lookups = new WhsPickFaceLookups(pickface);

			Factory.Save();

			AssertEquals(3, lookups.Locations.Count);
			AssertCollectionContains(data.Whs1.FindLocation("A-1"), lookups.Locations);
			AssertCollectionContains(data.Whs1.FindLocation("A-2"), lookups.Locations);
			AssertCollectionContains(data.Whs1.DefaultOutboundDockDoorLocation, lookups.Locations);
			AssertCollectionNotContains(warehouse.FindLocation("Row1-1"), lookups.Locations);
		}

		#endregion

	}
}

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsClientPickPackParamsByWhsLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		#region TestSalesChannels

		public void TestSalesChannels()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertEquals("Sales Channel Collection should have correct type.", typeof(WhsSalesChannelCollection), pickPackParameter.Lookups.SalesChannels.GetType());
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertEquals("Warehouse Collection should have correct type.", typeof(WhsWarehouseCollectionWithSecurityCheck), pickPackParameter.Lookups.Warehouses.GetType());
		}

		#endregion
	}
}

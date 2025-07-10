using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	internal class WhsInvoiceLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestClients

		public void TestClients()
		{
			var invoice = Factory.New<WhsInvoice>();
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;
			AssertNotNull(invoice.Lookups.Clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), invoice.Lookups.Clients.GetType());
			AssertEquals("Collection should not be loaded", 0, invoice.Lookups.Clients.Count);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			var invoice = Factory.New<WhsInvoice>();
			Factory.New<WhsWarehouse>();
			AssertNotNull(invoice.Lookups.Warehouses);
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), invoice.Lookups.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, invoice.Lookups.Warehouses.Count);
			Env.Security.WhsAllowedWarehouses.IsAllowed = true;
		}

		#endregion

		#region TestOffBandProcessingStatus

		public void TestOffBandProcessingStatus()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertEquals(typeof(StorageOffBandProcessingStatus), invoice.Lookups.OffBandProcessingStatus.GetType());
			AssertEquals("Should have 3 items.", 3, invoice.Lookups.OffBandProcessingStatus.Count);
			AssertEquals("In Queue", invoice.Lookups.OffBandProcessingStatus["QUE"].Description);
			AssertEquals("Has Error", invoice.Lookups.OffBandProcessingStatus["ERR"].Description);
			AssertEquals("Not In Queue", invoice.Lookups.OffBandProcessingStatus["NIQ"].Description);
		}

		#endregion
	}
}

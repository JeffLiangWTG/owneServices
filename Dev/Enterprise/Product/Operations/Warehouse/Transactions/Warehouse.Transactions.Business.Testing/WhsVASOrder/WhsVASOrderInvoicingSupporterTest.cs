using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderInvoicingSupporter))]
	class WhsVASOrderInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestConsumerType()
		{
			var vasOrder = GetNewBusinessObject();
			AssertEquals(JobInvoicingConsumerTypes.WarehouseVASOrder, vasOrder.InvoicingSupporter.ConsumerType);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient();
			var whs = helper.CreateWarehouse("Warehouse");
			var serviceArea = helper.CreateServiceAreaForVASOrder(whs);
			return helper.CreateWhsVASOrder(serviceArea, client);
		}
	}
}

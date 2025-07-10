using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderToBulkUpdateCollectionTest : TestCaseWithFactory
	{
		public void TestAddOrderToBulkUpdate()
		{
			var bo = new OrderDetailsBulkUpdateBusinessObject(Factory);
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			order.JD_OrderNumber = "order";
			order.JD_OrderNumberSplit = 1;
			order.BuyerPK = buyer.PK;

			var orderToBulkUpdate = bo.SelectedOrders.AddOrderToBulkUpdate(order.PK);
			AssertEquals("order", orderToBulkUpdate.OrderNumber);
			AssertEquals((byte)1, orderToBulkUpdate.OrderNumberSplit);
			AssertEquals(buyer.PK, orderToBulkUpdate.BuyerFK);
		}
	}
}

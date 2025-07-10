using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderLineDeliveryCollection))]
	class ImportedOrderLineDeliveryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportedOrderLineDeliveryCollection>
	{
		public void TestConstructorToAddOrderLineDeliveries()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine1 = order.OrderLines.AddNew();
			OrderLine orderLine2 = order.OrderLines.AddNew();
			OrderLineDelivery delivery1 = orderLine1.Deliveries.AddNew();
			OrderLineDelivery delivery2 = orderLine2.Deliveries.AddNew();

			ImportedOrderCollection importedOrders = new ImportedOrderCollection();
			importedOrders.Add(new ImportedOrder(order));

			ImportedOrderLineDeliveryCollection importedDeliveries = new ImportedOrderLineDeliveryCollection(importedOrders);
			AssertEquals(2, importedDeliveries.Count);
			AssertEquals("OrderLineDelivery from 1st order line", orderLine1.JO_LineNoAndSplitAndSubLine, importedDeliveries[0].OrderLine.JO_LineNoAndSplitAndSubLine.Value);
			AssertEquals("OrderLineDelivery from 2nd order line", orderLine2.JO_LineNoAndSplitAndSubLine, importedDeliveries[1].OrderLine.JO_LineNoAndSplitAndSubLine.Value);
		}

		#region Implementation

		protected override ImportedOrderLineDeliveryCollection GetCollectionToTest()
		{
			return new ImportedOrderLineDeliveryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrderLineDelivery delivery = Factory.NewWithValidTestData<OrderLineDelivery>();
			ImportedOrderLine importedOrderLine = new ImportedOrderLine(new ImportedOrder(delivery.Order), delivery.OrderLine);
			ImportedOrderLineDelivery importedDelivery = new ImportedOrderLineDelivery(importedOrderLine, delivery);
			return new ImportedOrderLineDelivery(importedOrderLine, delivery);
		}

		#endregion
	}
}

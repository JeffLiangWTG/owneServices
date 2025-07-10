using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderLineCollection))]
	class ImportedOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportedOrderLineCollection>
	{
		public void TestConstructorToAddOrderLines()
		{
			Order order1 = Factory.New<Order>();
			Order order2 = Factory.New<Order>();
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			OrderLine orderLine2 = order2.OrderLines.AddNew();

			orderLine1.JO_Quantity = 1;
			orderLine2.JO_Quantity = 2;

			ImportedOrderCollection importedOrders = new ImportedOrderCollection();
			importedOrders.Add(new ImportedOrder(order1));
			importedOrders.Add(new ImportedOrder(order2));

			ImportedOrderLineCollection importedOrderLines = new ImportedOrderLineCollection(importedOrders);
			AssertEquals(2, importedOrderLines.Count);
			AssertEquals("OrderLine from 1st order", 1m, importedOrderLines[0].JO_Quantity.Value);
			AssertEquals("OrderLine from 2nd order", 2m, importedOrderLines[1].JO_Quantity.Value);
		}

		#region Implementation

		protected override ImportedOrderLineCollection GetCollectionToTest()
		{
			return new ImportedOrderLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrderLine orderLine = Factory.NewWithValidTestData<OrderLine>();
			return new ImportedOrderLine(new ImportedOrder(orderLine.Order), orderLine);
		}

		#endregion
	}
}

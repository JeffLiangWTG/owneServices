using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLineWorkCollection))]
	internal class WhsOrderLineWorkCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsOrderLineWorkCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Order.Lines.AddNew();
		}

		public void TestRelationshipFilter()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var adjustmentLine = adjustment.Lines.AddNew();

			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();

			var orderLines = new WhsOrderLineWorkCollection(Factory);
			orderLines.Load();
			AssertEquals("WhsOrderLineWorkCollection should load WhsDocketLine with type ORD.", true, orderLines.Contains(orderLine));
			AssertEquals("WhsOrderLineWorkCollection should not load WhsDocketLine with type ADJ.", false, orderLines.Contains(adjustmentLine));
		}

		WhsOrder Order
		{
			get { return order ?? (order = Factory.New<WhsOrder>()); }
		}

		WhsOrder order;
	}
}

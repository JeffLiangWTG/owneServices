using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderDeliveryLine))]
	sealed class OrderDeliveryLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReceiveAllIfNonReceived()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order = preAdvice.Orders.AddNew();
			order.JD_OrderNumber = "1";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 11;
			orderLine.JO_Quantity = 100m;

			OrderDeliveryLine dlvLine = preAdvice.OrderLines.AddNew();
			dlvLine.ReceiveAllIfNonReceived();

			dlvLine.OrderNumber = "1";
			dlvLine.OrderLineNumber = "11";

			AssertEquals("Precondition", 0m, dlvLine.Line.JO_QtyReceived);

			dlvLine.ReceiveAllIfNonReceived();
			AssertEquals(100m, dlvLine.Line.JO_QtyReceived);
		}

		public void TestOrderAndLineNumber()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;
			OrderLine order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_LineNo = 12;

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "2";
			OrderLine order2Line1 = order2.OrderLines.AddNew();
			order2Line1.JO_LineNo = 21;

			Order order3 = preAdvice.Orders.AddNew();
			order3.JD_OrderNumber = "3";
			OrderLine order3Line1 = order3.OrderLines.AddNew();
			order3Line1.JO_LineNo = 31;
			OrderLine order3Line2 = order3.OrderLines.AddNew();
			order3Line2.JO_LineNo = 32;

			Order order3Split1 = order3.SplitOrder(CreateOrderType.Split);
			OrderLine order3Line3 = order3Split1.OrderLines.AddNew();
			order3Line3.JO_LineNo = 33;

			preAdvice.Orders.Add(order3Split1);

			OrderDeliveryLine dlvLine = preAdvice.OrderLines.AddNew();
			AssertNull(dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "4";
			AssertNull(dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "1";
			AssertEquals(order1, dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderLineNumber = "12";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(order1Line2, dlvLine.Line);

			dlvLine.OrderLineNumber = "34";
			AssertEquals(order1, dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "2";
			AssertNull(dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "3";
			dlvLine.OrderLineNumber = "31";
			AssertEquals(order3, dlvLine.Order);
			AssertEquals(order3Line1, dlvLine.Line);

			dlvLine.OrderNumber = "3";
			dlvLine.OrderNumberSplit = 1;
			dlvLine.OrderLineNumber = "33";
			AssertEquals(order3Split1, dlvLine.Order);
			AssertEquals(order3Line3, dlvLine.Line);
		}

		public void TestOrderAndLine_WhenOrderLineNumberIsOutOfShortRange()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();

			var order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";

			var line1 = order1.OrderLines.AddNew();
			line1.JO_LineNo = 12;
			var line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 32767;
			var line3 = order1.OrderLines.AddNew();
			line3.JO_LineNo = 32768;
			var line4 = order1.OrderLines.AddNew();
			line4.JO_LineNo = 50000;
			var line5 = order1.OrderLines.AddNew();
			line5.JO_LineNo = 60000;

			var dlvLine = preAdvice.OrderLines.AddNew();
			AssertNull(dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "4";
			AssertNull(dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderNumber = "1";
			AssertEquals(order1, dlvLine.Order);
			AssertNull(dlvLine.Line);

			dlvLine.OrderLineNumber = "12";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(line1, dlvLine.Line);

			dlvLine.OrderLineNumber = "32767";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(line2, dlvLine.Line);

			dlvLine.OrderLineNumber = "32768";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(line3, dlvLine.Line);

			dlvLine.OrderLineNumber = "50000";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(line4, dlvLine.Line);

			dlvLine.OrderLineNumber = "60000";
			AssertEquals(order1, dlvLine.Order);
			AssertEquals(line5, dlvLine.Line);
		}

		public void TestOrderNumberAndSplit()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			OrderDeliveryLine dlvLine = preAdvice.OrderLines.AddNew();

			dlvLine.OrderNumber = "101";
			AssertEquals("101", dlvLine.OrderNumberAndSplit);

			dlvLine.OrderNumberSplit = 0;
			AssertEquals("101", dlvLine.OrderNumberAndSplit);

			dlvLine.OrderNumberSplit = 7;
			AssertEquals("101-7", dlvLine.OrderNumberAndSplit);
		}

		public void TestOrderNumberMaxLength()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var deliveryLine = preAdvice.OrderLines.AddNew();

			AssertEquals("OrderNumber max length", AutoJobOrderHeader.Schema.JD_OrderNumberMaxLength, deliveryLine.OrderNumberInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			return new OrderDeliveryLine(preAdvice);
		}
	}
}

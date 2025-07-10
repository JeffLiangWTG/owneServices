using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderDeliveryLineCollection))]
	sealed class OrderDeliveryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrderDeliveryLineCollection>
	{
		#region Sort

		public void TestSort()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "3";
			OrderLine line1 = order1.OrderLines.AddNew();
			line1.JO_LineNo = 4;

			OrderLine line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 2;

			OrderLine line3 = order1.OrderLines.AddNew();
			line3.JO_LineNo = 3;

			Order order2 = preAdvice.Orders.AddNew();
			order2.JD_OrderNumber = "2";
			OrderLine line4 = order2.OrderLines.AddNew();
			line4.JO_LineNo = 1;

			OrderLine line5 = order2.OrderLines.AddNew();
			line5.JO_LineNo = 3;

			OrderLine line6 = order2.OrderLines.AddNew();
			line6.JO_LineNo = 2;

			preAdvice.OrderNumberList[0].Value = true;
			preAdvice.OrderNumberList[1].Value = true;
			preAdvice.OrderLines.SortByOrderNumberAndLineNumber();

			AssertEquals("2", preAdvice.OrderLines[0].OrderNumber);
			AssertEquals("1", preAdvice.OrderLines[0].OrderLineNumber);

			AssertEquals("2", preAdvice.OrderLines[1].OrderNumber);
			AssertEquals("2", preAdvice.OrderLines[1].OrderLineNumber);

			AssertEquals("2", preAdvice.OrderLines[2].OrderNumber);
			AssertEquals("3", preAdvice.OrderLines[2].OrderLineNumber);

			AssertEquals("3", preAdvice.OrderLines[3].OrderNumber);
			AssertEquals("2", preAdvice.OrderLines[3].OrderLineNumber);

			AssertEquals("3", preAdvice.OrderLines[4].OrderNumber);
			AssertEquals("3", preAdvice.OrderLines[4].OrderLineNumber);

			AssertEquals("3", preAdvice.OrderLines[5].OrderNumber);
			AssertEquals("4", preAdvice.OrderLines[5].OrderLineNumber);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			order1.JD_RN_NKCountryOfSupply = "NZ";
			order1.JD_InvoiceNumber = "991122";

			OrderLine line1 = order1.OrderLines.AddNew();
			line1.JO_LineNo = 2;

			OrderLine line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 3;

			OrderLine line3 = order1.OrderLines.AddNew();
			line3.JO_LineNo = 4;

			OrderDeliveryLineCollection lines = new OrderDeliveryLineCollection(preAdvice);

			OrderDeliveryLine dlvLine = lines.AddNew();
			AssertEquals("", dlvLine.OrderNumber);
			AssertEquals("", dlvLine.OrderLineNumber);

			dlvLine.OrderNumber = "1";
			dlvLine.OrderLineNumber = "1";

			OrderDeliveryLine dlvLine2 = lines.AddNew();
			AssertEquals("1", dlvLine2.OrderNumber);
			AssertEquals("2", dlvLine2.OrderLineNumber);
			AssertEquals("", dlvLine2.Line.JO_RN_NKCountryOfOrigin);
			AssertEquals("", dlvLine2.Line.JO_CommercialInvoiceNo);
			dlvLine2.Line.JO_ContainerNumber = "12345";
			dlvLine2.Line.JO_ContainerPackingOrder = 0;
			dlvLine2.Line.JO_RN_NKCountryOfOrigin = "JP";
			dlvLine2.Line.JO_CommercialInvoiceNo = "3535";

			OrderDeliveryLine dlvLine3 = lines.AddNew();
			AssertEquals("1", dlvLine3.OrderNumber);
			AssertEquals("3", dlvLine3.OrderLineNumber);
			AssertEquals("12345", dlvLine3.Line.JO_ContainerNumber);
			AssertEquals("JP", dlvLine3.Line.JO_RN_NKCountryOfOrigin);
			AssertEquals("3535", dlvLine3.Line.JO_CommercialInvoiceNo);
			AssertEquals(0, dlvLine3.Line.JO_ContainerPackingOrder);

			dlvLine3.Line.JO_ContainerPackingOrder = 1;

			OrderDeliveryLine dlvLine4 = lines.AddNew();
			AssertEquals(2, dlvLine4.Line.JO_ContainerPackingOrder);
			AssertEquals("JP", dlvLine4.Line.JO_RN_NKCountryOfOrigin);
			AssertEquals("3535", dlvLine4.Line.JO_CommercialInvoiceNo);
		}

		public void TestDefaultValues_WhenOrderLineNumberIsOutOfShortRange()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";

			var lines = new OrderDeliveryLineCollection(preAdvice);

			var dlvLine = lines.AddNew();
			AssertEquals("", dlvLine.OrderNumber);
			AssertEquals("", dlvLine.OrderLineNumber);

			dlvLine.OrderNumber = "1";
			dlvLine.OrderLineNumber = "32767";

			var dlvLine2 = lines.AddNew();
			AssertEquals("1", dlvLine2.OrderNumber);
			AssertEquals("32768", dlvLine2.OrderLineNumber);

			var dlvLine3 = lines.AddNew();
			AssertEquals("1", dlvLine3.OrderNumber);
			AssertEquals("32769", dlvLine3.OrderLineNumber);
		}

		#endregion

		#region Load

		public void TestLoad()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;
			Order order2 = preAdvice.Orders.AddNew();
			order2.JD_OrderNumber = "2";
			order2.JD_OrderNumberSplit = 3;
			order2.OrderLines.AddNew();

			AssertEquals(2, preAdvice.OrderNumberList.Count);
			preAdvice.OrderLines.Load();
			AssertEquals(0, preAdvice.OrderLines.Count);

			preAdvice.OrderNumberList[0].Value = true;
			AssertEquals(1, preAdvice.OrderLines.Count);
			AssertEquals("1", preAdvice.OrderLines[0].OrderNumber);
			AssertEquals("11", preAdvice.OrderLines[0].OrderLineNumber);
			AssertEquals(order1Line1, preAdvice.OrderLines[0].Line);

			preAdvice.OrderNumberList[1].Value = true;
			AssertEquals(2, preAdvice.OrderLines.Count);
			AssertEquals((ZByte)3, preAdvice.OrderLines[1].OrderNumberSplit);

			preAdvice.OrderNumberList[0].Value = false;
			preAdvice.OrderNumberList[1].Value = false;
			AssertEquals(0, preAdvice.OrderLines.Count);
		}

		public void TestLoadWithCanceledLines()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;

			Order order2 = preAdvice.Orders.AddNew();
			order2.JD_OrderNumber = "2";
			OrderLine order2Line1 = order2.OrderLines.AddNew();
			order2Line1.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;

			OrderLine order2Line2 = order2.OrderLines.AddNew();

			preAdvice.OrderLines.Load();

			preAdvice.OrderNumberList[0].Value = true;
			preAdvice.OrderNumberList[1].Value = true;

			AssertEquals("1 order line in the 1st order", 1, order1.OrderLines.Count);
			AssertEquals("2 order lines in second one", 2, order2.OrderLines.Count);
			AssertEquals("2 order lines in pre advice, because 1 is canceled", 2, preAdvice.OrderLines.Count);
		}

		#endregion

		#region Implementation

		protected override OrderDeliveryLineCollection GetCollectionToTest()
		{
			return new OrderDeliveryLineCollection(PreAdvice);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrderDeliveryLine(PreAdvice);
		}

		JobShipmentPreplanning PreAdvice
		{
			get { return fPreAdvice ?? (fPreAdvice = Factory.New<JobShipmentPreplanning>()); }
		}

		JobShipmentPreplanning fPreAdvice;

		#endregion
	}
}

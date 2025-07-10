using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineCollectionTest : TestCaseWithFactory
	{
		public void TestAreAllOrderLinesEmpty()
		{
			Order order = Factory.New<Order>();
			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();
			Assert(order.OrderLines.AreAllOrderLinesEmpty);

			line1.JO_Quantity = 100;
			line2.JO_Quantity = 200;
			Assert(order.OrderLines.AreAllOrderLinesEmpty);

			line1.JO_QtyInvoiced = 10;
			line1.JO_QtyReceived = 0;
			Assert(order.OrderLines.AreAllOrderLinesEmpty);

			line2.JO_QtyReceived = 20;
			Assert(!order.OrderLines.AreAllOrderLinesEmpty);

			OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!order.OrderLines.AreAllOrderLinesEmpty);

			line1.JO_QtyInvoiced = 0;
			Assert(order.OrderLines.AreAllOrderLinesEmpty);

			line1.JO_QtyInvoiced = 100;
			line2.JO_QtyReceived = 0;
			Assert(!order.OrderLines.AreAllOrderLinesEmpty);
		}

		public void TestIsOrderPartiallyComplete()
		{
			Order order = Factory.New<Order>();
			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			line1.JO_Quantity = 100;
			line2.JO_Quantity = 200;
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			line1.JO_QtyInvoiced = 10;
			line1.JO_QtyReceived = 0;
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			line2.JO_QtyReceived = 200;
			line1.JO_QtyInvoiced = 100;
			line1.JO_QtyReceived = 0;
			Assert(order.OrderLines.IsOrderPartiallyComplete);

			line2.JO_QtyInvoiced = 100;
			line1.JO_QtyReceived = 100;
			Assert(order.OrderLines.IsOrderPartiallyComplete);

			line2.JO_QtyInvoiced = 200;
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			line2.JO_QtyInvoiced = 100;
			Assert(order.OrderLines.IsOrderPartiallyComplete);

			line1.JO_QtyReceived = 50;
			line2.JO_QtyReceived = 50;
			line2.JO_QtyInvoiced = 200;
			Assert(!order.OrderLines.IsOrderPartiallyComplete);

			line1.JO_QtyInvoiced = 50;
			Assert(order.OrderLines.IsOrderPartiallyComplete);
		}

		public void TestFindOrderline()
		{
			OrderLine newOrderline = MainOrder.OrderLines.AddNew();
			newOrderline.JO_LineNo = 100;

			OrderLine foundOrderline = MainOrder.OrderLines.Find(999);
			AssertNull("should be null because it does not exists", foundOrderline);
			foundOrderline = MainOrder.OrderLines.Find(100);
			AssertNotNull("found it", foundOrderline);
			Assert("should be the right orderline", foundOrderline.JO_LineNo == 100);
		}

		public void TestFreezeSortOnGridCollectionElementModify()
		{
			AssertEquals(
				"For convenience of re-ordering order lines, FreezeSortOnGridCollectionElementModify is false for order line grids",
				false, FreezeSortOnGridCollectionElementModifyAttribute.IsEnabled(new OrderLineCollection(Factory)));
		}

		public void TestHasOrHadOrderLinesWithQuantityReceived()
		{
			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			Order order = preadvice.Orders.AddNew();
			AssertEquals("No order lines initially", false, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());

			OrderLine orderLine = order.OrderLines.AddNew();
			AssertEquals("No order lines with quantity received", false, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());

			orderLine.JO_QtyReceived = 2m;
			AssertEquals("When order line has quantity received", true, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());

			Factory.Save();
			orderLine.JO_QtyReceived = 0m;
			Factory.Save();
			AssertEquals("When order line had quantity received", true, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());
		}

		public void TestStatusDefaultedOnCopy()
		{
			CopiedOrder = (Order)MainOrder.TemplateCopy();

			AssertEquals("Order Status Defaulted on Copy", Constants.OrderStatus.Incomplete, CopiedOrder.JD_OrderStatus);
			AssertEquals("Order Line Status Defaulted on Copy", Constants.OrderStatus.Incomplete, CopiedOrder.OrderLines[0].JO_LineStatus);
		}

		public void TestQuantitiesClearedOnCopy()
		{
			CopiedOrder = (Order)MainOrder.TemplateCopy();

			AssertEquals("Qty Ordered Defaulted on Copy", 30m, CopiedOrder.OrderLines[0].JO_Quantity);
			AssertEquals("Qty Received Cleared on Copy", 0m, CopiedOrder.OrderLines[0].JO_QtyReceived);
			AssertEquals("Qty Invoiced Defaulted on Copy", 0m, CopiedOrder.OrderLines[0].JO_QtyInvoiced);
		}

		public void TestCommercialInvoiceClearedOnCopy()
		{
			CopiedOrder = (Order)MainOrder.TemplateCopy();

			AssertEquals("CommercialInvoiceNo should be empty", ZString.Empty, CopiedOrder.OrderLines[0].JO_CommercialInvoiceNo);
		}

		public void TestDeliveriesAddToCopiedCollection()
		{
			OrderLineDelivery delivery = MainOrder.OrderLines[0].Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";

			Order newOrder = Factory.New<Order>();

			MainOrder.OrderLines.AddToCopiedCollection(newOrder.OrderLines);

			AssertEquals(1, newOrder.OrderLines.Count);
			AssertEquals(1, newOrder.OrderLines[0].Deliveries.Count);
			AssertEquals("AUSYD", newOrder.OrderLines[0].Deliveries[0].J4_RL_NKDestinationPort);
		}

		public void TestManufacturerIsCopied()
		{
			var newOrderline = MainOrder.OrderLines.AddNew();
			var manufacturer = newOrderline.ManufacturerNameOrPK;

			var newOrder = Factory.New<Order>();
			MainOrder.OrderLines.AddToCopiedCollection(newOrder.OrderLines);

			AssertEquals(manufacturer, newOrder.OrderLines[0].ManufacturerNameOrPK);
		}

		public void TestDatesClearedOnCopy()
		{
			CopiedOrder = (Order)MainOrder.TemplateCopy();

			AssertEquals("OrderLine Dates Cleared on Copy", ZDateTime.Empty, CopiedOrder.OrderLines[0].JO_LineDropDate);
		}

		public void TestDeleteExportedLine()
		{
			OrdersDataRegistry.Instance.AllowExportedOrderLinesToBeDeleted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Order order = Factory.New<Order>();
			order.OrderLines.ExporterOrderLineDeleteAttempt += new EventHandler(OrderLines_ExporterOrderLineDeleteAttempt);
			OrderLine line = order.OrderLines.AddNew();

			Order orderWithDexEvent = Factory.New<Order>();
			orderWithDexEvent.OrderLines.ExporterOrderLineDeleteAttempt += new EventHandler(OrderLines_ExporterOrderLineDeleteAttempt);
			OrderLine lineWithDexEvent = orderWithDexEvent.OrderLines.AddNew();
			orderWithDexEvent.Logs.AddNew(Events.DataExport, "Testing Only");

			Order otherOrderWithDexEvent = Factory.New<Order>();
			otherOrderWithDexEvent.OrderLines.ExporterOrderLineDeleteAttempt += new EventHandler(OrderLines_ExporterOrderLineDeleteAttempt);
			OrderLine otherLineWithDexEvent = otherOrderWithDexEvent.OrderLines.AddNew();
			otherOrderWithDexEvent.Logs.AddNew(Events.DataExport, "Testing Only");

			order.OrderLines.Delete(line);
			AssertEquals(true, line.IsDeleted);

			orderWithDexEvent.OrderLines.Delete(lineWithDexEvent);
			AssertEquals(false, lineWithDexEvent.IsDeleted);
			AssertEquals(true, AttemptDeleteEventFired);
			AttemptDeleteEventFired = false;

			OrdersDataRegistry.Instance.AllowExportedOrderLinesToBeDeleted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			otherOrderWithDexEvent.OrderLines.Delete(otherLineWithDexEvent);
			AssertEquals(true, otherLineWithDexEvent.IsDeleted);
			AssertEquals(false, AttemptDeleteEventFired);
		}

		void OrderLines_ExporterOrderLineDeleteAttempt(object sender, EventArgs e)
		{
			AttemptDeleteEventFired = true;
		}

		bool AttemptDeleteEventFired;

		public void TestFindBoxListProvider()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "aab";
			order1.BuyerPK = org.PK;
			OrderLine line11 = order1.OrderLines.AddNew();
			OrderLine line12 = order1.OrderLines.AddNew();
			OrderLine line13 = order1.OrderLines.AddNew();

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "aac";
			order2.BuyerPK = org.PK;
			OrderLine line21 = order2.OrderLines.AddNew();
			OrderLine line22 = order2.OrderLines.AddNew();
			OrderLine line23 = order2.OrderLines.AddNew();
			Factory.Save();

			OrderLineCollection coll = new OrderLineCollection(Factory);
			IFindBoxListProvider provider = coll;
			AssertEquals(line11, provider.GetBusinessObjectFromCode("aab - 1"));
			AssertEquals(line12, provider.GetBusinessObjectFromCode("aab - 2"));
			AssertEquals(line21, provider.GetBusinessObjectFromCode("aac - 1"));
			AssertEquals(line23, provider.GetBusinessObjectFromCode("aac - 3"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode("aaf - 3"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode("aab"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode("aab - "));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode("aab - 4"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode(" - 3"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode("aab - f"));
			AssertNull("Order Line", provider.GetBusinessObjectFromCode(" -  - 3"));

			AssertEquals("aab - 1", provider.NearestMatch("aab - 1", false, -1).Item1);
			AssertEquals("aab - 2", provider.NearestMatch("aab - 2", false, -1).Item1);
			AssertEquals("aac - 1", provider.NearestMatch("aac - 1", false, -1).Item1);
			AssertEquals("aac - 3", provider.NearestMatch("aac - 3", false, -1).Item1);
			AssertEquals("aaf - 3", provider.NearestMatch("aaf - 3", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("aab", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("a", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("aa", false, -1).Item1);
			AssertEquals("aac - 1", provider.NearestMatch("aac", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("aab - ", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("aab -", false, -1).Item1);
			AssertEquals("aab - 1", provider.NearestMatch("aab ", false, -1).Item1);
			AssertEquals("aab - 4", provider.NearestMatch("aab - 4", false, -1).Item1);
			AssertEquals(" - 3", provider.NearestMatch(" - 3", false, -1).Item1);
			AssertEquals("aab - f", provider.NearestMatch("aab - f", false, -1).Item1);
		}

		public void TestJO_LineNoWhenCopyOrderWithMultipleLineItems()
		{
			var order1 = Factory.New<Order>();
			var line1 = order1.OrderLines.AddNew();
			line1.JO_LineNo = 1;
			line1.JO_Partno = "NOPRD1";
			line1.JO_Description = "No product record.Just text";
			line1.JO_OuterPacks = 1000m;
			line1.JO_OuterPacksUQ = "PKG";
			line1.JO_Quantity = 1000m;

			var line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 2;
			line2.JO_Partno = "NOPRD2";
			line2.JO_Description = "No product record.Just text2";
			line2.JO_OuterPacks = 1000m;
			line2.JO_OuterPacksUQ = "PKG";
			line2.JO_Quantity = 1000m;

			var order2 = (Order)order1.TemplateCopy();

			AssertEquals("Order2 has 2 order lines.", 2, order2.OrderLines.Count);
			AssertEquals("Line1 shouldn't have any notifications.", 0, order2.OrderLines[0].JO_LineNoInfo.Notifications.Count());
			AssertEquals("Line2 shouldn't have any notifications.", 0, order2.OrderLines[1].JO_LineNoInfo.Notifications.Count());
		}

		#region JO_LineStatus Tests

		void AssertJO_LineStatus(bool registrySettingOn, string orderStatus, ZString expectedLineStatus, string assertMessage)
		{
			AdvOrmFeatureHelper.RunTestWith(registrySettingOn, action: () =>
			{
				var order = Factory.NewWithValidTestData<Order>();
				Factory.Save();

				order.JD_OrderStatus = orderStatus;
				var line = order.OrderLines.AddNew();

				AssertEquals(assertMessage, expectedLineStatus, line.JO_LineStatus);
			});
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsINC_DefaultStatusIsINC()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Incomplete, Constants.OrderStatus.Incomplete,
				"Order Line status is defaulted to Incomplete when registry setting is on and Order Status is Incomplete");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsPLC_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Open, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Placed");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsPRT_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.PartDelivered, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Part Delivered");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsDLV_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Delivered, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Delivered");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsCAN_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Cancelled, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Cancelled");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsSHP_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Shipped, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Delivered");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOn_OrderStatusIsCNF_DefaultStatusIsBlank()
		{
			AssertJO_LineStatus(true, Constants.OrderStatus.Confirmed, ZString.Empty,
				"Order Line status is defaulted to blank when registry setting is on and Order Status is Cancelled");
		}

		public void TestJO_LineStatus_EnableAdvOrmFeatureIsOff_DefaultStatusIsPLC_RegardlessOfOrderStatus()
		{
			var orderStatuses = typeof(Constants.OrderStatus).GetFields();

			foreach (var status in orderStatuses)
			{
				var orderStatus = status.GetValue(null) as string;
				AssertJO_LineStatus(false, orderStatus, Constants.OrderStatus.Open,
				"Order Line status is defaulted to Placed when registry setting is off regardless of the order status");
			}
		}

		#endregion

		#region Implementation

		Order MainOrder;
		Order CopiedOrder;

		protected override void SetUp()
		{
			base.SetUp();
			CreateNewBusinessObject();
		}

		void CreateNewBusinessObject()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			MainOrder = Factory.New<Order>();

			MainOrder.JD_OrderNumber = "x";
			MainOrder.BuyerPK = org.PK;
			MainOrder.SupplierPK = org.PK;
			MainOrder.JD_OrderStatus = Constants.OrderStatus.Delivered;

			OrderLine line = MainOrder.OrderLines.AddNew();
			line.JO_LineStatus = "ZZZ";
			line.JO_LineDropDate = new ZDateTime(2002, 5, 28);
			line.JO_Quantity = 30m;
			line.JO_QtyReceived = 8m;
			line.JO_QtyInvoiced = 11m;
			line.JO_CommercialInvoiceNo = "123";
		}

		#endregion
	}
}

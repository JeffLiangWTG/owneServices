using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class OrderLinesToPackLinesMappingTest : WebControlTest
	{
		[QueryString("Ref=SomeBookingKey")]
		public void TestBookingKey()
		{
			var page = (OrderLinesToPackLinesMappingPageForTest)GetNewControl();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var testBooking = new TrackingBooking(shipment, Factory, page.SiteUser);
			HttpContext.Current.Session["nonLinkedOrders"] = new List<Order> { Factory.NewWithValidTestData<Order>() };
			Factory.Save();

			HttpContext.Current.Session["SomeBookingKey"] = testBooking;
			page.LoadOrCreateDataSource_ForTest();

			AssertEquals(testBooking, page.Booking_ForTest);
		}

		[QueryString("Ref=5c79dcbd-f212-4aca-829f-d76e1ecf3947")]
		public void TestFinish_SavesBookingToSession()
		{
			var page = (OrderLinesToPackLinesMappingPageForTest)GetNewControl();
			var bookingKey = "5c79dcbd-f212-4aca-829f-d76e1ecf3947";
			Factory.NewWithPrimaryKey<ForwardingShipment>(new Guid(bookingKey));
			HttpContext.Current.Session["nonLinkedOrders"] = new List<Order> { Factory.NewWithValidTestData<Order>() };
			Factory.Save();

			page.LoadOrCreateDataSource_ForTest();
			page.Finish_ForTest();

			var savedBooking = (TrackingBooking)HttpContext.Current.Session[bookingKey];
			AssertEquals(page.Booking_ForTest, savedBooking);
			AssertEquals(bookingKey, savedBooking.BookingPK.ToString());
			Assert(page.ZClientScript.IsStartupScriptRegistered(typeof(OrderLinesToPackLinesMappingPageForTest), "ClosePopupScript"));
		}

		public void TestOrderLinesGridHasPartNumberColumn()
		{
			var page = (OrderLinesToPackLinesMappingPageForTest)GetNewControl();
			AssertEquals(page.OrderLinesGridForTest.Columns[1].HeaderText, "Part#");
		}

		[QueryString("Ref=5c79dcbd-f212-4aca-829f-d76e1ecf3947")]
		public void TestFinish_WithSplitOrder_DoesNotSaveShipment()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_Quantity = 200;

			order.OrderLines.Add(orderLine);

			HttpContext.Current.Session["nonLinkedOrders"] = new List<Order> { order };
			var bookingKey = "5c79dcbd-f212-4aca-829f-d76e1ecf3947";
			var shipment = Factory.NewWithPrimaryKey<ForwardingShipment>(new Guid(bookingKey));
			Factory.Save();

			shipment.JS_HouseBill = shipment.JS_HouseBill + '1';
			AssertEquals(shipment.HasChanges, true);

			var page = (OrderLinesToPackLinesMappingPageForTest)GetNewControl();
			page.PageConfirmationsForTest[0].ResponseHolder.Value = "Split";

			page.LoadOrCreateDataSource_ForTest();

			var dataSource = (OrderLineToPackLineConversionHelper)page.DataSource;
			dataSource.OrderLines[0].JO_QtyReceived = 100;

			var originalOrder = dataSource.OriginalOrders.ElementAt(0);
			AssertEquals(originalOrder.HasChanges, true);

			page.Finish_ForTest();

			AssertEquals("Changes to order lines are saved", originalOrder.HasChanges, false);
			AssertEquals("Shipment changes are not saved", shipment.HasChanges, true);
		}

		[QueryString("Ref=5c79dcbd-f212-4aca-829f-d76e1ecf3947")]
		public void TestFinish_WithSplitOrder_DoesNotSaveShipment_ForDoNothingResponseHolder()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_Quantity = 200;

			order.OrderLines.Add(orderLine);

			HttpContext.Current.Session["nonLinkedOrders"] = new List<Order> { order };
			var bookingKey = "5c79dcbd-f212-4aca-829f-d76e1ecf3947";
			var shipment = Factory.NewWithPrimaryKey<ForwardingShipment>(new Guid(bookingKey));
			Factory.Save();

			shipment.JS_HouseBill = shipment.JS_HouseBill + '1';
			AssertEquals(shipment.HasChanges, true);

			var page = (OrderLinesToPackLinesMappingPageForTest)GetNewControl();
			page.PageConfirmationsForTest[0].ResponseHolder.Value = "Do Nothing";

			page.LoadOrCreateDataSource_ForTest();

			var dataSource = (OrderLineToPackLineConversionHelper)page.DataSource;
			dataSource.OrderLines[0].JO_QtyReceived = 50;

			var originalOrder = dataSource.OriginalOrders.ElementAt(0);
			AssertEquals(originalOrder.HasChanges, true);

			page.Finish_ForTest();

			AssertEquals("Changes to order lines are saved", originalOrder.HasChanges, false);
			AssertEquals("Shipment changes are not saved", shipment.HasChanges, true);
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new OrderLinesToPackLinesMappingPageForTest();
		}
	}
}

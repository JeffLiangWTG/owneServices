using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobSupplierBookingLineCollectionTest : TestCaseWithFactory
	{
		public void TestNewSupplierBookingLine_DefaultValues()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var line = booking.SupplierBookingLines.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("The new Supplier Booking Line should have no booked packages", 0m, line.JSL_BookedPackages);
				AssertEquals("The new Supplier Booking Line should have no booked quantity", 0m, line.JSL_BookedQuantity);
				AssertEquals("The new Supplier Booking Line should have no gross weight", 0m, line.JSL_GrossWeight);
				AssertEquals("The new Supplier Booking Line should have no volume", 0m, line.JSL_Volume);
				AssertEquals("The new Supplier Booking Line should not be connected to an order line", Guid.Empty, line.JSL_JO_OrderLine);
				AssertEquals("The new Supplier Booking Line should be connected to the Supplier Booking", booking.PK, line.JSL_JSB_Booking);
			});
		}

		public void TestSupplierBookingLinesWithOrderLine()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			Factory.Save();

			AssertEquals(1, orderLine.SupplierBookingLines.Count);
			AssertEquals(bookingLine, orderLine.SupplierBookingLines[0]);
		}

		public void TestCollectionWithFactoryAsArgument()
		{
			CreateTestBookingLine();
			CreateTestBookingLine();

			Factory.Save();

			var collection = new JobSupplierBookingLineCollection(Factory);

			AssertEquals(2, collection.Count);
		}

		public void TestCollectionWithFactoryAndQueryAsArgument()
		{
			var bookingLine1 = CreateTestBookingLine();
			var bookingLine2 = CreateTestBookingLine();

			bookingLine1.JSL_BookingLineId = "JSL001";
			bookingLine2.JSL_BookingLineId = "JSL002";

			Factory.Save();

			var collection = new JobSupplierBookingLineCollection(Factory, new CargoWise.EntityFramework.ZQuery(JobSupplierBookingLineSchema.JSL_BookingLineId, "JSL002"));

			AssertEquals(1, collection.Count);
			AssertEquals("JSL002", collection[0].JSL_BookingLineId);
		}

		JobSupplierBookingLine CreateTestBookingLine()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			return bookingLine;
		}
	}
}

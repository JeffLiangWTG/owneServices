using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class CFSReceiveLineDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		JobSupplierBooking booking;
		TestErrorLogger logger;
		JobSupplierBookingLine bookingLine1, bookingLine2;

		public void TestBasicFieldMappings()
		{
			var receiptDate1 = new ZDateTime(2023, 10, 10);
			var receiptDate2 = new ZDateTime(2023, 10, 20);
			var receiptDate3 = new ZDateTime(2023, 10, 30);
			var expectDate = new ZDateTime(2023, 10, 9, 13, 0, 0);

			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 0, 0, 0, 0, receiptDate2, receiptDate3);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5);
			var packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 5, 1, 1, 2, receiptDate1);
			var bookingLine = new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine, 5, 1, 1, 2, expectDate, receiptDate3);
			AssertEquals(false, logger.HasWarnings);

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", 1, 1, 1, 2, receiptDate1);
			bookingLine = new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine, 1, 1, 1, 2, expectDate, expectDate);
			AssertEquals("Booked Quantity of 5 is not fully received.", logger.GetWarnings());

			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 0, 0, 0, 0, receiptDate1, receiptDate2);
			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 5, 1, 1, 2, receiptDate3);
			bookingLine = new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine, 5, 1, 1, 2, receiptDate1, new ZDateTime(2023, 10, 29, 13, 0, 0));

			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 0, 0, 0, 0);
			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 5, 1, 1, 2, ZDateTime.Empty);
			AssertExceptionThrown<DataObjectReadFailureException>(
				"Last CFS Receipt Date cannot be empty.",
				() => new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject());

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -1, 1, 1, 2);
			AssertExceptionThrown<DataObjectReadFailureException>(
				"quantity and pack must be of same sign",
				"Both Received Quantity and Received Packs should either be positive or negative value.",
				() => new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject());

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000003", 1, 1, 1, 2);
			AssertExceptionThrown<DataObjectReadFailureException>(
				"SBK line is not found",
				"Supplier Booking Line JSL00000000000000003 cannot be found in the system.",
				() => new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject());

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 6, 1, 1, 2);
			AssertExceptionThrown<DataObjectReadFailureException>(
				"received quantity is less than booked quantity",
				"Total Received Quantity cannot be greater than the Booked Quantity.",
				() => new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestBasicFieldMappings_Adjustment()
		{
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 3, 3, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5, 5, 5, 10, 10);

			var packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -2, -2, -1, -2);
			var bookingLine = new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine, 1, 1, 9, 8);

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", -2, -2, 1, 2);
			bookingLine = new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(bookingLine, 3, 3, 9, 8);

			packLine = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -4, -2, -1, -2);
			AssertExceptionThrown<DataObjectReadFailureException>(
				"any of the totals cannot be negative",
				"Total Received Qty, Total Received Package, Total Received Volume, or Total Received Weight would calculate to a negative value. Please adjust Received Qty, Received Package, Received Volume or Received Weight.",
				() => new CFSReceiveLineDataObjectReader(packLine, booking, logger, Factory).ReadIntoBusinessObject());
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
			booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			CFSReceiveTestHelper.SetBookingCfsAddress(Factory, booking);

			booking.JSB_BookingId = "SB00000001";
			bookingLine1 = booking.SupplierBookingLines.AddNew();
			bookingLine2 = booking.SupplierBookingLines.AddNew();
		}
	}
}

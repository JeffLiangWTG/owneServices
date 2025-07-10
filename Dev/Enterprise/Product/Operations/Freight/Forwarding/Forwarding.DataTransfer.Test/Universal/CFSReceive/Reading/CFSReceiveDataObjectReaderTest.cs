using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class CFSReceiveDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		JobSupplierBooking booking;
		TestErrorLogger logger;
		UniversalShipment shipment;
		DataObjectList<PackingLine> packlines;
		JobSupplierBookingLine bookingLine1, bookingLine2;
		ZDateTime receiptDate1, receiptDate2, receiptDate3;

		public void TestBasicFieldMappings()
		{
			var packLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 1, 1, 1, 2, receiptDate1);
			var packLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", 1, 2, 1, 2, receiptDate1);
			Factory.SaveForTesting();

			packlines.Add(packLine1);
			packlines.Add(packLine2);
			shipment.SetPackingLineCollection(() => packlines);

			new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			var expectDate = new ZDateTime(2023, 10, 9, 13, 0, 0);
			CFSReceiveTestHelper.AssertPackingLine(booking.SupplierBookingLines[0], 1, 1, 1, 2, expectDate, receiptDate3);
			CFSReceiveTestHelper.AssertPackingLine(booking.SupplierBookingLines[1], 1, 2, 1, 2, expectDate, expectDate);
		}

		public void TestBasicFieldMappings_Adjustment()
		{
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 3, 3, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5, 5, 5, 10, 10);
			Factory.SaveForTesting();

			var packLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -1, -1, -1, -2);
			var packLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", -1, -2, -1, -2);
			packlines.Add(packLine1);
			packlines.Add(packLine2);
			shipment.SetPackingLineCollection(() => packlines);

			new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			CFSReceiveTestHelper.AssertPackingLine(booking.SupplierBookingLines[0], 2, 2, 9, 8);
			CFSReceiveTestHelper.AssertPackingLine(booking.SupplierBookingLines[1], 4, 3, 9, 8);
		}

		public void TestCFSReceiveReader_LoadModeNotMatch()
		{
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			Factory.SaveForTesting();
			AssertExceptionThrown<DataObjectReadFailureException>(
				"SBK load mode should be CFS",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestCFSReceiveReader_StatusNotMatch()
		{
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Placed;
			Factory.SaveForTesting();

			AssertExceptionThrown<DataObjectReadFailureException>(
				"SBK status should be APP",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestCFSReceiveReader_EmptyCFSAddress()
		{
			booking.JSB_OA_CFSAddress = Guid.Empty;
			Factory.SaveForTesting();

			AssertExceptionThrown<DataObjectReadFailureException>(
				"SBK CFS Address is required",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestCFSReceiveReader_QuantityAndPackDifferentSign()
		{
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 3, 3, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5, 5, 5, 10, 10);
			Factory.SaveForTesting();

			var packLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -1, 1, -1, -2);
			var packLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", -1, -2, -1, -2);
			packlines.Add(packLine1);
			packlines.Add(packLine2);
			shipment.SetPackingLineCollection(() => packlines);

			AssertExceptionThrown<DataObjectReadFailureException>(
				"Packing line quantity and pack must be of same sign",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestCFSReceiveReader_CalculatedFieldsContainNegative()
		{
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 3, 3, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5, 5, 5, 10, 10);
			Factory.SaveForTesting();

			var packLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", -6, -1, -1, -2);
			var packLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", -1, -2, -1, -2);
			packlines.Add(packLine1);
			packlines.Add(packLine2);
			shipment.SetPackingLineCollection(() => packlines);

			AssertExceptionThrown<DataObjectReadFailureException>(
				"any of the totals cannot be negative",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestCFSReceiveReader_InvalidReceiptTime()
		{
			var packLine1 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000001", 1, 1, 1, 2, ZDateTime.Empty);
			var packLine2 = CFSReceiveTestHelper.CreatePackingLine("JSL00000000000000002", 1, 2, 1, 2);
			packlines.Add(packLine1);
			packlines.Add(packLine2);
			shipment.SetPackingLineCollection(() => packlines);
			Factory.SaveForTesting();

			AssertExceptionThrown<DataObjectReadFailureException>(
				"Last CFS Receipt Date cannot be empty.",
				() => new CFSReceiveDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();

			var order = Factory.NewWithValidTestData<Order>();
			booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "SB00000001";

			var orderLine = order.OrderLines.AddNew();
			bookingLine1 = booking.SupplierBookingLines.AddNew();
			bookingLine2 = booking.SupplierBookingLines.AddNew();
			CFSReceiveTestHelper.SetBookingCfsAddress(Factory, booking);
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;

			receiptDate1 = new ZDateTime(2023, 10, 10);
			receiptDate2 = new ZDateTime(2023, 10, 20);
			receiptDate3 = new ZDateTime(2023, 10, 30);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 0, 0, 0, 0, receiptDate2, receiptDate3);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 5);
			bookingLine1.JSL_JO_OrderLine = orderLine.PK;
			bookingLine2.JSL_JO_OrderLine = orderLine.PK;

			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.CFSReceive, booking.JSB_BookingId);

			packlines = new DataObjectList<PackingLine>();
		}
	}
}

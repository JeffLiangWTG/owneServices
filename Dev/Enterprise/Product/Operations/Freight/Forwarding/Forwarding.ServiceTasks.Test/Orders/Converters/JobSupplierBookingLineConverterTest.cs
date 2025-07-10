using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	class JobSupplierBookingLineConverterTest : TestCaseWithFactory
	{
		public void TestEmptyBookingLine()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.LooseCargo;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;
			var shipment = Factory.New<ForwardingShipment>();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			var orderLine = Factory.New<OrderLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_DispatchedPackages = 1;
			bookingLine.JSL_DispatchedVolume = 1;
			bookingLine.JSL_DispatchedWeight = 1;

			AssertEquals(0, shipment.OuterPackLines.Count);

			new JobSupplierBookingLineConverter().ConvertDispatchedLooseCargoBookingLineToPackLine(shipment, bookingLine);
			AssertEquals("should not new pack line for empty dispatched booking line", 0, shipment.OuterPackLines.Count);

			bookingLine.JSL_DispatchedQuantity = 1;
			new JobSupplierBookingLineConverter().ConvertDispatchedLooseCargoBookingLineToPackLine(shipment, bookingLine);
			AssertEquals("should new pack line for valid dispatched booking line", 1, shipment.OuterPackLines.Count);
		}

		public void TestBasicMapping_LSE_ConvertToShipment()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.LooseCargo;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Shipped;
			var shipment = Factory.New<ForwardingShipment>();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			var orderLine = Factory.New<OrderLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			bookingLine.JSL_DispatchedPackages = 12;
			bookingLine.JSL_DispatchedQuantity = 23.1;
			bookingLine.JSL_DispatchedVolume = 34.1;
			bookingLine.JSL_DispatchedWeight = 45.1;
			bookingLine.JSL_VolumeUnit = Core.Constants.Volume.Litre;
			bookingLine.JSL_GrossWeightUnit = Core.Constants.Weight.Hectograms;
			bookingLine.JSL_F3_NKBookedPackagesUnit = Core.Constants.PkgUnit.Box;
			bookingLine.JSL_RH_NKCommodityCode = "NKCO";
			bookingLine.JSL_MarksAndNumbers = "test marks & numbers";
			bookingLine.JSL_Description = "test booking line description";
			bookingLine.JSL_HarmonisedCode = "HC001";
			bookingLine.JSL_PackHeight = 10;
			bookingLine.JSL_PackLength = 20;
			bookingLine.JSL_PackWidth = 30;
			bookingLine.JSL_PackUnitOfDimension = Core.Constants.Dimension.Feet;

			orderLine.JO_HSCode = "HCODE";
			orderLine.JO_Partno = "PARTNO";
			orderLine.JO_ItemPrice = 2.0;
			orderLine.JO_Description = "test order line desc.";
			orderLine.JO_RN_NKCountryOfOrigin = "CN";
			orderLine.JO_AdditionalInformation = "test additional information.";

			new JobSupplierBookingLineConverter().ConvertDispatchedLooseCargoBookingLineToPackLine(shipment, bookingLine);
			AssertEquals(1, shipment.OuterPackLines.Count);

			var packLine = shipment.OuterPackLines[0];

			AssertEquals(bookingLine.PK, packLine.JL_JSL_BookingLine);
			AssertEquals(34.1m, packLine.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.Litre, packLine.JL_ActualVolumeUQ);
			AssertEquals(45.1m, packLine.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Hectograms, packLine.JL_ActualWeightUQ);
			AssertEquals(12, packLine.JL_PackageCount);
			AssertEquals(Core.Constants.PkgUnit.Box, packLine.JL_F3_NKPackType);
			AssertEquals(46.2m, packLine.JL_LinePrice);
			AssertEquals(23.1m, packLine.Products[0].D2_ProductQuantity);
			AssertEquals("NKCO", packLine.JL_RH_NKCommodityCode);
			AssertEquals("test marks & numbers", packLine.JL_MarksAndNumbers);
			AssertEquals("test booking line description", packLine.JL_Description);
			AssertEquals("HC001", packLine.JL_HarmonisedCode);
			AssertEquals("CN", packLine.JL_RN_NKOrigin);
			AssertEquals("test additional information.", packLine.JL_DetailedDescription);
			AssertEquals(10m, packLine.JL_Height);
			AssertEquals(20m, packLine.JL_Length);
			AssertEquals(30m, packLine.JL_Width);
			AssertEquals(Core.Constants.Dimension.Feet, packLine.JL_UnitOfDimension);
		}
	}
}

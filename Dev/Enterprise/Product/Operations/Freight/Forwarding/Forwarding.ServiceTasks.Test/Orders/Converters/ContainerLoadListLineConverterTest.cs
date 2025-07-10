using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	class ContainerLoadListLineConverterTest : TestCaseWithFactory
	{
		public void TestBasicMapping_ShouldNotLinkToBookingLine()
		{
			var orderLine = Factory.New<OrderLine>();

			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_PackHeight = 10;
			bookingLine.JSL_PackLength = 20;
			bookingLine.JSL_PackWidth = 30;
			bookingLine.JSL_PackUnitOfDimension = Core.Constants.Dimension.Feet;

			var loadList = Factory.New<CYContainerLoadList>();
			var loadListLine = loadList.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			loadListLine.CLL_JC_Container = container.PK;
			loadListLine.CLL_PackedQuantity = 11.2m;
			loadListLine.CLL_Volume = 12.1m;
			loadListLine.CLL_VolumeUnit = Core.Constants.Volume.CubicInches;
			loadListLine.CLL_Weight = 2.3m;
			loadListLine.CLL_WeightUnit = Core.Constants.Weight.Tonnes;
			loadListLine.CLL_Packages = 25;
			loadListLine.CLL_F3_NKPackagesUnit = Core.Constants.PkgUnit.Coil;
			loadListLine.CLL_RH_NKCommodityCode = "XYW";
			loadListLine.CLL_HarmonizedCode = "ARW";
			loadListLine.CLL_ReferenceNumber = "cll ref";
			loadListLine.CLL_Description = "cll description";
			loadListLine.CLL_MarksAndNumbers = "cll marks and numbers";

			orderLine.JO_HSCode = "HCODE";
			orderLine.JO_Partno = "PARTNO";
			orderLine.JO_ItemPrice = 2.0;

			AssertEquals(0, shipment.OuterPackLines.Count);
			new ContainerLoadListLineConverter().ConvertLoadListLineToPackLine(shipment, loadListLine);

			var packLine = shipment.OuterPackLines[0];
			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals(container.PK, packLine.JL_JC);
			AssertEquals(12.1m, packLine.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicInches, packLine.JL_ActualVolumeUQ);
			AssertEquals(2.3m, packLine.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Tonnes, packLine.JL_ActualWeightUQ);
			AssertEquals(25, packLine.JL_PackageCount);
			AssertEquals(Core.Constants.PkgUnit.Coil, packLine.JL_F3_NKPackType);
			AssertEquals("XYW", packLine.JL_RH_NKCommodityCode);
			AssertEquals("ARW", packLine.JL_HarmonisedCode);
			AssertEquals("cll ref", packLine.JL_RefNumber);
			AssertEquals("cll marks and numbers", packLine.JL_MarksAndNumbers);
			AssertEquals("cll description", packLine.JL_Description);
			AssertEquals(22.4m, packLine.JL_LinePrice);
			AssertEquals(11.2m, packLine.Products[0].D2_ProductQuantity);
			AssertEquals(ZGuid.Empty, packLine.JL_JSL_BookingLine);
			AssertEquals(10m, packLine.JL_Height);
			AssertEquals(20m, packLine.JL_Length);
			AssertEquals(30m, packLine.JL_Width);
			AssertEquals(Core.Constants.Dimension.Feet, packLine.JL_UnitOfDimension);
		}
	}
}

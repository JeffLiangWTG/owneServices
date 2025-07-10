using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	internal class OrderShipmentPlanningLineConverterTest : TestCaseWithFactory
	{
		public void TestMarkForDelete()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var planningLine = Factory.New<OrderShipmentPlanningLine>();
			planningLine.OPL_MarkForDelete = true;
			planningLine.OPL_JL_PackLine = packLine.PK;

			CombineAssertions("pre: rerun planning line is mark deleted", () =>
			{
				Assert(planningLine.OPL_MarkForDelete);
				Assert(!planningLine.PackLine.IsDeleted);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);

			Assert("should be removed if rerun planning line is marked deleted", planningLine.PackLine.IsDeleted);
		}

		public void TestUpdateExisting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			orderLine.JO_ItemPrice = 1;
			orderLine.JO_F3_NKPackType = "UNT";
			var bookingLine = Factory.New<JobSupplierBooking>().SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = 12;
			bookingLine.JSL_PackHeight = 10;
			bookingLine.JSL_PackLength = 20;
			bookingLine.JSL_PackWidth = 30;
			bookingLine.JSL_PackUnitOfDimension = Core.Constants.Dimension.Feet;

			var planningLine = Factory.New<OrderShipmentPlanningLine>();
			planningLine.OPL_JL_PackLine = packLine.PK;
			planningLine.OPL_Quantity = 12.3;
			planningLine.OPL_Packages = 13;
			planningLine.OPL_Volume = 14.5;
			planningLine.OPL_Weight = 15.6;
			planningLine.OPL_F3_NKPackagesUnit = "PLT";
			planningLine.OPL_WeightUnit = "KG";
			planningLine.OPL_VolumeUnit = "M3";
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;

			CombineAssertions("pre: non-empty rerun planning line", () =>
			{
				AssertEquals(false, planningLine.OPL_MarkForDelete);
				AssertLessThan(0, planningLine.OPL_Quantity);
				AssertEquals(1, shipment.OuterPackLines.Count);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);

			CombineAssertions(() =>
			{
				AssertEquals(1, shipment.OuterPackLines.Count);

				AssertEquals(13, packLine.JL_PackageCount);
				AssertEquals(12.3m, packLine.JL_LinePrice);
				AssertEquals(14.5m, packLine.JL_ActualVolume);
				AssertEquals(15.6m, packLine.JL_ActualWeight);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals("UNT", packLine.Products[0].D2_ProductUnitOfQty);
				AssertEquals(10m, packLine.JL_Height);
				AssertEquals(20m, packLine.JL_Length);
				AssertEquals(30m, packLine.JL_Width);
				AssertEquals(Core.Constants.Dimension.Feet, packLine.JL_UnitOfDimension);
			});
		}

		public void TestUpdateExistingProduct()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines.AddNew();

			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			orderLine.JO_ItemPrice = 1;
			orderLine.JO_F3_NKPackType = "UNT";
			var product = packLine.Products.AddNew();
			product.D2_JO = orderLine.PK;

			var bookingLine = Factory.New<JobSupplierBooking>().SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = 12;

			var planningLine = Factory.New<OrderShipmentPlanningLine>();
			planningLine.OPL_JL_PackLine = packLine.PK;
			planningLine.OPL_Quantity = 12.3;
			planningLine.OPL_Packages = 13;
			planningLine.OPL_Volume = 14.5;
			planningLine.OPL_Weight = 15.6;
			planningLine.OPL_F3_NKPackagesUnit = "PLT";
			planningLine.OPL_WeightUnit = "KG";
			planningLine.OPL_VolumeUnit = "M3";
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;

			CombineAssertions("pre: non-empty rerun planning line", () =>
			{
				AssertEquals(false, planningLine.OPL_MarkForDelete);
				AssertLessThan(0, planningLine.OPL_Quantity);
				AssertEquals(2, shipment.OuterPackLines.Count);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);
			CombineAssertions(() =>
			{
				AssertEquals(2, shipment.OuterPackLines.Count);

				AssertEquals(13, packLine.JL_PackageCount);
				AssertEquals(12.3m, packLine.JL_LinePrice);
				AssertEquals(14.5m, packLine.JL_ActualVolume);
				AssertEquals(15.6m, packLine.JL_ActualWeight);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals(product.PK, packLine.Products[0].PK);
				AssertEquals("UNT", packLine.Products[0].D2_ProductUnitOfQty);
				AssertEquals(12.3m, packLine.Products[0].D2_ProductQuantity);
			});
		}

		public void TestExistingPackLineShouldBeRemove_IfEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var planningLine = Factory.New<OrderShipmentPlanningLine>();
			planningLine.OPL_JL_PackLine = packLine.PK;

			CombineAssertions("pre: rerun planning line should be empty if quantity is zero", () =>
			{
				AssertEquals(false, planningLine.OPL_MarkForDelete);
				AssertEquals(0m, planningLine.OPL_Quantity);
				AssertEquals(1, shipment.OuterPackLines.Count);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);

			AssertEquals("should delete pack line while rerun planning line is empty", 0, shipment.OuterPackLines.Count);
		}

		public void TestAddNew()
		{
			var orderLine = Factory.New<Order>().OrderLines.AddNew();
			orderLine.JO_Description = "test desc.";
			orderLine.JO_ItemPrice = 1;
			orderLine.JO_F3_NKPackType = "UNT";
			orderLine.JO_AdditionalInformation = "test add info.";

			var bookingLine = Factory.New<JobSupplierBooking>().SupplierBookingLines.AddNew();
			bookingLine.JSL_BookedQuantity = 12;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_RH_NKCommodityCode = "GEN";
			bookingLine.JSL_MarksAndNumbers = "test mark & num.";
			bookingLine.JSL_HarmonisedCode = "HSC";
			bookingLine.JSL_Description = "booking line test desc.";
			bookingLine.JSL_PackHeight = 10;
			bookingLine.JSL_PackLength = 20;
			bookingLine.JSL_PackWidth = 30;
			bookingLine.JSL_PackUnitOfDimension = Core.Constants.Dimension.Feet;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew();
			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			var shipmentPlanning = Factory.New<OrderShipmentPlanning>();
			shipmentPlanning.OPS_RL_NKOrigin = "AUSYD";
			shipmentPlanning.OPS_RL_NKDestination = "SGSIN";
			var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine.OPL_Quantity = 12.3;
			planningLine.OPL_Packages = 13;
			planningLine.OPL_Volume = 14.5;
			planningLine.OPL_Weight = 15.6;
			planningLine.OPL_F3_NKPackagesUnit = "PLT";
			planningLine.OPL_WeightUnit = "KG";
			planningLine.OPL_VolumeUnit = "M3";
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;

			CombineAssertions("pre: non-empty new planning line", () =>
			{
				AssertEquals(false, planningLine.OPL_MarkForDelete);
				AssertLessThan(0, planningLine.OPL_Quantity);
				AssertEquals(0, shipment.OuterPackLines.Count);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);
			var packLine = shipment.OuterPackLines[0];
			CombineAssertions(() =>
			{
				AssertEquals(1, shipment.OuterPackLines.Count);
				AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
				AssertEquals("test mark & num.", packLine.JL_MarksAndNumbers);
				AssertEquals("HSC", packLine.JL_HarmonisedCode);
				AssertEquals("AU", packLine.JL_RN_NKOrigin);
				AssertEquals("booking line test desc.", packLine.JL_Description);
				AssertEquals("test add info.", packLine.JL_DetailedDescription);
				AssertNull(packLine.GetContainer(packLine.CurrentConsol));

				AssertEquals(13, packLine.JL_PackageCount);
				AssertEquals(12.3m, packLine.JL_LinePrice);
				AssertEquals(14.5m, packLine.JL_ActualVolume);
				AssertEquals(15.6m, packLine.JL_ActualWeight);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals(10m, packLine.JL_Height);
				AssertEquals(20m, packLine.JL_Length);
				AssertEquals(30m, packLine.JL_Width);
				AssertEquals(Core.Constants.Dimension.Feet, packLine.JL_UnitOfDimension);
				AssertEquals("UNT", packLine.Products[0].D2_ProductUnitOfQty);
				AssertEquals(12.3m, packLine.Products[0].D2_ProductQuantity);
			});
		}

		public void TestNewPackLineShouldNotBeAdded_IfEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentPlanning = Factory.New<OrderShipmentPlanning>();
			var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();

			CombineAssertions("pre: new planning line should be empty if quantity is zero", () =>
			{
				AssertEquals(false, planningLine.OPL_MarkForDelete);
				AssertEquals(0m, planningLine.OPL_Quantity);
				AssertEquals(0, shipment.OuterPackLines.Count);
			});

			new OrderShipmentPlanningLineConverter().ConvertToPackLine(planningLine, shipment);
			AssertEquals("should not create pack line while planning line is empty", 0, shipment.OuterPackLines.Count);
		}
	}
}

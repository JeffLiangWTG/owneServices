using System.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ForwardersCargoReceiptSummaryBuilderTest : ForwardersCargoReceiptBuilderTest
	{
		public override void TestBuild()
		{
			base.TestBuild();

			var shipment = GetShipmentForTest();
			shipment.JS_OuterPacks = 66;
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods descr";
			shipment.DetailedGoodsDescriptionNoteText = "detailed desc";

			var consigneeDelivery = Factory.New<OrgHeader>();
			consigneeDelivery.OH_FullName = "ConsigneeDelivery";
			consigneeDelivery.OH_RL_NKClosestPort = "NZAKL";
			consigneeDelivery.MainAddress.Address1 = "ConsigneeDelivery Address1";
			consigneeDelivery.MainAddress.Address2 = "ConsigneeDelivery Address2";
			consigneeDelivery.MainAddress.City = "NZAKL";
			consigneeDelivery.MainAddress.Postcode = "1003";
			consigneeDelivery.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDelivery.MainAddress.PK;

			Factory.Save();

			var builder = new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters);
			var fcr = builder.Build();
			AssertType<ForwardersCargoReceiptSummary>(fcr);
			AssertEquals("MarksAndNumbers should come from shipment", shipment.JS_MarksAndNumbers, fcr.MarksAndNumbers);
			AssertEquals("DescriptionOfGoods should come from shipment", "goods descr" + System.Environment.NewLine + "detailed desc", fcr.ParticularFurnishedByShipper);
			AssertEquals("TotalPackages", 66, fcr.TotalPackages);
			AssertEquals("ConsigneeDeliveryAddress", "NZ", fcr.Destination.Country.Code);
			AssertEquals("TotalPackagesUnit", "PLT", fcr.TotalPackagesUnit.Code);
			AssertEquals("TotalWeightKG", 100m, fcr.TotalWeightKG);
			AssertEquals("TotalVolumeM3", 100m, fcr.TotalVolumeM3);
			AssertEquals("TotalOrderLines", 2, fcr.TotalOrderLines);
			AssertEquals("OrderLineNumbers", "ORD_01-1, ORD_02-1", fcr.OrderLineNumbers);
			AssertEquals("ItemNumbers", "orderLine1_PartNo, orderLine2_PartNo", fcr.ItemNumbers);
		}

		public void TestWeightConversion()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 50000;
			packLine1.JL_ActualWeightUQ = "KG";

			var builder = new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters);
			AssertEquals("weight should correctly copy when unit is KG", 50000m, builder.Build().TotalWeightKG);

			packLine1.JL_ActualWeightUQ = "T";
			AssertEquals("weight should correctly convert when unit is not KG", 50000000m, builder.Build().TotalWeightKG);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 50000;
			packLine2.JL_ActualWeightUQ = "KG";
			AssertEquals("weight should correctly add values of different units", 50050000m, builder.Build().TotalWeightKG);
		}

		public void TestVolumeConversion()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualVolume = 50000;
			packLine1.JL_ActualVolumeUQ = "M3";

			var builder = new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters);
			AssertEquals("volume should correctly copy when unit is M3", 50000m, builder.Build().TotalVolumeM3);

			packLine1.JL_ActualVolumeUQ = "L";
			AssertEquals("volume should correctly copy when unit is not M3", 50m, builder.Build().TotalVolumeM3);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 50000;
			packLine2.JL_ActualVolumeUQ = "M3";
			AssertEquals("volume should correctly add vaues of different units", 50050m, builder.Build().TotalVolumeM3);
		}

		public void TestEmptyPartNumbersAreExcluded()
		{
			var shipment = GetShipmentForTest();
			var container = shipment
				.Consols
				.OfType<ForwardingConsol>()
				.FirstOrDefault()
				.Containers
				.OfType<ForwardingContainer>()
				.FirstOrDefault(x => !x.JC_JSB_SupplierBooking.IsDefault);

			var supplierBooking = Factory.Load<JobSupplierBooking>(container.JC_JSB_SupplierBooking);
			var orderLines = supplierBooking.SupplierBookingLines.OfType<JobSupplierBookingLine>().Select(x => x.OrderLine);

			foreach (var line in orderLines)
			{
				line.JO_Partno = string.Empty;
			}

			Factory.Save();
			var builder = new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters);
			AssertEquals("part numbers should be blank", string.Empty, builder.Build().ItemNumbers);

			orderLines.First().JO_Partno = "part 1";
			Factory.Save();
			AssertEquals("part numbers should container one part", "part 1", builder.Build().ItemNumbers);
		}

		ForwardingShipment GetShipmentForTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_MarksAndNumbers = "shipment_MarksAndNumbers";
			shipment.JS_GoodsDescription = "shipment_GoodsDescription";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.Consols.Add(consol);
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();

			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "ORD_01";
			order1.JD_OrderGoodsDescription = "order1_Description";
			order1.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			var orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_Quantity = 50;
			orderLine1.JO_OuterPacks = 30;
			orderLine1.JO_Description = "orderLine1_Description";
			orderLine1.JO_ActualWeight = 50;
			orderLine1.JO_UnitOfWeight = "KG";
			orderLine1.JO_ActualVolume = 50;
			orderLine1.JO_UnitOfVolume = "M3";
			orderLine1.JO_Partno = "orderLine1_PartNo";
			orderLine1.JO_LineNo = 1;
			var bookingLine1 = booking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_BookingLineId = "JSL001";
			bookingLine1.JSL_BookedQuantity = 50;
			bookingLine1.JSL_BookedPackages = 30;
			bookingLine1.JSL_GrossWeight = 50;
			bookingLine1.JSL_GrossWeightUnit = "KG";
			bookingLine1.JSL_Volume = 50;
			bookingLine1.JSL_VolumeUnit = "M3";
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;

			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "ORD_02";
			order2.JD_OrderGoodsDescription = "order2_Description";
			order2.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			var orderLine2 = order2.OrderLines.AddNew();
			orderLine2.JO_Quantity = 50;
			orderLine2.JO_OuterPacks = 30;
			orderLine2.JO_Description = "orderLine2_Description";
			orderLine2.JO_ActualWeight = 50;
			orderLine2.JO_UnitOfWeight = "KG";
			orderLine2.JO_ActualVolume = 50;
			orderLine2.JO_UnitOfVolume = "M3";
			orderLine2.JO_Partno = "orderLine2_PartNo";
			orderLine2.JO_LineNo = 1;
			var bookingLine2 = booking.SupplierBookingLines.AddNew();
			bookingLine2.JSL_BookingLineId = "JSL002";
			bookingLine2.JSL_BookedQuantity = 50;
			bookingLine2.JSL_BookedPackages = 30;
			bookingLine2.JSL_GrossWeight = 50;
			bookingLine2.JSL_GrossWeightUnit = "KG";
			bookingLine2.JSL_Volume = 50;
			bookingLine2.JSL_VolumeUnit = "M3";
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;

			var order3 = Factory.NewWithValidTestData<Order>();
			order3.JD_OrderNumber = "ORD_03";
			order3.JD_OrderGoodsDescription = "order3_Description";
			order3.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			var orderLine3 = order3.OrderLines.AddNew();
			orderLine3.JO_Quantity = 50;
			orderLine3.JO_OuterPacks = 30;
			orderLine3.JO_Description = "orderLine3_Description";
			orderLine3.JO_ActualWeight = 50;
			orderLine3.JO_UnitOfWeight = "KG";
			orderLine3.JO_ActualVolume = 50;
			orderLine3.JO_UnitOfVolume = "M3";
			orderLine3.JO_Partno = "orderLine3_PartNo";
			orderLine3.JO_LineNo = 1;
			var bookingLine3 = booking.SupplierBookingLines.AddNew();
			bookingLine3.JSL_BookingLineId = "JSL003";
			bookingLine3.JSL_BookedQuantity = 50;
			bookingLine3.JSL_BookedPackages = 30;
			bookingLine3.JSL_GrossWeight = 50;
			bookingLine3.JSL_GrossWeightUnit = "KG";
			bookingLine3.JSL_Volume = 50;
			bookingLine3.JSL_VolumeUnit = "M3";
			bookingLine3.JSL_JO_OrderLine = orderLine3.PK;

			var container = consol.Containers.AddNew();
			container.JC_JSB_SupplierBooking = booking.PK;

			var containerLoadList = Factory.NewWithValidTestData<CommonContainerLoadList>();
			var loadListLine1 = containerLoadList.LoadListLines.AddNew();
			loadListLine1.CLL_JC_Container = container.PK;
			loadListLine1.CLL_JSL_BookingLine = bookingLine1.PK;
			loadListLine1.CLL_PackedQuantity = 50;
			loadListLine1.CLL_F3_NKPackagesUnit = "PKG";
			loadListLine1.CLL_Packages = 30;
			loadListLine1.CLL_Weight = 50;
			loadListLine1.CLL_WeightUnit = "KG";
			loadListLine1.CLL_Volume = 50;
			loadListLine1.CLL_VolumeUnit = "M3";

			var loadListLine2 = containerLoadList.LoadListLines.AddNew();
			loadListLine2.CLL_JC_Container = container.PK;
			loadListLine2.CLL_JSL_BookingLine = bookingLine2.PK;
			loadListLine2.CLL_PackedQuantity = 50;
			loadListLine2.CLL_F3_NKPackagesUnit = "PKG";
			loadListLine2.CLL_Packages = 30;
			loadListLine2.CLL_Weight = 50;
			loadListLine2.CLL_WeightUnit = "KG";
			loadListLine2.CLL_Volume = 50;
			loadListLine2.CLL_VolumeUnit = "M3";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_MarksAndNumbers = "packLine1_MarksAndNumbers";
			packLine1.JL_Description = "packLine1_Description";
			packLine1.JL_ActualWeight = 50;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_ActualVolume = 50;
			packLine1.JL_ActualVolumeUQ = "M3";
			packLine1.JL_PackageCount = 30;
			packLine1.SetContainer(consol, container);
			loadListLine1.CLL_JL_PackLine = packLine1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_MarksAndNumbers = "packLine2_MarksAndNumbers";
			packLine2.JL_Description = "packLine2_Description";
			packLine2.JL_ActualWeight = 50;
			packLine2.JL_ActualWeightUQ = "KG";
			packLine2.JL_ActualVolume = 50;
			packLine2.JL_ActualVolumeUQ = "M3";
			packLine2.JL_PackageCount = 30;
			packLine2.SetContainer(consol, container);
			loadListLine2.CLL_JL_PackLine = packLine2.PK;

			return shipment;
		}

		protected override ForwardersCargoReceipt Build(ForwardingShipment shipment)
		{
			return new ForwardersCargoReceiptSummaryBuilder(shipment, Parameters).Build();
		}

		protected override IDocDataObjectParameters Parameters
		{
			get
			{
				var mock = new Moq.Mock<IDocDataObjectParameters>();
				mock.SetupGet(m => m.DataStoreName).Returns("Forwarders Cargo Receipt");
				mock.SetupGet(m => m.DocumentTitle).Returns("Forwarders Cargo Receipt");
				return mock.Object;
			}
		}
	}
}

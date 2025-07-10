using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingBuilderHelperTest : TestCaseWithFactory
	{
		public void TestPackLinesForPopulateQuotedBookingFromOrderWithDifferentConversion()
		{
			var order = SetupAndGetOrder();
			order.OrderLines.AddNew().JO_Description = "desc 1";
			order.OrderLines.AddNew().JO_Description = "desc 2";
			Factory.Save();
			order.JD_JSInfo.ValueChanged += new EventHandler((sender, e) =>
			{
				order.Shipment.OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>((_, oe) => oe.ShouldCreatePacklines = false);
				order.Shipment.CreatePackLinesFromOrderLines(new Order[] { order });
			});

			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals(1, populatedBooking.Booking.OuterPackLines.Count);

			order = SetupAndGetOrder();
			order.OrderLines.AddNew().JO_Description = "desc 1";
			order.OrderLines.AddNew().JO_Description = "desc 2";
			Factory.Save();
			order.JD_JSInfo.ValueChanged += new EventHandler((sender, e) =>
			{
				order.Shipment.OnOrderLineToPackLineConversion += new EventHandler<OrderLineToPackLineConversionEventArgs>((_, oe) => oe.ShouldCreatePacklines = true);
				order.Shipment.CreatePackLinesFromOrderLines(new Order[] { order });
			});
			quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals(2, populatedBooking.Booking.OuterPackLines.Count);
		}

		public void TestPopulateQuotedBookingFromOrder()
		{
			var order = SetupAndGetOrder();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			CombineAssertions("Booking should be populated", () =>
			{
				AssertEquals(order.BuyerPK, populatedBooking.Booking.ConsigneePK);
				AssertEquals(order.SupplierPK, populatedBooking.Booking.ConsignorPK);
				AssertEquals(order.Packs, populatedBooking.Booking.JS_OuterPacks);
				AssertEquals(order.Volume, populatedBooking.Booking.JS_ActualVolume);
				AssertEquals(order.Weight, populatedBooking.Booking.JS_ActualWeight);
				AssertEquals(order.JD_Waybill, populatedBooking.Booking.JS_HouseBill);
				AssertEquals(order.JD_RS_NKServiceLevel_NI, populatedBooking.ServiceLevel);
				AssertEquals(order.JD_RL_NKGoodsAvailableAt, populatedBooking.Origin);
				AssertEquals(order.JD_RL_NKGoodsDeliveredTo, populatedBooking.Destination);
				AssertEquals(order.JD_OH_Carrier, populatedBooking.OH_Carrier);
				AssertEquals(Core.Constants.ContainerModes.FCL, populatedBooking.Mode);
				AssertEquals(order.JD_RL_NKPortOfLoading, populatedBooking.LoadPort);
				AssertEquals(order.JD_RL_NKPortOfDischarge, populatedBooking.DischargePort);
				AssertEquals(order.JD_OrderGoodsDescription, populatedBooking.GoodsDescription);
				AssertEquals(order.ControllingAgentDocAddress.Address1, populatedBooking.ControllingAgentDocumentaryAddress.Address1);
				AssertEquals(order.ControllingCustomerDocAddress.Address1, populatedBooking.ControllingCustomerDocumentaryAddress.Address1);
			}

			);
		}

		public void TestPopulateQuotedBookingFromOrderWithAddress()
		{
			var order = SetupAndGetOrder();
			var supplierAddress1 = order.Supplier.Addresses.AddNew();
			supplierAddress1.OA_OH = order.Supplier.PK;
			supplierAddress1.OA_Address1 = "Supplier Address1";
			supplierAddress1.OA_Language = "EN";
			var supplierAddress2 = order.Supplier.Addresses.AddNew();
			supplierAddress2.OA_OH = order.Supplier.PK;
			supplierAddress2.OA_Address1 = "Supplier Address2";
			supplierAddress2.OA_Language = "EN";
			var buyerAddress1 = order.Buyer.Addresses.AddNew();
			buyerAddress1.OA_OH = order.Buyer.PK;
			buyerAddress1.OA_Address1 = "Buyer Address1";
			buyerAddress1.OA_Language = "EN";
			var buyerAddress2 = order.Buyer.Addresses.AddNew();
			buyerAddress2.OA_OH = order.Buyer.PK;
			buyerAddress2.OA_Address1 = "Buyer Address2";
			buyerAddress2.OA_Language = "EN";
			order.JD_OA_BuyerAddress = buyerAddress2.PK;
			order.JD_OA_SupplierAddress = supplierAddress1.PK;
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			CombineAssertions("Address of Booking should be same as the ones of Order", () =>
			{
				AssertEquals(order.JD_OA_BuyerAddress, populatedBooking.Booking.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals(order.GoodsDeliveredToAddress.Address.PK, populatedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals(order.JD_OA_SupplierAddress, populatedBooking.Booking.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(order.GoodsAvailableAtAddress.Address.PK, populatedBooking.Booking.ConsignorPickupAddress.E2_OA_Address);
			}

			);
		}

		public void TestPopulateQuotedBookingFromOrder_NotOverrideOriginAndPort_WhenAddressChanged()
		{
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order);
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals("Booking's Origin is copied from Goods Available at on Order", order.JD_RL_NKGoodsAvailableAt, populatedBooking.Origin);
			AssertEquals("Booking's Loading Port is copied from Loading Port on Order", order.JD_RL_NKPortOfLoading, populatedBooking.LoadPort);
			AssertEquals("Booking's Discharge Port is copied from Discharge Port on Order", order.JD_RL_NKPortOfDischarge, populatedBooking.DischargePort);
		}

		public void TestPopulateQuotedBookingFromOrderWithMultipleSupplierAddresses()
		{
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order);
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals("Consignor's address copied from Order", order.JD_OA_SupplierAddress, populatedBooking.ConsignorDocumentaryAddress.E2_OA_Address);
			AssertEquals("Booking's consignor pickup address is defaulted from order GoodsAvailableAtAddress' address", order.GoodsAvailableAtAddress.Address.PK, populatedBooking.Booking.ConsignorPickupAddress.E2_OA_Address);
			AssertEquals("Booking Origin is copied from Goods Available at on Order", order.JD_RL_NKGoodsAvailableAt, populatedBooking.Booking.JS_RL_NKOrigin);
		}

		public void TestPopulateQuotedBookingFromOrderWithMultipleBuyerAddresses()
		{
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order);
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals("Consignee's address copied from Order", order.JD_OA_BuyerAddress, populatedBooking.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertEquals("Booking's consignee delivery address is copied from order buyer's address", order.JD_OA_BuyerAddress, populatedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("Booking Dest is copied from Goods DeliveredTo on Order", order.JD_RL_NKGoodsDeliveredTo, populatedBooking.Booking.JS_RL_NKDestination);
		}

		public void TestPopulateQuotedBookingFromOrder_ServiceLevelIsPopulatedProperly()
		{
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order, true);
			order.JD_RS_NKServiceLevel_NI = "DIR";
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals("Booking service level is populated", "DIR", populatedBooking.ServiceLevel);
		}

		public void TestPopulateQuotedBookingFromOrderWithBuyerLinks()
		{
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order, true);
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			var populatedBooking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, quotedBooking);
			AssertEquals("Consigner's address copied from Order with buyer links", order.JD_OA_SupplierAddress, populatedBooking.ConsignorDocumentaryAddress.E2_OA_Address);
			AssertEquals("Consignee's address copied from Order with buyer links", order.JD_OA_BuyerAddress, populatedBooking.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertEquals("Booking Load Port is copied from Loading Port at on Order (not from buyer link)", order.JD_RL_NKPortOfLoading, populatedBooking.LoadPort);
			AssertEquals("Booking Discharge Port is copied from Discharge Port on Order (not from buyer links)", order.JD_RL_NKPortOfDischarge, populatedBooking.DischargePort);
			AssertEquals("Booking Origin is from order GoodsAvailableAt", order.JD_RL_NKGoodsAvailableAt, populatedBooking.Origin);
			AssertEquals("Booking Destination is from order GoodsDeliveredTo", order.JD_RL_NKGoodsDeliveredTo, populatedBooking.Destination);
		}

		public void TestBookingModePopulation()
		{
			var orderSeaFcl = SetupAndGetOrder();
			var booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderSeaFcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("SEA + FCL => FCL", Core.Constants.ContainerModes.FCL, booking.Mode);
			var orderSeaLcl = SetupAndGetOrder();
			orderSeaLcl.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderSeaLcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("SEA + LCL => LCL", Core.Constants.ContainerModes.LCL, booking.Mode);
			var orderAirLse = SetupAndGetOrder();
			orderAirLse.JD_TransportMode = Core.Constants.TransportModes.Air;
			orderAirLse.JD_ContainerMode = Core.Constants.ContainerModes.Loose;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderAirLse, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("AIR + LSE => LSE", Core.Constants.ContainerModes.Loose, booking.Mode);
			var orderAirUld = SetupAndGetOrder();
			orderAirUld.JD_TransportMode = Core.Constants.TransportModes.Air;
			orderAirUld.JD_ContainerMode = Core.Constants.ContainerModes.ULD;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderAirUld, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("AIR + ULD => ULD", Core.Constants.ContainerModes.ULD, booking.Mode);
			var orderFsaLcl = SetupAndGetOrder();
			orderFsaLcl.JD_TransportMode = Core.Constants.TransportModes.SeaAir;
			orderFsaLcl.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderFsaLcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("FSA + LCL => LCL", Core.Constants.ContainerModes.LCL, booking.Mode);
			var orderFsaFcl = SetupAndGetOrder();
			orderFsaFcl.JD_TransportMode = Core.Constants.TransportModes.SeaAir;
			orderFsaFcl.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderFsaFcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("FSA + FCL => FCL", Core.Constants.ContainerModes.FCL, booking.Mode);
			var orderFasUld = SetupAndGetOrder();
			orderFasUld.JD_TransportMode = Core.Constants.TransportModes.AirSea;
			orderFasUld.JD_ContainerMode = Core.Constants.ContainerModes.ULD;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderFasUld, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("FAS + ULD => ULD", Core.Constants.ContainerModes.ULD, booking.Mode);
			var orderFasLse = SetupAndGetOrder();
			orderFasLse.JD_TransportMode = Core.Constants.TransportModes.AirSea;
			orderFasLse.JD_ContainerMode = Core.Constants.ContainerModes.Loose;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderFasLse, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("FAS + LSE => LSE", Core.Constants.ContainerModes.Loose, booking.Mode);
			var orderRoaFtl = SetupAndGetOrder();
			orderRoaFtl.JD_TransportMode = Core.Constants.TransportModes.Road;
			orderRoaFtl.JD_ContainerMode = Core.Constants.ContainerModes.FTL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderRoaFtl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("ROA + FTL => FTL", Core.Constants.RateMode.FTL, booking.Mode);
			var orderRoaLtl = SetupAndGetOrder();
			orderRoaLtl.JD_TransportMode = Core.Constants.TransportModes.Road;
			orderRoaLtl.JD_ContainerMode = Core.Constants.ContainerModes.LTL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderRoaLtl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("ROA + LTL => LRO", Core.Constants.RateMode.LRO, booking.Mode);
			var orderRaiFcl = SetupAndGetOrder();
			orderRaiFcl.JD_TransportMode = Core.Constants.TransportModes.Rail;
			orderRaiFcl.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderRaiFcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("RAI + FCL => FRA", Core.Constants.RateMode.FRA, booking.Mode);
			var orderRaiLcl = SetupAndGetOrder();
			orderRaiLcl.JD_TransportMode = Core.Constants.TransportModes.Rail;
			orderRaiLcl.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			booking = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(orderRaiLcl, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals("RAI + LCL => LRA", Core.Constants.RateMode.LRA, booking.Mode);
		}

		public void TestConsignorPickupAddressDefaulting()
		{
			var order = SetupAndGetOrder();
			order.GoodsAvailableAtAddress.E2_OA_Address = ZGuid.Empty;

			var booking1 = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals(order.SupplierAddress.PK, booking1.Booking.ConsignorPickupAddress.E2_OA_Address);

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerAddress = order.DocAddresses.FindOrCreateWithDocAddressType(manufacturer.MainAddress.PK, DocAddressType.Manufacturer);
			var booking2 = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals(manufacturerAddress.Address.PK, booking2.Booking.ConsignorPickupAddress.E2_OA_Address);

			order.GoodsAvailableAtAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var booking3 = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals(order.GoodsAvailableAtAddress.Address.PK, booking3.Booking.ConsignorPickupAddress.E2_OA_Address);
		}

		public void TestConsigneeDeliveryAddressDefaulting()
		{
			var order = SetupAndGetOrder();
			order.GoodsDeliveredToAddress.E2_OA_Address = ZGuid.Empty;

			var booking1 = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals(order.BuyerAddress.PK, booking1.Booking.ConsigneeDeliveryAddress.E2_OA_Address);

			order.GoodsDeliveredToAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var booking2 = QuotedBookingBuilderHelper.PopulateQuotedBookingFromOrder(order, QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertEquals(order.GoodsDeliveredToAddress.Address.PK, booking2.Booking.ConsigneeDeliveryAddress.E2_OA_Address);
		}

		Order SetupAndGetOrder()
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.SupplierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_Packs = 3;
			order.JD_ActualVolume = 10;
			order.JD_ActualWeight = 20;
			order.JD_Waybill = "HOUSEBILL";
			order.JD_RS_NKServiceLevel_NI = "STD";
			order.JD_RL_NKPortOfLoading = "LOADP";
			order.JD_RL_NKPortOfDischarge = "DISCP";
			order.JD_OH_Carrier = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order.JD_OrderGoodsDescription = "GOODS";
			order.JD_RL_NKGoodsAvailableAt = "GORGN";
			order.JD_RL_NKGoodsDeliveredTo = "GDEST";
			order.DocAddresses.CreateWithAddressType(DocAddressType.ControllingAgent);
			order.ControllingAgentDocAddress.Address1 = "Whiterun";
			order.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer);
			order.ControllingCustomerDocAddress.Address1 = "Falkreath";
			return order;
		}

		void AddAdditionalAddressInfoToOrder(Order order, bool isAddBuyerLinks = false)
		{
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			var supplier = Factory.New<OrgHeader>();
			var supplierAddressBNE = supplier.Addresses[0];
			supplierAddressBNE.OA_RL_NKRelatedPortCode = "AUBNE";
			var supplierAddressSYD = supplier.Addresses.AddNew();
			supplierAddressSYD.OA_RL_NKRelatedPortCode = "AUSYD";
			var buyer = Factory.New<OrgHeader>();
			var buyerAddressCNSHA = buyer.Addresses[0];
			buyerAddressCNSHA.OA_RL_NKRelatedPortCode = "CNBEI";
			var buyerAddressCNBEI = buyer.Addresses.AddNew();
			buyerAddressCNBEI.OA_RL_NKRelatedPortCode = "CNSHA";
			if (isAddBuyerLinks)
			{
				var link = supplier.BuyerLinks.AddNew(buyer);
				var linkDetails = (OrgSupBuyLinkTrnMode)link.OrgSupBuyLinkTrnModes.First();
				linkDetails.PF_TransportMode = Core.Constants.TransportModes.Sea;
				linkDetails.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
				linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUDRW";
				linkDetails.PF_RL_NKPlaceOfDeliveryPort = "HKHKG";
				linkDetails.PF_RL_NKLoadPort = "AUMEL";
				linkDetails.PF_RL_NKDischargePort = "HKKWL";
				linkDetails.PF_RS_NKDefaultServiceLevel = "ABC";
			}

			order.JD_OA_SupplierAddress = supplierAddressSYD.PK;
			order.JD_OA_BuyerAddress = buyerAddressCNSHA.PK;
			order.JD_RL_NKGoodsAvailableAt = "AUPER";
			order.JD_RL_NKGoodsDeliveredTo = "MYKUL";
			order.JD_RL_NKPortOfLoading = "AUADL";
			order.JD_RL_NKPortOfDischarge = "SGSIN";
		}
	}
}

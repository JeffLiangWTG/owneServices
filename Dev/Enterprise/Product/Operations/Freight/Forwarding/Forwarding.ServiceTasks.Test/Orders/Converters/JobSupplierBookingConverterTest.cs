using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	internal class JobSupplierBookingConverterTest : TestCaseWithFactory
	{
		#region LinkTransportModeMode

		public void TestConvertToShipmentForLoadList_LinkTransportModeMode_GoodsDescription_CY()
		{
			var shipment = TestConvertToShipmentForLoadList_LinkTransportModeMode(Core.Constants.SupplierBookingLoadMode.ContainerYard);

			AssertEquals("should not trigger EnsurePackLineExists while update goods description", 0, shipment.OuterPackLines.Count);
			AssertEquals("Test Goods Description", shipment.JS_GoodsDescription);
		}

		public void TestConvertToShipmentForLoadList_LinkTransportModeMode_GoodsDescription_CFS()
		{
			var shipment = TestConvertToShipmentForLoadList_LinkTransportModeMode(Core.Constants.SupplierBookingLoadMode.ContainerFreightStation);
			AssertEquals("should not trigger EnsurePackLineExists while update goods description", 0, shipment.OuterPackLines.Count);
			AssertEquals("Test Goods Description", shipment.JS_GoodsDescription);
		}

		ForwardingShipment TestConvertToShipmentForLoadList_LinkTransportModeMode(string loadMode, string goodsDescription = "")
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "SGSIN";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "CNSHA";

			var supplierLink = buyer.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = supplier.PK;
			supplierLink.OL_RN_NKImporterCountry = "SG";

			var linkTransportModeMode = supplierLink.OrgSupBuyLinkTrnModes.Single() as OrgSupBuyLinkTrnMode;
			linkTransportModeMode.PF_RL_NKPlaceOfReceivalPort = "CNSHA";
			linkTransportModeMode.PF_RL_NKPlaceOfDeliveryPort = "SGSIN";
			linkTransportModeMode.PF_TransportMode = "ALL";
			linkTransportModeMode.PF_GoodsDescription = "Test Goods Description";

			var order = Factory.New<Order>();
			order.JD_RL_NKGoodsAvailableAt = "CNSHA";
			order.JD_RL_NKGoodsDeliveredTo = "SGSIN";
			order.BuyerPK = buyer.PK;

			var supplierBooking = Factory.New<JobSupplierBooking>();
			supplierBooking.SupplierAddress.E2_OA_Address = supplier.MainAddress.PK;
			supplierBooking.JSB_LoadMode = loadMode;
			supplierBooking.JSB_TransportMode = Core.Constants.TransportModes.Sea;
			supplierBooking.JSB_GoodsDescription = goodsDescription;

			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = order.OrderLines.AddNew().PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			return new JobSupplierBookingConverter().ConvertToShipmentForLoadList(supplierBooking, order, consol);
		}

		#endregion

		#region ConvertToShipmentForBooking

		public void TestConvertToShipmentForBooking_LSE_Default()
		{
			TestConvertToShipmentForBooking_LSE(false);
		}

		public void TestConvertToShipmentForBooking_LSE_WhenBookingHasConsigneeDocumentaryAddress()
		{
			TestConvertToShipmentForBooking_LSE(true);
		}

		void TestConvertToShipmentForBooking_LSE(bool bookingHasConsigneeDocumentary)
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			order.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			orderLine.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			var consol = AddVoyage(builder.BuildConsol("JK001"), new ZDateTime(2023, 1, 2), new ZDateTime(2024, 1, 5));
			var booking = builder.BuildSupplierBooking("JSB001", "INC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = Core.Constants.SupplierBookingLoadMode.LooseCargo,
				[nameof(JobSupplierBooking.JSB_IncoTerm)] = "INX",
				[nameof(JobSupplierBooking.JSB_RL_NKOrigin)] = "THXXX",
				[nameof(JobSupplierBooking.JSB_RL_NKDestination)] = "SGXXX",
				[nameof(JobSupplierBooking.JSB_MarksAndNumbers)] = "test marks & numbers.",
				[nameof(JobSupplierBooking.JSB_GoodsDescription)] = "test SBK goods desc.",
				[nameof(JobSupplierBooking.JSB_DetailedGoodsDescription)] = "test SBK detailed goods desc.",
			});
			booking.JSB_OH_BookingParty = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.JD_RX_NKOrderCurrency = Core.Constants.CurrencyCodes.China;

			builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001", new Dictionary<string, string>
			{
				[nameof(JobSupplierBookingLine.JSL_VolumeUnit)] = Core.Constants.Volume.CubicYards,
				[nameof(JobSupplierBookingLine.JSL_GrossWeightUnit)] = Core.Constants.Weight.Tonnes,
				[nameof(JobSupplierBookingLine.JSL_F3_NKBookedPackagesUnit)] = Core.Constants.PkgUnit.Coil,
				[nameof(JobSupplierBookingLine.JSL_BookedPackages)] = "2",
				[nameof(JobSupplierBookingLine.JSL_BookedQuantity)] = "3",
				[nameof(JobSupplierBookingLine.JSL_Volume)] = "4",
				[nameof(JobSupplierBookingLine.JSL_GrossWeight)] = "5"
			});
			builder.BuildSupplierBookingLine("JSL002", "JSB001", null, "JO001", new Dictionary<string, string>
			{
				[nameof(JobSupplierBookingLine.JSL_VolumeUnit)] = Core.Constants.Volume.CubicFeet,
				[nameof(JobSupplierBookingLine.JSL_GrossWeightUnit)] = Core.Constants.Weight.LongTons,
				[nameof(JobSupplierBookingLine.JSL_F3_NKBookedPackagesUnit)] = Core.Constants.PkgUnit.Case,
				[nameof(JobSupplierBookingLine.JSL_BookedPackages)] = "6",
				[nameof(JobSupplierBookingLine.JSL_BookedQuantity)] = "7",
				[nameof(JobSupplierBookingLine.JSL_Volume)] = "8",
				[nameof(JobSupplierBookingLine.JSL_GrossWeight)] = "9"
			});

			var consigneeDocumentaryOrg = Factory.NewWithValidTestData<OrgHeader>();
			if (bookingHasConsigneeDocumentary)
			{
				booking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentaryOrg.PK;
				booking.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryOrg.MainAddress.PK;
			}

			var shipment = Factory.New<ForwardingShipment>();
			new JobSupplierBookingConverter().ConvertToShipmentForBooking(booking, shipment);

			CombineAssertions(() =>
			{
				AssertEquals(Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.LCL, shipment.JS_PackingMode);
				AssertEquals("INX", shipment.JS_INCO);
				AssertEquals("THXXX", shipment.JS_RL_NKOrigin);
				AssertEquals("SGXXX", shipment.JS_RL_NKDestination);
				AssertEquals(Core.Constants.CurrencyCodes.China, shipment.JS_RX_NKGoodsValueCurr);
				AssertEquals("test marks & numbers.", shipment.JS_MarksAndNumbers);
				AssertEquals("test SBK goods desc.", shipment.JS_GoodsDescription);
				AssertEquals("test SBK detailed goods desc.", shipment.DetailedGoodsDescriptionNoteText);

				AssertEquals(Core.Constants.Volume.CubicMetres, shipment.JS_UnitOfVolume);
				AssertEquals(Core.Constants.Weight.Kilograms, shipment.JS_UnitOfWeight);
				AssertEquals(Core.Constants.PkgUnit.Package, shipment.JS_F3_NKPackType);
				AssertEquals(8, shipment.JS_OuterPacks);
				AssertEquals(14144.422m, shipment.JS_ActualWeight);
				AssertEquals(3.285m, shipment.JS_ActualVolume);
				AssertEquals(10m, shipment.JS_GoodsValue);

				if (bookingHasConsigneeDocumentary)
				{
					AssertEquals(consigneeDocumentaryOrg.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals(order.JD_OA_BuyerAddress, shipment.BuyerDocAddress.E2_OA_Address);
				}
				else
				{
					AssertEquals(order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals(ZGuid.Empty, shipment.BuyerDocAddress.E2_OA_Address);
				}

				AssertEquals(booking.SupplierAddress.Address.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(booking.JSB_OH_BookingParty.ToString(), shipment.BookingPartyDocumentaryAddress.OrganisationNameOrPK);
				AssertEquals(order.GoodsAvailableAtAddress.Address.PK, shipment.ConsignorPickupAddress.E2_OA_Address);
				AssertEquals(order.GoodsDeliveredToAddress.Address.PK, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals(order.NotifyPartyDocAddress.Address.PK, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
				AssertEquals(order.NotifyParty2DocAddress.Address.PK, shipment.NotifyParty2DocumentaryAddress.E2_OA_Address);
				AssertEquals(order.NotifyParty3DocAddress.Address.PK, shipment.NotifyParty3DocumentaryAddress.E2_OA_Address);
				AssertEquals(booking.ControllingCustomerAddress.Address.PK, shipment.ControllingCustomerAddress.E2_OA_Address);
				AssertEquals(order.ControllingAgentDocAddress.Address.PK, shipment.ControllingAgentDocumentaryAddress.E2_OA_Address);
			});
		}

		#endregion

		#region ConvertToShipmentForLoadList

		public void TestConvertToShipmentForLoadList_FieldsMapping_CY_Default()
		{
			TestConvertToShipmentForLoadList_FieldsMapping_CY(false, false);
		}

		public void TestConvertToShipmentForLoadList_FieldsMapping_CY_WhenBookingHasConsigneeDocumentaryAddress_ConsolHasNotDepartureTimeAndArrivalTime()
		{
			TestConvertToShipmentForLoadList_FieldsMapping_CY(true, true);
		}

		void TestConvertToShipmentForLoadList_FieldsMapping_CY(bool bookingHasConsigneeDocumentary, bool hasETDAndETA)
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			order.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			orderLine.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			var consol = builder.BuildConsol("JK001");
			var booking = builder.BuildSupplierBooking("JSB001", "INC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_IncoTerm)] = "INX",
				[nameof(JobSupplierBooking.JSB_RL_NKOrigin)] = "THXXX",
				[nameof(JobSupplierBooking.JSB_RL_NKDestination)] = "SGXXX",
				[nameof(JobSupplierBooking.JSB_GoodsDescription)] = "test SBK goods desc.",
				[nameof(JobSupplierBooking.JSB_DetailedGoodsDescription)] = "test SBK detailed goods desc.",
			});

			var consigneeDocumentaryOrg = Factory.NewWithValidTestData<OrgHeader>();
			if (bookingHasConsigneeDocumentary)
			{
				booking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentaryOrg.PK;
				booking.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryOrg.MainAddress.PK;
			}
			var bookingLine = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");

			if (hasETDAndETA)
			{
				AddVoyage(consol, new ZDateTime(2024, 1, 2), new ZDateTime(2024, 1, 5));
			}

			var shipment = new JobSupplierBookingConverter().ConvertToShipmentForLoadList(booking, order, consol);
			shipment.UpdateShipmentFromSupplierBooking(booking);

			CombineAssertions(() =>
			{
				AssertEquals(shipment.PK, consol.Shipments.Single().PK);
				AssertEquals(Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
				AssertEquals("INX", shipment.JS_INCO);
				AssertEquals("THXXX", shipment.JS_RL_NKOrigin);
				AssertEquals("SGXXX", shipment.JS_RL_NKDestination);
				AssertEquals("test SBK goods desc.", shipment.JS_GoodsDescription);
				AssertEquals("test SBK detailed goods desc.", shipment.DetailedGoodsDescriptionNoteText);

				if (hasETDAndETA)
				{
					AssertEquals(new ZDateTime(2024, 1, 2), shipment.JS_E_DEP);
					AssertEquals(new ZDateTime(2024, 1, 5), shipment.JS_E_ARV);
				}
				else
				{
					AssertEquals(ZDateTime.Empty, shipment.JS_E_DEP);
					AssertEquals(ZDateTime.Empty, shipment.JS_E_ARV);
				}

				if (bookingHasConsigneeDocumentary)
				{
					AssertEquals(consigneeDocumentaryOrg.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals(order.JD_OA_BuyerAddress, shipment.BuyerDocAddress.E2_OA_Address);
				}
				else
				{
					AssertEquals(order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals(ZGuid.Empty, shipment.BuyerDocAddress.E2_OA_Address);
				}

				AssertEquals(booking.SupplierAddress.Address.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(order.GoodsAvailableAtAddress.Address.PK, shipment.ConsignorPickupAddress.E2_OA_Address);
				AssertEquals(order.GoodsDeliveredToAddress.Address.PK, shipment.ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals(order.NotifyPartyDocAddress.Address.PK, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
				AssertEquals(order.NotifyParty2DocAddress.Address.PK, shipment.NotifyParty2DocumentaryAddress.E2_OA_Address);
				AssertEquals(order.NotifyParty3DocAddress.Address.PK, shipment.NotifyParty3DocumentaryAddress.E2_OA_Address);
				AssertEquals(booking.ControllingCustomerAddress.Address.PK, shipment.ControllingCustomerAddress.E2_OA_Address);
				AssertEquals(order.ControllingAgentDocAddress.Address.PK, shipment.ControllingAgentDocumentaryAddress.E2_OA_Address);
			});
		}

		public void TestConvertToShipmentForLoadList_FieldsMapping_CFS_Default()
		{
			TestConvertToShipmentForLoadList_FieldsMapping_CFS(false, false);
		}

		public void TestConvertToShipmentForLoadList_FieldsMapping_CFS_WhenBookingHasConsigneeDocumentaryAddress_ConsolHasNotDepartureTimeAndArrivalTime()
		{
			TestConvertToShipmentForLoadList_FieldsMapping_CFS(true, true);
		}

		void TestConvertToShipmentForLoadList_FieldsMapping_CFS(bool bookingHasConsigneeDocumentary, bool hasETDAndETA)
		{
			var builder = new OrderManagerMockDataBuilder(Factory);
			var order = builder.BuildOrder("JD001", "PLC");
			var orderLine = builder.BuildOrderLine("JO001", "JD001");
			order.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			orderLine.ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			var consol = builder.BuildConsol("JK001");
			var booking = builder.BuildSupplierBooking("JSB001", "INC", new Dictionary<string, string>
			{
				[nameof(JobSupplierBooking.JSB_LoadMode)] = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation,
				[nameof(JobSupplierBooking.JSB_IncoTerm)] = "INX",
				[nameof(JobSupplierBooking.JSB_RL_NKOrigin)] = "THXXX",
				[nameof(JobSupplierBooking.JSB_RL_NKDestination)] = "SGXXX",
				[nameof(JobSupplierBooking.JSB_GoodsDescription)] = "test SBK goods desc.",
				[nameof(JobSupplierBooking.JSB_DetailedGoodsDescription)] = "test SBK detailed goods desc.",
			});

			booking.JSB_OA_CFSAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var consigneeDocumentaryOrg = Factory.NewWithValidTestData<OrgHeader>();
			if (bookingHasConsigneeDocumentary)
			{
				booking.ConsigneeDocumentaryAddress.OrganisationPK = consigneeDocumentaryOrg.PK;
				booking.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryOrg.MainAddress.PK;
			}

			if (hasETDAndETA)
			{
				AddVoyage(consol, new ZDateTime(2024, 1, 2), new ZDateTime(2024, 1, 5));
			}

			var bookingLine = builder.BuildSupplierBookingLine("JSL001", "JSB001", null, "JO001");

			var shipment = new JobSupplierBookingConverter().ConvertToShipmentForLoadList(booking, order, consol);

			CombineAssertions(() =>
			{
				AssertEquals(shipment.PK, consol.Shipments.Single().PK);
				AssertEquals(Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
				AssertEquals("INX", shipment.JS_INCO);
				AssertEquals("THXXX", shipment.JS_RL_NKOrigin);
				AssertEquals("SGXXX", shipment.JS_RL_NKDestination);
				AssertEquals(booking.SupplierAddress.Address.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(ZString.Empty, shipment.JS_GoodsDescription);
				AssertEquals(ZString.Empty, shipment.DetailedGoodsDescriptionNoteText);

				if (hasETDAndETA)
				{
					AssertEquals(new ZDateTime(2024, 1, 2), shipment.JS_E_DEP);
					AssertEquals(new ZDateTime(2024, 1, 5), shipment.JS_E_ARV);
				}
				else
				{
					AssertEquals(ZDateTime.Empty, shipment.JS_E_DEP);
					AssertEquals(ZDateTime.Empty, shipment.JS_E_ARV);
				}

				if (bookingHasConsigneeDocumentary)
				{
					AssertEquals("ConsigneeDocumentaryAddress", consigneeDocumentaryOrg.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals("BuyerDocAddress", order.JD_OA_BuyerAddress, shipment.BuyerDocAddress.E2_OA_Address);
				}
				else
				{
					AssertEquals("ConsigneeDocumentaryAddress", order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					AssertEquals("BuyerDocAddress", ZGuid.Empty, shipment.BuyerDocAddress.E2_OA_Address);
				}

				AssertEquals("JS_OA_ExportReceivingDepot", booking.CFSAddress.PK, shipment.JS_OA_ExportReceivingDepot);
				AssertEquals("ConsignorAddress", booking.SupplierAddress.Address.PK, shipment.ConsignorPickupAddress.E2_OA_Address);
				AssertEquals("NotifyPartyDocumentaryAddress", ZGuid.Empty, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
				AssertEquals("NotifyParty2DocumentaryAddress", ZGuid.Empty, shipment.NotifyParty2DocumentaryAddress.E2_OA_Address);
				AssertEquals("NotifyParty3DocumentaryAddress", ZGuid.Empty, shipment.NotifyParty3DocumentaryAddress.E2_OA_Address);
				AssertEquals("ControllingCustomerAddress", booking.ControllingCustomerAddress.Address.PK, shipment.ControllingCustomerAddress.E2_OA_Address);
				AssertEquals("ControllingAgentDocumentaryAddress", ZGuid.Empty, shipment.ControllingAgentDocumentaryAddress.E2_OA_Address);
			});
		}

		ForwardingConsol AddVoyage(ForwardingConsol consol, ZDateTime etd, ZDateTime eta)
		{
			var mainTransport = consol.Transports.AddNew();
			mainTransport.JW_ETD = etd;
			mainTransport.JW_ETA = eta;

			return consol;
		}
		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	internal class OrderShipmentPlanningConverterTest : TestCaseWithFactory
	{
		public void TestNewShipment()
		{
			var (order, supplierBooking, planningShipment) = PrepareSetup(Factory);
			Factory.Save();

			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;

			CombineAssertions("Basic Information & Addresses", () =>
			{
				AssertNull("planningShipment should be deleted", Factory.Load<OrderShipmentPlanning>(planningShipment.PK));
				AssertEquals(Constants.TransportModes.Air, shipment.JS_TransportMode);
				AssertEquals(Constants.ContainerModes.BuyersConsol, shipment.JS_PackingMode);
				AssertEquals(Constants.ShipmentTypes.StandardHouse, shipment.JS_ShipmentType);
				AssertEquals(Constants.IncoTerms.CostAndFreight, shipment.JS_INCO);
				AssertEquals("CNSZX", shipment.JS_RL_NKOrigin);
				AssertEquals("SGSIN", shipment.JS_RL_NKDestination);
				AssertEquals(new ZDateTime(2024, 7, 8), shipment.JS_E_DEP);
				AssertEquals(new ZDateTime(2024, 7, 9), shipment.JS_E_ARV);
				AssertEquals("test desc", shipment.JS_GoodsDescription);
				AssertEquals("details desc test", shipment.DetailedGoodsDescriptionNoteText);
				AssertEquals("mark & num", shipment.JS_MarksAndNumbers);
				AssertEquals(supplierBooking.SupplierAddress.E2_OA_Address, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals(supplierBooking.ControllingCustomerAddress.E2_OA_Address, shipment.ControllingCustomerAddress.E2_OA_Address);
				AssertEquals(supplierBooking.NotifyPartyDocAddress.E2_OA_Address, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
				AssertEquals(supplierBooking.NotifyParty2DocAddress.E2_OA_Address, shipment.NotifyParty2DocumentaryAddress.E2_OA_Address);
				AssertEquals(supplierBooking.NotifyParty3DocAddress.E2_OA_Address, shipment.NotifyParty3DocumentaryAddress.E2_OA_Address);
				AssertEquals(order.GoodsAvailableAtAddress.E2_OA_Address, shipment.ConsignorPickupAddress.E2_OA_Address);
				AssertEquals(order.GoodsDeliveredToAddress.E2_OA_Address, shipment.ConsigneeDeliveryAddress.E2_OA_Address);

				var shipmentNumber = shipment.Numbers.Find(number => number.CE_EntryType == ShipmentNonCustomsAdditionalReferenceCodesCodeList.Codes.SPT).Single();
				AssertEquals("planning shipment name test", shipmentNumber.CE_EntryNum);
				AssertEquals(string.Empty, shipmentNumber.CE_RN_NKCountryCode);

				AssertEquals(order.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer).E2_OA_Address, shipment.ManufacturerDocAddress.E2_OA_Address);
			});

			AssertEquals(2, shipment.OuterPackLines.Count);
			var packLine = shipment.OuterPackLines.OfType<ForwardingPackLine>().Single(line => line.JL_PackageCount == 13);
			CombineAssertions("Pack Line 1", () =>
			{
				AssertEquals(12m, packLine.JL_ActualVolume);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals(11m, packLine.JL_ActualWeight);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals(13, packLine.JL_PackageCount);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
				AssertEquals("marks 1", packLine.JL_MarksAndNumbers);
				AssertEquals("CN", packLine.JL_RN_NKOrigin);
				AssertEquals("booking line1 desc", packLine.JL_Description);
				AssertEquals(14m, packLine.JL_LinePrice);
				AssertEquals(14m, packLine.Products[0].D2_ProductQuantity);
				AssertEquals(Constants.PkgUnit.Box, packLine.Products[0].D2_ProductUnitOfQty);
			});

			packLine = shipment.OuterPackLines.OfType<ForwardingPackLine>().Single(line => line.JL_PackageCount == 23);
			CombineAssertions("Pack Line 2", () =>
			{
				AssertEquals(22m, packLine.JL_ActualVolume);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals(21m, packLine.JL_ActualWeight);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals(23, packLine.JL_PackageCount);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
				AssertEquals("marks 2", packLine.JL_MarksAndNumbers);
				AssertEquals("CN", packLine.JL_RN_NKOrigin);
				AssertEquals("booking line2 desc", packLine.JL_Description);
				AssertEquals(24m, packLine.JL_LinePrice);
				AssertEquals(24m, packLine.Products[0].D2_ProductQuantity);
				AssertEquals(Constants.PkgUnit.Box, packLine.Products[0].D2_ProductUnitOfQty);
			});

			CombineAssertions("Shipment Totals", () =>
			{
				AssertEquals(36, shipment.JS_OuterPacks);
				AssertEquals(32m, shipment.JS_ActualWeight);
				AssertEquals(34m, shipment.JS_ActualVolume);
				AssertEquals(38m, shipment.JS_GoodsValue);
				AssertEquals("M3", shipment.JS_UnitOfVolume);
				AssertEquals("KG", shipment.JS_UnitOfWeight);
				AssertEquals("PLT", shipment.JS_F3_NKPackType);
			});
		}

		public void TestPlanningShipmentShouldUpdateShipmentWhileGeneratingShipment()
		{
			var (order, supplierBooking, planningShipment) = PrepareSetup(Factory);
			planningShipment.OPS_RL_NKOrigin = "CNSHA";
			planningShipment.OPS_RL_NKDestination = "SGSIN";

			supplierBooking.ConsigneeDocumentaryAddress.Address.OA_RL_NKRelatedPortCode = "AUSYD";
			supplierBooking.SupplierAddress.Address.OA_RL_NKRelatedPortCode = "THBKK";
			var supplierLink = supplierBooking.ConsigneeDocumentaryAddress.Address.Header.SupplierLinks.AddNew(supplierBooking.SupplierAddress.Address.Header);
			var supBuyLinkTrnMode = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			supBuyLinkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Sea;
			supBuyLinkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.Bulk;
			supBuyLinkTrnMode.PF_RL_NKLoadPort = "NZWLG";
			supBuyLinkTrnMode.PF_RL_NKPlaceOfReceivalPort = "NZCHC";
			supBuyLinkTrnMode.PF_RL_NKDischargePort = "AUBNE";
			supBuyLinkTrnMode.PF_RL_NKPlaceOfDeliveryPort = "AUMEL";
			supBuyLinkTrnMode.PF_GoodsDescription = "link desc.";
			supBuyLinkTrnMode.PF_IncoTerm = "DAP";
			Factory.Save();

			CombineAssertions("Prerequisite: planning shipment has been initialized", () =>
			{
				AssertEquals("CFR", supplierBooking.JSB_IncoTerm);

				AssertEquals(Core.Constants.TransportModes.Air, planningShipment.OPS_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.BuyersConsol, planningShipment.OPS_ContainerMode);
				AssertEquals("CNSHA", planningShipment.OPS_RL_NKOrigin);
				AssertEquals("SGSIN", planningShipment.OPS_RL_NKDestination);
				AssertEquals((ZString)"test desc", planningShipment.OPS_GoodsDescription);
			});

			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;
			CombineAssertions("Planning shipment should update shipment", () =>
			{
				AssertEquals(Core.Constants.TransportModes.Air, shipment.JS_TransportMode);
				AssertEquals(Core.Constants.ContainerModes.BuyersConsol, shipment.JS_PackingMode);
				AssertEquals("CNSHA", shipment.JS_RL_NKOrigin);
				AssertEquals("SGSIN", shipment.JS_RL_NKDestination);
				AssertEquals("CFR", shipment.JS_INCO);
				AssertEquals((ZString)"test desc", shipment.JS_GoodsDescription);
			});
		}

		public void TestPlanningShipmentShouldUpdateShipmentWhileGeneratingShipment_SupplierBuyerLinkShouldNotOverride()
		{
			var (order, supplierBooking, planningShipment) = PrepareSetup(Factory);
			planningShipment.OPS_GoodsDescription = "";
			supplierBooking.NotifyPartyDocAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var supplierLink = supplierBooking.ConsigneeDocumentaryAddress.Address.Header.SupplierLinks.AddNew(supplierBooking.SupplierAddress.Address.Header);
			var supBuyLinkTrnMode = supplierLink.OrgSupBuyLinkTrnModes.AddNew();
			supBuyLinkTrnMode.PF_TransportMode = Core.Constants.TransportModes.Air;
			supBuyLinkTrnMode.PF_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			supBuyLinkTrnMode.PF_GoodsDescription = "link desc.";
			supBuyLinkTrnMode.PF_IncoTerm = "DAP";
			supBuyLinkTrnMode.PF_RS_NKDefaultServiceLevel = "XXX";
			supBuyLinkTrnMode.PF_OA_OverrideNotifyPartyAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();

			CombineAssertions("Prerequisite: matched supplier buyer link mode should be initialized", () =>
			{
				AssertEquals((ZString)"", planningShipment.OPS_GoodsDescription);
				AssertEquals((ZString)"link desc.", supBuyLinkTrnMode.PF_GoodsDescription);
				AssertEquals((ZString)"DAP", supBuyLinkTrnMode.PF_IncoTerm);
				Assert(!supBuyLinkTrnMode.PF_OA_OverrideNotifyPartyAddress.IsEmpty);
			});

			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;
			CombineAssertions("matched supplier buyer link should not override the properties of shipment", () =>
			{
				AssertEquals((ZString)"", shipment.JS_GoodsDescription);
				AssertEquals("CFR", shipment.JS_INCO);
				AssertEquals(supplierBooking.NotifyPartyDocAddress.E2_OA_Address, shipment.NotifyPartyDocumentaryAddress.E2_OA_Address);
			});
		}

		public void TestExistingShipment()
		{
			var (order, supplierBooking, planningShipment) = PrepareSetup(Factory);
			Factory.Save();

			var existingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			existingShipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			existingShipment.JS_RL_NKOrigin = "THBKK";
			existingShipment.JS_RL_NKDestination = "AUSYD";

			planningShipment.OPS_JS_Shipment = existingShipment.PK;

			Factory.Save();

			CombineAssertions("pre: rerun OPS with different transport mode, original port and destination port than exiting shipment", () =>
			{
				AssertEquals(Core.Constants.TransportModes.Air, planningShipment.OPS_TransportMode);
				AssertEquals("CNSZX", planningShipment.OPS_RL_NKOrigin);
				AssertEquals("SGSIN", planningShipment.OPS_RL_NKDestination);

				AssertEquals(Core.Constants.TransportModes.Road, existingShipment.JS_TransportMode);
				AssertEquals("THBKK", existingShipment.JS_RL_NKOrigin);
				AssertEquals("AUSYD", existingShipment.JS_RL_NKDestination);
			});
			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = existingShipment;
			CombineAssertions("should not update existing shipment", () =>
			{
				AssertEquals(Core.Constants.TransportModes.Road, existingShipment.JS_TransportMode);
				AssertEquals("THBKK", existingShipment.JS_RL_NKOrigin);
				AssertEquals("AUSYD", existingShipment.JS_RL_NKDestination);
			});

			AssertEquals(2, shipment.OuterPackLines.Count);
			var packLine = shipment.OuterPackLines.OfType<ForwardingPackLine>().Single(line => line.JL_PackageCount == 13);
			CombineAssertions("Pack Line 1", () =>
			{
				AssertEquals(12m, packLine.JL_ActualVolume);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals(11m, packLine.JL_ActualWeight);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals(13, packLine.JL_PackageCount);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
				AssertEquals("marks 1", packLine.JL_MarksAndNumbers);
				AssertEquals("CN", packLine.JL_RN_NKOrigin);
				AssertEquals("booking line1 desc", packLine.JL_Description);
				AssertEquals(14m, packLine.JL_LinePrice);
				AssertEquals(14m, packLine.Products[0].D2_ProductQuantity);
				AssertEquals(Constants.PkgUnit.Box, packLine.Products[0].D2_ProductUnitOfQty);
			});

			packLine = shipment.OuterPackLines.OfType<ForwardingPackLine>().Single(line => line.JL_PackageCount == 23);
			CombineAssertions("Pack Line 2", () =>
			{
				AssertEquals(22m, packLine.JL_ActualVolume);
				AssertEquals("M3", packLine.JL_ActualVolumeUQ);
				AssertEquals(21m, packLine.JL_ActualWeight);
				AssertEquals("KG", packLine.JL_ActualWeightUQ);
				AssertEquals(23, packLine.JL_PackageCount);
				AssertEquals("PLT", packLine.JL_F3_NKPackType);
				AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
				AssertEquals("marks 2", packLine.JL_MarksAndNumbers);
				AssertEquals("CN", packLine.JL_RN_NKOrigin);
				AssertEquals("booking line2 desc", packLine.JL_Description);
				AssertEquals(24m, packLine.JL_LinePrice);
				AssertEquals(24m, packLine.Products[0].D2_ProductQuantity);
				AssertEquals(Constants.PkgUnit.Box, packLine.Products[0].D2_ProductUnitOfQty);
			});

			CombineAssertions("Shipment Totals", () =>
			{
				AssertEquals(36, shipment.JS_OuterPacks);
				AssertEquals(32m, shipment.JS_ActualWeight);
				AssertEquals(34m, shipment.JS_ActualVolume);
				AssertEquals(38m, shipment.JS_GoodsValue);
				AssertEquals("M3", shipment.JS_UnitOfVolume);
				AssertEquals("KG", shipment.JS_UnitOfWeight);
				AssertEquals("PLT", shipment.JS_F3_NKPackType);
			});
		}

		[TestDate(2024, 1, 5)]
		public void TestNewShipment_GoodsValueShouldConvert()
		{
			var (order, supplierBooking, planningShipment) = PrepareSetup(Factory);

			order.JD_RX_NKOrderCurrency = "USD";

			var usdExchangeRate = order.OrderCurrency.ExchangeRates.AddNew();
			usdExchangeRate.RE_StartDate = new DateTime(2024, 1, 1);
			usdExchangeRate.RE_ExpiryDate = new DateTime(2024, 1, 10);
			usdExchangeRate.RE_SellRate = 0.5m;
			usdExchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			Factory.Save();

			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;
			CombineAssertions("Shipment Goods Value & Currency", () =>
			{
				AssertEquals("Should be $76 AUD (converted from $38 USD to the currency of the shipment using the rate provided above)", 76m, shipment.JS_GoodsValue);
				AssertEquals("Default value from current company", "AUD", shipment.JS_RX_NKGoodsValueCurr);
			});
		}

		public void TestConvertToShipment_ForceGeneratesHouseBill()
		{
			var (_, supplierBooking, planningShipment) = PrepareSetup(Factory);
			Factory.Save();

			ConvertShipmentPlanningToShipment(planningShipment);
			Factory.Save();

			var shipment = Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, supplierBooking.SupplierBookingLines[0].PK)).Shipment;
			AssertNotNull(shipment);
			AssertNotNullOrEmpty("HBL# is not empty", shipment.JS_HouseBill);
		}

		#region ConsigneeDocumentaryAddress

		public void TestCopyConsigneeDocumentaryAddress_PreferBookingConsigneeOverOrderConsigneeAndBuyerAddress()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.Contacts.AddNew();

			var order = Factory.New<Order>();
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			order.JD_OC_BuyerContact = buyer.Contacts[0].PK;
			order.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine = order.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;

			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestCopyConsigneeDocumentaryAddress_PreferOrderConsigneeOverBuyerAddress()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.Contacts.AddNew();

			var order = Factory.New<Order>();
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			order.JD_OC_BuyerContact = buyer.Contacts[0].PK;
			order.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine = order.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;
			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(order.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestCopyConsigneeDocumentaryAddress_FromBuyerAddressIfBookingAndOrderConsigneeAbsent()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.Contacts.AddNew();

			var order = Factory.New<Order>();
			order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			order.JD_OC_BuyerContact = buyer.Contacts[0].PK;
			var orderLine = order.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine.OPL_JSL_BookingLine = bookingLine.PK;
			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestConsigneeDocumentaryAddressNotCopied_FromDifferentConsigneeDocumentaryAddressOfOrder()
		{
			var order1 = Factory.New<Order>();
			order1.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine1 = order1.OrderLines.AddNew();

			var order2 = Factory.New<Order>();
			order2.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine2 = order2.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			var bookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine1 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine1.OPL_JSL_BookingLine = bookingLine1.PK;
			var planningLine2 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine2.OPL_JSL_BookingLine = bookingLine2.PK;

			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(ZGuid.Empty, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestCopyConsigneeDocumentaryAddress_FromSameConsigneeDocumentaryAddressOfOrder()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			buyer1.Contacts.AddNew();

			var order1 = Factory.New<Order>();
			order1.ConsigneeDocumentaryAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var orderLine1 = order1.OrderLines.AddNew();

			var order2 = Factory.New<Order>();
			order2.ConsigneeDocumentaryAddress.E2_OA_Address = order1.ConsigneeDocumentaryAddress.E2_OA_Address;
			var orderLine2 = order2.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			var bookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine1 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine1.OPL_JSL_BookingLine = bookingLine1.PK;
			var planningLine2 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine2.OPL_JSL_BookingLine = bookingLine2.PK;

			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(order1.ConsigneeDocumentaryAddress.E2_OA_Address, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestConsigneeDocumentaryAddressNotCopied_FromDifferentBuyersOfOrder()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			buyer1.Contacts.AddNew();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			buyer2.Contacts.AddNew();

			var order1 = Factory.New<Order>();
			order1.JD_OA_BuyerAddress = buyer1.MainAddress.PK;
			order1.JD_OC_BuyerContact = buyer1.Contacts[0].PK;
			var orderLine1 = order1.OrderLines.AddNew();

			var order2 = Factory.New<Order>();
			order2.JD_OA_BuyerAddress = buyer2.MainAddress.PK;
			order2.JD_OC_BuyerContact = buyer2.Contacts[0].PK;
			var orderLine2 = order2.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			var bookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine1 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine1.OPL_JSL_BookingLine = bookingLine1.PK;
			var planningLine2 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine2.OPL_JSL_BookingLine = bookingLine2.PK;

			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(ZGuid.Empty, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestCopyConsigneeDocumentaryAddress_FromSameBuyersOfOrder()
		{
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			buyer1.Contacts.AddNew();

			var order1 = Factory.New<Order>();
			order1.JD_OA_BuyerAddress = buyer1.MainAddress.PK;
			order1.JD_OC_BuyerContact = buyer1.Contacts[0].PK;
			var orderLine1 = order1.OrderLines.AddNew();

			var order2 = Factory.New<Order>();
			order2.JD_OA_BuyerAddress = buyer1.MainAddress.PK;
			order2.JD_OC_BuyerContact = buyer1.Contacts[0].PK;
			var orderLine2 = order2.OrderLines.AddNew();

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var bookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			var bookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine2.JSL_JO_OrderLine = orderLine2.PK;

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			var planningLine1 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine1.OPL_JSL_BookingLine = bookingLine1.PK;
			var planningLine2 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine2.OPL_JSL_BookingLine = bookingLine2.PK;

			var shipment = new OrderShipmentPlanningConverter().ConvertToShipment(shipmentPlanning);

			AssertEquals(buyer1.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		#endregion

		static void ConvertShipmentPlanningToShipment(OrderShipmentPlanning shipmentPlanning)
		{
			new OrderManagerConvertService().ConvertOrderShipmentPlanning(shipmentPlanning.SupplierBooking);
		}

		internal static (Order order, JobSupplierBooking supplierBooking, OrderShipmentPlanning planningShipment) PrepareSetup(BusinessObjectFactory factory)
		{
			var order = OrderHelpers.CreateOrder(factory);
			order.GoodsAvailableAtAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			order.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer).E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_RN_NKCountryOfOrigin = "CN";
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_ItemPrice = 1;
			orderLine1.JO_Partno = "partno1";
			orderLine1.JO_F3_NKPackType = Constants.PkgUnit.Box;
			orderLine1.JO_Description = "ord1 desc";

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_RN_NKCountryOfOrigin = "CN";
			orderLine2.JO_LineNo = 2;
			orderLine2.JO_ItemPrice = 1;
			orderLine2.JO_Partno = "partno2";
			orderLine2.JO_F3_NKPackType = Constants.PkgUnit.Box;
			orderLine2.JO_Description = "ord2 desc";

			var supplierBooking = BuildSupplierBooking(factory);
			supplierBooking.JSB_Status = Constants.SupplierBookingStatus.Approved;
			supplierBooking.JSB_IncoTerm = "CFR";

			var supplierBookingLine1 = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine1.JSL_JO_OrderLine = orderLine1.PK;
			supplierBookingLine1.JSL_GrossWeight = 11;
			supplierBookingLine1.JSL_GrossWeightUnit = "KG";
			supplierBookingLine1.JSL_Volume = 12;
			supplierBookingLine1.JSL_VolumeUnit = "M3";
			supplierBookingLine1.JSL_BookedPackages = 13;
			supplierBookingLine1.JSL_F3_NKBookedPackagesUnit = "PLT";
			supplierBookingLine1.JSL_BookedQuantity = 14;
			supplierBookingLine1.JSL_RH_NKCommodityCode = "GEN";
			supplierBookingLine1.JSL_MarksAndNumbers = "marks 1";
			supplierBookingLine1.JSL_Description = "booking line1 desc";

			var supplierBookingLine2 = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine2.JSL_JO_OrderLine = orderLine2.PK;
			supplierBookingLine2.JSL_GrossWeight = 21;
			supplierBookingLine2.JSL_GrossWeightUnit = "KG";
			supplierBookingLine2.JSL_Volume = 22;
			supplierBookingLine2.JSL_VolumeUnit = "M3";
			supplierBookingLine2.JSL_BookedPackages = 23;
			supplierBookingLine2.JSL_F3_NKBookedPackagesUnit = "PLT";
			supplierBookingLine2.JSL_BookedQuantity = 24;
			supplierBookingLine2.JSL_RH_NKCommodityCode = "GEN";
			supplierBookingLine2.JSL_MarksAndNumbers = "marks 2";
			supplierBookingLine2.JSL_Description = "booking line2 desc";

			var shipmentPlanning = supplierBooking.OrderShipmentPlannings.AddNew();
			shipmentPlanning.OPS_JSB_Booking = supplierBooking.PK;
			shipmentPlanning.OPS_ShipmentName = "planning shipment name test";
			shipmentPlanning.OPS_TransportMode = Core.Constants.TransportModes.Air;
			shipmentPlanning.OPS_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			shipmentPlanning.OPS_GoodsDescription = "test desc";
			shipmentPlanning.OPS_RL_NKOrigin = "CNSZX";
			shipmentPlanning.OPS_RL_NKDestination = "SGSIN";
			shipmentPlanning.OPS_ETD = new ZDateTimeOffset(2024, 7, 8);
			shipmentPlanning.OPS_ETA = new ZDateTimeOffset(2024, 7, 9);

			var planningLine1 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine1.OPL_Weight = 11;
			planningLine1.OPL_WeightUnit = "KG";
			planningLine1.OPL_Volume = 12;
			planningLine1.OPL_VolumeUnit = "M3";
			planningLine1.OPL_Packages = 13;
			planningLine1.OPL_F3_NKPackagesUnit = "PLT";
			planningLine1.OPL_Quantity = 14;
			planningLine1.OPL_JSL_BookingLine = supplierBookingLine1.PK;

			var planningLine2 = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
			planningLine2.OPL_Weight = 21;
			planningLine2.OPL_WeightUnit = "KG";
			planningLine2.OPL_Volume = 22;
			planningLine2.OPL_VolumeUnit = "M3";
			planningLine2.OPL_Packages = 23;
			planningLine2.OPL_F3_NKPackagesUnit = "PLT";
			planningLine2.OPL_Quantity = 24;
			planningLine2.OPL_JSL_BookingLine = supplierBookingLine2.PK;

			return (order, supplierBooking, shipmentPlanning);
		}

		static JobSupplierBooking BuildSupplierBooking(BusinessObjectFactory factory)
		{
			var supplierBooking = factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_TransportMode = Constants.TransportModes.Sea;
			supplierBooking.JSB_LoadMode = Constants.SupplierBookingLoadMode.ContainerYard;
			supplierBooking.JSB_IncoTerm = Constants.IncoTerms.ExWorks;
			supplierBooking.JSB_RL_NKOrigin = "CNSHA";
			supplierBooking.JSB_RL_NKDestination = "AUSYD";
			supplierBooking.JSB_GoodsDescription = "good desc";
			supplierBooking.JSB_MarksAndNumbers = "mark & num";
			supplierBooking.SupplierAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			supplierBooking.ControllingCustomerAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			supplierBooking.NotifyPartyDocAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			supplierBooking.NotifyParty2DocAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			supplierBooking.NotifyParty3DocAddress.E2_OA_Address = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var note = supplierBooking.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note.ST_NoteText = "details desc test";

			return supplierBooking;
		}
	}
}

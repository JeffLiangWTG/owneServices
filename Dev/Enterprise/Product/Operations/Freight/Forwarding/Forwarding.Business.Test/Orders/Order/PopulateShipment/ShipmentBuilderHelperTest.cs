using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class ShipmentBuilderHelperTest : TestCaseWithFactory
	{
		public void TestOverriddenGoodsDeliveredToAddressAndGoodsAvailableAtAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder(true);
			order.GoodsAvailableAtAddress.E2_AddressOverride = true;
			order.GoodsAvailableAtAddress.E2_CompanyName = "Overridden Goods Available At";
			order.GoodsAvailableAtAddress.E2_AddressOverride = true;
			order.GoodsAvailableAtAddress.UnrestrictedAdditionalAddressInformation = "Test Note 1";
			order.GoodsAvailableAtAddress.E2_Address1 = "Address1 Test 1";
			order.GoodsAvailableAtAddress.E2_Address2 = "Address2 Test 1";
			order.GoodsAvailableAtAddress.E2_RN_NKCountryCode = "SG";
			order.GoodsAvailableAtAddress.E2_City = "Singapore";
			order.GoodsAvailableAtAddress.E2_Postcode = "010123456";
			order.GoodsAvailableAtAddress.E2_State = "SIN";

			order.GoodsDeliveredToAddress.E2_AddressOverride = true;
			order.GoodsDeliveredToAddress.E2_CompanyName = "Overridden Goods Delivered To";
			order.GoodsDeliveredToAddress.E2_AddressOverride = true;
			order.GoodsDeliveredToAddress.UnrestrictedAdditionalAddressInformation = "Test Note 2";
			order.GoodsDeliveredToAddress.E2_Address1 = "Address1 Test 2";
			order.GoodsDeliveredToAddress.E2_Address2 = "Address2 Test 2";
			order.GoodsDeliveredToAddress.E2_RN_NKCountryCode = "AU";
			order.GoodsDeliveredToAddress.E2_City = "ALEXANDRIA";
			order.GoodsDeliveredToAddress.E2_Postcode = "011123456";
			order.GoodsDeliveredToAddress.E2_State = "SYD";
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment, (sender, e) => e.ShouldCreatePacklines = false);

			AssertEquals(true, shipment.ConsignorPickupAddress.E2_AddressOverride);
			AssertEquals("Test Note 1", shipment.ConsignorPickupAddress.UnrestrictedAdditionalAddressInformation);
			AssertEquals("Address1 Test 1", shipment.ConsignorPickupAddress.E2_Address1);
			AssertEquals("Address2 Test 1", shipment.ConsignorPickupAddress.E2_Address2);
			AssertEquals("SG", shipment.ConsignorPickupAddress.E2_RN_NKCountryCode);
			AssertEquals("Singapore", shipment.ConsignorPickupAddress.E2_City);
			AssertEquals("010123456", shipment.ConsignorPickupAddress.E2_Postcode);
			AssertEquals("SIN", shipment.ConsignorPickupAddress.E2_State);
			AssertEquals(DocAddressTypes.Codes.ConsignorPickupDeliveryAddress, shipment.ConsignorPickupAddress.E2_AddressType);

			AssertEquals(true, shipment.ConsigneeDeliveryAddress.E2_AddressOverride);
			AssertEquals("Overridden Goods Delivered To", shipment.ConsigneeDeliveryAddress.E2_CompanyName);
			AssertEquals("Test Note 2", shipment.ConsigneeDeliveryAddress.UnrestrictedAdditionalAddressInformation);
			AssertEquals("Address1 Test 2", shipment.ConsigneeDeliveryAddress.E2_Address1);
			AssertEquals("Address2 Test 2", shipment.ConsigneeDeliveryAddress.E2_Address2);
			AssertEquals("AU", shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode);
			AssertEquals("ALEXANDRIA", shipment.ConsigneeDeliveryAddress.E2_City);
			AssertEquals("011123456", shipment.ConsigneeDeliveryAddress.E2_Postcode);
			AssertEquals("SYD", shipment.ConsigneeDeliveryAddress.E2_State);
			AssertEquals(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, shipment.ConsigneeDeliveryAddress.E2_AddressType);

			shipment = Factory.New<ForwardingShipment>();
			order.GoodsAvailableAtAddress.E2_OA_Address = ZGuid.Empty;
			order.GoodsAvailableAtAddress.E2_AddressOverride = false;
			order.GoodsDeliveredToAddress.E2_OA_Address = ZGuid.Empty;
			order.GoodsDeliveredToAddress.E2_AddressOverride = false;
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment, (sender, e) => e.ShouldCreatePacklines = false);

			AssertEquals(true, shipment.ConsignorPickupAddress.IsEmpty);
			AssertEquals(true, shipment.ConsigneeDeliveryAddress.IsEmpty);
		}

		public void TestPackLinesForPopulateShipmentFromOrderWithDifferentConversion()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder(true);
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment, (sender, e) => e.ShouldCreatePacklines = false);
			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("GOODS", shipment.OuterPackLines[0].JL_Description);

			shipment = Factory.New<ForwardingShipment>();
			order = SetupAndGetOrder(true);
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment, (sender, e) => e.ShouldCreatePacklines = true);
			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertEquals("lemon", shipment.OuterPackLines[0].JL_Description);
			AssertEquals("grab", shipment.OuterPackLines[1].JL_Description);
		}

		public void TestPopulateShipmentFromOrder()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder(true);

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			CombineAssertions(() =>
			{
				AssertEquals(shipment.ConsigneePK, order.BuyerPK);
				AssertEquals(shipment.ConsignorPK, order.SupplierPK);
				AssertEquals(shipment.ServiceLevel.RS_Code, order.JD_RS_NKServiceLevel_NI);
				AssertEquals(shipment.JS_INCO, order.JD_IncoTerm);
				AssertEquals(shipment.JS_GoodsDescription, order.JD_OrderGoodsDescription);
				AssertEquals(shipment.JS_TransportMode, order.JD_TransportMode);
				AssertEquals(shipment.JS_PackingMode, order.JD_ContainerMode);
				AssertEquals(shipment.JS_RL_NKOrigin, order.JD_RL_NKPortOfLoading);
				AssertEquals(shipment.JS_RL_NKDestination, order.JD_RL_NKPortOfDischarge);
				AssertEquals(shipment.JS_OuterPacks, order.JD_Packs);
				AssertEquals(shipment.JS_HouseBill, order.JD_Waybill);
				AssertEquals(1, shipment.GenericOrders.Count);
				foreach (var orderLine in order.OrderLines)
				{
					Assert(string.Format("All orderlines should be copied as packlines. Missing packline with description: {0}", orderLine.JO_Description), shipment.OuterPackLines.Any(x => ((ForwardingPackLine)x).JL_Description.Equals(orderLine.JO_Description)));
				}
				AssertEquals(shipment.ControllingAgentDocumentaryAddress.Address1, order.ControllingAgentDocAddress.Address1);
				AssertEquals(shipment.ControllingCustomerAddress.Address1, order.ControllingCustomerDocAddress.Address1);
				AssertEquals(shipment.ConsignorPickupAddress.Address.OA_Address1, order.GoodsAvailableAtAddress.Address.OA_Address1);
				AssertEquals(shipment.ConsigneeDeliveryAddress.Address.OA_Address1, order.GoodsDeliveredToAddress.Address.OA_Address1);
				AssertEquals(shipment.DocsAndCartage.JP_PickupRequiredBy, order.JD_ExWorksRequiredBy);
				AssertEquals(shipment.DocsAndCartage.JP_DeliveryRequiredBy, order.JD_DeliveryRequiredBy);
			});
		}

		public void TestPopulateShipmentFromOrder_OrderDoesNotExist()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var notAnOrderPK = ZGuid.NewZGuid();

			AssertNoExceptionThrown("Don't try access order that doesn't exist", () => ShipmentBuilderHelper.PopulateShipmentFromOrder(notAnOrderPK, shipment));
		}

		public void TestPopulateShipment_ServiceLevelIsPopulatedProperly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order, true);
			order.JD_RS_NKServiceLevel_NI = "DIR";

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			AssertEquals("Shipment service level is populated", "DIR", shipment.JS_RS_NKServiceLevel);
		}

		public void TestPopulateShipment_NotifyParty_RealOrganization()
		{
			var notifyPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			notifyPartyOrg.MainAddress.Address1 = "Dark School";
			notifyPartyOrg.OH_FullName = "Dark Company";
			notifyPartyOrg.MainAddress.City = "Dark City";

			var notifyParty2Org = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2Org.MainAddress.Address1 = "Dark School2";
			notifyParty2Org.OH_FullName = "Dark Company2";
			notifyParty2Org.MainAddress.City = "Dark City2";

			var notifyParty3Org = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3Org.MainAddress.Address1 = "Dark School3";
			notifyParty3Org.OH_FullName = "Dark Company3";
			notifyParty3Org.MainAddress.City = "Dark City3";
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();
			order.NotifyPartyDocAddress.OrganisationPK = notifyPartyOrg.PK;
			order.NotifyParty2DocAddress.OrganisationPK = notifyParty2Org.PK;
			order.NotifyParty3DocAddress.OrganisationPK = notifyParty3Org.PK;

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			CombineAssertions(() =>
			{
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_Address1, "Dark School");
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_CompanyName, "Dark Company");
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_City, "Dark City");
				AssertEquals(shipment.PK, shipment.NotifyPartyDocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyPartyDocumentaryAddress.E2_ParentTableCode);

				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_Address1, "Dark School2");
				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_CompanyName, "Dark Company2");
				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_City, "Dark City2");
				AssertEquals(shipment.PK, shipment.NotifyParty2DocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyParty2DocumentaryAddress.E2_ParentTableCode);

				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_Address1, "Dark School3");
				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_CompanyName, "Dark Company3");
				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_City, "Dark City3");
				AssertEquals(shipment.PK, shipment.NotifyParty3DocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyParty3DocumentaryAddress.E2_ParentTableCode);
			});
		}

		public void TestPopulateShipment_NotifyParty_OverriddenAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();

			order.NotifyPartyDocAddress.E2_AddressOverride = true;
			order.NotifyPartyDocAddress.E2_Address1 = "Magic School";
			order.NotifyPartyDocAddress.E2_CompanyName = "Magic Company";
			order.NotifyPartyDocAddress.E2_City = "Magic City";

			order.NotifyParty2DocAddress.E2_AddressOverride = true;
			order.NotifyParty2DocAddress.E2_Address1 = "Magic School 2";
			order.NotifyParty2DocAddress.E2_CompanyName = "Magic Company 2";
			order.NotifyParty2DocAddress.E2_City = "Magic City 2";

			order.NotifyParty3DocAddress.E2_AddressOverride = true;
			order.NotifyParty3DocAddress.E2_Address1 = "Magic School 3";
			order.NotifyParty3DocAddress.E2_CompanyName = "Magic Company 3";
			order.NotifyParty3DocAddress.E2_City = "Magic City 3";

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			CombineAssertions(() =>
			{
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_Address1, "Magic School");
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_CompanyName, "Magic Company");
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_City, "Magic City");
				AssertEquals(shipment.PK, shipment.NotifyPartyDocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyPartyDocumentaryAddress.E2_ParentTableCode);

				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_Address1, "Magic School 2");
				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_CompanyName, "Magic Company 2");
				AssertEquals(shipment.NotifyParty2DocumentaryAddress.E2_City, "Magic City 2");
				AssertEquals(shipment.PK, shipment.NotifyParty2DocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyParty2DocumentaryAddress.E2_ParentTableCode);

				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_Address1, "Magic School 3");
				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_CompanyName, "Magic Company 3");
				AssertEquals(shipment.NotifyParty3DocumentaryAddress.E2_City, "Magic City 3");
				AssertEquals(shipment.PK, shipment.NotifyParty3DocumentaryAddress.E2_ParentID);
				AssertEquals("JS", shipment.NotifyParty3DocumentaryAddress.E2_ParentTableCode);
			});
		}

		public void TestPopulateShipment_EmptyNotifyParty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();

			order.NotifyPartyDocAddress.E2_Address1 = "Magic School";
			order.NotifyPartyDocAddress.E2_CompanyName = "Magic Company";
			order.NotifyPartyDocAddress.E2_City = "Magic City";
			order.NotifyPartyDocAddress.E2_OA_Address = ZGuid.Invalid;

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			CombineAssertions(() =>
			{
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_Address1, ZString.Empty);
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_CompanyName, ZString.Empty);
				AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_City, ZString.Empty);
			});
		}

		public void TestPopulateShipmentFromOrderWithMultipleSupplierAddresses()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order);
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);

			AssertEquals("Supplier address copied from Order", order.JD_OA_SupplierAddress, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
			AssertEquals("Booking Origin is copied from Goods Available at on Order", order.JD_RL_NKPortOfLoading, shipment.JS_RL_NKOrigin);
		}

		public void TestPopulateShipmentFromOrderWithMultipleBuyerAddresses()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order);
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);

			AssertEquals("Buyer address copied from Order", order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertEquals("Booking Dest is copied from Goods DeliveredTo on Order", order.JD_RL_NKPortOfDischarge, shipment.JS_RL_NKDestination);
		}

		public void TestPopulateShipmentFromOrderWithBuyerLinks()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = SetupAndGetOrder();
			AddAdditionalAddressInfoToOrder(order, true);
			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);
			AssertEquals("Supplier address copied from Order with buyer links", order.JD_OA_SupplierAddress, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
			AssertEquals("Buyer address copied from Order with buyer links", order.JD_OA_BuyerAddress, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
			AssertEquals("Shipment Origin is copied from Loading Port at on Order with buyer links", order.JD_RL_NKPortOfLoading, shipment.JS_RL_NKOrigin);
			AssertEquals("Shipment Dest is copied from Port Of Discharge on Order with buyer links", order.JD_RL_NKPortOfDischarge, shipment.JS_RL_NKDestination);
		}

		public void TestConsignorPickupAddress_CallsEventHandlers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consignorPickupAddressChangedCalls = 0;
			void ConsignorPickupAddressChanged(object sender, EventArgs e) => consignorPickupAddressChangedCalls++;
			shipment.ConsignorPickupAddress.DocAddressChanged += new EventHandler(ConsignorPickupAddressChanged);

			var order = SetupAndGetOrder();
			order.DocAddresses.CreateWithAddressType(DocAddressType.GoodsAvailableAt);
			order.GoodsAvailableAtAddress.Address1 = "Consignor Pickup Address";

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);

			AssertGreaterThan("Should call DocAddressChanged atleast once so defaulting occurs on shipment", consignorPickupAddressChangedCalls, 0);
		}

		public void TestConsigneeDeliveryAddress_CallsEventHandlers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consigneeDeliveryAddressChangedCalls = 0;
			void ConsigneeDeliveryAddressChanged(object sender, EventArgs e) => consigneeDeliveryAddressChangedCalls++;
			shipment.ConsigneeDeliveryAddress.DocAddressChanged += new EventHandler(ConsigneeDeliveryAddressChanged);

			var order = SetupAndGetOrder();
			order.DocAddresses.CreateWithAddressType(DocAddressType.GoodsDeliveredTo);
			order.GoodsDeliveredToAddress.Address1 = "Consignee Delivery Address";

			shipment = ShipmentBuilderHelper.PopulateShipmentFromOrder(order.PK, shipment);

			AssertGreaterThan("Should call DocAddressChanged atleast once so defaulting occurs on shipment", consigneeDeliveryAddressChangedCalls, 0);
		}

		Order SetupAndGetOrder(bool generateOrderLines = false)
		{
			var order = Factory.New<Order>();
			order.BuyerPK = ZGuid.NewZGuid();
			order.SupplierPK = ZGuid.NewZGuid();
			order.JD_Packs = 3;
			order.JD_RS_NKServiceLevel_NI = "STD";
			order.JD_IncoTerm = "ABC";
			order.JD_RL_NKPortOfLoading = "LOADP";
			order.JD_RL_NKPortOfDischarge = "DISCP";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order.JD_OrderGoodsDescription = "GOODS";
			order.JD_Waybill = "WAYBILL";

			order.DocAddresses.CreateWithAddressType(DocAddressType.ControllingAgent);
			order.ControllingAgentDocAddress.Address1 = "Ye olde agente streete";
			order.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer);
			order.ControllingCustomerDocAddress.Address1 = "Ye olde customerre streete";

			var buyer = Factory.New<OrgHeader>();
			var address1 = buyer.Addresses.AddNew(OrgAddressType.Pickup, true);
			address1.OA_Address1 = "33 Pitt St";
			order.GoodsAvailableAtAddress.E2_OA_Address = address1.PK;

			var supplier = Factory.New<OrgHeader>();
			var address2 = supplier.Addresses.AddNew(OrgAddressType.Delivery, true);
			address2.OA_Address1 = "55 Young St";
			order.GoodsDeliveredToAddress.E2_OA_Address = address2.PK;

			order.JD_ExWorksRequiredBy = new ZDateTime(ZDateTime.Now);
			order.JD_DeliveryRequiredBy = new ZDateTime(ZDateTime.Now);

			if (generateOrderLines)
			{
				var orderLine1 = order.OrderLines.AddNew();
				orderLine1.JO_Description = "lemon";

				var orderLine2 = order.OrderLines.AddNew();
				orderLine2.JO_Description = "grab";
			}

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
				linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUMEL";
				linkDetails.PF_RL_NKPlaceOfDeliveryPort = "HKHKG";
				linkDetails.PF_RL_NKLoadPort = "AUDRW";
				linkDetails.PF_RL_NKDischargePort = "HKKWL";
				linkDetails.PF_RS_NKDefaultServiceLevel = "ABC";
			}

			order.JD_OA_SupplierAddress = supplierAddressSYD.PK;
			order.JD_OA_BuyerAddress = buyerAddressCNBEI.PK;

			order.JD_RL_NKGoodsAvailableAt = "AUPER";
			order.JD_RL_NKGoodsDeliveredTo = "MYKUL";
			order.JD_RL_NKPortOfLoading = "AUADL";
			order.JD_RL_NKPortOfDischarge = "SGSIN";
		}
	}
}

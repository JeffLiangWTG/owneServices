using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocAddressValidationTest : BaseFreightTest
	{
		#region TestValidateOrderRefsForImport

		public void TestValidateOrderRefsForImport()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Test Consignor";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Test Consignee";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.HomePort.RL_Code;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Not an import, no error", false, shipment.ConsigneePKInfo.HasErrors());

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.HomePort.RL_Code;
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("No order refs for import. Should be error", true, shipment.ConsigneePKInfo.HasErrors());

			shipment.GenericOrders.Add(Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>()));
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Attached an warehouse order. Should be no error", false, shipment.ConsigneePKInfo.HasErrors());

			shipment.GenericOrders.RemoveAndDeleteAll();

			shipment.AttachedOrders.AddNew();
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Attached an order. Should be no error", false, shipment.ConsigneePKInfo.HasErrors());

			shipment.AttachedOrders.DeleteAll();
			shipment.DocsAndCartage.JP_OrderItemsAsString = "Order Refs Note";
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Entered Notes Order Refs. Should be no error", false, shipment.ConsigneePKInfo.HasErrors());
		}

		#endregion

		#region TestNoFunnyCharactersInValidateOrderRefsForImport

		public void TestNoFunnyCharactersInValidateOrderRefsForImport()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("Precondition - Shipment should be an import", true, shipment.IsImport());

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			shipment.ConsigneePK = consignee.PK;

			AssertEquals("Precondition - AttachedOrders.Count == 0", 0, shipment.AttachedOrders.Count);
			AssertEquals("Precondition - JobDocsAndCartage.OrderItems.Count == 0", 0, shipment.DocsAndCartage.OrderItems.Count);

			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();

			AssertEquals("Shipment should have an error on consignee", true, shipment.ConsigneePKInfo.HasErrors());
		}

		#endregion

		#region Delivery Address Change Warning

		public void TestDeliveryAddressChangeWarning()
		{
			DummyShipmentAnnouncer dummyShipmentAnnouncer = new DummyShipmentAnnouncer();
			var mockShipment = Factory.NewMoq<ForwardingShipment>();
			mockShipment.Setup(m => m.ShipmentAnnouncer).Returns(dummyShipmentAnnouncer);
			ForwardingShipment shipment = mockShipment.Object;
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "CODE";
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			OrgAddress deliveryAddress = header.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			shipment.ConsigneeDeliveryAddress.OrganisationPK = header.PK;
			shipment.ConsigneeDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Pre-condition", false, shipment.ConsigneeDeliveryAddress.OrganisationPKInfo.HasWarnings());
			shipment.UserHasEnteredDeliverAddressControl = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "QAZWSX222";
			shipment.UserHasEnteredDeliverAddressControl = false;
			shipment.ConsigneeDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Should have warning", true, shipment.ConsigneeDeliveryAddress.OrganisationPKInfo.HasWarnings());
			AssertEquals("Should have correct text", true, shipment.ConsigneeDeliveryAddress.OrganisationPKInfo.HasWarning("The delivery address of this shipment has been changed and there is at least one attached declaration where your Customs Brokerage Department should be made aware of this change. Additional Text."));
		}

		public void TestCheckOrganisationDocumentDeliveryAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE";
			var mainAddress = org.MainAddress;
			mainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var deliveryAddress = org.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			deliveryAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			deliveryAddress.OA_Address1 = "Address1";
			deliveryAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			DummyShipmentAnnouncer dummyShipmentAnnouncer = new DummyShipmentAnnouncer();
			var mockShipment = Factory.NewMoq<ForwardingShipment>();
			mockShipment.Setup(m => m.ShipmentAnnouncer).Returns(dummyShipmentAnnouncer);

			var shipment = mockShipment.Object;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = org.PK;
			shipment.UserHasEnteredDeliverAddressControl = true;
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "Garbage Address";
			shipment.UserHasEnteredDeliverAddressControl = false;
			Assert("Pre-condition: Shipment Consignee Delivery Address should not be overriden", !shipment.ConsigneeDeliveryAddress.E2_AddressOverride);

			shipment.ConsigneeDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Should have warning on the org", true, shipment.ConsigneeDeliveryAddress.OrganisationPKInfo.HasWarnings());

			shipment.ConsigneeDeliveryAddress.Validation.ValidateE2_CompanyName();
			AssertEquals("Should not have warning on the company name", false, shipment.ConsigneeDeliveryAddress.E2_CompanyNameInfo.HasWarnings());

			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			shipment.ConsigneeDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertEquals("Warning should have been cleared", false, shipment.ConsigneeDeliveryAddress.OrganisationPKInfo.HasWarnings());

			shipment.ConsigneeDeliveryAddress.Validation.ValidateE2_CompanyName();
			AssertEquals("Should have warning on the company name", true, shipment.ConsigneeDeliveryAddress.E2_CompanyNameInfo.HasWarnings());
		}

		#endregion

		public void TestAttachOrderToShipmentWithNonMatchingControllingCustomer()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;

			var order = Factory.New<Order>();
			var shipment = Factory.New<ForwardingShipment>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";

			order.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;
			order.JD_JS = shipment.PK;

			shipment.ControllingCustomerAddress.RunPreSaveValidation();
			AssertHasError(shipment.ControllingCustomerAddress.E2_OA_AddressInfo, "You do not have security rights to allow order and its shipment have non-matching Controlling Customers.");

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}

		public void TestAttachOrderToShipmentHandlesWhsOrders_WithoutAddressOverride()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;

			var order = (BusinessObject)Factory.New<IWhsOrder>();
			order.FillWithValidTestData();

			var shipment = Factory.New<ForwardingShipment>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";

			var controllingCustomerJobDocAddress = (order as IDocAddresses).DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingCustomerJobDocAddress.E2_OA_Address = controllingCustomerAddress.PK;

			shipment.GenericOrders.Add(order);

			AssertNoExceptionThrown(() =>
			{
				shipment.ControllingCustomerAddress.RunPreSaveValidation();
			});

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}

		public void TestAttachOrderToShipmentHandlesWhsOrders_WithAddressOverride()
		{
			var origin = Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed;
			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = false;

			var order = (BusinessObject)Factory.New<IWhsOrder>();
			order.FillWithValidTestData();

			var shipment = Factory.New<ForwardingShipment>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";

			var controllingCustomerJobDocAddress = (order as IDocAddresses).DocAddresses.AddNew(DocAddressType.ControllingCustomer);
			controllingCustomerJobDocAddress.E2_AddressOverride = true;
			controllingCustomerJobDocAddress.E2_Address1 = "Some overridden address";

			shipment.GenericOrders.Add(order);

			AssertNoExceptionThrown(() =>
			{
				shipment.ControllingCustomerAddress.RunPreSaveValidation();
			});

			Env.Security.AllowAttachOrdersWithNonMatchingControllingCustomerForShipment.IsAllowed = origin;
		}
	}
}

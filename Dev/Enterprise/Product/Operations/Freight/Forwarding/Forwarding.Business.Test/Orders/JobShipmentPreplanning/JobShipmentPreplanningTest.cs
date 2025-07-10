using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobShipmentPreplanning))]
	public sealed class JobShipmentPreplanningTest : EnterpriseBusinessObjectTestCase
	{
		#region Match Broker

		public void TestBrokerMatchesRelatedParty()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);

			buyer.AddRelatedParty(relatedParty.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			var prePlanning = Factory.New<JobShipmentPreplanning>();
			prePlanning.BuyerPK = buyer.PK;

			Action<string, bool> assertTransportModeMatches = (mode, matches) =>
				{
					prePlanning.PreAdviceTransports[0].JW_TransportMode = mode;
					AssertEquals(matches, prePlanning.IsBuyersBrokerThisOrg(ZString.Empty));
				};

			assertTransportModeMatches(Core.Constants.TransportModes.Air, true);
			assertTransportModeMatches(Core.Constants.TransportModes.Sea, true);
			assertTransportModeMatches(Core.Constants.TransportModes.Road, true);

			buyer.AllRelatedParties[0].PR_FreightTransportMode = Core.Constants.TransportModes.Sea;

			assertTransportModeMatches(Core.Constants.TransportModes.Air, false);
			assertTransportModeMatches(Core.Constants.TransportModes.Sea, true);
			assertTransportModeMatches(Core.Constants.TransportModes.Road, false);
		}

		#endregion

		#region Attribute

		public void TestUniversalCopyAssociateElementAttribute()
		{
			var planning = Factory.New<JobShipmentPreplanning>();
			var type = planning.GetType();

			var attributes = type
							.GetCustomAttributes(typeof(UniversalCopyAssociateElementAttribute), true);

			var attribute = attributes.Cast<UniversalCopyAssociateElementAttribute>().First(x => x.DefinitionElement == "JobShipment");
			AssertNotNull(attribute);
			AssertEquals("Shipment", attribute.ComponentElement);

			var propertyInfo = type.GetProperty("Shipment");
			AssertNotNull(propertyInfo);
			AssertEquals(typeof(ForwardingShipment), propertyInfo.PropertyType);
		}

		#endregion

		#region Operations Job

		public void TestOperationsJobAttached()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			BusinessObject dec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			Factory.Save();

			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			AssertEquals("", preAdvice.PreAdviceLockedMessage);

			preAdvice.EF_JS = shipment.PK;
			AssertEquals("", preAdvice.PreAdviceLockedMessage);

			Factory.Save();
			AssertEquals("This Shipment Pre Advice is linked to an operations job. Up to date data and modifications should occur on " + shipment.HumanReadableName + ".", preAdvice.PreAdviceLockedMessage);

			preAdvice.EF_JS = ZGuid.Empty;
			Factory.Save();
			AssertEquals("", preAdvice.PreAdviceLockedMessage);

			preAdvice.EF_JE = dec.PK;
			AssertEquals("", preAdvice.PreAdviceLockedMessage);

			Factory.Save();
			AssertEquals("This Shipment Pre Advice is linked to an operations job. Up to date data and modifications should occur on " + dec.HumanReadableName + ".", preAdvice.PreAdviceLockedMessage);

			preAdvice.EF_JE = ZGuid.Empty;
			Order order = preAdvice.Orders.AddNew();
			order.JD_JS = shipment.PK;

			Factory.Save();
			AssertEquals("This Shipment Pre Advice is linked to an operations job. Up to date data and modifications should occur on the shipments attached to the respective orders.", preAdvice.PreAdviceLockedMessage);
		}

		public void TestCantAttachToShipmentWhenAttachedOrdersHasShipments()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			AssertEquals(false, preAdvice.EF_JSInfo.ReadOnly);

			Order order = preAdvice.Orders.AddNew();
			AssertEquals(false, preAdvice.EF_JSInfo.ReadOnly);

			order.JD_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			AssertEquals(true, preAdvice.EF_JSInfo.ReadOnly);
		}

		public void TestCantAttachToDeclarationWhenAttachedOrdersHasDeclarations()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			AssertEquals(false, preAdvice.EF_JEInfo.ReadOnly);

			Order order = preAdvice.Orders.AddNew();
			AssertEquals(false, preAdvice.EF_JEInfo.ReadOnly);

			order.JD_JE = ((BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>()).PK;
			AssertEquals(true, preAdvice.EF_JEInfo.ReadOnly);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			AssertEquals(Env.Registry.FreightWeightUnit, preAdvice.EF_UnitOfWeight);
			AssertEquals(Env.Registry.FreightVolumeUnit, preAdvice.EF_UnitOfVolume);
			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, preAdvice.EF_F3_NKPackType);
		}

		#endregion

		#region Delete

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Delete is sometimes not supported", true);
		}

		public void TestDelete()
		{
			bool thrown = false;
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			try
			{
				preAdvice.Delete();
			}
			catch (CannotDeleteException)
			{
				Fail("Cannot Delete Exception should not be thrown when pre-advice has nothing attached");
			}

			preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.Orders.AddNew();
			try
			{
				preAdvice.Delete();
			}
			catch (CannotDeleteException)
			{
				thrown = true;
			}

			AssertEquals(true, thrown);
			thrown = false;

			preAdvice.Orders.DeleteAll();
			preAdvice.EF_JS = Factory.New<ForwardingShipment>().PK;
			try
			{
				preAdvice.Delete();
			}
			catch (CannotDeleteException)
			{
				thrown = true;
			}

			AssertEquals(true, thrown);
			thrown = false;

			preAdvice.EF_JS = ZGuid.Empty;
			try
			{
				preAdvice.Delete();
			}
			catch (CannotDeleteException)
			{
				Fail("Cannot Delete Exception should not be thrown when pre-advice has nothing attached");
			}
		}

		#endregion

		#region Related Business Objects

		public void TestOrderNumbers()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1234";

			Order order2 = preAdvice.Orders.AddNew();
			order2.JD_OrderNumber = "9876";
			OrderLine line = order2.OrderLines.AddNew();
			line.JO_QtyReceived = 1;

			AssertEquals(2, preAdvice.OrderNumberList.Count);
			AssertEquals("1234", preAdvice.OrderNumberList[0].Description);
			AssertEquals("9876", preAdvice.OrderNumberList[1].Description);

			Order order3 = preAdvice.Orders.AddNew();
			order3.JD_OrderNumber = "5566";
			AssertEquals(3, preAdvice.OrderNumberList.Count);

			preAdvice.Orders.RemoveFromRelationship(order1);
			AssertEquals(2, preAdvice.OrderNumberList.Count);
			AssertEquals("9876", preAdvice.OrderNumberList[0].Description);
			AssertEquals("5566", preAdvice.OrderNumberList[1].Description);

			AssertEquals(0, preAdvice.OrderLines.Count);
			preAdvice.OrderNumberList[0].Value = true;
			AssertEquals("Collection loaded on value ticked/unticked", 1, preAdvice.OrderLines.Count);

			preAdvice.OrderNumberList[0].Value = false;
			AssertEquals("Collection loaded on value ticked/unticked", 0, preAdvice.OrderLines.Count);
		}

		#endregion

		#region Saving

		public void TestSaving()
		{
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			AssertEquals(ZString.Empty, preplanning.EF_PreshipID);
			Factory.Save();
			AssertNotEquals(ZString.Empty, preplanning.EF_PreshipID);
		}

		public void TestOnFactorySaving()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.EF_MasterBill = "1234";
			preAdvice.BuyerPK = org.PK;

			Order order = preAdvice.Orders.AddNew();
			order.FillWithValidTestData();
			Order order2 = preAdvice.Orders.AddNew();
			order2.FillWithValidTestData();

			Factory.Save();
			AssertEquals("1234", order.JD_MasterWaybill);
			AssertEquals("1234", order2.JD_MasterWaybill);
		}

		public void TestDEXEventIsCopiedOnSaving()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.Logs.AddNew(Events.DataExport);
			Factory.Save();

			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Logs.AddNew(Events.DataExport);
			preplanning.EF_JS = shipment1.PK;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX");
			StmALog[] logs = shipment1.Logs.Find(filter);
			Assert("Event must be copied", logs.Length == 1);

			preplanning.EF_JS = shipment2.PK;
			Factory.Save();

			logs = shipment2.Logs.Find(filter);
			Assert("No more DEX events must be copied", logs.Length == 1);
		}

		#endregion

		#region Properties

		public void TestCancelled()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.Orders.AddNew();
			preplanning.Orders.AddNew();

			AssertEquals(false, preplanning.Orders[0].JD_IsCancelled);
			AssertEquals(false, preplanning.Orders[1].JD_IsCancelled);

			preplanning.EF_IsCancelled = true;
			AssertEquals(true, preplanning.Orders[0].JD_IsCancelled);
			AssertEquals(true, preplanning.Orders[1].JD_IsCancelled);
		}

		public void TestHumanReadableName()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_PreshipID = "PA00001111";
			AssertEquals("Shipment Pre Advice PA00001111", preplanning.HumanReadableName);
		}

		public void TestPreShipIdReadOnly()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			AssertEquals(true, preplanning.EF_PreshipIDInfo.ReadOnly);
		}

		#endregion

		#region Create Shipment

		#region ConsigneeDeliveryPoint

		OrgAddress address1;
		OrgAddress address2;
		OrgAddress address3;

		void SetupBuyerAddresses()
		{
			address1 = Buyer.Addresses.AddNew(OrgAddressType.Office, true);
			address1.OA_Address1 = "Address 1";
			address2 = Buyer.Addresses.AddNew(OrgAddressType.Pickup, true);
			address2.OA_Address1 = "Address 2";
			address3 = Buyer.Addresses.AddNew(OrgAddressType.Delivery, true);
			address3.OA_Address1 = "Address 3";

			Order1.GoodsAvailableAtAddress.E2_OA_Address = address2.PK;
			Order1.GoodsDeliveredToAddress.E2_OA_Address = address3.PK;
			Order2.GoodsAvailableAtAddress.E2_OA_Address = address2.PK;
			Order2.GoodsDeliveredToAddress.E2_OA_Address = address3.PK;

			Factory.Save();
		}

		public void TestCreateConsolAndShipment_ConsigneeDeliveryPoint_Default_CheckNotification()
		{
			SetupBuyerAddresses();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[0].ConsigneeDeliveryAddress.E2_OA_Address);
				AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[1].ConsigneeDeliveryAddress.E2_OA_Address);

				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
			}
		}

		public void TestCreateConsolAndShipment_ConsigneeDeliveryPoint_OrderDeliveryAddress()
		{
			SetupBuyerAddresses();
			Order1.GoodsDeliveredToAddress.E2_OA_Address = address3.PK;
			Order2.GoodsDeliveredToAddress.E2_OA_Address = address1.PK;
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("ship1: Should be order delivery address", address3.PK, consol.Shipments[0].ConsigneeDeliveryAddress.E2_OA_Address);
		}

		public void TestCreateConsolAndShipment_ConsignorPickupPoint_OrderPickupAddress_WithGoodsAvailableAtAddress()
		{
			var sup1Address1 = Supplier1.Addresses.AddNew(OrgAddressType.Office, true);
			sup1Address1.OA_Address1 = "Address 1";

			var sup1Address2 = Supplier1.Addresses.AddNew(OrgAddressType.Pickup, true);
			sup1Address2.OA_Address1 = "Address 2";

			var sup2Address1 = Supplier2.Addresses.AddNew(OrgAddressType.Office, true);
			sup2Address1.OA_Address1 = "Address 3";

			var sup2Address2 = Supplier2.Addresses.AddNew(OrgAddressType.Pickup, true);
			sup2Address2.OA_Address1 = "Address 4";

			Order1.GoodsAvailableAtAddress.E2_OA_Address = sup1Address1.PK;
			Order2.GoodsAvailableAtAddress.E2_OA_Address = sup2Address2.PK;

			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertEquals("ship1: Should be order pickup address", sup1Address1.PK, consol.Shipments[0].ConsignorPickupAddress.E2_OA_Address);
			AssertEquals("ship1: Should be order pickup address", sup1Address1.OA_Address1, consol.Shipments[0].ConsignorPickupAddress.Address.OA_Address1);
			AssertEquals("ship2: Should be order pickup address", sup2Address2.PK, consol.Shipments[1].ConsignorPickupAddress.E2_OA_Address);
			AssertEquals("ship1: Should be order pickup address", sup2Address2.OA_Address1, consol.Shipments[1].ConsignorPickupAddress.Address.OA_Address1);
		}

		public void TestCreateConsolAndShipment_ControllingAgentAddress()
		{
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "CAG";
			var controllingAgentAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingAgentAddress.OA_OH = controllingAgent.PK;
			controllingAgentAddress.OA_Code = "CAG";
			Order1.ControllingAgentDocAddress.E2_OA_Address = controllingAgentAddress.PK;

			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier);
			AssertEquals("ship1: Should be controlling agent", controllingAgent.PK, consol.Shipments[0].ControllingAgent.PK);
			AssertEquals("ship1: Should be controlling agent address", controllingAgentAddress.PK, consol.Shipments[0].ControllingAgentDocumentaryAddress.E2_OA_Address);
		}

		public void TestCreateConsolAndShipment_ControllingCustomerAddress()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";
			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "CCM";
			Order1.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomerAddress.PK;

			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier);
			AssertEquals("ship1: Should be controlling customer", controllingCustomer.PK, consol.Shipments[0].ControllingCustomer.PK);
			AssertEquals("ship1: Should be controlling customer address", controllingCustomerAddress.PK, consol.Shipments[0].ControllingCustomerAddress.E2_OA_Address);
		}

		public void TestCreateConsolAndShipment_NotifyPartyDocAddress()
		{
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.MainAddress.Address1 = "Dark School";
			notifyParty.OH_FullName = "Dark Company";
			notifyParty.MainAddress.City = "Dark City";

			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.MainAddress.Address1 = "Dark School 2";
			notifyParty2.OH_FullName = "Dark Company 2";
			notifyParty2.MainAddress.City = "Dark City 2";

			var notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.MainAddress.Address1 = "Dark School 3";
			notifyParty3.OH_FullName = "Dark Company 3";
			notifyParty3.MainAddress.City = "Dark City 3";

			Order1.NotifyPartyDocAddress.OrganisationPK = notifyParty.PK;
			Order1.NotifyParty2DocAddress.OrganisationPK = notifyParty2.PK;
			Order1.NotifyParty3DocAddress.OrganisationPK = notifyParty3.PK;

			Factory.Save();

			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier);
			AssertNotNull(consol.Shipments.FirstOrDefault());
			var shipment = consol.Shipments[0];
			AssertEquals("Dark School", shipment.NotifyPartyDocumentaryAddress.E2_Address1);
			AssertEquals("Dark Company", shipment.NotifyPartyDocumentaryAddress.E2_CompanyName);
			AssertEquals("Dark City", shipment.NotifyPartyDocumentaryAddress.E2_City);
			AssertEquals(shipment.PK, shipment.NotifyPartyDocumentaryAddress.E2_ParentID);
			AssertEquals("JS", shipment.NotifyPartyDocumentaryAddress.E2_ParentTableCode);

			AssertEquals("Dark School 2", shipment.NotifyParty2DocumentaryAddress.E2_Address1);
			AssertEquals("Dark Company 2", shipment.NotifyParty2DocumentaryAddress.E2_CompanyName);
			AssertEquals("Dark City 2", shipment.NotifyParty2DocumentaryAddress.E2_City);
			AssertEquals(shipment.PK, shipment.NotifyParty2DocumentaryAddress.E2_ParentID);
			AssertEquals("JS", shipment.NotifyParty2DocumentaryAddress.E2_ParentTableCode);

			AssertEquals("Dark School 3", shipment.NotifyParty3DocumentaryAddress.E2_Address1);
			AssertEquals("Dark Company 3", shipment.NotifyParty3DocumentaryAddress.E2_CompanyName);
			AssertEquals("Dark City 3", shipment.NotifyParty3DocumentaryAddress.E2_City);
			AssertEquals(shipment.PK, shipment.NotifyParty3DocumentaryAddress.E2_ParentID);
			AssertEquals("JS", shipment.NotifyParty3DocumentaryAddress.E2_ParentTableCode);
		}

		public void TestCreateConsolAndShipment_EmptyNotifyPartyDocAddress()
		{
			Buyer.OH_RL_NKClosestPort = "USLAX";
			var contact = Buyer.Contacts.AddNew();
			var link = Buyer.SupplierLinks.AddNew(Supplier1);
			link.OL_OC_NotifyPartyContact = contact.PK;

			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);
			var shipment = (ForwardingShipment)consol.Shipments.First();
			AssertNoErrors(shipment.NotifyPartyDocumentaryAddress.E2_OA_AddressInfo);
		}

		public void TestCreateConsolAndShipment_CalculatedShipmentActualChargeableIsOutOfRange_ResetItToDefaultValue()
		{
			SetUpPreplanning(false);
			SetupBuyerAddresses();

			// Such values should produce an incorrect ActualChargeable
			Preplanning.EF_RL_NKPortDisch = "INBOM";
			Order1.JD_ActualVolume = 7287.000m;
			Order1.JD_ActualWeight = 0;

			Factory.Save();

			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);
			AssertEquals("The JS_ActualChargeable should have default value", -1m, consol.Shipments[0].JS_ActualChargeable);
		}

		public void TestCreateConsolAndShipment_BuyerSupplierLinks()
		{
			var notifier = new TestNotificationSubscriber();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_RL_NKClosestPort = "USLAX";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "CNNBO";

			var link = buyer.SupplierLinks.AddNew(supplier);
			var linkDetails = (OrgSupBuyLinkTrnMode)link.OrgSupBuyLinkTrnModes.First();
			linkDetails.PF_TransportMode = Core.Constants.TransportModes.Sea;
			linkDetails.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			linkDetails.PF_RL_NKPlaceOfReceivalPort = "AUBNE";
			linkDetails.PF_RL_NKPlaceOfDeliveryPort = "AUSYD";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();

			var preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_HouseBill = "housebill";
			preplanning.EF_MasterBill = "masterbill";
			preplanning.BuyerPK = buyer.PK;
			preplanning.EF_OH_Carrier = carrier.PK;
			preplanning.EF_OH_SendingAgent = agent1.PK;
			preplanning.EF_OH_ReceivingAgent = agent2.PK;
			preplanning.EF_RL_NKPortLoad = "AUSYD";
			preplanning.EF_RL_NKPortDisch = "USLAX";

			var testDate = ZDateTime.Today.AddMonths(-6);

			var order = preplanning.Orders.AddNew();
			order.JD_OrderNumber = "order";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "USLAX";
			order.JD_TransportMode = "SEA";
			order.JD_IncoTerm = "FCA";
			order.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			order.JD_RN_NKCountryOfSupply = "NZ";
			order.JD_RS_NKServiceLevel_NI = serviceLevel.RS_Code;

			buyer.OH_IsConsignee = true;
			buyer.OH_IsForwarder = true;

			supplier.OH_IsConsignor = true;
			supplier.OH_IsForwarder = true;

			var supplierAddress = supplier.Addresses.AddNewMainAddress();
			supplierAddress.OA_Address1 = "supplier address";

			Factory.Save();

			var consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, notifier);
			var shipment = (ForwardingShipment)consol.Shipments.First();

			AssertEquals("AUBNE", shipment.JS_RL_NKOrigin);
			AssertEquals("AUSYD", shipment.JS_RL_NKDestination);
		}

		public void TestPickupDateFirstDateYounger()
		{
			Order1.JD_ExWorksRequiredBy = new ZDateTime(2009, 10, 30, 05, 00, 00);
			Order2.JD_ExWorksRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("DeliveryDate should be transferred to shipment1", new ZDateTime(2009, 10, 30, 05, 00, 00), consol.Shipments[0].DocsAndCartage.JP_PickupRequiredBy);
		}

		public void TestPickupDateSecondDateYounger()
		{
			Order1.JD_ExWorksRequiredBy = new ZDateTime(2009, 10, 30, 07, 00, 00);
			Order2.JD_ExWorksRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("DeliveryDate should be transferred to shipment1 (the smalldatetime accuracy of JD_ExWorksRequiredBy field is one minute)", new ZDateTime(2009, 10, 30, 06, 00, 00), consol.Shipments[0].DocsAndCartage.JP_PickupRequiredBy);
		}

		public void TestPickupDateFirstDateEmpty()
		{
			Order1.JD_ExWorksRequiredBy = ZDateTime.Empty;
			Order2.JD_ExWorksRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("DeliveryDate should be transferred to shipment1 (the smalldatetime accuracy of JD_ExWorksRequiredBy field is one minute)", new ZDateTime(2009, 10, 30, 06, 00, 00), consol.Shipments[0].DocsAndCartage.JP_PickupRequiredBy);
		}

		public void TestDeliveryDateFirstDateYounger()
		{
			Order1.JD_DeliveryRequiredBy = new ZDateTime(2009, 10, 30, 05, 00, 00);
			Order2.JD_DeliveryRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("DeliveryDate should be transferred to shipment1", new ZDateTime(2009, 10, 30, 05, 00, 00), consol.Shipments[0].DocsAndCartage.JP_DeliveryRequiredBy);
		}

		public void TestDeliveryDateSecondDateYounger()
		{
			Order1.JD_DeliveryRequiredBy = new ZDateTime(2009, 10, 30, 07, 00, 00);
			Order2.JD_DeliveryRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier);
			AssertEquals("DeliveryDate should be transferred to shipment1 (the smalldatetime accuracy of JD_DeliveryRequiredBy field is one minute)", new ZDateTime(2009, 10, 30, 06, 00, 00), consol.Shipments[0].DocsAndCartage.JP_DeliveryRequiredBy);
		}

		public void TestDeliveryDateFirstDateEmpty()
		{
			Order1.JD_DeliveryRequiredBy = ZDateTime.Empty;
			Order2.JD_DeliveryRequiredBy = new ZDateTime(2009, 10, 30, 06, 00, 01);
			Factory.Save();
			JobShipmentPreplanning preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.Orders.Add(Order1);
			preplanning.Orders.Add(Order2);
			Factory.Save();

			ForwardingConsol consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
			AssertEquals("DeliveryDate should be transferred to shipment1 (the smalldatetime accuracy of JD_DeliveryRequiredBy field is one minute)", new ZDateTime(2009, 10, 30, 06, 00, 00), consol.Shipments[0].DocsAndCartage.JP_DeliveryRequiredBy);
		}

		public void TestCreateConsolAndShipment_ConsigneeDeliveryPoint_1DeliveryPoint()
		{
			SetupBuyerAddresses();
			OrderLineDelivery delivery11 = Order1Line1.Deliveries.AddNew();
			delivery11.J4_OA_NKDeliveryPoint = address1.OA_Code;
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertEquals("ship1: Should be default consignee delivery address", address1.PK, consol.Shipments[0].ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[1].ConsigneeDeliveryAddress.E2_OA_Address);
		}

		public void TestCreateConsolAndShipment_ConsigneeDeliveryPoint_2DeliveryPoints()
		{
			SetupBuyerAddresses();
			OrderLineDelivery delivery11 = Order1Line1.Deliveries.AddNew();
			delivery11.J4_OA_NKDeliveryPoint = address1.OA_Code;
			OrderLineDelivery delivery12 = Order1Line2.Deliveries.AddNew();
			delivery12.J4_OA_NKDeliveryPoint = address2.OA_Code;
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[0].ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[1].ConsigneeDeliveryAddress.E2_OA_Address);
		}

		public void TestCreateConsolAndShipment_ConsigneeDeliveryPoint_3DeliveryPoints()
		{
			SetupBuyerAddresses();
			OrderLineDelivery delivery11 = Order1Line1.Deliveries.AddNew();
			delivery11.J4_OA_NKDeliveryPoint = address1.OA_Code;
			OrderLineDelivery delivery12 = Order1Line2.Deliveries.AddNew();
			delivery12.J4_OA_NKDeliveryPoint = address2.OA_Code;
			OrderLineDelivery delivery21 = Order2Line1.Deliveries.AddNew();
			delivery21.J4_OA_NKDeliveryPoint = address2.OA_Code;
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertEquals("ship1: Should be default consignee delivery address", address3.PK, consol.Shipments[0].ConsigneeDeliveryAddress.E2_OA_Address);
			AssertEquals("ship1: Should be default consignee delivery address", address2.PK, consol.Shipments[1].ConsigneeDeliveryAddress.E2_OA_Address);
		}

		public void TestCreateDeclaration_ConsigneeDeliveryPoint_Default()
		{
			SetupBuyerAddresses();

			BusinessObject dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertEquals("Should be default consignee delivery address", address3.PK, ((IDocAddresses)dec).DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress).E2_OA_Address);
		}

		public void TestCreateDeclaration_ConsigneeDeliveryPoint_1DeliveryPoint()
		{
			SetupBuyerAddresses();
			OrderLineDelivery delivery11 = Order1Line1.Deliveries.AddNew();
			delivery11.J4_OA_NKDeliveryPoint = address1.OA_Code;
			Factory.Save();

			BusinessObject dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertEquals("Should be default consignee delivery address", address1.PK, ((IDocAddresses)dec).DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress).E2_OA_Address);
		}

		public void TestCreateDeclaration_ConsigneeDeliveryPoint_2DeliveryPoints()
		{
			SetupBuyerAddresses();
			OrderLineDelivery delivery11 = Order1Line1.Deliveries.AddNew();
			delivery11.J4_OA_NKDeliveryPoint = address1.OA_Code;
			OrderLineDelivery delivery12 = Order1Line2.Deliveries.AddNew();
			delivery12.J4_OA_NKDeliveryPoint = address2.OA_Code;
			Factory.Save();

			BusinessObject dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertEquals("Should be default consignee delivery address", address3.PK, ((IDocAddresses)dec).DocAddresses.FindByDocAddressType(DocAddressType.ImporterPickupDeliveryAddress).E2_OA_Address);
		}

		#endregion

		#region Events

		public void TestCreateConsolAndShipment_CopyExistingAIDEvent()
		{
			Preplanning.Logs.AddNew(Events.AllImportDocumentsReceived);
			Preplanning.Logs.AddNew(Events.DataExport);
			Factory.Save();
			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			var logs = new StmALogDependentCollection(consol);
			logs.Load();
			Assert("Should contain AID log", ContainsLog("AID", logs));
		}

		public void TestCopyExistingAIDEvent_ToShipment()
		{
			Preplanning.Logs.AddNew(Events.AllImportDocumentsReceived);
			Preplanning.Logs.AddNew(Events.DataExport);
			Factory.Save();
			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			var logs = new StmALogDependentCollection(consol.Shipments[0]);
			logs.Load();
			Assert("Should contain AID log", ContainsLog("AID", logs));
		}

		public void TestCopyExistingAIDEvent_ToStandaloneDec()
		{
			Preplanning.Logs.AddNew(Events.AllImportDocumentsReceived);
			Preplanning.Logs.AddNew(Events.DataExport);
			Factory.Save();
			var dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			var logs = new StmALogDependentCollection((IStmALogParent)dec);
			logs.Load();
			Assert("Should contain AID log", ContainsLog("AID", logs));
		}

		bool ContainsLog(string logCode, StmALogDependentCollection logs)
		{
			foreach (StmALog log in logs)
			{
				if (log.SL_SE_NKEvent == logCode)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		public void TestNoBranchOrgProxyDoesNotThrowException()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNoExceptionThrown(() => Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors));
		}

		public void TestCreateConsolAndShipment_NoOrders()
		{
			Preplanning.Orders.DeleteAll();
			Factory.Save();
			AssertEquals("Precondition", 0, Preplanning.Orders.Count);

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNull("Consol not created", consol);
			AssertEquals("This Pre Advice has no orders attached.", Notifier.LastMessage);
		}

		public void TestCreateConsolAndShipment_HasChanges()
		{
			Preplanning.EF_ActualVolume = 10m;
			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNull(consol);
			AssertEquals("Please save your changes before performing any Actions on this Pre Advice.", Notifier.LastMessage);
		}

		public void TestCreateConsolAndShipment_HasLinkToOperationsJob()
		{
			Preplanning.EF_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNull(consol);
			AssertEquals("This Pre Advice has already been linked to an operations job and cannot be linked again.", Notifier.LastMessage);
		}

		public void TestCreateConsolAndShipment_ExistingConsol()
		{
			ForwardingConsol existingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			existingConsol.JK_MasterBillNum = "MASTER";
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				Notifier.NextQueryUserResult = false;
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNull(consol);
				AssertEquals("A consol already exists for this Master Bill. If you continue, a new consol and shipment(s) will be created.\r\n\r\nDo you want to continue?", Notifier.LastMessage);
				AssertEquals(false, Notifier.LastQueryUserResult);
				Notifier.LastMessage = "";

				Notifier.NextQueryUserResult = true;
				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNotNull(consol);
				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
				AssertEquals("MASTER", consol.JK_MasterBillNum);
				AssertEquals(false, Notifier.LastQueryUserResult);
				Notifier.LastMessage = "";
			}
		}

		public void TestCreateConsolAndShipment_DoesNotDoubleSave()
		{
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Preplanning.EF_RL_NKPortDisch = "AUMEL";
			Factory.Save();

			var saveCount = 0;

			bool FixAndSaveConsol(ForwardingConsol consol)
			{
				consol.Factory.Saved += (sender, e) => saveCount++;

				consol.JK_RL_NKLastForeignPort = "USLAX";

				foreach (var transport in consol.Transports.OfType<Transport>())
				{
					transport.CarrierPK = Carrier.PK;
				}

				try
				{
					consol.Factory.Save();
				}
				catch (ZSaveException)
				{
					return false;
				}

				return true;
			}

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				Buyer.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty, "AU");
				Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, FixAndSaveConsol);

				Notifier.LastMessage = "";

				AssertEquals("Saving of consol should be done in FixAndSaveConsol", saveCount, 1);
			}
		}

		public void TestCreateConsolAndShipment_CheckValidContainers()
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
			var newContainer1 = Preplanning.Containers.AddNew();
			newContainer1.J1_RC = refContainer.PK;
			newContainer1.J1_ContainerNumber = "CON001";
			Order1Line1.JO_ContainerNumber = newContainer1.J1_ContainerNumber;
			Order1Line2.JO_ContainerNumber = "CON002";
			Order1Line1.JO_ActualWeight = 100m;
			Order1Line2.JO_ActualWeight = 100m;
			Factory.Save();

			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNull(consol);

			ZString expectedError = new ZStringBuilder().Append("Invalid containers found in the following order lines:").Append(String.Format("container CON002 in order line {0}", Order1Line2.JO_LineNo)).ToStringWithNewLineBetweenAppends();
			AssertEquals(expectedError, Notifier.LastMessage);
		}

		public void TestCreateConsolAndShipment_WithPlannedContainers()
		{
			string containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK.ToString();
			string containerType40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK.ToString();

			var orderContainers = new[]
			{
				$"CONT1|{containerType20GP}|2|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|4|SEAL1|SEAL2|SEAL3",
				$"CONT2|{containerType40GP}|3|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|5|SEAL1|SEAL2|SEAL3",
				$"|{containerType40GP}|6|SEAL1|SEAL2|SEAL3",
			};

			Preplanning.Containers.RemoveAndDeleteAll();
			orderContainers.ToList().ForEach(x => CreateNewOrderContainer(Preplanning.Containers, x));
			Factory.Save();

			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);

			Func<ForwardingContainer, string> containerToString = delegate(ForwardingContainer container)
			{
				var result = "";
				result += container.JC_ContainerNum;
				result += "|" + container.JC_RC.ToString();
				result += "|" + container.JC_ContainerCount.ToString();
				result += "|" + container.JC_SealNum;
				result += "|" + container.JC_AdditionalSealNum;
				result += "|" + container.JC_Additional2SealNum;

				return result;
			};

			var expectedContainers = new[]
			{
				$"CONT1|{containerType20GP}|2|SEAL1|SEAL2|SEAL3",
				$"CONT2|{containerType40GP}|3|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|4|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|5|SEAL1|SEAL2|SEAL3",
				$"|{containerType40GP}|6|SEAL1|SEAL2|SEAL3",
			};

			AssertContainsExactElementsInAnyOrder(expectedContainers, consol.Containers.Cast<ForwardingContainer>().Select(x => containerToString(x)));
		}

		public void TestCreateConsolAndShipment_PreEstimateCotainersGrossWeights()
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));

			var newContainer1 = Preplanning.Containers.AddNew();
			newContainer1.J1_RC = refContainer.PK;
			newContainer1.J1_ContainerNumber = "CON0001";

			var newContainer2 = Preplanning.Containers.AddNew();
			newContainer2.J1_RC = refContainer.PK;
			newContainer2.J1_ContainerNumber = "CON0002";

			Order1Line1.JO_ContainerNumber = newContainer1.J1_ContainerNumber;
			Order1Line2.JO_ContainerNumber = newContainer2.J1_ContainerNumber;
			Order1Line1.JO_ActualWeight = 999999m;
			Order1Line2.JO_ActualWeight = 9000m;
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNull(consol);

				var expectedError = "Total actual weight of container CON0001 is too large. Maximum actual weight for this container is 997049.000 KG.";
				AssertEquals(expectedError, Notifier.LastMessage);

				Order1Line1.JO_UnitOfWeight = "G";
				Order1Line2.JO_UnitOfWeight = "T";
				Factory.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNull(consol);

				expectedError = "Total actual weight of container CON0002 is too large. Maximum actual weight for this container is 997049.000 KG.";
				AssertEquals(expectedError, Notifier.LastMessage);

				Order1Line2.JO_UnitOfWeight = "KG";
				Factory.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNotNull(consol);
				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
				AssertEquals(true, consol.IsInDatabase);
				AssertEquals(false, consol.HasChanges);
			}
		}

		public void TestCreateConsolAndShipment_PreEstimateContainersGrossWeightsWithInvalidUnitOfWeight()
		{
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var newContainer = Preplanning.Containers.AddNew();
			newContainer.J1_RC = refContainer.PK;
			newContainer.J1_ContainerNumber = "CON0001";
			newContainer.HasChanges = false;

			Order1Line1.JO_ContainerNumber = newContainer.J1_ContainerNumber;
			Order1Line1.JO_ActualWeight = 100m;
			Order1Line1.JO_UnitOfWeight = "0";
			Order1Line1.HasChanges = false;

			var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNotNull(consol);
		}

		public void TestCreateConsolAndShipment_Success()
		{
			Preplanning.EF_ActualVolume = 30m;
			Preplanning.EF_ActualWeight = 100m;
			Preplanning.EF_Packs = 10;
			Preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			Preplanning.EF_UnitOfWeight = Core.Constants.Weight.LongTons;
			Preplanning.EF_F3_NKPackType = "BOX";
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			var container3 = Preplanning.Containers.AddNew();
			container3.J1_ContainerNumber = ZString.Empty;
			container3.J1_ContainerCount = 3;
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
			container3.J1_RC = refContainer.PK;
			Factory.Save();

			AssertEquals("Container with a number must have count number equals 1", (ZShort)1, Preplanning.Containers[0].J1_ContainerCount);
			AssertEquals("Container with a number must have count number equals 1", (ZShort)1, Preplanning.Containers[1].J1_ContainerCount);
			AssertEquals("Container without a number can have a count number other than 1", (ZShort)3, Preplanning.Containers[2].J1_ContainerCount);

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNotNull(consol);
				AssertEquals(Carrier.PK, consol.ShippingLinePK);
				AssertEquals(Agent1.PK, consol.SendingForwarderPK);
				AssertEquals(Agent2.PK, consol.ReceivingForwarderPK);
				AssertEquals("MASTER", consol.JK_MasterBillNum);
				AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
				AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
				AssertEquals("SEA", consol.JK_TransportMode);
				AssertEquals("FCL", consol.JK_ConsolMode);
				AssertEquals(2, consol.Transports.Count);

				AssertEquals(1, consol.Shipments.Count);

				AssertEquals("SEA", consol.Shipments[0].JS_TransportMode);
				AssertEquals("FCL", consol.Shipments[0].JS_PackingMode);
				AssertEquals("HOUSE", consol.Shipments[0].JS_HouseBill);
				AssertEquals(Buyer.PK, consol.Shipments[0].ConsigneePK);
				AssertEquals(Supplier1.PK, consol.Shipments[0].ConsignorPK);
				AssertEquals("Shipment's service level set from order", "AAA", consol.Shipments[0].JS_RS_NKServiceLevel);
				if (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value)
				{
					AssertEquals("AUSYD", consol.Shipments[0].JS_RL_NKOrigin);
				}
				if (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value)
				{
					AssertEquals("USLAX", consol.Shipments[0].JS_RL_NKDestination);
				}
				AssertEquals(100m, consol.Shipments[0].JS_ActualWeight);
				AssertEquals(30m, consol.Shipments[0].JS_ActualVolume);
				AssertEquals(10, consol.Shipments[0].JS_OuterPacks);
				AssertEquals(Core.Constants.Weight.LongTons, consol.Shipments[0].JS_UnitOfWeight);
				AssertEquals(Core.Constants.Volume.CubicFeet, consol.Shipments[0].JS_UnitOfVolume);
				AssertEquals("BOX", consol.Shipments[0].JS_F3_NKPackType);

				AssertEquals(consol.Shipments[0].PK, Order1.JD_JS);

				AssertEquals(0, consol.Shipments[0].Declarations.Length);

				AssertEquals(Preplanning.Containers.Count, consol.Containers.Count);
				AssertEquals(Preplanning.Containers[0].J1_ContainerNumber, consol.Containers[0].JC_ContainerNum);
				AssertEquals((ZShort)1, consol.Containers[0].JC_ContainerCount);
				AssertEquals(Preplanning.Containers[0].J1_RC, consol.Containers[0].JC_RC);
				AssertEquals(Preplanning.Containers[0].J1_SealNum, consol.Containers[0].JC_SealNum);
				AssertEquals(Preplanning.Containers[0].J1_AdditionalSealNum, consol.Containers[0].JC_AdditionalSealNum);
				AssertEquals(Preplanning.Containers[0].J1_Additional2SealNum, consol.Containers[0].JC_Additional2SealNum);
				AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[0].JC_ContainerMode);

				AssertEquals(Preplanning.Containers[1].J1_ContainerNumber, consol.Containers[1].JC_ContainerNum);
				AssertEquals((ZShort)1, consol.Containers[1].JC_ContainerCount);
				AssertEquals(Preplanning.Containers[1].J1_RC, consol.Containers[1].JC_RC);
				AssertEquals(Preplanning.Containers[1].J1_SealNum, consol.Containers[1].JC_SealNum);
				AssertEquals(Preplanning.Containers[1].J1_AdditionalSealNum, consol.Containers[1].JC_AdditionalSealNum);
				AssertEquals(Preplanning.Containers[1].J1_Additional2SealNum, consol.Containers[1].JC_Additional2SealNum);
				AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[1].JC_ContainerMode);

				AssertEquals(Preplanning.Containers[2].J1_ContainerNumber, consol.Containers[2].JC_ContainerNum);
				AssertEquals((ZShort)3, consol.Containers[2].JC_ContainerCount);
				AssertEquals(Preplanning.Containers[2].J1_RC, consol.Containers[2].JC_RC);
				AssertEquals(Preplanning.Containers[2].J1_SealNum, consol.Containers[2].JC_SealNum);
				AssertEquals(Preplanning.Containers[2].J1_AdditionalSealNum, consol.Containers[2].JC_AdditionalSealNum);
				AssertEquals(Preplanning.Containers[2].J1_Additional2SealNum, consol.Containers[2].JC_Additional2SealNum);
				AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[2].JC_ContainerMode);

				AssertEquals("FCA", consol.Shipments[0].JS_INCO);
				AssertEquals(consol.Shipments[0].PK, Preplanning.EF_JS);
				AssertEquals(120.0000m, consol.Shipments[0].JS_GoodsValue);

				AssertEquals(3, consol.Containers.Count);

				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
				AssertEquals(true, consol.IsInDatabase);
				AssertEquals(false, consol.HasChanges);

				AssertEquals("still 2 orders", 2, Preplanning.Orders.Count);
			}
		}

		public void TestCreateConsolAndShipment_ReportErrorMessage_WhenConsolHasErrors()
		{
			Preplanning.EF_ActualVolume = 30m;
			Preplanning.EF_ActualWeight = 100m;
			Preplanning.EF_Packs = 10;
			Preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			Preplanning.EF_UnitOfWeight = Core.Constants.Weight.LongTons;
			Preplanning.EF_F3_NKPackType = "BOX";
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				var consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);
				AssertNotNull(consol);

				AssertEquals("Error: Consol could not be saved because of validation errors.", Notifier.LastMessage);
				AssertEquals(false, consol.IsInDatabase);
			}
		}

		public void TestCreateConsolAndShipment_TransportMode_Courier()
		{
			var preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();

			var order = preplanning.Orders.AddNew();
			order.JD_TransportMode = Core.Constants.TransportModes.Courier;

			var transport = preplanning.PreAdviceTransports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Courier;

			Factory.Save();

			var consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNotNull(consol);
			AssertEquals(Core.Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals(Core.Constants.AgentType.Courier, consol.JK_AgentType);
			AssertEquals(Core.Constants.ContainerModes.Other, consol.JK_ConsolMode);

			AssertEquals(Core.Constants.TransportModes.Air, consol.Transports[0].JW_TransportMode);
		}

		public void TestHouseBillMaxLength()
		{
			var notificationBuffer = new NotificationBuffer();

			Preplanning.EF_HouseBill = "ASDFGHJKLPOIUYTREWQZXCVBN";
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, notificationBuffer, TryToFixConsolErrors);
			Assert(notificationBuffer.AsString.Contains("House bill number has been truncated to fit shipment limit of 20 characters."));
			AssertEquals("ASDFGHJKLPOIUYTREWQZ", consol.Shipments[0].JS_HouseBill);

			Order1.JD_Waybill = "ASDFGHJKLPOIUYTREWQZXCCC";
			DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
			Preplanning.EF_HouseBill = "";
			Factory.Save();
			var factoryForConsol = consol.Factory;
			consol.Delete();
			factoryForConsol.Save();

			consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, notificationBuffer, TryToFixConsolErrors);
			Assert(notificationBuffer.AsString.Contains("House bill number has been truncated to fit shipment limit of 20 characters."));
			AssertEquals("House Bill from order", "ASDFGHJKLPOIUYTREWQZ", consol.Shipments[0].JS_HouseBill);
		}

		public void TestCreateConsolAndShipment_WithBlankMasterBill_Success()
		{
			Preplanning.EF_ActualVolume = 30m;
			Preplanning.EF_ActualWeight = 100m;
			Preplanning.EF_Packs = 10;
			Preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			Preplanning.EF_UnitOfWeight = Core.Constants.Weight.LongTons;
			Preplanning.EF_F3_NKPackType = "BOX";
			Preplanning.EF_MasterBill = "";
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNotNull(consol);
				AssertEquals(Carrier.PK, consol.ShippingLinePK);
				AssertEquals(Agent1.PK, consol.SendingForwarderPK);
				AssertEquals(Agent2.PK, consol.ReceivingForwarderPK);
				AssertEquals("", consol.JK_MasterBillNum);
				AssertEquals("AUSYD", consol.JK_RL_NKLoadPort);
				AssertEquals("USLAX", consol.JK_RL_NKDischargePort);
				AssertEquals("SEA", consol.JK_TransportMode);
				AssertEquals("FCL", consol.JK_ConsolMode);
				AssertEquals(2, consol.Transports.Count);

				AssertEquals(1, consol.Shipments.Count);

				AssertEquals("SEA", consol.Shipments[0].JS_TransportMode);
				AssertEquals("FCL", consol.Shipments[0].JS_PackingMode);
				AssertEquals("HOUSE", consol.Shipments[0].JS_HouseBill);
				AssertEquals(Buyer.PK, consol.Shipments[0].ConsigneePK);
				AssertEquals(Supplier1.PK, consol.Shipments[0].ConsignorPK);
				if (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.Value)
				{
					AssertEquals("AUSYD", consol.Shipments[0].JS_RL_NKOrigin);
				}
				if (Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.Value)
				{
					AssertEquals("USLAX", consol.Shipments[0].JS_RL_NKDestination);
				}
				AssertEquals(100m, consol.Shipments[0].JS_ActualWeight);
				AssertEquals(30m, consol.Shipments[0].JS_ActualVolume);
				AssertEquals(10, consol.Shipments[0].JS_OuterPacks);
				AssertEquals(Core.Constants.Weight.LongTons, consol.Shipments[0].JS_UnitOfWeight);
				AssertEquals(Core.Constants.Volume.CubicFeet, consol.Shipments[0].JS_UnitOfVolume);
				AssertEquals("BOX", consol.Shipments[0].JS_F3_NKPackType);

				AssertEquals(consol.Shipments[0].PK, Order1.JD_JS);

				AssertEquals(0, consol.Shipments[0].Declarations.Length);

				AssertEquals(Preplanning.Containers.Count, consol.Containers.Count);
				AssertEquals(Preplanning.Containers[0].J1_ContainerNumber, consol.Containers[0].JC_ContainerNum);
				AssertEquals(Preplanning.Containers[0].J1_RC, consol.Containers[0].JC_RC);
				AssertEquals(Preplanning.Containers[0].J1_SealNum, consol.Containers[0].JC_SealNum);
				AssertEquals(Preplanning.Containers[0].J1_AdditionalSealNum, consol.Containers[0].JC_AdditionalSealNum);
				AssertEquals(Preplanning.Containers[0].J1_Additional2SealNum, consol.Containers[0].JC_Additional2SealNum);
				AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[0].JC_ContainerMode);

				AssertEquals(Preplanning.Containers[1].J1_ContainerNumber, consol.Containers[1].JC_ContainerNum);
				AssertEquals(Preplanning.Containers[1].J1_RC, consol.Containers[1].JC_RC);
				AssertEquals(Preplanning.Containers[1].J1_SealNum, consol.Containers[1].JC_SealNum);
				AssertEquals(Preplanning.Containers[1].J1_AdditionalSealNum, consol.Containers[1].JC_AdditionalSealNum);
				AssertEquals(Preplanning.Containers[1].J1_Additional2SealNum, consol.Containers[1].JC_Additional2SealNum);
				AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[1].JC_ContainerMode);

				AssertEquals("FCA", consol.Shipments[0].JS_INCO);
				AssertEquals(consol.Shipments[0].PK, Preplanning.EF_JS);
				AssertEquals(120.0000m, consol.Shipments[0].JS_GoodsValue);

				AssertEquals(2, consol.Containers.Count);

				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
				AssertEquals(true, consol.IsInDatabase);
				AssertEquals(false, consol.HasChanges);

				AssertEquals("still 2 orders", 2, Preplanning.Orders.Count);
			}
		}

		public void TestCreateConsolAndShipment_TransferDataToShipment()
		{
			Order1.JD_ActualVolume = 10m;
			Order1.JD_ActualWeight = 20m;
			Order1.JD_Packs = 30;
			Order1.JD_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			Order1.JD_UnitOfWeight = Core.Constants.Weight.LongTons;
			Order1.JD_F3_NKPackType = "ZZ";
			Order1.JD_Waybill = "HOUSEBILL";
			Order1.JD_OrderGoodsDescription = "Goods description";
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Preplanning.EF_HouseBill = "";
			Factory.Save();

			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
			AssertNotNull(consol);

			AssertEquals(10m, consol.Shipments[0].JS_ActualVolume);
			AssertEquals(20m, consol.Shipments[0].JS_ActualWeight);
			AssertEquals(30, consol.Shipments[0].JS_OuterPacks);
			AssertEquals(Core.Constants.Weight.LongTons, consol.Shipments[0].JS_UnitOfWeight);
			AssertEquals(Core.Constants.Volume.CubicFeet, consol.Shipments[0].JS_UnitOfVolume);
			AssertEquals("ZZ", consol.Shipments[0].JS_F3_NKPackType);

			AssertEquals("Goods description", consol.Shipments[0].JS_GoodsDescription);
			AssertEquals("House Bill from order", "HOUSEBILL", consol.Shipments[0].JS_HouseBill);
		}

		public void TestCreateConsolAndShipment_BuyersConsol()
		{
			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);
			AssertNotNull(consol);

			AssertEquals(2, consol.Shipments.Count);
			AssertEquals(Core.Constants.ContainerModes.BuyersConsol, consol.JK_ConsolMode);

			AssertEquals(Buyer.PK, consol.Shipments[0].ConsigneePK);
			AssertEquals(Supplier1.PK, consol.Shipments[0].ConsignorPK);

			AssertEquals(Buyer.PK, consol.Shipments[1].ConsigneePK);
			AssertEquals(Supplier2.PK, consol.Shipments[1].ConsignorPK);
		}

		public void TestCreateConsolAndShipment_ContainerMode()
		{
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("FCL", consol.JK_ConsolMode);
				AssertEquals("FCL", consol.Shipments[0].JS_PackingMode);

				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.Containers.RemoveAndDeleteAll();
				Preplanning.Orders[0].JD_ContainerMode = Core.Constants.ContainerModes.FCL;
				Factory.Save();
				var factoryfromConsol = consol.Factory;
				consol.Delete();
				factoryfromConsol.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("Consol mode comes from the first order's container mode on preplanning", Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
				AssertEquals("Shipment packing mode comes from the first order's container mode on preplanning", Core.Constants.ContainerModes.FCL, consol.Shipments[0].JS_PackingMode);

				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.Containers.RemoveAndDeleteAll();
				Preplanning.Orders[0].JD_ContainerMode = "";
				Factory.Save();
				factoryfromConsol = consol.Factory;
				consol.Delete();
				factoryfromConsol.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("Default consol mode for sea when no containers and no orders", Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
				AssertEquals("Default shipment packing mode for sea when no containers and no orders", Core.Constants.ContainerModes.LCL, consol.Shipments[0].JS_PackingMode);

				Preplanning.Orders[1].Delete();
				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.Containers.RemoveAndDeleteAll();
				Preplanning.Orders[0].JD_ContainerMode = Core.Constants.ContainerModes.LCL;
				var container3 = Preplanning.Containers.AddNew();
				var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
				container3.J1_RC = refContainer.PK;
				container3.J1_ContainerCount = 2;
				Factory.Save();
				factoryfromConsol = consol.Factory;
				consol.Delete();
				factoryfromConsol.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("Default consol mode for sea when no containers and no orders", Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
				AssertEquals("Default shipment packing mode for sea when no containers and no orders", Core.Constants.ContainerModes.LCL, consol.Shipments[0].JS_PackingMode);

				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.Containers.RemoveAndDeleteAll();
				Preplanning.Orders[0].JD_ContainerMode = Core.Constants.ContainerModes.AgentConsol;
				Factory.Save();
				factoryfromConsol = consol.Factory;
				consol.Delete();
				factoryfromConsol.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("Container mode should fallback to Groupage", Core.Constants.ContainerModes.Groupage, consol.JK_ConsolMode);
				AssertEquals("Shipment PackingMode should be LCL", Core.Constants.ContainerModes.LCL, consol.Shipments[0].JS_PackingMode);

				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.Containers.RemoveAndDeleteAll();
				Preplanning.Orders[0].JD_ContainerMode = Core.Constants.ContainerModes.Mail;
				Factory.Save();
				factoryfromConsol = consol.Factory;
				consol.Delete();
				factoryfromConsol.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals("Container mode should fallback to OTH", Core.Constants.ContainerModes.Other, consol.JK_ConsolMode);
				AssertEquals("Shipment PackingMode should be default", Core.Constants.ContainerModes.FCL, consol.Shipments[0].JS_PackingMode);
			}
		}

		public void TestCreateConsolAndShipment_ContainerMode_SameWithDefault()
		{
			var preplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preplanning.PreAdviceTransports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			preplanning.Containers.AddNew();

			var order = preplanning.Orders.AddNew();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.LCL;

			Factory.Save();

			var consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);
			AssertNotNull(consol);
			AssertEquals(Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.LCL, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.ContainerModes.LCL, consol.Containers[0].JC_ContainerMode);

			DetachShipmentButReAttachOrdersToPreAdvice(preplanning);
			consol.Delete();
			preplanning.PreAdviceTransports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			order.JD_ContainerMode = Core.Constants.ContainerModes.Loose;
			Factory.Save();
			consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);

			AssertNotNull(consol);
			AssertEquals(Core.Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Loose, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.ContainerModes.ULD, consol.Containers[0].JC_ContainerMode);

			DetachShipmentButReAttachOrdersToPreAdvice(preplanning);
			consol.Delete();
			preplanning.PreAdviceTransports[0].JW_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			consol = preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier);

			AssertNotNull(consol);
			AssertEquals(Core.Constants.TransportModes.Sea, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.ContainerModes.FCL, consol.Containers[0].JC_ContainerMode);
		}

		void DetachShipmentButReAttachOrdersToPreAdvice(JobShipmentPreplanning preadvice)
		{
			preadvice.EF_JS = ZGuid.Empty;
			foreach (Order order in preadvice.Orders.ToArray())
			{
				order.JD_JS = ZGuid.Empty;
				order.JD_EF_ShipmentPrePlanning = preadvice.PK;
			}
		}

		public void TestCreateConsolAndShipment_Declaration()
		{
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Preplanning.EF_RL_NKPortDisch = "AUMEL";
			Factory.Save();

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals(0, consol.Shipments[0].Declarations.Length);
				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Factory.Save();
				var factoryForConsol = consol.Factory;
				consol.Delete();
				factoryForConsol.Save();

				Buyer.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty, "AUMEL");
				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals(0, consol.Shipments[0].Declarations.Length);
				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Factory.Save();
				factoryForConsol = consol.Factory;
				consol.Delete();
				factoryForConsol.Save();

				Buyer.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty, "AU");
				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals(1, consol.Shipments[0].Declarations.Length);
				AssertEquals(true, consol.Shipments[0].Declarations[0].IsInDatabase);
				AssertEquals(2, consol.Containers.Count);

				ZQuery query = new ZQuery(CusContainerSchema.CO_JE, consol.Shipments[0].Declarations[0].PK);
				BusinessObject[] decContainers = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
				AssertEquals("Will appear FROM the shipment", 0, decContainers.Length);

				AssertEquals(consol.Shipments[0].PK, Preplanning.EF_JS);
				AssertEquals(consol.Shipments[0].Declarations[0].PK, Preplanning.EF_JE);
			}
		}

		public void TestCreateConsolAndShipment_ForceOneHouseBill()
		{
			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				Buyer.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, TryToFixConsolErrors);
				AssertEquals(1, consol.Shipments.Count);
				AssertEquals(1, consol.Shipments[0].Declarations.Length);
				AssertEquals(true, consol.Shipments[0].Declarations[0].IsInDatabase);
				AssertEquals(2, consol.Containers.Count);

				ZQuery query = new ZQuery(CusContainerSchema.CO_JE, consol.Shipments[0].Declarations[0].PK);
				BusinessObject[] decContainers = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
				AssertEquals("Will appear FROM the shipment", 0, decContainers.Length);

				AssertEquals(consol.Shipments[0].PK, Preplanning.EF_JS);
				AssertEquals(consol.Shipments[0].Declarations[0].PK, Preplanning.EF_JE);

				DetachShipmentButReAttachOrdersToPreAdvice(Preplanning);
				Preplanning.EF_JE = ZGuid.Empty;
				Preplanning.Orders[0].JD_JE = ZGuid.Empty;
				Preplanning.Orders[1].JD_JE = ZGuid.Empty;
				Preplanning.EF_MasterBill = "NewMast";
				Factory.Save();

				consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertEquals(2, consol.Shipments.Count);
				AssertEquals(1, consol.Shipments[0].Declarations.Length);
				AssertEquals(1, consol.Shipments[1].Declarations.Length);
				AssertEquals(true, consol.Shipments[0].Declarations[0].IsInDatabase);
				AssertEquals(true, consol.Shipments[1].Declarations[0].IsInDatabase);
				AssertEquals(2, consol.Containers.Count);

				AssertEquals("more than 1 shipment, so do not set", ZGuid.Empty, Preplanning.EF_JS);
				AssertEquals("more than 1 dec, so do not set", ZGuid.Empty, Preplanning.EF_JE);
			}
		}

		public void TestCreateConsolAndShipment_HousebillPerOrder()
		{
			Preplanning.Orders[1].SupplierPK = Supplier1.PK;
			Factory.Save();
			AssertEquals("Precondition - same buyer/supplier on both orders", Preplanning.Orders[0].SupplierPK, Preplanning.Orders[1].SupplierPK);
			AssertEquals("Precondition - same buyer/supplier on both orders", Preplanning.Orders[0].BuyerPK, Preplanning.Orders[1].BuyerPK);

			Buyer.SetRelatedParty(GlbCompany.CurrentCompany.OrgProxy, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);
			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerOrder, Notifier);
			AssertEquals(2, consol.Shipments.Count);
		}

		public void TestAppendShipmentToConsolWhenDefaultsFromConsolLoadAreTrue()
		{
			Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Preplanning.EF_RL_NKPortLoad = "MYPKG";
			Preplanning.EF_RL_NKPortDisch = "AUSYD";
			Factory.Save();

			NotificationBuffer notifications = new NotificationBuffer();
			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, notifications);
			AssertNotNull("Error making consol/shipment - " + notifications.AsString, consol);
			AssertEquals("Origin set from preadvice load", "MYPKG", consol.Shipments[0].JS_RL_NKOrigin);
			AssertEquals("Destination set from preadvice discharge", "AUSYD", consol.Shipments[0].JS_RL_NKDestination);
		}

		public void TestAppendShipmentToConsolWhenDefaultsFromConsolLoadAreFalse()
		{
			Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Enterprise.Registry.Business.FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Preplanning.EF_RL_NKPortLoad = "MYPKG";
			Preplanning.EF_RL_NKPortDisch = "AUSYD";
			Order preAdviceOrder = Preplanning.Orders.AddNew();
			preAdviceOrder.BuyerPK = Buyer.PK;
			preAdviceOrder.JD_OrderNumber = "999333";
			preAdviceOrder.SupplierPK = Supplier1.PK;
			Factory.Save();

			NotificationBuffer notifications = new NotificationBuffer();
			ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, notifications);
			AssertNotNull("Error making consol/shipment - " + notifications.AsString, consol);
			AssertEquals("Origin set from preadvice load", Supplier1.OH_RL_NKClosestPort, consol.Shipments[0].JS_RL_NKOrigin);
			AssertEquals("Destination set from preadvice discharge", Buyer.OH_RL_NKClosestPort, consol.Shipments[0].JS_RL_NKDestination);
		}

		public void TestMatchingPreAdvices_WhenNoMasterBill()
		{
			//Preplanning - 1 already Exists

			JobShipmentPreplanning preplanning2 = Factory.New<JobShipmentPreplanning>();
			preplanning2.BuyerPK = Preplanning.BuyerPK;
			preplanning2.EF_MasterBill = Preplanning.EF_MasterBill;
			Order preAdvice2Order = preplanning2.Orders.AddNew();
			preAdvice2Order.BuyerPK = Buyer.PK;
			preAdvice2Order.JD_OrderNumber = "353535";
			Transport transport = preplanning2.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";

			JobShipmentPreplanning preplanning3 = Factory.New<JobShipmentPreplanning>();
			preplanning3.BuyerPK = Preplanning.BuyerPK;
			preplanning3.EF_MasterBill = "";
			Order preAdvice3Order = preplanning3.Orders.AddNew();
			preAdvice3Order.BuyerPK = Buyer.PK;
			preAdvice3Order.JD_OrderNumber = "454545";

			JobShipmentPreplanning preplanning4 = Factory.New<JobShipmentPreplanning>();
			preplanning4.BuyerPK = Preplanning.BuyerPK;
			preplanning4.EF_MasterBill = "";
			Order preAdvice4Order = preplanning4.Orders.AddNew();
			preAdvice4Order.BuyerPK = Buyer.PK;
			preAdvice4Order.JD_OrderNumber = "565656";

			Factory.Save();

			AssertEquals("Matches on itself and other with same masterbill", 2, Preplanning.MatchingPreAdvices.Count);
			AssertEquals("Matches on itself and other with same masterbill", 2, preplanning2.MatchingPreAdvices.Count);
			AssertEquals("Only matches on itself", 1, preplanning3.MatchingPreAdvices.Count);
			AssertEquals("Only matches on itself", preplanning3, preplanning3.MatchingPreAdvices[0]);
			AssertEquals("Only matches on itself", 1, preplanning4.MatchingPreAdvices.Count);
			AssertEquals("Only matches on itself", preplanning4, preplanning4.MatchingPreAdvices[0]);
		}

		public void TestCreateConsolAndShipment_MultiplePreAdvices()
		{
			JobShipmentPreplanning preplanning2 = Factory.New<JobShipmentPreplanning>();
			preplanning2.BuyerPK = Preplanning.BuyerPK;
			preplanning2.EF_MasterBill = Preplanning.EF_MasterBill;

			Transport transport = preplanning2.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "GRUMANT";
			transport.JW_VoyageFlight = "QF123";

			Order preAdvice2Order = preplanning2.Orders.AddNew();
			preAdvice2Order.BuyerPK = Buyer.PK;
			preAdvice2Order.JD_OrderNumber = "353535";

			JobShipmentPreplanning preplanning3 = Factory.New<JobShipmentPreplanning>();
			preplanning3.BuyerPK = Preplanning.BuyerPK;
			preplanning3.EF_MasterBill = "CRAP";

			Transport transport2 = preplanning3.PreAdviceTransports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_Vessel = "NOONE";
			transport2.JW_VoyageFlight = "QF124";

			Order preAdvice3Order = preplanning3.Orders.AddNew();
			preAdvice3Order.BuyerPK = Buyer.PK;
			preAdvice3Order.JD_OrderNumber = "999333";

			Factory.Save();

			AssertEquals(2, Preplanning.MatchingPreAdvices.Count);

			using (FreightDataRegistry.Instance.ReleaseType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.ShipmentReleaseTypes.BankLetterOfCredit))
			{
				ForwardingConsol consol = Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, TryToFixConsolErrors);
				AssertNotNull(consol);
				AssertNotEquals(ZGuid.Empty, preAdvice2Order.JD_JS);
				AssertEquals(false, preAdvice2Order.HasChanges);

				AssertEquals(consol.HumanReadableName + " and shipment(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
			}
		}

		public void TestCreateConsolAndShipment_LinkedTransport()
		{
			JobShipmentPreplanning preplanning = GetPreAdviceWithLinkedTransport();
			Factory.Save();

			AssertEquals("From a linked transport, itself is found", 1, preplanning.MatchingPreAdvices.Count);

			JobShipmentPreplanning preplanning2 = GetPreAdviceWithUnLinkedTransport(preplanning.PreAdviceTransports[0].JW_RL_NKDiscPort);
			Factory.Save();

			AssertEquals("From a linked transport, both it and the unlinked transport are found", 2, preplanning.MatchingPreAdvices.Count);
			AssertEquals("From an unlinked transport, both it and the linked transport are found", 2, preplanning2.MatchingPreAdvices.Count);
		}

		public void TestCreateConsolAndShipment_DoesNotThrowOutOfRangeExceptionWhenTransportModeIsInvalid()
		{
			Preplanning.PreAdviceTransports.RemoveAll();
			AssertEquals(0, Preplanning.PreAdviceTransports.Count);

			var transport = Preplanning.PreAdviceTransports.AddNew();
			transport.JW_TransportMode = "ABC";
			transport.JW_RL_NKDiscPort = "AUSYD";
			Preplanning.EF_RL_NKPortDisch = "AUSYD";
			Factory.Save();

			AssertNoExceptionThrown(() => Preplanning.CreateConsolAndShipment(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier));
		}

		#region CreateShipmentAndAttachToConsol

		public void TestCreateShipmentAndAttachToConsol_SingleShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Factory.Save();

			var populatedConsol = Preplanning.CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, Notifier, consol);

			AssertEquals(consol.PK, populatedConsol.PK);
			AssertEquals("One shipment should be created.", 1, populatedConsol.ShipmentCount);

			var onlyShipment = (ForwardingShipment)populatedConsol.Shipments.First();
			var firstOrder = Preplanning.Orders.First();

			AssertEquals(onlyShipment.ConsigneeDocumentaryAddress.OrganisationPK, firstOrder.BuyerPK);
			AssertEquals(onlyShipment.ConsignorDocumentaryAddress.OrganisationPK, firstOrder.SupplierPK);

			AssertEquals(2, populatedConsol.Containers.Count);
			AssertEquals(2, populatedConsol.Transports.Count);
		}

		public void TestCreateShipmentAndAttachToConsol_ShipmentPerBuyerSupplier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var existingBuyerSupplierOrder = Preplanning.Orders.AddNew();
			existingBuyerSupplierOrder.JD_OrderNumber = "ORDER3";
			existingBuyerSupplierOrder.BuyerPK = Buyer.PK;
			existingBuyerSupplierOrder.SupplierPK = Supplier2.PK;
			existingBuyerSupplierOrder.JD_RL_NKGoodsAvailableAt = "AUSYD";
			existingBuyerSupplierOrder.JD_RL_NKGoodsDeliveredTo = "USLAX";
			existingBuyerSupplierOrder.JD_IncoTerm = "EXW";
			existingBuyerSupplierOrder.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			existingBuyerSupplierOrder.JD_RN_NKCountryOfSupply = "IN";

			Factory.Save();

			AssertEquals("Precondition: Preplanning has 3 orders", 3, Preplanning.Orders.Count);

			var populatedConsol = Preplanning.CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, Notifier, consol);

			AssertEquals(consol.PK, populatedConsol.PK);
			AssertEquals("Two shipments should be created.", 2, populatedConsol.ShipmentCount);

			var buyerSupplierSet = new HashSet<string>();

			foreach (var order in Preplanning.Orders)
			{
				buyerSupplierSet.Add(order.BuyerPK.ToStringKey() + order.SupplierPK.ToStringKey());
			}

			AssertEquals("BuyerSupplier set should have 2 items", 2, buyerSupplierSet.Count);

			foreach (var shipment in populatedConsol.Shipments.OfType<ForwardingShipment>())
			{
				Assert("Each shipment should be made from buyer supplier pair", buyerSupplierSet.Contains(shipment.ConsigneeDocumentaryAddress.OrganisationPK.ToStringKey() + shipment.ConsignorDocumentaryAddress.OrganisationPK.ToStringKey()));
			}

			AssertEquals(2, populatedConsol.Containers.Count);
			AssertEquals(2, populatedConsol.Transports.Count);
		}

		public void TestCreateShipmentAndAttachToConsol_ShipmentPerOrder()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var newOrder = Preplanning.Orders.AddNew();
			newOrder.JD_OrderNumber = "ORDER3";
			newOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			newOrder.SupplierPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			newOrder.JD_IncoTerm = "EXW";
			newOrder.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			newOrder.JD_RN_NKCountryOfSupply = "IN";

			Factory.Save();

			AssertEquals("Precondition: Preplanning has 3 orders", 3, Preplanning.Orders.Count);

			var populatedConsol = Preplanning.CreateShipmentAndAttachToConsol(JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerOrder, Notifier, consol);

			AssertEquals(consol.PK, populatedConsol.PK);
			AssertEquals("Shipment should be created for each order", 3, populatedConsol.ShipmentCount);

			var shipmentDetails = populatedConsol.Shipments
				.Cast<ForwardingShipment>()
				.Select(shipment => Tuple.Create
				(shipment.ConsigneeDocumentaryAddress.OrganisationPK,
				shipment.ConsignorDocumentaryAddress.OrganisationPK)).ToArray();

			foreach (var order in Preplanning.Orders)
			{
				Assert(shipmentDetails.Contains(Tuple.Create
					(order.BuyerPK,
					order.SupplierPK)));
			}

			AssertEquals(2, populatedConsol.Containers.Count);
			AssertEquals(2, populatedConsol.Transports.Count);
		}

		#endregion

		#region Implementation

		JobShipmentPreplanning GetPreAdviceWithLinkedTransport()
		{
			JobShipmentPreplanning result = Factory.New<JobShipmentPreplanning>();
			result.BuyerPK = Preplanning.BuyerPK;
			result.EF_MasterBill = Preplanning.EF_MasterBill;

			Transport transport = result.PreAdviceTransports[0];
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "VESSELX";
			transport.JW_VoyageFlight = "QF9911";
			transport.JW_IsLinked = true;
			transport.JW_JX = Factory.NewWithValidTestData<JobSailing>().PK;
			transport.JW_Vessel = "VESSELX";
			transport.JW_VoyageFlight = "QF9911";

			result.EF_RL_NKPortDisch = transport.JW_RL_NKDiscPort;

			return result;
		}

		JobShipmentPreplanning GetPreAdviceWithUnLinkedTransport(ZString dischargePort)
		{
			JobShipmentPreplanning result = Factory.New<JobShipmentPreplanning>();
			result.BuyerPK = Preplanning.BuyerPK;
			result.EF_MasterBill = Preplanning.EF_MasterBill;

			Transport transport = result.PreAdviceTransports[0];
			transport.JW_IsLinked = false;
			transport.JW_TransportMode = "SEA";
			transport.JW_Vessel = "VESSELX";
			transport.JW_VoyageFlight = "QF9911";
			transport.JW_RL_NKDiscPort = dischargePort;

			result.EF_RL_NKPortDisch = transport.JW_RL_NKDiscPort;

			return result;
		}

		#endregion

		#endregion

		#region Create Declaration

		public void TestCreateBrokerage_HasChanges()
		{
			Preplanning.EF_ActualVolume = 10m;
			BusinessObject dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNull(dec);
			AssertEquals("Please save your changes before performing any Actions on this Pre Advice.", Notifier.LastMessage);
		}

		public void TestCreateBrokerage_HasLinkToOperationsJob()
		{
			Preplanning.EF_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			Factory.Save();

			BusinessObject consol = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNull(consol);
			AssertEquals("This Pre Advice has already been linked to an operations job and cannot be linked again.", Notifier.LastMessage);
		}

		public void TestCreateBrokerage_ExistingConsol()
		{
			BusinessObject existingDec = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			existingDec[JobDeclarationSchema.JE_MasterBill] = "MASTER";
			existingDec[JobDeclarationSchema.JE_HouseBill] = "HOUSE";
			existingDec[JobDeclarationSchema.JE_OH_Importer] = Buyer.PK;
			Factory.Save();

			Notifier.NextQueryUserResult = false;
			BusinessObject dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNull(dec);
			AssertEquals("A declaration already exists for this Master Bill, House Bill and Buyer. If you continue, a new declaration will be created.\r\n\r\nDo you want to continue?", Notifier.LastMessage);
			AssertEquals(false, Notifier.LastQueryUserResult);
			Notifier.LastMessage = "";

			Notifier.NextQueryUserResult = true;
			dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNotNull(dec);
			AssertEquals(dec.HumanReadableName + " and commercial invoice line(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
			AssertEquals(false, Notifier.LastQueryUserResult);
			Notifier.LastMessage = "";
		}

		public void TestCreateBrokerage_Success()
		{
			Preplanning.EF_ActualWeight = 100m;
			Preplanning.EF_UnitOfWeight = Core.Constants.Weight.Grams;
			Preplanning.EF_ActualVolume = 200m;
			Preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			Preplanning.EF_Packs = 300;
			Preplanning.EF_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			Factory.Save();

			var dec = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNotNull(dec);

			AssertEquals(Carrier.PK, dec[JobDeclarationSchema.JE_OH_ShippingLine]);
			AssertEquals(Agent2.PK, dec[JobDeclarationSchema.JE_OH_Forwarder]);
			AssertEquals("AUSYD", dec[JobDeclarationSchema.JE_RL_NKPortOfLoading]);
			AssertEquals("USLAX", dec[JobDeclarationSchema.JE_RL_NKPortOfArrival]);
			AssertEquals("MASTER", dec[JobDeclarationSchema.JE_MasterBill]);
			AssertEquals("HOUSE", dec[JobDeclarationSchema.JE_HouseBill]);
			AssertEquals(Buyer.PK, dec[JobDeclarationSchema.JE_OH_Importer]);
			AssertEquals(ZGuid.Empty, dec[JobDeclarationSchema.JE_JS]);
			AssertEquals("GRUMANT", dec[JobDeclarationSchema.JE_VesselName]);

			AssertEquals(Supplier1.PK, dec[JobDeclarationSchema.JE_OH_Supplier]);
			AssertEquals("AUSYD", dec[JobDeclarationSchema.JE_RL_NKOrigin]);
			AssertEquals("USLAX", dec[JobDeclarationSchema.JE_RL_NKFinalDestination]);
			AssertEquals(dec.PK, Order1.JD_JE);
			AssertEquals(dec.PK, Order2.JD_JE);

			AssertEquals(100m, dec[JobDeclarationSchema.JE_TotalWeight]);
			AssertEquals(Core.Constants.Weight.Grams, dec[JobDeclarationSchema.JE_TotalWeightUnit]);
			AssertEquals(200m, dec[JobDeclarationSchema.JE_TotalVolume]);
			AssertEquals(Core.Constants.Volume.CubicDecimetres, dec[JobDeclarationSchema.JE_TotalVolumeUnit]);
			AssertEquals(300, dec[JobDeclarationSchema.JE_TotalNoOfPacks]);
			AssertEquals(Core.Constants.PkgUnit.Piece, dec[JobDeclarationSchema.JE_TotalNoOfPacksPackType]);

			var invoiceHeaders = GetInvoiceHeadersForDec(dec.PK);
			AssertEquals(2, invoiceHeaders.Length);
			AssertEquals("COMM1", invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_InvoiceNumber]);
			AssertEquals("COMM2", invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_InvoiceNumber]);

			AssertEquals(dec.PK, invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_JE]);
			AssertEquals(dec.PK, invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_JE]);
			AssertEquals(false, ((ZGuid)invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK]).IsEmpty);
			AssertEquals(false, ((ZGuid)invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK]).IsEmpty);

			AssertEquals(Order1.SupplierPK, invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_OH_Supplier]);
			AssertEquals(Order2.SupplierPK, invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_OH_Supplier]);

			AssertEquals(Order1.JD_RX_NKOrderCurrency, invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency]);
			AssertEquals(Order2.JD_RX_NKOrderCurrency, invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_RX_NKInvoice_Currency]);

			AssertEquals(Order1.JD_RN_NKCountryOfSupply, invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin]);
			AssertEquals(Order2.JD_RN_NKCountryOfSupply, invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin]);

			AssertEquals(Order1.JD_IncoTerm, invoiceHeaders[0][JobComInvoiceHeaderSchema.JZ_IncoTerm]);
			AssertEquals(Order2.JD_IncoTerm, invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_IncoTerm]);

			var invoice1Lines = GetInvoiceLinesForHeader(invoiceHeaders[0].PK);
			AssertEquals(1, invoice1Lines.Length);

			AssertEquals("ORDER1", invoice1Lines[0][JobComInvoiceLineSchema.JI_OrderNumber]);
			AssertEquals(10m, invoice1Lines[0][JobComInvoiceLineSchema.JI_InvoiceQuantity]);
			AssertEquals("Order 1 Line 1", invoice1Lines[0][JobComInvoiceLineSchema.JI_Description]);
			AssertEquals("Unit of Qty", "BOX", invoice1Lines[0][JobComInvoiceLineSchema.JI_InvoiceUQ]);
			AssertEquals("Part Number", "ABC", invoice1Lines[0][JobComInvoiceLineSchema.JI_PartNo]);
			AssertEquals("Origin", "FI", invoice1Lines[0][JobComInvoiceLineSchema.JI_CountryOfOrigin]);

			var invoice2Lines = GetInvoiceLinesForHeader(invoiceHeaders[1].PK);
			AssertEquals("Only 2 invoice lines as the other had no Qty Received", 2, invoice2Lines.Length);
			AssertEquals("Origin", "IN", invoice2Lines[0][JobComInvoiceLineSchema.JI_CountryOfOrigin]);

			AssertEquals("ORDER2", invoice2Lines[0][JobComInvoiceLineSchema.JI_OrderNumber]);
			AssertEquals("Invoiced Quantity", 5m, invoice2Lines[0][JobComInvoiceLineSchema.JI_InvoiceQuantity]);
			AssertEquals("Line price", 28.57m, invoice2Lines[0][JobComInvoiceLineSchema.JI_LinePrice]);
			AssertEquals("Unit price (Line Price/Qty)", 5.714m, invoice2Lines[0]["UnitPrice"]);
			AssertEquals("Order 2 Line 1", invoice2Lines[0][JobComInvoiceLineSchema.JI_Description]);

			AssertEquals("ORDER2", invoice2Lines[1][JobComInvoiceLineSchema.JI_OrderNumber]);
			AssertEquals("Invoiced Quantity", 6m, invoice2Lines[1][JobComInvoiceLineSchema.JI_InvoiceQuantity]);
			AssertEquals("Line price", 22.5m, invoice2Lines[1][JobComInvoiceLineSchema.JI_LinePrice]);
			AssertEquals("Unit price (Line Price/invoiced Qty)", 3.75m, invoice2Lines[1]["UnitPrice"]);
			AssertEquals("Order 2 Line 2", invoice2Lines[1][JobComInvoiceLineSchema.JI_Description]);

			AssertEquals("Total Invoice header value", new ZDecimal(28.57 + 22.5), invoiceHeaders[1][JobComInvoiceHeaderSchema.JZ_InvoiceAmount]);

			AssertEquals(dec.HumanReadableName + " and commercial invoice line(s) have been successfully created from this pre-advice.", Notifier.LastMessage);
			AssertEquals(true, dec.IsInDatabase);
			AssertEquals(false, dec.HasChanges);

			var containersLines = GetContainerForDec(dec.PK);
			AssertEquals("Container Number 1", "TEST1", containersLines[0][CusContainerSchema.CO_ContainerNumber]);
			AssertEquals("Conteiner Number 2", "TEST2", containersLines[1][CusContainerSchema.CO_ContainerNumber]);
			AssertEquals("Container Type 1", Container1.J1_RC, containersLines[0][CusContainerSchema.CO_RC]);
			AssertEquals("Container Type 2", Container2.J1_RC, containersLines[1][CusContainerSchema.CO_RC]);

			AssertEquals("Transport Mode", "SEA", dec[JobDeclarationSchema.JE_TransportMode]);

			var testDate = ZDateTime.Today.AddMonths(-6);
			AssertEquals("ETD", testDate, dec[JobDeclarationSchema.JE_DateAtOrigin]);
			AssertEquals("ATD", testDate.AddDays(1), dec[JobDeclarationSchema.JE_ExportDate]);
			AssertEquals("ETA", testDate.AddDays(6), dec[JobDeclarationSchema.JE_DateAtFinalDestination]);
			AssertEquals("ATA", testDate.AddDays(7), dec[JobDeclarationSchema.JE_DateOfArrival]);
		}

		public void TestCreateBrokerageOutputsAllNotifyParties()
		{
			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.MainAddress.Address1 = "500 Penny Lane";
			notifyParty.OH_FullName = "Smith Brothers Company";
			notifyParty.MainAddress.City = "Summerville";

			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.MainAddress.Address1 = "15 Thomas St.";
			notifyParty2.OH_FullName = "John Williams";
			notifyParty2.MainAddress.City = "Summerville";

			var notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.MainAddress.Address1 = "48 Johnson Ave";
			notifyParty3.OH_FullName = "Maxwell Pickup Service";
			notifyParty3.MainAddress.City = "Summerville";

			Order1.NotifyPartyDocAddress.OrganisationPK = notifyParty.PK;
			Order1.NotifyParty2DocAddress.OrganisationPK = notifyParty2.PK;
			Order1.NotifyParty3DocAddress.OrganisationPK = notifyParty3.PK;

			Factory.Save();

			var declaration = Preplanning.CreateStandAloneBrokerage(Notifier);
			AssertNotNull(declaration);
			AssertEquals("Created declaration should have NotifyParty address", Order1.NotifyPartyDocAddress.E2_OA_Address, ((IDocAddresses)declaration).DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty).E2_OA_Address);
			AssertEquals("Created declaration should have NotifyParty2 address", Order1.NotifyParty2DocAddress.E2_OA_Address, ((IDocAddresses)declaration).DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty2).E2_OA_Address);
			AssertEquals("Created declaration should have NotifyParty3 address", Order1.NotifyParty3DocAddress.E2_OA_Address, ((IDocAddresses)declaration).DocAddresses.FindByDocAddressType(DocAddressType.NotifyParty3).E2_OA_Address);
		}

		[ExpectNoExceptions]
		public void TestCreateDeclarationThrowsNoException()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "AAA";
			buyer.OH_FullName = "AAA";
			buyer.MainAddress.OA_Address1 = "buyer address";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BBB";
			supplier.OH_FullName = "BBB";
			supplier.MainAddress.OA_Address1 = "supplier address";

			var preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_MasterBill = "MASTER";
			preplanning.BuyerPK = buyer.PK;
			preplanning.EF_RL_NKPortLoad = "AUSYD";
			preplanning.EF_RL_NKPortDisch = "USLAX";
			preplanning.PreAdviceTransports[0].JW_TransportMode = "SEA";

			var plannedContainer = preplanning.Containers.AddNew();
			plannedContainer.J1_ContainerNumber = "CONT11111";

			var order = preplanning.Orders.AddNew();
			order.JD_OrderNumber = "ORDER1";
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order.JD_RL_NKGoodsDeliveredTo = "USLAX";
			order.JD_TransportMode = "SEA";
			order.JD_ContainerMode = "FCL";
			order.JD_Waybill = "HOUSE";

			supplier.Addresses[0].OA_Code = "PCK";
			order.GoodsAvailableAtAddress.E2_OA_Address = supplier.Addresses[0].PK;

			buyer.Addresses[0].OA_Code = "DLV";
			order.GoodsDeliveredToAddress.E2_OA_Address = buyer.Addresses[0].PK;

			Factory.Save();

			var declaration = preplanning.CreateStandAloneBrokerage(Notifier, order);
			AssertEquals(buyer.MainAddress.OA_Address1, (declaration["ImporterDocumentaryAddress"] as JobDocAddress).Address.OA_Address1);
			AssertEquals(supplier.MainAddress.OA_Address1, (declaration["ClientPickupDeliveryAddress"] as JobDocAddress).Address.OA_Address1);
			AssertEquals("MASTER", declaration[JobDeclarationSchema.JE_MasterBill]);
			AssertEquals("HOUSE", declaration[JobDeclarationSchema.JE_HouseBill]);
			AssertEquals(Core.Constants.TransportModes.Sea, declaration[JobDeclarationSchema.JE_TransportMode]);
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration[JobDeclarationSchema.JE_ContainerMode]);

			var containerInDeclaration = GetContainerForDec(declaration.PK);
			AssertEquals("CONT11111", containerInDeclaration[0][CusContainerSchema.CO_ContainerNumber]);
			AssertEquals("FCL", containerInDeclaration[0][CusContainerSchema.CO_FCL_LCL_AIR]);
		}

		BusinessObject[] GetInvoiceLinesForHeader(ZGuid headerPK)
		{
			ZQuery linesQuery = new ZQuery(JobComInvoiceLineSchema.JI_JZ, headerPK);
			return (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>(linesQuery);
		}

		BusinessObject[] GetInvoiceHeadersForDec(ZGuid decPK)
		{
			ZQuery headerQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, decPK);
			headerQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, false);
			return (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>(headerQuery);
		}

		BusinessObject[] GetContainerForDec(ZGuid decPK)
		{
			ZQuery headerQuery = new ZQuery(CusContainerSchema.CO_JE, decPK);
			return (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(headerQuery);
		}

		#endregion

		#region Shipment/Declaration

		public void TestAttachingShipmentAndDeclaration()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());

			Preplanning.EF_JS = shipment.PK;
			Preplanning.EF_JE = declaration.PK;
			AssertEquals(ZGuid.Empty, Preplanning.EF_JS);
			AssertEquals(declaration.PK, Preplanning.EF_JE);

			Preplanning.EF_JS = shipment.PK;
			AssertEquals(shipment.PK, Preplanning.EF_JS);
			AssertEquals(ZGuid.Empty, Preplanning.EF_JE);

			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			Preplanning.EF_JE = declaration.PK;
			AssertEquals(shipment.PK, Preplanning.EF_JS);
			AssertEquals(declaration.PK, Preplanning.EF_JE);
		}

		#endregion

		#region OnAttachingOrder

		public void TestAppendContainersFromOrder()
		{
			string containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK.ToString();
			string containerType40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK.ToString();

			var existingPreadviseContainers = new[]
			{
				$"EXISTING1|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
			};

			var orderContainers = new[]
			{
				$"CONT1|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"EXISTING1|{containerType40GP}|1|SEAL1|SEAL2|SEAL3",
				$"CONT1|{containerType40GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|3|SEAL1|SEAL2|SEAL3",
				$"|{containerType40GP}|4|SEAL1|SEAL2|SEAL3",
				$"CONT2|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
			};

			Func<OrderContainer, string> containerToString = delegate(OrderContainer container)
			{
				var result = "";
				result += container.J1_ContainerNumber;
				result += "|" + container.J1_RC.ToString();
				result += "|" + container.J1_ContainerCount.ToString();
				result += "|" + container.J1_SealNum;
				result += "|" + container.J1_AdditionalSealNum;
				result += "|" + container.J1_Additional2SealNum;

				return result;
			};

			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			existingPreadviseContainers.ToList().ForEach(x => CreateNewOrderContainer(preAdvice.Containers, x));

			var order = Factory.NewWithValidTestData<Order>();
			orderContainers.ToList().ForEach(x => CreateNewOrderContainer(order.PlannedContainers, x));

			preAdvice.AppendContainersFromOrder(order);

			var expectedContainers = new[]
			{
				$"EXISTING1|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"CONT1|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
				$"|{containerType20GP}|3|SEAL1|SEAL2|SEAL3",
				$"|{containerType40GP}|4|SEAL1|SEAL2|SEAL3",
				$"CONT2|{containerType20GP}|1|SEAL1|SEAL2|SEAL3",
			};
			AssertContainsExactElementsInAnyOrder(expectedContainers, preAdvice.Containers.Cast<OrderContainer>().Select(x => containerToString(x)));
		}

		OrderContainer CreateNewOrderContainer(IDependentBusinessObjectCollection containers, string containerAsString)
		{
			OrderContainer container = (OrderContainer)containers.AddNew();
			var props = containerAsString.Split('|');
			container.J1_ContainerNumber = props[0];
			container.J1_RC = new ZGuid(props[1]);
			container.J1_ContainerCount = ZShort.Parse(props[2]);
			container.J1_SealNum = props[3];
			container.J1_AdditionalSealNum = props[4];
			container.J1_Additional2SealNum = props[5];

			return container;
		}

		#endregion

		#region Invoice Lines

		public void TestAppendInvoiceLineToCustomsDeclaration_HasChanges()
		{
			Preplanning.EF_ActualVolume = 10m;
			BusinessObject dec = Preplanning.AppendInvoiceLineToCustomsDeclaration(Notifier);
			AssertNull(dec);
			AssertEquals("Please save your changes before performing any Actions on this Pre Advice.", Notifier.LastMessage);
		}

		public void TestAppendInvoiceLineToCustomsDeclaration_HasLinkToOperationsJob()
		{
			Preplanning.EF_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			Factory.Save();

			BusinessObject consol = Preplanning.AppendInvoiceLineToCustomsDeclaration(Notifier);
			AssertNull(consol);
			AssertEquals("This Pre Advice has already been linked to an operations job and cannot be linked again.", Notifier.LastMessage);
		}

		public void TestAppendInvoiceLineToCustomsDeclaration_NoMatchingDec()
		{
			Order1Line2.JO_QtyReceived = 4m;

			Preplanning.EF_MasterBill = "NEWMASTER";

			BusinessObject someDec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BusinessObject existingInvoiceHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_JE] = someDec.PK;
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = "COMM1";

			BusinessObject existingInvoiceGroupHeader = (BusinessObject)((IBusinessObjectCollection)someDec["JobComInvoiceGroupHeaders"])[0];

			Factory.Save();

			Notifier.NextSelectedItemPK = someDec.PK;
			BusinessObject dec = Preplanning.AppendInvoiceLineToCustomsDeclaration(Notifier);
			AssertNotNull(dec);
			AssertEquals(someDec.PK, dec.PK);
			BusinessObject[] invoiceHeaders = GetInvoiceHeadersForDec(dec.PK);
			AssertEquals(2, invoiceHeaders.Length);
			AssertEquals(existingInvoiceHeader.PK, invoiceHeaders[0].PK);

			AssertEquals("Commercial invoice lines were successfully appended to " + dec.HumanReadableName + ".", Notifier.LastMessage);
		}

		public void TestAppendInvoiceLineToCustomsDeclaration_None()
		{
			Order1Line1.JO_QtyReceived = 0m;
			Order1Line2.JO_QtyReceived = 0m;
			Order2Line1.JO_QtyReceived = 0m;
			Order2Line2.JO_QtyReceived = 0m;

			Preplanning.EF_MasterBill = "NEWMASTER";

			BusinessObject someDec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BusinessObject existingInvoiceHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_JE] = someDec.PK;
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = "COMM1";

			BusinessObject existingInvoiceGroupHeader = (BusinessObject)((IBusinessObjectCollection)someDec["JobComInvoiceGroupHeaders"])[0];

			Factory.Save();

			Notifier.NextSelectedItemPK = someDec.PK;
			BusinessObject dec = Preplanning.AppendInvoiceLineToCustomsDeclaration(Notifier);
			AssertNotNull(dec);
			AssertEquals(someDec.PK, dec.PK);
			BusinessObject[] invoiceHeaders = GetInvoiceHeadersForDec(dec.PK);
			AssertEquals(1, invoiceHeaders.Length);
			AssertEquals(existingInvoiceHeader.PK, invoiceHeaders[0].PK);

			AssertEquals("No Commercial Lines were appended to " + dec.HumanReadableName + ". Please check Quantity Received has been entered on all Order Lines.", Notifier.LastMessage);
		}

		public void TestAppendInvoiceLineToCustomsDeclaration_Some()
		{
			Preplanning.EF_MasterBill = "NEWMASTER";

			BusinessObject someDec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			BusinessObject existingInvoiceHeader = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_JE] = someDec.PK;
			existingInvoiceHeader[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = "COMM1";

			BusinessObject existingInvoiceGroupHeader = (BusinessObject)((IBusinessObjectCollection)someDec["JobComInvoiceGroupHeaders"])[0];

			Factory.Save();

			Notifier.NextSelectedItemPK = someDec.PK;
			BusinessObject dec = Preplanning.AppendInvoiceLineToCustomsDeclaration(Notifier);
			AssertNotNull(dec);
			AssertEquals(someDec.PK, dec.PK);
			BusinessObject[] invoiceHeaders = GetInvoiceHeadersForDec(dec.PK);
			AssertEquals(2, invoiceHeaders.Length);
			AssertEquals(existingInvoiceHeader.PK, invoiceHeaders[0].PK);

			AssertEquals("Not all Commercial Lines were appended to " + dec.HumanReadableName + ". Please check Quantity Received has been entered on all Order Lines.", Notifier.LastMessage);
		}

		#endregion

		#region No Split Creation on Save

		public void TestNoSplitOnSave()
		{
			var preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			preAdvice.BuyerPK = Buyer.PK;
			var order = preAdvice.Orders.AddNew();
			order.JD_OrderNumber = "ORD9911";

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 10m;
			orderLine.JO_QtyReceived = 5m;

			Factory.Save();
			AssertEquals("Has no split", 0, order.OrderSplitSiblings.Count);
		}

		#endregion

		#region IJobNumber

		public void TestJobNumber()
		{
			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			Factory.Save();
			AssertEquals(((IJobNumber)preadvice).JobNumber, preadvice.EF_PreshipID);
		}

		#endregion

		#region IRoutingSupport

		public void TestRouting()
		{
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			AssertNotNull(((IRoutingSupport)preplanning).Transports);

			AssertEquals("1 added automatically", 1, ((IRoutingSupport)preplanning).Transports.Count);
			AssertEquals("Auto-added transport should be linked", true, ((IRoutingSupport)preplanning).Transports[0].JW_IsLinked);

			AssertNotNull(((IRoutingSupport)preplanning).TransportsIncludingRelated);
			((IRoutingSupport)preplanning).Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Air, ((IRoutingSupport)preplanning).TransportMode);
		}

		#endregion

		#region IWorkflowProvider

		public void TestWorkflowItems()
		{
			AssertEquals(true, ((IWorkflowProvider)Preplanning).WorkflowItems.IsLoaded);
			AssertEquals(true, Preplanning.IsRegisteredEditableChildObject(((IWorkflowProvider)Preplanning).WorkflowItems));
		}

		#endregion

		#region Implementation

		TestNotificationSubscriber Notifier;

		OrgHeader Buyer;
		OrgHeader Carrier;
		OrgHeader Agent1;
		OrgHeader Agent2;
		OrgHeader Supplier1;
		OrgHeader Supplier2;

		JobShipmentPreplanning Preplanning;
		Order Order1;
		Order Order2;
		OrderContainer Container1;
		OrderContainer Container2;

		OrderLine Order1Line1;
		OrderLine Order1Line2;
		OrderLine Order2Line1;
		OrderLine Order2Line2;

		RefServiceLevel serviceLevel;

		protected override void SetUp()
		{
			base.SetUp();

			serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "AAA";

			SetUpPreplanning();
		}

		void SetUpPreplanning(bool firstTransportSeaMode = true)
		{
			Notifier = new TestNotificationSubscriber();
			Buyer = Factory.NewWithValidTestData<OrgHeader>();

			Carrier = Factory.NewWithValidTestData<OrgHeader>();
			Carrier.OH_IsShippingLine = true;
			Carrier.OH_IsShippingProvider = true;

			Agent1 = Factory.NewWithValidTestData<OrgHeader>();
			Agent2 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier2 = Factory.NewWithValidTestData<OrgHeader>();

			Preplanning = Factory.New<JobShipmentPreplanning>();
			Preplanning.EF_HouseBill = "HOUSE";
			Preplanning.EF_MasterBill = "MASTER";
			Preplanning.BuyerPK = Buyer.PK;
			Preplanning.EF_OH_Carrier = Carrier.PK;
			Preplanning.EF_OH_SendingAgent = Agent1.PK;
			Preplanning.EF_OH_ReceivingAgent = Agent2.PK;
			Preplanning.EF_RL_NKPortLoad = "AUSYD";
			Preplanning.EF_RL_NKPortDisch = "USLAX";

			var testDate = ZDateTime.Today.AddMonths(-6);

			var transport1 = Preplanning.PreAdviceTransports[0];
			transport1.JW_TransportMode = firstTransportSeaMode ? "SEA" : "AIR";
			if (firstTransportSeaMode)
			{
				transport1.JW_Vessel = "BRIGIT";
				transport1.JW_RL_NKLoadPort = "AUSYD";
			}
			transport1.JW_VoyageFlight = "QF122";
			transport1.JW_RL_NKDiscPort = "INBOM";
			transport1.JW_TransportType = firstTransportSeaMode
				? Core.Constants.TransportPlanningType.Other
				: Core.Constants.TransportPlanningType.Flight1;
			transport1.JW_ETD = testDate;
			transport1.JW_ATD = testDate.AddDays(1);
			transport1.JW_ETA = testDate.AddDays(2);
			transport1.JW_ATA = testDate.AddDays(3);

			var transport2 = Preplanning.PreAdviceTransports.AddNew();
			transport2.JW_TransportMode = "SEA";
			transport2.JW_Vessel = "GRUMANT";
			transport2.JW_VoyageFlight = "QF123";
			transport2.JW_RL_NKLoadPort = "INBOM";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_ETD = testDate.AddDays(4);
			transport2.JW_ATD = testDate.AddDays(5);
			transport2.JW_ETA = testDate.AddDays(6);
			transport2.JW_ATA = testDate.AddDays(7);

			// Order 1
			Order1 = Preplanning.Orders.AddNew();
			Order1.JD_OrderNumber = "ORDER1";
			Order1.BuyerPK = Buyer.PK;
			Order1.SupplierPK = Supplier1.PK;
			Order1.JD_RL_NKGoodsAvailableAt = "AUSYD";
			Order1.JD_RL_NKGoodsDeliveredTo = "USLAX";
			Order1.JD_TransportMode = "SEA";
			Order1.JD_IncoTerm = "FCA";
			Order1.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Order1.JD_RN_NKCountryOfSupply = "NZ";
			Order1.JD_RS_NKServiceLevel_NI = serviceLevel.RS_Code;

			Order1Line1 = Order1.OrderLines.AddNew();
			Order1Line1.JO_Quantity = 10m;
			Order1Line1.JO_QtyReceived = 10m;
			Order1Line1.JO_QtyInvoiced = 10m;
			Order1Line1.JO_Description = "Order 1 Line 1";
			Order1Line1.JO_LinePrice = 20;
			Order1Line1.JO_CommercialInvoiceNo = "COMM1";
			Order1Line1.JO_F3_NKPackType = "BOX";
			Order1Line1.JO_Partno = "ABC";
			Order1Line1.JO_RN_NKCountryOfOrigin = "FI";

			Order1Line2 = Order1.OrderLines.AddNew();
			Order1Line2.JO_Quantity = 4m;
			Order1Line2.JO_Description = "Order 1 Line 2";
			Order1Line2.JO_LinePrice = 30;
			Order1Line2.JO_CommercialInvoiceNo = "COMM2";

			// Order 2
			Order2 = Preplanning.Orders.AddNew();
			Order2.JD_OrderNumber = "ORDER2";
			Order2.BuyerPK = Buyer.PK;
			Order2.SupplierPK = Supplier2.PK;
			Order2.JD_RL_NKGoodsAvailableAt = "AUSYD";
			Order2.JD_RL_NKGoodsDeliveredTo = "USLAX";
			Order2.JD_IncoTerm = "EXW";
			Order2.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			Order2.JD_RN_NKCountryOfSupply = "IN";

			Order2Line1 = Order2.OrderLines.AddNew();
			Order2Line1.JO_Quantity = 7m;
			Order2Line1.JO_QtyReceived = 5m;
			Order2Line1.JO_QtyInvoiced = 5m;
			Order2Line1.JO_Description = "Order 2 Line 1";
			Order2Line1.JO_LinePrice = 40;
			Order2Line1.JO_CommercialInvoiceNo = "COMM2";

			Order2Line2 = Order2.OrderLines.AddNew();
			Order2Line2.JO_Quantity = 8m;
			Order2Line2.JO_QtyReceived = 6m;
			Order2Line2.JO_QtyInvoiced = 6m;
			Order2Line2.JO_Description = "Order 2 Line 2";
			Order2Line2.JO_LinePrice = 30;
			Order2Line2.JO_CommercialInvoiceNo = "COMM2";

			//Container
			Container1 = Preplanning.Containers.AddNew();
			Container1.J1_RC = new ZGuid();
			Container1.J1_ContainerNumber = "TEST1";
			Container1.J1_SealNum = "SEAL11";
			Container1.J1_AdditionalSealNum = "SEAL12";
			Container1.J1_Additional2SealNum = "SEAL13";
			Container2 = Preplanning.Containers.AddNew();
			Container2.J1_RC = new ZGuid();
			Container2.J1_ContainerNumber = "TEST2";
			Container2.J1_SealNum = "SEAL21";
			Container2.J1_AdditionalSealNum = "SEAL22";
			Container2.J1_Additional2SealNum = "SEAL23";

			SetupOrganizations();

			Factory.Save();
		}

		void SetupOrganizations()
		{
			Buyer.OH_IsConsignee = true;
			Buyer.OH_IsForwarder = true;

			Supplier1.OH_IsConsignor = true;
			Supplier1.OH_IsForwarder = true;
			var supplierAddress1 = Supplier1.Addresses.AddNewMainAddress();
			supplierAddress1.OA_Address1 = "supplier1 address";

			Supplier2.OH_IsConsignor = true;
			Supplier2.OH_IsForwarder = true;
			var supplierAddress2 = Supplier2.Addresses.AddNewMainAddress();
			supplierAddress1.OA_Address1 = "supplier2 address";

			Agent1.OH_IsConsignor = true;
			Agent1.OH_IsForwarder = true;
			var agent1Address = Agent1.Addresses.AddNewMainAddress();
			agent1Address.OA_Address1 = "agent1 address";
			var sendingAppointedAgentPort1 = Agent1.AppointedAgentPorts.AddNew();
			sendingAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingAppointedAgentPort1.O5_SeaAgentStatus = "PUB";
			sendingAppointedAgentPort1.O5_AirAgentStatus = "PUB";
			sendingAppointedAgentPort1.O5_OA_AgentOfficeAddress = agent1Address.PK;
			sendingAppointedAgentPort1.O5_PortOrCountry = "AUSYD";
			var sendingAppointedAgentPort2 = Agent1.AppointedAgentPorts.AddNew();
			sendingAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingAppointedAgentPort2.O5_SeaAgentStatus = "PUB";
			sendingAppointedAgentPort2.O5_AirAgentStatus = "PUB";
			sendingAppointedAgentPort2.O5_OA_AgentOfficeAddress = agent1Address.PK;
			sendingAppointedAgentPort2.O5_PortOrCountry = "MYPKG";

			Agent2.OH_IsConsignee = true;
			Agent2.OH_IsForwarder = true;
			var agent2Address = Agent2.Addresses.AddNewMainAddress();
			agent2Address.OA_Address1 = "agent2 address";
			var receivingAppointedAgentPort1 = Agent2.AppointedAgentPorts.AddNew();
			receivingAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingAppointedAgentPort1.O5_AirAgentStatus = "PUB";
			receivingAppointedAgentPort1.O5_SeaAgentStatus = "PUB";
			receivingAppointedAgentPort1.O5_OA_AgentOfficeAddress = agent2Address.PK;
			receivingAppointedAgentPort1.O5_PortOrCountry = "USLAX";
			var receivingAppointedAgentPort2 = Agent2.AppointedAgentPorts.AddNew();
			receivingAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingAppointedAgentPort2.O5_AirAgentStatus = "PUB";
			receivingAppointedAgentPort2.O5_SeaAgentStatus = "PUB";
			receivingAppointedAgentPort2.O5_OA_AgentOfficeAddress = agent2Address.PK;
			receivingAppointedAgentPort2.O5_PortOrCountry = "AUSYD";
			var receivingAppointedAgentPort3 = Agent2.AppointedAgentPorts.AddNew();
			receivingAppointedAgentPort3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingAppointedAgentPort3.O5_AirAgentStatus = "PUB";
			receivingAppointedAgentPort3.O5_SeaAgentStatus = "PUB";
			receivingAppointedAgentPort3.O5_OA_AgentOfficeAddress = agent2Address.PK;
			receivingAppointedAgentPort3.O5_PortOrCountry = "AUMEL";

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
			Container1.J1_RC = refContainer.PK;
			Container2.J1_RC = refContainer.PK;
		}

		Func<ForwardingConsol, bool> TryToFixConsolErrors =>
			(consol) =>
			{
				consol.JK_RL_NKLastForeignPort = "USLAX";

				foreach (Transport transport in consol.Transports)
				{
					transport.CarrierPK = Carrier.PK;
				}

				try
				{
					consol.Factory.Save();
				}
				catch (Exception)
				{
					return false;
				}

				return true;
			};

		public class TestNotificationSubscriber : INotifications, INotificationSubscriberQueryUser
		{
			void INotifications.Add(INotification @event)
			{
				LastMessage = @event.Message;
			}

			public string LastMessage;

			void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
			{
				QueryUserMsgBoxEventArgs msgBoxArgs = e as QueryUserMsgBoxEventArgs;
				if (msgBoxArgs != null)
				{
					LastMessage = msgBoxArgs.Message;
					LastQueryUserResult = msgBoxArgs.Response;
					msgBoxArgs.Response = NextQueryUserResult;
				}

				QueryUserFindboxEventArgs findboxArgs = e as QueryUserFindboxEventArgs;
				if (findboxArgs != null)
				{
					findboxArgs.SelectedItemPK = NextSelectedItemPK;
				}
			}

			public bool NextQueryUserResult;

			public bool LastQueryUserResult;

			public ZGuid NextSelectedItemPK;
		}

		#endregion
	}
}

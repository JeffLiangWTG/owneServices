using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class ShippingOrderBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateFromNewConsolDoesNotThrowException

		public void TestPopulateFromNewConsolDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoExceptionThrown(() => new ShippingOrderBuilder(consol).Build());
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_PackageGrouping = Core.Constants.PackageGrouping.Codes.DoNotGroup;
				AssertNoExceptionThrown(() => new ShippingOrderBuilder(consol).Build());
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_PackageGrouping = Core.Constants.PackageGrouping.Codes.GroupByShipment;
				AssertNoExceptionThrown(() => new ShippingOrderBuilder(consol).Build());
			}
		}

		#endregion

		#region TestPopulateFromForwardingConsol

		[TestDate(2018, 1, 1)]
		public void TestPopulateFromForwardingConsol()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("SourceType", "ForwardingConsol", shippingOrder.SourceType);
				AssertEquals("SourceID", "C00001000", shippingOrder.SourceID);
				AssertEquals("DocumentName", "ShippingOrder", shippingOrder.DocumentName);

				AssertEquals("CarrierBookingReference", "驴100", shippingOrder.CarrierBookingReference);
				AssertEquals("NumberOfOriginals", 1, shippingOrder.NumberOfOriginals);
				AssertEquals("NumberOfCopies", 3, shippingOrder.NumberOfCopies);
				AssertEquals("RequestedDateOfIssue", new ZDateTime(2018, 10, 1), shippingOrder.RequestedDateOfIssue);
				AssertEquals("IsDoorPickup", true, shippingOrder.IsDoorPickup);
				AssertEquals("IsDoorDelivery", false, shippingOrder.IsDoorDelivery);
				AssertEquals("ForwardingInstructions", "forwarding instructions", shippingOrder.ForwardingInstructions);
				AssertEquals("GoodsHandlingInstructions", "goods handling instructions", shippingOrder.GoodsHandlingInstructions);
				AssertEquals("SpecialInstructions", "special instructions", shippingOrder.SpecialInstructions);

				AssertEquals("CarrierContractNumber", "11111", shippingOrder.CarrierContractNumber);
				AssertEquals("NamedAccount", "Contract Named Account", shippingOrder.ContractNamedAccount);
				AssertEquals("LetterOfCredit", "22222", shippingOrder.LetterOfCredit);
				AssertEquals("CarrierBookingPrefix", "12345", shippingOrder.CarrierBookingPrefix);

				AssertEquals("IssueFreightedBillOfLading", false, shippingOrder.IssueFreightedBillOfLading);

				AssertEquals("ShipmentType.Code", Constants.AgentType.Agent, shippingOrder.ShipmentType.Code);
				AssertEquals("ShipmentType.Description", Constants.AgentTypeDescriptions.Agent, shippingOrder.ShipmentType.Description);

				AssertEquals("ReleaseType.Code", "BOL", shippingOrder.ReleaseType.Code);
				AssertEquals("ReleaseType.Description", "BOL Original", shippingOrder.ReleaseType.Description);

				AssertEquals("ContainerMode.Code", "FCL", shippingOrder.ContainerMode.Code);
				AssertEquals("ContainerMode.Description", "Full Container Load", shippingOrder.ContainerMode.Description);

				AssertEquals("Vessel.Name", "Dragon", shippingOrder.Vessel.Name);
				AssertEquals("Vessel.LloydsIMO", "", shippingOrder.Vessel.LloydsIMO);
				AssertEquals("VoyageFlightNumber", "111", shippingOrder.VoyageFlightNumber);

				AssertEquals("PortOfLoading.Code", "CNSHA", shippingOrder.PortOfLoading.Code);
				AssertEquals("PortOfDischarge.Code", "AUSYD", shippingOrder.PortOfDischarge.Code);
				AssertEquals("PlaceOfReceipt.Code", "CNSHA", shippingOrder.PlaceOfReceipt.Code);
				AssertEquals("PlaceOfIssue.Code", "DKAAL", shippingOrder.PlaceOfIssue.Code);
				AssertEquals("PlaceOfDelivery.Code", "AUSYD", shippingOrder.PlaceOfDelivery.Code);
				AssertEquals("FreightPayableAt.Code", "AUSYD", shippingOrder.FreightPayableAt.Code);
				AssertEquals("OperationalPort.Code", "CNSHA", shippingOrder.OperationalPort.Code);

				AssertTransports(shippingOrder);
				AssertAddresses(shippingOrder);
				AssertContainers(shippingOrder);
			});

			AssertionHelper.AssertCurrentUserAddressData(shippingOrder.CurrentUser);
		}

		void AssertTransports(IShippingOrder shippingOrder)
		{
			AssertContainsExactElementsInAnyOrder("Transports",
				new[]
				{
					"CNSHA -> SGSIN",
					"SGSIN -> NZAKL",
					"NZAKL -> AUSYD",
					"AUSYD -> AUMEL",
				},
				shippingOrder.Transports.Select(t => $"{t.PortOfLoading.Code} -> {t.PortOfDischarge.Code}"));
		}

		void AssertAddresses(IShippingOrder shippingOrder)
		{
			var addresses = new[]
			{
				$"{nameof(shippingOrder.Shipper)}\r\n{shippingOrder.Shipper.ToAssertString()}",
				$"{nameof(shippingOrder.Carrier)}\r\n{shippingOrder.Carrier.ToAssertString()}",
				$"{nameof(shippingOrder.Consignee)}\r\n{shippingOrder.Consignee.ToAssertString()}",
				$"{nameof(shippingOrder.CarrierHandlingAgent)}\r\n{shippingOrder.CarrierHandlingAgent.ToAssertString()}",
				$"{nameof(shippingOrder.CarrierBookingAgent)}\r\n{shippingOrder.CarrierBookingAgent.ToAssertString()}",
				$"{nameof(shippingOrder.NotifyParty)}\r\n{shippingOrder.NotifyParty.ToAssertString()}",
				$"{nameof(shippingOrder.NotifyParty2)}\r\n{shippingOrder.NotifyParty2.ToAssertString()}",
				$"{nameof(shippingOrder.Forwarder)}\r\n{shippingOrder.Forwarder.ToAssertString()}",
				$"{nameof(shippingOrder.PickupFrom)}\r\n{shippingOrder.PickupFrom.ToAssertString()}",
				$"{nameof(shippingOrder.DeliverTo)}\r\n{shippingOrder.DeliverTo.ToAssertString()}"
			};

			AssertContainsExactElementsInAnyOrder("Addresses",
				new[]
				{
@"Shipper
I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222",

@"Carrier
MAERSK
Unit 13
4 Lost Lane",

@"Consignee
I'm Receiving Stuff
Unit 399
50 What Lane
Receiver Name
name@receiver.com
3333333
4444444",

@"Forwarder
I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222",

"CarrierHandlingAgent",
"CarrierBookingAgent",
"NotifyParty",
"NotifyParty2",
@"PickupFrom
CONSPA
Unit 15
5 Lost Lane",
@"DeliverTo
BLOOP
199 Crab Road
Crabby"
				},
				addresses.Select(address => address.TrimEnd()));
		}

		void AssertContainers(IShippingOrder shippingOrder)
		{
			var containersAsAssertString = shippingOrder
				.Containers
				.Select(container => container.ToAssertString());

			AssertMultilineASCIIEquals("Containers",
@"AAAA0000007|10||
   2 PLT|pack1|REF001
      DG SHIPPER NAME
      CN|1234
   5 PLT|pack2|REF001
   3 PLT|pack3|REF002",
				string.Join("/r/n", containersAsAssertString));
		}

		public void TestCarrierContractNumber()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertEquals("CarrierContractNumber", "11111", shippingOrder.CarrierContractNumber);

			var quotationNumber = consol.Numbers.AddNew();
			quotationNumber.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			quotationNumber.CE_EntryType = AdditionalReferences.Codes.CarrierQuoteNumber;
			quotationNumber.CE_EntryNum = "QUOT123";

			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertEquals("CarrierContractNumber", "QUOT123", shippingOrder.CarrierContractNumber);
		}

		public void TestContainerMode()
		{
			var consol = Factory.New<ForwardingConsol>();

			var modeMap = new Dictionary<string, string>
			{
				[Core.Constants.ContainerModes.FCL] = Core.Constants.ContainerModes.FCL,
				[Core.Constants.ContainerModes.Groupage] = Core.Constants.ContainerModes.FCL,
				[Core.Constants.ContainerModes.BuyersConsol] = Core.Constants.ContainerModes.FCL,
				[Core.Constants.ContainerModes.Other] = Core.Constants.ContainerModes.LCL,
				[Core.Constants.ContainerModes.LCL] = Core.Constants.ContainerModes.LCL
			};

			CombineAssertions(() =>
			{
				foreach (var map in modeMap)
				{
					AssertContainerModeMapping(consol, map.Key, map.Value);
				}
			});

			consol.Containers.AddNew();
			AssertContainerModeMapping(consol, Core.Constants.ContainerModes.Other, Core.Constants.ContainerModes.FCL);
		}

		void AssertContainerModeMapping(ForwardingConsol consol, string consolMode, string expecedContainerMode)
		{
			consol.JK_ConsolMode = consolMode;
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertEquals($"Mode mapped {consolMode} -> {expecedContainerMode}",
				shippingOrder.ContainerMode.Code, expecedContainerMode);
		}

		#endregion

		#region TestDefaultContactAndEmail

		public void TestDefaultContactAndEmail()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ShippingOrderAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";

			var shippingOrderMessageData = new ShippingOrderBuilder(consol).Build();
			AssertEquals("test@test.com", shippingOrderMessageData.Carrier.Email);
			AssertEquals("TEST NAME", shippingOrderMessageData.Carrier.Contact);

			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingOrderMessageData = new ShippingOrderBuilder(consol).Build();
			AssertEquals("", shippingOrderMessageData.Carrier.Email);
			AssertEquals("", shippingOrderMessageData.Carrier.Contact);
		}

		#endregion

		#region TestEmptyCountryCodeValidation

		public void TestEmptyCountryCodeValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var shipper = shippingOrder.Shipper as Address;
			var carrier = shippingOrder.Carrier as Address;
			var notifyParty = shippingOrder.NotifyParty as Address;
			var notifyParty2 = shippingOrder.NotifyParty2 as Address;
			var consignee = shippingOrder.Consignee as Address;
			var carrierHandlingAgent = shippingOrder.CarrierHandlingAgent as Address;
			var carrierBookingAgent = shippingOrder.CarrierBookingAgent as Address;
			var forwarder = shippingOrder.Forwarder as Address;
			var pickupFrom = shippingOrder.PickupFrom as Address;
			var deliverTo = shippingOrder.DeliverTo as Address;

			AssertEmptyCountryCodeValidation(shippingOrder, shipper);
			AssertEmptyCountryCodeValidation(shippingOrder, carrier);
			AssertEmptyCountryCodeValidation(shippingOrder, notifyParty);
			AssertEmptyCountryCodeValidation(shippingOrder, notifyParty2);
			AssertEmptyCountryCodeValidation(shippingOrder, consignee);
			AssertEmptyCountryCodeValidation(shippingOrder, carrierHandlingAgent);
			AssertEmptyCountryCodeValidation(shippingOrder, carrierBookingAgent);
			AssertEmptyCountryCodeValidation(shippingOrder, forwarder);
			AssertEmptyCountryCodeValidation(shippingOrder, pickupFrom);
			AssertEmptyCountryCodeValidation(shippingOrder, deliverTo);
		}

		void AssertEmptyCountryCodeValidation(ShippingOrder shippingOrder, Address address)
		{
			const string errorMessage = "Country code is required, if Country is entered.";

			address.Country.Code = ZString.Empty;
			address.Country.Name = "China";
			shippingOrder.ValidateAllIncludingChildren();

			AssertHasMessageError(address.Country.NameInfo, errorMessage);

			address.Country.Code = Constants.CountryCodes.China;
			address.Country.Name = "China";
			shippingOrder.ValidateAllIncludingChildren();

			AssertNoMessageError(address.Country.NameInfo, errorMessage);
		}

		#endregion

		#region TestDefaultContactAndEmailValidation

		public void TestDefaultContactAndEmailValidation()
		{
			var errorMessage = "This carrier does not support electronic Shipping Order. Contact name and email address are required to send your Shipping Order by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ShippingOrderAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "";

			var shippingOrderMessageData = new ShippingOrderBuilder(consol).Build();
			AssertEquals("", shippingOrderMessageData.Carrier.Email);
			AssertEquals("", shippingOrderMessageData.Carrier.Contact);
			AssertHasMessageError(((Address)shippingOrderMessageData.Carrier).ContactInfo, errorMessage);

			shippingOrderMessageData.Carrier.Contact = string.Empty;
			shippingOrderMessageData.Carrier.Email = "email";
			AssertHasMessageError(((Address)shippingOrderMessageData.Carrier).ContactInfo, errorMessage);

			shippingOrderMessageData.Carrier.Contact = "ABC";
			shippingOrderMessageData.Carrier.Email = string.Empty;
			AssertHasMessageError(((Address)shippingOrderMessageData.Carrier).ContactInfo, errorMessage);

			shippingOrderMessageData.Carrier.Email = "email";
			AssertNoMessageError(((Address)shippingOrderMessageData.Carrier).ContactInfo, errorMessage);

			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingOrderMessageData = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(((Address)shippingOrderMessageData.Carrier).ContactInfo, errorMessage);
		}

		#endregion

		#region Consol has shipments

		public void TestConsolHasShipments()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals("Precondition: Consol has no shipments", false, consol.Shipments.Any());

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(shippingOrder.ErrorPlaceholderInfo, "There are no shipments attached to the Consolidation.");

			consol.Shipments.AddNew();

			AssertEquals("Precondition: Consol now has shipments", true, consol.Shipments.Any());

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(shippingOrder.ErrorPlaceholderInfo, "There are no shipments attached to the Consolidation.");
		}

		#endregion

		#region PickupFrom & DeliverTo
		public void TestDoorPickupDRTConsolPickupFrom_WillFallBack()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment = consol.Shipments.AddNew();

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("2000", shippingOrder.PickupFrom.Postcode);
				AssertEquals("AU", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("Unit 15", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("Sydney", shippingOrder.PickupFrom.City);
			});

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_FullName = "YUMMY";
			exportReceivingDepot.OH_RL_NKClosestPort = "FRPAR";
			exportReceivingDepot.MainAddress.Address1 = "Unit 200";
			exportReceivingDepot.MainAddress.Address2 = "55 Why Lane";
			exportReceivingDepot.MainAddress.City = "Paris";
			exportReceivingDepot.MainAddress.Postcode = "9000";
			exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("9000", shippingOrder.PickupFrom.Postcode);
				AssertEquals("FR", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("Unit 200", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("55 Why Lane", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("Paris", shippingOrder.PickupFrom.City);
			});
		}

		public void TestDoorPickupConsolPickupFrom_WillFallBack_IsDoorPickupChange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			var shipment = consol.Shipments.AddNew();

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});

			shippingOrder.IsDoorPickup = true;
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("2000", shippingOrder.PickupFrom.Postcode);
				AssertEquals("AU", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("Unit 15", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("Sydney", shippingOrder.PickupFrom.City);
			});

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_FullName = "YUMMY";
			exportReceivingDepot.OH_RL_NKClosestPort = "FRPAR";
			exportReceivingDepot.MainAddress.Address1 = "Unit 200";
			exportReceivingDepot.MainAddress.Address2 = "55 Why Lane";
			exportReceivingDepot.MainAddress.City = "Paris";
			exportReceivingDepot.MainAddress.Postcode = "9000";
			exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			shippingOrder.IsDoorPickup = false;
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});

			shippingOrder.IsDoorPickup = true;
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("9000", shippingOrder.PickupFrom.Postcode);
				AssertEquals("FR", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("Unit 200", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("55 Why Lane", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("Paris", shippingOrder.PickupFrom.City);
			});
		}

		public void TestPickupFrom_ShipmentAllHaveSamePickupAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment1.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;
			shipment2.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("2000", shippingOrder.PickupFrom.Postcode);
				AssertEquals("AU", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("Unit 15", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("Sydney", shippingOrder.PickupFrom.City);
			});
		}

		public void TestPickupFrom_ShipmentsDontAllHaveSamePickupAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consignorPickupAddress1 = Factory.New<OrgHeader>();
			consignorPickupAddress1.OH_FullName = "CONSPA";
			consignorPickupAddress1.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress1.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress1.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress1.MainAddress.City = "Sydney";
			consignorPickupAddress1.MainAddress.Postcode = "2000";
			consignorPickupAddress1.MainAddress.OA_RN_NKCountryCode = "AU";

			var consignorPickupAddress2 = Factory.New<OrgHeader>();
			consignorPickupAddress2.OH_FullName = "UMBRA";
			consignorPickupAddress2.OH_RL_NKClosestPort = "NZAKL";
			consignorPickupAddress2.MainAddress.Address1 = "34 Remuera Road";
			consignorPickupAddress2.MainAddress.Address2 = "Remuera";
			consignorPickupAddress2.MainAddress.City = "Auckland";
			consignorPickupAddress2.MainAddress.Postcode = "1050";
			consignorPickupAddress2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment1.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress1.MainAddress.PK;
			shipment2.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress2.MainAddress.PK;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});
		}

		public void TestPickupFrom_ShipmentsDontAllHaveSameCFSAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var exportReceivingDepot1 = Factory.New<OrgHeader>();
			exportReceivingDepot1.OH_FullName = "CONSPA";
			exportReceivingDepot1.OH_RL_NKClosestPort = "AUSYD";
			exportReceivingDepot1.MainAddress.Address1 = "Unit 15";
			exportReceivingDepot1.MainAddress.Address2 = "5 Lost Lane";
			exportReceivingDepot1.MainAddress.City = "Sydney";
			exportReceivingDepot1.MainAddress.Postcode = "2000";
			exportReceivingDepot1.MainAddress.OA_RN_NKCountryCode = "AU";

			var exportReceivingDepot2 = Factory.New<OrgHeader>();
			exportReceivingDepot2.OH_FullName = "UMBRA";
			exportReceivingDepot2.OH_RL_NKClosestPort = "NZAKL";
			exportReceivingDepot2.MainAddress.Address1 = "34 Remuera Road";
			exportReceivingDepot2.MainAddress.Address2 = "Remuera";
			exportReceivingDepot2.MainAddress.City = "Auckland";
			exportReceivingDepot2.MainAddress.Postcode = "1050";
			exportReceivingDepot2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment1.JS_OA_ExportReceivingDepot = exportReceivingDepot1.MainAddress.PK;
			shipment2.JS_OA_ExportReceivingDepot = exportReceivingDepot2.MainAddress.PK;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});
		}

		public void TestPickupFrom_ConsolIsNotDoorPickup()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment1.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;
			shipment2.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "BLOOP";
			packDepotAddress.OH_RL_NKClosestPort = "USJFK";
			packDepotAddress.MainAddress.Address1 = "199 Crab Road";
			packDepotAddress.MainAddress.Address2 = "Crabby";
			packDepotAddress.MainAddress.City = "New York";
			packDepotAddress.MainAddress.Postcode = "10005";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.PickupFrom.CompanyName);
				AssertEquals("10005", shippingOrder.PickupFrom.Postcode);
				AssertEquals("US", shippingOrder.PickupFrom.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.PickupFrom.AddressLine1);
				AssertEquals("Crabby", shippingOrder.PickupFrom.AddressLine2);
				AssertEquals("New York", shippingOrder.PickupFrom.City);
			});
		}

		public void TestDoorDeliveryDRTConsolDeliveryTo_WillFallBack()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment = consol.Shipments.AddNew();

			var unPackDepotAddress = Factory.New<OrgHeader>();
			unPackDepotAddress.OH_FullName = "BLOOP";
			unPackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unPackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unPackDepotAddress.MainAddress.Address2 = "Crabby";
			unPackDepotAddress.MainAddress.City = "New York";
			unPackDepotAddress.MainAddress.Postcode = "10005";
			unPackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unPackDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONSPA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("2000", shippingOrder.DeliverTo.Postcode);
				AssertEquals("AU", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("Unit 15", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("Sydney", shippingOrder.DeliverTo.City);
			});

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "YUMMY";
			importReleaseDepot.OH_RL_NKClosestPort = "FRPAR";
			importReleaseDepot.MainAddress.Address1 = "Unit 200";
			importReleaseDepot.MainAddress.Address2 = "55 Why Lane";
			importReleaseDepot.MainAddress.City = "Paris";
			importReleaseDepot.MainAddress.Postcode = "9000";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("9000", shippingOrder.DeliverTo.Postcode);
				AssertEquals("FR", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("Unit 200", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("55 Why Lane", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("Paris", shippingOrder.DeliverTo.City);
			});
		}

		public void TestDoorDeliveryConsolDeliveryTo_WillFallBack_IsDoorDeliveryChange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = "DRT";

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			var shipment = consol.Shipments.AddNew();

			var unPackDepotAddress = Factory.New<OrgHeader>();
			unPackDepotAddress.OH_FullName = "BLOOP";
			unPackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unPackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unPackDepotAddress.MainAddress.Address2 = "Crabby";
			unPackDepotAddress.MainAddress.City = "New York";
			unPackDepotAddress.MainAddress.Postcode = "10005";
			unPackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unPackDepotAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONSPA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});

			shippingOrder.IsDoorDelivery = true;
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("2000", shippingOrder.DeliverTo.Postcode);
				AssertEquals("AU", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("Unit 15", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("Sydney", shippingOrder.DeliverTo.City);
			});

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "YUMMY";
			importReleaseDepot.OH_RL_NKClosestPort = "FRPAR";
			importReleaseDepot.MainAddress.Address1 = "Unit 200";
			importReleaseDepot.MainAddress.Address2 = "55 Why Lane";
			importReleaseDepot.MainAddress.City = "Paris";
			importReleaseDepot.MainAddress.Postcode = "9000";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;

			shippingOrder.IsDoorDelivery = false;
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});

			shippingOrder.IsDoorDelivery = true;
			CombineAssertions(() =>
			{
				AssertEquals("YUMMY", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("9000", shippingOrder.DeliverTo.Postcode);
				AssertEquals("FR", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("Unit 200", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("55 Why Lane", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("Paris", shippingOrder.DeliverTo.City);
			});
		}

		public void TestDeliveryTo_AllShipmentsHaveSameDeliveryAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONSPA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment1.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
			shipment2.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("CONSPA", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("2000", shippingOrder.DeliverTo.Postcode);
				AssertEquals("AU", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("Unit 15", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("5 Lost Lane", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("Sydney", shippingOrder.DeliverTo.City);
			});
		}

		public void TestDeliveryTo_ShipmentsDontAllHaveSameDeliveryAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consigneeDeliveryAddress1 = Factory.New<OrgHeader>();
			consigneeDeliveryAddress1.OH_FullName = "CONSPA";
			consigneeDeliveryAddress1.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress1.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress1.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress1.MainAddress.City = "Sydney";
			consigneeDeliveryAddress1.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress1.MainAddress.OA_RN_NKCountryCode = "AU";

			var consigneeDeliveryAddress2 = Factory.New<OrgHeader>();
			consigneeDeliveryAddress2.OH_FullName = "UMBRA";
			consigneeDeliveryAddress2.OH_RL_NKClosestPort = "NZAKL";
			consigneeDeliveryAddress2.MainAddress.Address1 = "34 Remuera Road";
			consigneeDeliveryAddress2.MainAddress.Address2 = "Remuera";
			consigneeDeliveryAddress2.MainAddress.City = "Auckland";
			consigneeDeliveryAddress2.MainAddress.Postcode = "1050";
			consigneeDeliveryAddress2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment1.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress1.MainAddress.PK;
			shipment2.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress2.MainAddress.PK;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});
		}

		public void TestDeliveryTo_ShipmentsDontAllHaveSameCFSAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CFS;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var importReleaseDepot1 = Factory.New<OrgHeader>();
			importReleaseDepot1.OH_FullName = "CONSPA";
			importReleaseDepot1.OH_RL_NKClosestPort = "AUSYD";
			importReleaseDepot1.MainAddress.Address1 = "Unit 15";
			importReleaseDepot1.MainAddress.Address2 = "5 Lost Lane";
			importReleaseDepot1.MainAddress.City = "Sydney";
			importReleaseDepot1.MainAddress.Postcode = "2000";
			importReleaseDepot1.MainAddress.OA_RN_NKCountryCode = "AU";

			var importReleaseDepot2 = Factory.New<OrgHeader>();
			importReleaseDepot2.OH_FullName = "UMBRA";
			importReleaseDepot2.OH_RL_NKClosestPort = "NZAKL";
			importReleaseDepot2.MainAddress.Address1 = "34 Remuera Road";
			importReleaseDepot2.MainAddress.Address2 = "Remuera";
			importReleaseDepot2.MainAddress.City = "Auckland";
			importReleaseDepot2.MainAddress.Postcode = "1050";
			importReleaseDepot2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment1.JS_OA_ImportReleaseDepot = importReleaseDepot1.MainAddress.PK;
			shipment2.JS_OA_ImportReleaseDepot = importReleaseDepot2.MainAddress.PK;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});
		}

		public void TestDeliverTo_ConsolIsNotDoorDelivery()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container = consol.Containers.AddNew();
			container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CY;

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONSPA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 15";
			consigneeDeliveryAddress.MainAddress.Address2 = "5 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment1.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;
			shipment2.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BLOOP", shippingOrder.DeliverTo.CompanyName);
				AssertEquals("10005", shippingOrder.DeliverTo.Postcode);
				AssertEquals("US", shippingOrder.DeliverTo.Country.Code);
				AssertEquals("199 Crab Road", shippingOrder.DeliverTo.AddressLine1);
				AssertEquals("Crabby", shippingOrder.DeliverTo.AddressLine2);
				AssertEquals("New York", shippingOrder.DeliverTo.City);
			});
		}

		#endregion

		#region TestCarrierBookingReferenceIsRequired

		public void TestCarrierBookingReferenceIsRequired()
		{
			var carrierONE = CreateOrgHeader("ONE", "CARRIER ONE");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrierONE.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoErrors(shippingOrder.CarrierBookingReferenceInfo);

			carrierONE.CustomsCodes.AddNew("CCC", "ONEY", "US");

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(shippingOrder.CarrierBookingReferenceInfo, "For carrier CARONEAAL Carrier Booking Reference is mandatory.");

			shippingOrder.CarrierBookingReference = "aaaa";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoErrors(shippingOrder.CarrierBookingReferenceInfo);

			var carrierTWO = CreateOrgHeader("TWO", "CARRIER TWO");
			consol.JK_OA_ShippingLineAddress = carrierTWO.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoErrors(shippingOrder.CarrierBookingReferenceInfo);

			shippingOrder.CarrierBookingReference = "";
			AssertNoErrors(shippingOrder.CarrierBookingReferenceInfo);
		}

		#endregion

		public void TestNoOfCopiesMandatoryOnSeaWayBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var dirShipment = consol.Shipments.AddNew();
			dirShipment.FillWithValidTestData();
			dirShipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			dirShipment.JS_NoCopyBills = 0;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(shippingOrder.NumberOfCopiesInfo, "No of Copies are required.");

			shippingOrder.NumberOfCopies = 2;
			AssertNoMessageError(shippingOrder.NumberOfCopiesInfo, "No of Copies are required.");
		}

		#region TestCarrierBookingOfficeDefaultsFromCarrierSelectedAddress

		public void TestCarrierBookingOfficeDefaultsFromCarrierSelectedAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "carrier bla";
			carrier.OH_RL_NKClosestPort = "CNCAN";
			carrier.MainAddress.Address1 = "Unit 343";
			carrier.MainAddress.Address2 = "Carrier Lane";
			carrier.MainAddress.City = "Carrier city";
			carrier.MainAddress.Postcode = "3453";
			carrier.MainAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("CNSHA", shippingOrder.CarrierBookingOffice.Code);
		}

		public void TestCarrierBookingOfficeValidation_Build()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var consol = CreateConsol();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("DKAAL", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertHasWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			var firstSeaPort = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Constants.TransportModes.Sea);

			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "AUSYD";
			firstSeaPort.JW_RL_NKLoadPort = "CNSHA";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUSYD", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			consol.JK_RL_NKLoadPort = "AUBNE";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			consol.JK_RL_NKLoadPort = "DEHAM";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertHasWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			firstSeaPort.JW_RL_NKLoadPort = "AUSYD";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUSYD", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			firstSeaPort.JW_RL_NKLoadPort = "AUBNE";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUBNE", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			consol.JK_RL_NKCarrierBookingOffice = "";
			firstSeaPort.JW_RL_NKLoadPort = "CNSHA";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			consol.JK_RL_NKCarrierBookingOffice = "AUSYD";
			consol.JK_RL_NKLoadPort = "AUBNE";
			firstSeaPort.JW_RL_NKLoadPort = "AUMEL";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeValidation_OnValueChanged()
		{
			var warning = "Carrier booking office does not match to UNLOCO or country code of Place of Receipt/Port of Loading.\r\nPlease provide a valid Carrier Booking Office to avoid booking rejection by carrier.";

			var consol = CreateConsol();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("DKAAL", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("CNSHA", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertHasWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			var firstSeaPort = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportMode == Constants.TransportModes.Sea);

			shippingOrder.CarrierBookingOffice.Code = "AUSYD";
			shippingOrder.PlaceOfReceipt.Code = "AUSYD";
			shippingOrder.PortOfLoading.Code = "CNSHA";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUSYD", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.PlaceOfReceipt.Code = "AUBNE";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.PlaceOfReceipt.Code = "DEHAM";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertHasWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.PortOfLoading.Code = "AUSYD";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUSYD", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.PortOfLoading.Code = "AUBNE";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUBNE", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.CarrierBookingOffice.Code = "";
			shippingOrder.PortOfLoading.Code = "CNSHA";
			AssertEquals("", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("DEHAM", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("CNSHA", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);

			shippingOrder.CarrierBookingOffice.Code = "AUSYD";
			shippingOrder.PlaceOfReceipt.Code = "AUBNE";
			shippingOrder.PortOfLoading.Code = "AUMEL";
			AssertEquals("AUSYD", shippingOrder.CarrierBookingOffice.Code);
			AssertEquals("AUBNE", shippingOrder.PlaceOfReceipt.Code);
			AssertEquals("AUMEL", shippingOrder.PortOfLoading.Code);
			AssertNoWarning(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, warning);
		}

		public void TestCarrierBookingOfficeMandatory()
		{
			var carrierBookingOfficeMandatoryMessage = "The Carrier Booking Office is mandatory.\r\nPlease provide it on Consol > Details > Docs > Carrier Booking Office.";

			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoMessageError(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, carrierBookingOfficeMandatoryMessage);

			consol.CarrierBookingOffice.Code = string.Empty;
			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertHasMessageError(((Unloco)shippingOrder.CarrierBookingOffice).CodeInfo, carrierBookingOfficeMandatoryMessage);
		}

		#endregion

		#region TestAddressValidation

		public void TestUSCanadaManifestSelfFilerIDDefault_CA() => TestUSCanadaManifestSelfFilerIDDefault_Helper("CA2KS", "CA");
		public void TestUSCanadaManifestSelfFilerIDDefault_PR() => TestUSCanadaManifestSelfFilerIDDefault_Helper("PRADJ", "PR");
		public void TestUSCanadaManifestSelfFilerIDDefault_GU() => TestUSCanadaManifestSelfFilerIDDefault_Helper("GUAGA", "GU");
		public void TestUSCanadaManifestSelfFilerIDDefault_MP() => TestUSCanadaManifestSelfFilerIDDefault_Helper("MPROP", "MP");
		public void TestUSCanadaManifestSelfFilerIDDefault_VI() => TestUSCanadaManifestSelfFilerIDDefault_Helper("VIAGL", "VI");
		public void TestUSCanadaManifestSelfFilerIDDefault_AS() => TestUSCanadaManifestSelfFilerIDDefault_Helper("ASAPI", "AS");

		public void TestUSCanadaManifestSelfFilerIDDefault_US_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("USLAX");
		public void TestUSCanadaManifestSelfFilerIDDefault_CA_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("CA2KS");
		public void TestUSCanadaManifestSelfFilerIDDefault_PR_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("PRADJ");
		public void TestUSCanadaManifestSelfFilerIDDefault_GU_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("GUAGA");
		public void TestUSCanadaManifestSelfFilerIDDefault_MP_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("MPROP");
		public void TestUSCanadaManifestSelfFilerIDDefault_VI_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("VIAGL");
		public void TestUSCanadaManifestSelfFilerIDDefault_AS_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("ASAPI");

		public void TestUSCanadaManifestSelfFilerIDDefault_Helper(string dischargePort, string dischargeCountry = "")
		{
			var sendingAgent = CreateOrgHeader("SOSA", "SO SendingAgent");
			sendingAgent.CustomsCodes.AddNew("CCC", "1234", "US");
			if (!string.IsNullOrEmpty(dischargeCountry))
			{
				sendingAgent.CustomsCodes.AddNew("CCC", "5555", dischargeCountry);
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNCAN";
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			if (!string.IsNullOrEmpty(dischargeCountry))
			{
				AssertEquals("5555", shippingOrder.USCanadaManifestSelfFilerID);
			}
			else
			{
				AssertEquals("1234", shippingOrder.USCanadaManifestSelfFilerID);
			}
		}

		public void TestUSCanadaManifestSelfFilerIdDefaultsToEmptyWhenDirect()
		{
			var sendingAgent = CreateOrgHeader("SOSA", "SO SendingAgent");
			sendingAgent.CustomsCodes.AddNew("CCC", "1234", "US");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNCAN";
			consol.JK_RL_NKDischargePort = "CA2KS";
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertEquals("SelfFilerId should be empty for DRT consolidations", ZString.Empty, shippingOrder.USCanadaManifestSelfFilerID);
		}

		public void TestCarrierENPWarning()
		{
			var carrier = CreateOrgHeader("MSK", "MAERSK");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			carrier.CustomsCodes.AddNew("CCC", "SUDU", "US");

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			const string warningMessage = "This party does not have an ENP Easipass Code in their Organization > Config which is required if the message is to be sent via Easipass.";

			AssertHasWarning((shippingOrder.Carrier as Address).CompanyNameInfo, warningMessage);

			carrier.CustomsCodes.AddNew("ENP", "12345", "CN");

			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoWarning((shippingOrder.Carrier as Address).CompanyNameInfo, warningMessage);
		}

		public void TestCarrierBookingAgentENPWarning()
		{
			var bookingAgent = CreateOrgHeader("BKG", "BOOKING AGENT");

			var consol = Factory.New<ForwardingConsol>();
			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			const string warningMessage = "This party does not have an ENP Easipass Code in their Organization > Config which is required if the message is to be sent via Easipass.";

			AssertHasWarning((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, warningMessage);

			bookingAgent.CustomsCodes.AddNew("ENP", "12345", "CN");

			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoWarning((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, warningMessage);
		}

		public void TestCarrierBookingAgentC1CError()
		{
			var bookingAgent = CreateOrgHeader("BKG", "BOOKING AGENT");
			bookingAgent.CustomsCodes.AddNew("ENP", "12345", "CN");

			var consol = Factory.New<ForwardingConsol>();
			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			const string errorMessage = "This party does not have a C1C CargoWiseOne Carrier Code in their Organization > Config, which is required for successful message routing.";

			AssertHasMessageError((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, errorMessage);

			bookingAgent.CustomsCodes.AddNew("C1C", "0000");

			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, errorMessage);
		}

		public void TestShippingOrderWithoutCarrierBookingAgent()
		{
			var bookingAgent = CreateOrgHeader("BKG", "BOOKING AGENT");
			bookingAgent.CustomsCodes.AddNew("ENP", "12345", "CN");
			bookingAgent.CustomsCodes.AddNew("C1C", "0000");

			var consol = Factory.New<ForwardingConsol>();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			const string warningMessage = "In the absence of Carrier Handling Agent, the message may be routed to the Carrier (subject to eHub configuration).";

			AssertHasWarning((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, warningMessage);

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoWarning((shippingOrder.CarrierBookingAgent as Address).CompanyNameInfo, warningMessage);
		}

		public void TestSupportElectronicShippingOrderWithCarrierBookingAgent()
		{
			var bookingAgent = CreateOrgHeader("BKG", "BOOKING AGENT");
			bookingAgent.CustomsCodes.AddNew("ENP", "12345", "CN");
			bookingAgent.CustomsCodes.AddNew("C1C", "0000");

			var consol = Factory.New<ForwardingConsol>();

			const string errorMessage = "This carrier does not support electronic Shipping Order. Contact name and email address are required to send your Shipping Order by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			bookingAgent.OH_RSL_ShippingLine = shippingLine.PK;
			shippingLine.RSL_ShippingOrderAvailable = false;
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertHasMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			var contact = bookingAgent.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);
		}

		public void TestSupportElectronicShippingOrderWithOutCarrierBookingAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			const string errorMessage = "This carrier does not support electronic Shipping Order. Contact name and email address are required to send your Shipping Order by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group SHP";

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			shippingLine.RSL_ShippingOrderAvailable = false;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);

			shippingLine.RSL_ShippingOrderAvailable = true;
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).ContactInfo, errorMessage);
			AssertNoMessageError((shippingOrder.CarrierBookingAgent as Address).ContactInfo, errorMessage);
		}

		public void TestPartyNameAndAddressValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var shipper = shippingOrder.Shipper as Address;
			var carrier = shippingOrder.Carrier as Address;
			var consignee = shippingOrder.Consignee as Address;
			var notifyParty = shippingOrder.NotifyParty as Address;

			const string shipperErrorMessage = "Shipper party name and address information is required.";
			const string carrierErrorMessage = "Carrier party name and address information is required.";
			const string consigneeErrorMessage = "Consignee party name and address information is required.";
			const string notifyPartyErrorMessage = "Notify Party party name and address information is required.";

			AssertHasMessageError(shipper.CompanyNameInfo, shipperErrorMessage);
			AssertHasMessageError(carrier.CompanyNameInfo, carrierErrorMessage);
			AssertHasMessageError(consignee.CompanyNameInfo, consigneeErrorMessage);
			AssertHasMessageError(notifyParty.CompanyNameInfo, notifyPartyErrorMessage);

			shipper.CompanyName = "aaa";
			shipper.AddressLine1 = "address line 1";
			shipper.Country.Name = "Sydney";

			carrier.CompanyName = "bbb";
			carrier.AddressLine1 = "address line 1";
			carrier.Country.Name = "Sydney";

			consignee.CompanyName = "ccc";
			consignee.AddressLine1 = "address line 1";
			consignee.Country.Name = "Sydney";

			notifyParty.CompanyName = "ddd";
			notifyParty.AddressLine1 = "address line 1";
			notifyParty.Country.Name = "Sydney";

			AssertNoMessageError(shipper.CompanyNameInfo, shipperErrorMessage);
			AssertNoMessageError(carrier.CompanyNameInfo, carrierErrorMessage);
			AssertNoMessageError(consignee.CompanyNameInfo, consigneeErrorMessage);
			AssertNoMessageError(notifyParty.CompanyNameInfo, notifyPartyErrorMessage);
		}

		public void TestSameAsConsigneeToOrderValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var so = new ShippingOrderBuilder(consol).Build();
			so.Consignee.CompanyName = "TO ORDER";
			so.NotifyParty.CompanyName = "SAME AS CONSIGNEE";

			var consineeAddress = (Address)so.Consignee;
			var notifyAddress = (Address)so.NotifyParty;

			AssertHasMessageError(consineeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertHasMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			so.Consignee.CompanyName = "Consignee A";
			so.Consignee.AddressLine1 = "consignee address 1";
			so.Consignee.Country.Name = "Heaven";

			AssertNoMessageError(consineeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertNoMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			so.Consignee.CompanyName = "TO ORDER";

			AssertHasMessageError(consineeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertHasMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			so.NotifyParty.CompanyName = "notify name A";
			so.NotifyParty.AddressLine1 = "notify address 1";
			so.NotifyParty.Country.Name = "Super Heaven";

			AssertNoMessageError(consineeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertNoMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");
		}

		public void TestDoorPickupDeliveryContactValidatesWhenChecked()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipping = new ShippingOrderBuilder(consol).Build();
			shipping.IsDoorPickup = false;
			shipping.IsDoorDelivery = false;

			AssertNoMessageError(((Address)shipping.PickupFrom).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.");
			AssertNoMessageError(((Address)shipping.DeliverTo).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.");

			shipping.IsDoorPickup = true;
			AssertHasMessageError(((Address)shipping.PickupFrom).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.");

			shipping.IsDoorPickup = false;
			AssertNoMessageError(((Address)shipping.PickupFrom).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.");

			shipping.IsDoorDelivery = true;
			AssertHasMessageError(((Address)shipping.DeliverTo).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.");

			shipping.IsDoorDelivery = false;
			AssertNoMessageError(((Address)shipping.DeliverTo).ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.");
		}

		public void TestDoorPickupContactValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var so = new ShippingOrderBuilder(consol).Build();
			so.IsDoorPickup = true;

			var pickupFromAddress = (Address)so.PickupFrom;
			AssertHasMessageError(pickupFromAddress.ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.");

			pickupFromAddress.Contact = "pickup contact";
			pickupFromAddress.Phone = "123456";
			AssertNoMessageError(pickupFromAddress.ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Pickup’ is selected.");
		}

		public void TestDoorDeliveryContactValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var so = new ShippingOrderBuilder(consol).Build();
			so.IsDoorDelivery = true;

			var deliverToAddress = (Address)so.DeliverTo;
			AssertHasMessageError(deliverToAddress.ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.");

			deliverToAddress.Contact = "deliver contact";
			deliverToAddress.Phone = "123456";
			AssertNoMessageError(deliverToAddress.ContactInfo, "Contact name and Telephone number are mandatory when ‘Door Delivery’ is selected.");
		}

		public void TestContactNameValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var shipper = shippingOrder.Shipper as Address;
			var carrier = shippingOrder.Carrier as Address;
			var consignee = shippingOrder.Consignee as Address;
			var carrierBookingAgent = shippingOrder.CarrierBookingAgent as Address;
			var notifyParty = shippingOrder.NotifyParty as Address;
			var notifyParty2 = shippingOrder.NotifyParty2 as Address;
			var forwarder = shippingOrder.Forwarder as Address;
			var pickupFrom = shippingOrder.PickupFrom as Address;
			var deliverTo = shippingOrder.DeliverTo as Address;

			AssertContactDetailsMessageErrors(shipper, false);
			AssertContactDetailsMessageErrors(carrier, false);
			AssertContactDetailsMessageErrors(consignee, false);
			AssertContactDetailsMessageErrors(carrierBookingAgent, false);
			AssertContactDetailsMessageErrors(notifyParty, false);
			AssertContactDetailsMessageErrors(notifyParty2, false);
			AssertContactDetailsMessageErrors(forwarder, false);
			AssertContactDetailsMessageErrors(pickupFrom, false);
			AssertContactDetailsMessageErrors(deliverTo, false);

			shipper.Contact = "Alan";
			carrier.Contact = "Alan";
			consignee.Contact = "Alan";
			carrierBookingAgent.Contact = "Alan";
			notifyParty.Contact = "Alan";
			notifyParty2.Contact = "Alan";
			forwarder.Contact = "Alan";
			pickupFrom.Contact = "Alan";
			deliverTo.Contact = "Alan";

			AssertContactDetailsMessageErrors(shipper, true);
			AssertContactDetailsMessageErrors(carrier, false);
			AssertContactDetailsMessageErrors(consignee, true);
			AssertContactDetailsMessageErrors(carrierBookingAgent, true);
			AssertContactDetailsMessageErrors(notifyParty, true);
			AssertContactDetailsMessageErrors(notifyParty2, true);
			AssertContactDetailsMessageErrors(forwarder, true);
			AssertContactDetailsMessageErrors(pickupFrom, true);
			AssertContactDetailsMessageErrors(deliverTo, true);

			shipper.Phone = "1234567";
			carrier.Email = "a@a.com";
			consignee.Fax = "7654321";
			carrierBookingAgent.Fax = "7654321";
			notifyParty.Phone = "999";
			notifyParty2.Email = "c@d.com";
			forwarder.Fax = "234747489";
			pickupFrom.Phone = "1092348576";
			deliverTo.Email = "blah@blah.com";

			AssertContactDetailsMessageErrors(shipper, false);
			AssertContactDetailsMessageErrors(carrier, false);
			AssertContactDetailsMessageErrors(consignee, false);
			AssertContactDetailsMessageErrors(carrierBookingAgent, false);
			AssertContactDetailsMessageErrors(notifyParty, false);
			AssertContactDetailsMessageErrors(notifyParty2, false);
			AssertContactDetailsMessageErrors(forwarder, false);
			AssertContactDetailsMessageErrors(pickupFrom, false);
			AssertContactDetailsMessageErrors(deliverTo, false);

			shipper.Contact = "1";
			consignee.Contact = ".";
			notifyParty.Contact = "a";
			carrierBookingAgent.Contact = "11";
			notifyParty.Contact = "...";
			notifyParty2.Contact = "aaaaaaa";
			forwarder.Contact = "bbbbbbbbbbbb";
			pickupFrom.Contact = "000000000000000";
			deliverTo.Contact = "????????????????????";
			AssertContactDetailsMessageErrors(shipper, true);
			AssertContactDetailsMessageErrors(consignee, true);
			AssertContactDetailsMessageErrors(notifyParty, true);
			AssertContactDetailsMessageErrors(carrierBookingAgent, true);
			AssertContactDetailsMessageErrors(notifyParty, true);
			AssertContactDetailsMessageErrors(notifyParty2, true);
			AssertContactDetailsMessageErrors(forwarder, true);
			AssertContactDetailsMessageErrors(pickupFrom, true);
			AssertContactDetailsMessageErrors(deliverTo, true);
		}

		void AssertContactDetailsMessageErrors(Address address, bool hasMessageErrors)
		{
			const string errorMessage = "Please enter both contact name and at least one communication: phone, email or fax.";

			if (hasMessageErrors)
			{
				AssertHasMessageError(address.ContactInfo, errorMessage);
			}
			else
			{
				AssertNoMessageError(address.ContactInfo, errorMessage);
			}
		}

		public void TestNotifyPartyValidation()
		{
			var usPostcodeErrorMessage = "Notify Party postcode is required for US imports.";

			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var consignee = (Address)shippingOrder.Consignee;
			var notifyParty = (Address)shippingOrder.NotifyParty;

			AssertNoMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);

			notifyParty.Country.Code = "US";
			AssertHasMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);

			notifyParty.Postcode = "50101";
			AssertNoMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);
		}

		public void TestPortsValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var portOfLading = shippingOrder.PortOfLoading;
			var portOfDischarge = shippingOrder.PortOfDischarge;

			const string portOfLoadingCodeErrorMessage = "Port Of Loading Code is required.";
			const string portOfLoadingNameErrorMessage = "Port Of Loading Name is required.";

			const string portOfDischargeCodeErrorMessage = "Port Of Discharge Code is required.";
			const string portOfDischargeNameErrorMessage = "Port Of Discharge Name is required.";

			AssertHasMessageError(portOfLading.CodeInfo, portOfLoadingCodeErrorMessage);
			AssertHasMessageError(portOfLading.NameInfo, portOfLoadingNameErrorMessage);

			AssertHasMessageError(portOfDischarge.CodeInfo, portOfDischargeCodeErrorMessage);
			AssertHasMessageError(portOfDischarge.NameInfo, portOfDischargeNameErrorMessage);

			portOfLading.Code = "AUSYD";
			portOfLading.Name = "Sydney";

			portOfDischarge.Code = "USCHI";
			portOfDischarge.Name = "Chicago";

			AssertNoMessageError(portOfLading.CodeInfo, portOfLoadingCodeErrorMessage);
			AssertNoMessageError(portOfLading.NameInfo, portOfLoadingNameErrorMessage);

			AssertNoMessageError(portOfDischarge.CodeInfo, portOfDischargeCodeErrorMessage);
			AssertNoMessageError(portOfDischarge.NameInfo, portOfDischargeNameErrorMessage);
		}

		public void TestPlaceAndDateOfIssueValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var placeOfIssue = shippingOrder.PlaceOfIssue;

			const string placeOfIssueErrorMessage = "Place of Issue is required if Date of Issue is entered.";
			const string dateOfIssueErrorMessage = "Date of Issue is required if Place of Issue is entered.";

			AssertNoMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertNoMessageError(shippingOrder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);

			shippingOrder.RequestedDateOfIssue = ZDateTime.Today;

			AssertHasMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertNoMessageError(shippingOrder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);

			placeOfIssue.Code = "AUSYD";

			AssertNoMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertNoMessageError(shippingOrder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);

			shippingOrder.RequestedDateOfIssue = ZDateTime.Empty;

			AssertNoMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertHasMessageError(shippingOrder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);
		}

		public void TestAddAsciiCharactersValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Containers.AddNew();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("Shipper", (Address)shippingOrder.Shipper);
				AssertAsciiCharactersValidation("Carrier", (Address)shippingOrder.Carrier);
				AssertAsciiCharactersValidation("Consignee", (Address)shippingOrder.Consignee);
				AssertAsciiCharactersValidation("CarrierHandlingAgent", (Address)shippingOrder.CarrierHandlingAgent);
				AssertAsciiCharactersValidation("CarrierBookingAgent", (Address)shippingOrder.CarrierBookingAgent);
				AssertAsciiCharactersValidation("NotifyParty", (Address)shippingOrder.NotifyParty);
				AssertAsciiCharactersValidation("NotifyParty2", (Address)shippingOrder.NotifyParty2);
				AssertAsciiCharactersValidation("Forwarder", (Address)shippingOrder.Forwarder);
				AssertAsciiCharactersValidation("PickupFrom", (Address)shippingOrder.PickupFrom);
				AssertAsciiCharactersValidation("DeliverTo", (Address)shippingOrder.DeliverTo);
				AssertAsciiCharactersValidation("CurrentUser", (Address)shippingOrder.CurrentUser);

				AssertAsciiCharactersValidation("Transports[0].Carrier", ((DocDataObjects.Transport)shippingOrder.Transports.ToArray()[0]).Carrier);
				AssertAsciiCharactersValidation("Containers[0].VerifiedByAddress", shippingOrder.Containers.ToArray()[0].VerifiedByAddress);

				AssertAsciiCharactersValidation("PortOfLoading", shippingOrder.PortOfLoading);
				AssertAsciiCharactersValidation("PortOfDischarge", shippingOrder.PortOfDischarge);
				AssertAsciiCharactersValidation("PlaceOfReceipt", shippingOrder.PlaceOfReceipt);
				AssertAsciiCharactersValidation("PlaceOfDelivery", shippingOrder.PlaceOfDelivery);
				AssertAsciiCharactersValidation("PlaceOfIssue", shippingOrder.PlaceOfIssue);
				AssertAsciiCharactersValidation("CarrierBookingOffice", (Unloco)shippingOrder.CarrierBookingOffice);
				AssertAsciiCharactersValidation("OperationalPort", (Unloco)shippingOrder.OperationalPort);
				AssertAsciiCharactersValidation("FreightPayableAt", (Unloco)shippingOrder.FreightPayableAt);

				AssertAsciiCharactersValidation("VoyageFlightNumber", shippingOrder.VoyageFlightNumberInfo);
				AssertAsciiCharactersValidation("Vessel", (Vessel)shippingOrder.Vessel);
				AssertAsciiCharactersValidation("ForwardingInstructions", shippingOrder.ForwardingInstructionsInfo);
				AssertAsciiCharactersValidation("GoodsHandlingInstructions", shippingOrder.GoodsHandlingInstructionsInfo);
				AssertAsciiCharactersValidation("SpecialInstructions", shippingOrder.SpecialInstructionsInfo);
			});
		}

		public void TestConsigneeValidation_ToOrder()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var consingee = shippingOrder.Consignee as Address;

			AssertAddressToOrder("Consignee", consingee);
		}

		#endregion

		#region TestPackingLinesValidation

		public void TestPackingLinesValidation_ContactDetailsOnDangerousGoods()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var undg = packline.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var dangerousGood = shippingOrder
				.Containers
				.Single()
				.PackingLines
				.Single()
				.DangerousGoods
				.Single();

			var dangerousGoodContact = dangerousGood.Contact;

			AssertHasMessageError(dangerousGoodContact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertHasMessageError(dangerousGoodContact.PhoneInfo, "Contact Phone is required for dangerous goods.");

			dangerousGoodContact.FullName = "Ronald";
			dangerousGoodContact.Phone = "+61411222333";

			AssertNoMessageError(dangerousGoodContact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertNoMessageError(dangerousGoodContact.PhoneInfo, "Contact Phone is required for dangerous goods.");
		}

		public void TestPackingLinesValidation_DangerousGoods_Substance()
		{
			var errorMessage = "DG Class, UNDG and Proper Shipping Name are required for dangerous goods.\r\nPlease enter Shipment > Packing > Pack Lines > Dangerous Goods > DG Substance.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline = shipment.OuterPackLines.AddNew();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "";
			contact.OC_Phone = "";

			var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_PSN = "CLASPSN";

			var undg = packline.UNDGs.AddNew();
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DG = undgSubstance.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			DangerousGood GetDangerousGood()
			{
				return shippingOrder.Containers.Single()
					.PackingLines.Single()
					.DangerousGoods.Single();
			}
			var dangerousGood = GetDangerousGood();

			Assert(!dangerousGood.Validator().Any());

			undg.DI_IMOClass = ZString.Empty;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(!dangerousGood.Validator().Any());

			undgSubstance.DG_PSN = ZString.Empty;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_PSN = "CLASPSN";
			undgSubstance.DG_Class = ZString.Empty;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undgSubstance.DG_Class = "CLAS";
			undgSubstance.DG_Code = ZString.Empty;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));

			undg.DI_DG = ZGuid.Empty;
			undg.DI_IMOClass = "CLAS";

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			dangerousGood = GetDangerousGood();
			Assert(dangerousGood.Validator().Contains(errorMessage));
		}

		public void TestPackingLinesValidation_ForMalaysia()
		{
			var toErrorMessage = "It is recommended to fill in Harmonized Code for Sea imports to Malaysia.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var harmonizedCode = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.First().HarmonizedCode;
			AssertHasWarningContaining("Harmonized Code is recommended for Malaysia", harmonizedCode.CodeInfo, toErrorMessage);

			void RefreshImportHarmonizedCode()
			{
				shippingOrder = new ShippingOrderBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.First().HarmonizedCode;
			}

			packLine.JL_HarmonisedCode = "11111";
			RefreshImportHarmonizedCode();
			AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

			packLine.JL_HarmonisedCode = string.Empty;
			var harmonizedCodes = packLine.HarmonisedCodes.AddNew();
			harmonizedCodes.JLH_RN_NKCountry = "MY";
			harmonizedCodes.JLH_Code = "55555";
			RefreshImportHarmonizedCode();
			AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);

			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";
			RefreshImportHarmonizedCode();
			AssertNoWarnings("Harmonized Code has no warnings", harmonizedCode.CodeInfo);
		}

		public void TestPackingLinesValidation_PackingLineCount()
		{
			var requireContainerAndPackingLinesMessageError = "Container and Packing Lines details are required for Shipping Order and Amendment messages.";
			var maximumPackingLinesCountMessageError = "Maximum 999 packlines can be included in a Shipping Order message.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			var shipment = consol.Shipments.AddNew();

			var shippingOrder = new ShippingOrderBuilder(consol);
			var data = shippingOrder.Build();
			AssertHasMessageError(data.ErrorPlaceholderInfo, requireContainerAndPackingLinesMessageError);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";

			data = shippingOrder.Build();
			AssertNoMessageError(data.ErrorPlaceholderInfo, requireContainerAndPackingLinesMessageError);
			AssertNoMessageError(data.ErrorPlaceholderInfo, maximumPackingLinesCountMessageError);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			for (int i = 0; i < 1000; i++)
			{
				container.PackLines.Add(shipment.OuterPackLines.AddNew());
			}
			data = shippingOrder.Build();
			AssertHasMessageError(data.ErrorPlaceholderInfo, maximumPackingLinesCountMessageError);
		}

		#endregion

		#region TestTransportDetailsValidation

		public void TestVesselVoyageETDValidation()
		{
			var futureETDMessageError = "ETD must not be more than 400 days in advance";
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var transport = (DocDataObjects.Transport)shippingOrder.Transports.First();
			AssertNoMessageError(transport.ETDInfo, futureETDMessageError);

			transport.ETD = ZDateTime.Now.AddDays(401);
			AssertHasMessageError(transport.ETDInfo, futureETDMessageError);
		}

		public void TestVesselVoyageETDValidation_ForMainLeg()
		{
			var missingETDMessageError = "ETD is required.";
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var mainTransport = (DocDataObjects.Transport)shippingOrder.Transports.First();

			mainTransport.ETD = ZDateTime.Empty;
			AssertHasMessageError(mainTransport.ETDInfo, missingETDMessageError);

			mainTransport.ETD = ZDateTime.Now;
			AssertNoMessageError(mainTransport.ETDInfo, missingETDMessageError);

			var otherTransport = (DocDataObjects.Transport)shippingOrder.Transports.Last();

			otherTransport.ETD = ZDateTime.Empty;
			AssertNoMessageError(mainTransport.ETDInfo, missingETDMessageError);
		}

		public void TestVesselVoyageETDValidation_ForSea()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var transport = (DocDataObjects.Transport)shippingOrder.Transports.First();

			const string vesselVoyageErrorMessage = "Vessel and Voyage are required when ETD is blank.";
			const string etdErrorMessage = "ETD is required when Vessel and Voyage are blank.";

			AssertEquals("Pre-condition", "SEA", transport.Mode.Code);
			AssertEquals("Pre-condition", "Dragon", transport.Vessel.Name);
			AssertEquals("Pre-condition", "111", transport.VoyageFlightNumber);
			AssertEquals("Pre-condition", ZDateTime.Empty, transport.ETD);

			var vessel = transport.Vessel;

			AssertNoMessageError(vessel.NameInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.VoyageFlightNumberInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.ETDInfo, etdErrorMessage);

			vessel.Name = ZString.Empty;

			AssertHasMessageError(vessel.NameInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.VoyageFlightNumberInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.ETDInfo, etdErrorMessage);

			transport.VoyageFlightNumber = ZString.Empty;

			AssertHasMessageError(vessel.NameInfo, vesselVoyageErrorMessage);
			AssertHasMessageError(transport.VoyageFlightNumberInfo, vesselVoyageErrorMessage);
			AssertHasMessageError(transport.ETDInfo, etdErrorMessage);

			transport.ETD = ZDateTime.Today;

			AssertNoMessageError(vessel.NameInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.VoyageFlightNumberInfo, vesselVoyageErrorMessage);
			AssertNoMessageError(transport.ETDInfo, etdErrorMessage);
		}

		public void TestVesselVoyageETDValidation_ForNonSea()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var transport = (DocDataObjects.Transport)shippingOrder.Transports.Last();

			AssertNotEquals("Pre-condition", "SEA", transport.Mode.Code);
			AssertEquals("Pre-condition", "Tommy", transport.Vessel.Name);
			AssertEquals("Pre-condition", "444", transport.VoyageFlightNumber);
			AssertEquals("Pre-condition", ZDateTime.Empty, transport.ETD);

			var vessel = transport.Vessel;
			Assert(!vessel.NameInfo.HasNotifications());
			Assert(!transport.VoyageFlightNumberInfo.HasNotifications());
			Assert(!transport.ETDInfo.HasNotifications());

			vessel.Name = ZString.Empty;
			Assert(!vessel.NameInfo.HasNotifications());
			Assert(!transport.VoyageFlightNumberInfo.HasNotifications());
			Assert(!transport.ETDInfo.HasNotifications());

			transport.VoyageFlightNumber = ZString.Empty;
			Assert(!vessel.NameInfo.HasNotifications());
			Assert(!transport.VoyageFlightNumberInfo.HasNotifications());
			Assert(!transport.ETDInfo.HasNotifications());

			transport.ETD = ZDateTime.Today;
			Assert(!vessel.NameInfo.HasNotifications());
			Assert(!transport.VoyageFlightNumberInfo.HasNotifications());
			Assert(!transport.ETDInfo.HasNotifications());
		}

		#endregion

		#region ShippingLineMessagingRequirements Validation

		public void TestContractNumberValidation()
		{
			var carrierContractNumberWarningMessage = "It is recommended to fill in Carrier Contract or Quote Number to assist with faster booking and reconciliation processes.";

			var consol = CreateConsolWithRefShippingLineMessagingRequirement("CON");
			var data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertNoWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);

			consol.JK_CarrierContractNumber = null;
			data = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertNoWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);

			consol = CreateConsolWithRefShippingLineMessagingRequirement("CON", false);
			data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertNoWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);

			consol = CreateConsol();
			data = new ShippingOrderBuilder(consol).Build();

			AssertNoMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertNoWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);

			consol.JK_CarrierContractNumber = null;
			data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertHasWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);

			var quotationNumber = consol.Numbers.AddNew();
			quotationNumber.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			quotationNumber.CE_EntryType = AdditionalReferences.Codes.CarrierQuoteNumber;
			quotationNumber.CE_EntryNum = "QUOT123";
			data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.CarrierContractNumberInfo, ShippingLineMessagingRequirement.ValidationMessages.ContractNumberMandatory);
			AssertNoWarning(data.CarrierContractNumberInfo, carrierContractNumberWarningMessage);
		}

		public void TestNamedAccountValidation()
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement("NAM");
			var data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);

			consol.Numbers.DeleteAll();
			data = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);

			consol = CreateConsolWithRefShippingLineMessagingRequirement("NAM", false);
			data = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(data.ContractNamedAccountInfo, ShippingLineMessagingRequirement.ValidationMessages.NamedAccountMandatory);
		}

		public void TestDangerousGoodsWeightValidation()
		{
			var consol = CreateConsolWithRefShippingLineMessagingRequirement("DGW");
			var data = new ShippingOrderBuilder(consol).Build();
			var dangerousGood = data.Containers.SelectMany(c => c.PackingLines.SelectMany(p => p.DangerousGoods)).First();
			AssertNoMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);

			dangerousGood.Weight.Value = 0;
			AssertHasMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);

			consol = CreateConsolWithRefShippingLineMessagingRequirement("DGW", false);
			data = new ShippingOrderBuilder(consol).Build();
			dangerousGood = data.Containers.SelectMany(c => c.PackingLines.SelectMany(p => p.DangerousGoods)).First();
			AssertNoMessageError(dangerousGood.Weight.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DGNetWeightMandatory);
		}

		public void TestAcceptEitherAirflowOrHumidityValidation()
		{
			SetupAndAssertAirflowHumidity(true, false, false, false);
			SetupAndAssertAirflowHumidity(true, true, true, true);
			SetupAndAssertAirflowHumidity(true, false, true, false);
			SetupAndAssertAirflowHumidity(true, true, false, false);

			SetupAndAssertAirflowHumidity(false, false, false, false);
			SetupAndAssertAirflowHumidity(false, true, true, false);

			SetupAndAssertAirflowHumidityOtherCases(0, 0, ZString.Empty, false, true);
			SetupAndAssertAirflowHumidityOtherCases(5, 0, "MQH", true, true);

			void SetupAndAssertAirflowHumidity(bool requirementFlag, bool hasHumidity, bool hasAirVentFlow, bool shouldHaveError)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement("AOH", requirementFlag);
				var consolContainer = (ForwardingContainer)consol.Containers.First();
				consolContainer.JC_IsControlledAtmosphere = true;

				if (hasHumidity)
				{
					consolContainer.JC_HumidityPercent = 5;
				}

				if (hasAirVentFlow)
				{
					consolContainer.JC_AirVentFlow = 5;
					consolContainer.JC_AirVentFlowRateUnit = "MQH";
				}

				var data = new ShippingOrderBuilder(consol).Build();
				var container = data.Containers.OfType<Container>().First();

				if (shouldHaveError)
				{
					AssertHasMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertHasMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
				else
				{
					AssertNoMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertNoMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
			}

			void SetupAndAssertAirflowHumidityOtherCases(int humidityPercent, int airVentFlow, string airVentFlowRateUnit, bool shouldHaveError, bool requirementFlag)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement("AOH", requirementFlag);
				var consolContainer = (ForwardingContainer)consol.Containers.First();
				consolContainer.JC_IsControlledAtmosphere = true;

				consolContainer.JC_HumidityPercent = (ZByte)humidityPercent;
				consolContainer.JC_AirVentFlow = airVentFlow;
				consolContainer.JC_AirVentFlowRateUnit = airVentFlowRateUnit;

				var data = new ShippingOrderBuilder(consol).Build();
				var container = data.Containers.OfType<Container>().First();

				if (shouldHaveError)
				{
					AssertHasMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertHasMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
				else
				{
					AssertNoMessageError(container.Humidity.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
					AssertNoMessageError(container.AirVentFlow.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.AcceptEitherAirflowOrHumidity);
				}
			}
		}

		public void TestDimensionsForOOGValidation()
		{
			void SetupAndAssertDimensions(bool requirementFlag, string containerType, bool hasDimensions, bool shouldHaveError)
			{
				var consol = CreateConsolWithRefShippingLineMessagingRequirement("DOG", requirementFlag);

				var consolContainer = (ForwardingContainer)consol.Containers.First();
				var refContainer = Factory.New<RefContainer>();
				refContainer.RC_ISOType = containerType;
				consolContainer.JC_RC = refContainer.PK;

				if (hasDimensions)
				{
					var consolPackline = (PackLine)consolContainer.PackLines.First();
					consolPackline.JL_Length = 1;
					consolPackline.JL_Width = 1;
					consolPackline.JL_Height = 1;
				}

				var data = new ShippingOrderBuilder(consol).Build();
				var packline = data.Containers.SelectMany(c => c.PackingLines).First();

				if (shouldHaveError)
				{
					AssertHasMessageError(packline.Height.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
					AssertHasMessageError(packline.Length.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
					AssertHasMessageError(packline.Width.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
				}
				else
				{
					AssertNoMessageError(packline.Height.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
					AssertNoMessageError(packline.Length.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
					AssertNoMessageError(packline.Width.ValueInfo, ShippingLineMessagingRequirement.ValidationMessages.DimensionsMandatoryForOOG);
				}
			}

			SetupAndAssertDimensions(true, "UUUU", false, true);
			SetupAndAssertDimensions(true, "PPPP", false, true);
			SetupAndAssertDimensions(true, "AAAA", false, false);
			SetupAndAssertDimensions(true, "UUUU", true, false);
			SetupAndAssertDimensions(true, "PPPP", true, false);
			SetupAndAssertDimensions(true, "AAAA", true, false);

			SetupAndAssertDimensions(false, "UUUU", false, false);
			SetupAndAssertDimensions(false, "PPPP", false, false);
		}

		ForwardingConsol CreateConsolWithRefShippingLineMessagingRequirement(string requirementType, bool requirementFlag = true)
		{
			var consol = CreateConsol();

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_ShippingOrderAvailable = true;

			var requirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			requirement.RSR_RST_NKType = requirementType;
			requirement.RSR_IsShippingOrder = requirementFlag;
			requirement.RSR_IsShippingInstruction = requirementFlag;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			return consol;
		}

		#endregion

		#region Goods Details

		public void TestPopulateContainerIsNonOperativeReefer()
		{
			var consol = CreateConsol();

			var container1 = consol.Containers.First() as ForwardingContainer;
			container1.JC_ContainerNum = "AAAA0000006";
			container1.JC_IsNonOperativeReefer = true;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "AAAA0000008";
			container2.JC_DeliveryMode = "CFS/CY";
			container2.JC_IsShipperOwned = true;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 1000;
			container2.JC_DunnageWeight = 1000;
			container2.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			container2.JC_IsNonOperativeReefer = false;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			Assert(shippingOrder.Containers.First(x => x.Number == "AAAA0000006").IsNonOperativeReefer);
			Assert(!shippingOrder.Containers.First(x => x.Number == "AAAA0000008").IsNonOperativeReefer);
		}

		public void TestContainerValidation_ISOType()
		{
			const string expectedErrorMessage = "This container does not have a valid ISO Code. Enter a Valid ISO Code to the Container Reference File via Consol > Container > Container Type.";
			const string expectedWarningMessage = "Container type entered does not have a valid ISO equipment code. Enter a valid ISO code to the matching Container Reference file.";

			var consol = CreateConsol();
			var consolContainer = (ForwardingContainer)consol.Containers.First();
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "AAAA";
			consolContainer.JC_RC = refContainer.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var container = shippingOrder.Containers.First();
			AssertNoMessageError(container.Type.CodeInfo, expectedErrorMessage);
			AssertHasWarning(container.Type.CodeInfo, expectedWarningMessage);

			refContainer.RC_ISOType = ZString.Empty;
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			container = shippingOrder.Containers.First();
			AssertHasMessageError(container.Type.CodeInfo, expectedErrorMessage);
			AssertNoWarning(container.Type.CodeInfo, expectedWarningMessage);

			refContainer.RC_ISOType = "22G0";
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			container = shippingOrder.Containers.First();
			AssertNoMessageError(container.Type.CodeInfo, expectedErrorMessage);
			AssertNoWarning(container.Type.CodeInfo, expectedWarningMessage);
		}

		public void TestContainerValidation_WithContainerNumber()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var container = shippingOrder.Containers.First();

			const string numberRequiredErrorMessage = "Container number is required when Shipper Owned.";

			container.IsShipperOwned = true;
			container.Number = "Container#Test";

			AssertNoMessageError(container.NumberInfo, numberRequiredErrorMessage);

			container.Number = ZString.Empty;

			AssertHasMessageError(container.NumberInfo, numberRequiredErrorMessage);

			container.IsShipperOwned = false;

			AssertNoMessageError(container.NumberInfo, numberRequiredErrorMessage);
		}

		public void TestContainerValidation_ContainerNumberIsValid()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var container = shippingOrder.Containers.First();
			const string invalidISONumberErrorMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";

			container.Number = "AAAA0000007";
			container.IsShipperOwned = true;
			AssertNoMessageError(container.NumberInfo, invalidISONumberErrorMessage);

			container.Number = "Not ISO Valid";
			container.IsShipperOwned = true;
			AssertNoMessageError(container.NumberInfo, invalidISONumberErrorMessage);

			container.IsShipperOwned = false;
			AssertHasMessageError(container.NumberInfo, invalidISONumberErrorMessage);
		}

		public void TestContainerValidation_WithPackCount()
		{
			var consol = CreateConsol();
			consol.Containers[0].JC_IsEmptyContainer = false;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var container = shippingOrder.Containers.First();

			const string packCountRequiredErrorMessage = "You have not entered a value or there are unpacked packings with 0 quantity. If this is intended, please flag the container as empty..";

			container.PackCount = 10;

			AssertNoMessageError(container.PackCountInfo, packCountRequiredErrorMessage);

			container.PackCount = 0;

			AssertHasMessageError(container.PackCountInfo, packCountRequiredErrorMessage);

			container.IsEmpty = true;

			AssertNoMessageError(container.PackCountInfo, packCountRequiredErrorMessage);
		}

		public void TestContainerValidation_WithAirVentFlow()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var container = shippingOrder.Containers.First();

			var airVentErrorMessage = "No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab.";
			var airVentUnitErrorMessage = "Selected Air Vent Setting measurement type is not supported by the carrier. Ensure each temperature controlled container has selected either '2L' or 'MQH' on the Containers > Refrigeration tab.";
			var airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH = "Air Vent is marked as \"Open\".\r\nTo indicate \"Closed\", leave the Consol > Containers > Refrigeration > Air Vent Setting > Unit field blank.";
			var airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L = "Carriers accept airflow only in metric units.\r\nAny value entered as cubic feet per minute (2L) is automatically converted to cubic meters per hour (MQH) during transmission.";

			container.HasControlledAtmosphere = true;
			container.AirVentFlow.Unit.Code = "2L";
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertHasWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);

			container.AirVentFlow.Unit.Code = "ABC";
			container.AirVentFlow.Value = 1;

			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertHasMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);

			container.AirVentFlow.Unit.Code = ZString.Empty;
			container.AirVentFlow.Value = 1;

			AssertHasMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);

			container.HasControlledAtmosphere = false;

			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentErrorMessage);
			AssertNoMessageError(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentUnitErrorMessage);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);

			container.AirVentFlow.Unit.Code = "MQH";
			container.AirVentFlow.Value = 0;

			AssertHasWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);

			container.AirVentFlow.Unit.Code = "2L";
			container.AirVentFlow.Value = 1;

			AssertHasWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIsNonZeroAndUnitIs2L);
			AssertNoWarning(((CodeDescription)container.AirVentFlow.Unit).CodeInfo, airVentWarningMessageWhenFlowRateIs0AndUnitIsMQH);
		}

		public void TestContainerValidation_WithSetTemperature()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var container = shippingOrder.Containers.First();

			const string setTemperatureErrorMessage = "The default temperature has not yet been verified by the user.Please check the temperature and the unit of temperature against the container on the Consol";

			container.HasControlledAtmosphere = true;

			container.SetTemperature.Unit.Code = "C";
			AssertNoMessageError(((CodeDescription)container.SetTemperature.Unit).CodeInfo, setTemperatureErrorMessage);

			container.SetTemperature.Unit.Code = ZString.Empty;
			AssertHasMessageError(((CodeDescription)container.SetTemperature.Unit).CodeInfo, setTemperatureErrorMessage);

			container.HasControlledAtmosphere = false;

			AssertNoMessageError(((CodeDescription)container.SetTemperature.Unit).CodeInfo, setTemperatureErrorMessage);
		}

		public void TestPacksValidation()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			var pack = shippingOrder.Containers.First().PackingLines.First();

			var stringWithNonASCIICharacters = "的,.123";
			var weightUnitErrorMessage = "Pack weight unit is mandatory.";
			var marksAndNumbersErrorMessage = "Marks are required.";
			var goodsDescriptionErrorMessage = "Goods Description is required.";
			var nonASCIICharactersErrorMessage = "Most messaging providers do not support non ASCII characters.";

			pack.Weight.Unit.Code = ZString.Empty;
			AssertHasMessageError(((CodeDescription)pack.Weight.Unit).CodeInfo, weightUnitErrorMessage);

			pack.Weight.Unit.Code = "KG";
			AssertNoMessageError(((CodeDescription)pack.Weight.Unit).CodeInfo, weightUnitErrorMessage);

			pack.MarksAndNumbers = ZString.Empty;
			AssertHasMessageError(pack.MarksAndNumbersInfo, marksAndNumbersErrorMessage);

			pack.MarksAndNumbers = stringWithNonASCIICharacters;
			AssertHasMessageError(pack.MarksAndNumbersInfo, nonASCIICharactersErrorMessage);

			pack.MarksAndNumbers = "Marks 123";
			AssertNoMessageError(pack.MarksAndNumbersInfo, marksAndNumbersErrorMessage);
			AssertNoMessageError(pack.MarksAndNumbersInfo, nonASCIICharactersErrorMessage);

			pack.GoodsDescription = ZString.Empty;
			AssertHasMessageError(pack.GoodsDescriptionInfo, goodsDescriptionErrorMessage);

			pack.GoodsDescription = stringWithNonASCIICharacters;
			AssertHasMessageError(pack.GoodsDescriptionInfo, nonASCIICharactersErrorMessage);

			pack.GoodsDescription = "Goods Desc";
			AssertNoMessageError(pack.GoodsDescriptionInfo, goodsDescriptionErrorMessage);
			AssertNoMessageError(pack.GoodsDescriptionInfo, nonASCIICharactersErrorMessage);
		}

		public void TestPacksValidation_IsGroupAndConsolidatePackingLines()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
				var shippingOrder = new ShippingOrderBuilder(consol).Build();

				var pack = shippingOrder.Shipments.First().PackingLines.First();

				var stringWithNonASCIICharacters = "的,.123";
				var weightUnitErrorMessage = "Pack weight unit is mandatory.";
				var marksAndNumbersErrorMessage = "Marks are required.";
				var goodsDescriptionErrorMessage = "Goods Description is required.";
				var nonASCIICharactersErrorMessage = "Most messaging providers do not support non ASCII characters.";

				pack.Weight.Unit.Code = ZString.Empty;
				AssertHasMessageError(((CodeDescription)pack.Weight.Unit).CodeInfo, weightUnitErrorMessage);

				pack.Weight.Unit.Code = "KG";
				AssertNoMessageError(((CodeDescription)pack.Weight.Unit).CodeInfo, weightUnitErrorMessage);

				pack.MarksAndNumbers = ZString.Empty;
				AssertHasMessageError(pack.MarksAndNumbersInfo, marksAndNumbersErrorMessage);

				pack.MarksAndNumbers = stringWithNonASCIICharacters;
				AssertHasMessageError(pack.MarksAndNumbersInfo, nonASCIICharactersErrorMessage);

				pack.MarksAndNumbers = "Marks 123";
				AssertNoMessageError(pack.MarksAndNumbersInfo, marksAndNumbersErrorMessage);
				AssertNoMessageError(pack.MarksAndNumbersInfo, nonASCIICharactersErrorMessage);

				pack.GoodsDescription = ZString.Empty;
				AssertHasMessageError(pack.GoodsDescriptionInfo, goodsDescriptionErrorMessage);

				pack.GoodsDescription = stringWithNonASCIICharacters;
				AssertHasMessageError(pack.GoodsDescriptionInfo, nonASCIICharactersErrorMessage);

				pack.GoodsDescription = "Goods Desc";
				AssertNoMessageError(pack.GoodsDescriptionInfo, goodsDescriptionErrorMessage);
				AssertNoMessageError(pack.GoodsDescriptionInfo, nonASCIICharactersErrorMessage);
			}
		}

		public void TestPacksValidation_IsGroupAndConsolidatePackingLines_WhenSubPackLineHasZeroPackWeightAndZeroVolume()
		{
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByShipment);
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByPackLine);

			void AssertSubPackLineHasZeroPackCount(ZString packageGrouping)
			{
				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
					var rule = CreateRefCountryRules(Constants.CountryCodes.China, Constants.CountryCodes.Australia, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

					var consol = CreateConsol();
					consol.JK_PackageGrouping = packageGrouping;

					var builder = new ShippingOrderBuilder(consol);
					var shippingOrder = builder.Build();

					var warningMessageWeight = @"There are pack lines with zero (0) Weight.
Please verify in Shipment>Packing>Weight on following Shipments:
SH0001000.";

					var errorMessageVolume = @"There are pack lines with zero (0) Volume.
Please verify in Shipment>Packing>Volume on following Shipments:
SH0001000.";

					var groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertNoWarning(groupedPackingLine.Weight.ValueInfo, warningMessageWeight);
					AssertNoMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageVolume);

					var firstShipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
					AssertNotNull(firstShipment);

					var firstPackLine = firstShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
					AssertNotNull(firstPackLine);

					firstPackLine.JL_ActualWeight = 0;
					firstPackLine.JL_ActualVolume = 0;
					shippingOrder = builder.Build();
					groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageWeight);
					AssertHasMessageError(groupedPackingLine.Volume.ValueInfo, errorMessageVolume);
				}
			}
		}

		public void TestPacksValidation_IsGroupAndConsolidatePackingLines_WhenPackLineHasZeroPackWeight()
		{
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByShipment);
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByPackLine);

			void AssertSubPackLineHasZeroPackCount(ZString packageGrouping)
			{
				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
					var rule = CreateRefCountryRules(Constants.CountryCodes.China, Constants.CountryCodes.Australia, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

					var consol = CreateConsol();
					consol.JK_PackageGrouping = packageGrouping;

					var builder = new ShippingOrderBuilder(consol);
					var shippingOrder = builder.Build();

					var warningMessageWeight = @"There are pack lines with zero (0) Weight.
Please verify in Shipment>Packing>Weight on following Shipments:
SH0001000.";
					var errorMessageWeight = @"Total packing line weight is required. Please enter a value.";

					var groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertNoWarning(groupedPackingLine.Weight.ValueInfo, warningMessageWeight);
					AssertNoMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageWeight);

					foreach (ForwardingShipment shipment in consol.Shipments)
					{
						foreach (ForwardingPackLine outerPackLines in shipment.OuterPackLines)
						{
							outerPackLines.JL_ActualWeight = 0;
						}
					}

					shippingOrder = builder.Build();
					groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageWeight);
					AssertHasMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageWeight);

					var shipment1 = consol.Shipments.FirstOrDefault() as ForwardingShipment;
					var packLine1 = shipment1.OuterPackLines.First() as ForwardingPackLine;
					packLine1.JL_ActualWeight = 2;

					var newPackline = shipment1.OuterPackLines.AddNew();
					newPackline.JL_PackageCount = 2;
					newPackline.JL_F3_NKPackType = packLine1.JL_F3_NKPackType;
					newPackline.JL_ActualWeight = 0;
					newPackline.JL_ActualWeightUQ = "KG";
					newPackline.JL_MarksAndNumbers = packLine1.JL_MarksAndNumbers;
					newPackline.JL_DetailedDescription = packLine1.JL_DetailedDescription;
					packLine1.GetContainer(consol).PackLines.Add(newPackline);

					builder = new ShippingOrderBuilder(consol);
					shippingOrder = builder.Build();
					groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertHasWarning(groupedPackingLine.Weight.ValueInfo, warningMessageWeight);
					AssertNoMessageError(groupedPackingLine.Weight.ValueInfo, errorMessageWeight);
				}
			}
		}

		public void TestConsolidatedPackingLineWeightValidation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "CNSHG";
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_IsEmptyContainer = false;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment.InnerPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var outerPackLine = PopulatePackLine(shipment.OuterPackLines.AddNew(), 1, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			outerPackLine.JL_JC = container.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var errorMessageEmptyWeight = @"No weight has been allocated to this container.
Please verify in Shipment > Packing > Weight.";

				consol.JK_AgentType = Constants.AgentType.CoLoad;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				var builder = new ShippingOrderBuilder(consol);
				var data = builder.Build();
				var groupedPackingLine = data.Shipments.First().PackingLines.First();
				var consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				outerPackLine.JL_ActualWeight = 1.5M;
				outerPackLine.JL_ActualVolume = 2.5M;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertNoMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				consol.JK_AgentType = Constants.AgentType.Direct;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
				outerPackLine.JL_ActualWeight = ZDecimal.Zero;
				outerPackLine.JL_ActualVolume = ZDecimal.Zero;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertHasMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);

				outerPackLine.JL_ActualWeight = 1.5M;
				outerPackLine.JL_ActualVolume = 2.5M;
				data = builder.Build();
				groupedPackingLine = data.Shipments.First().PackingLines.First();
				consolidatedPackingLine = groupedPackingLine.PackingLines.First();

				AssertNoMessageError(consolidatedPackingLine.Weight.ValueInfo, errorMessageEmptyWeight);
			}
		}

		public void TestPacksValidation_IsGroupAndConsolidatePackingLines_WhenSubPackLineHasZeroPackCount()
		{
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByShipment);
			AssertSubPackLineHasZeroPackCount(Constants.PackageGrouping.Codes.GroupByPackLine);

			void AssertSubPackLineHasZeroPackCount(ZString packageGrouping)
			{
				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var standardServiceLevel = Factory.LoadFromUniqueKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, new ZString("STD"));
					var rule = CreateRefCountryRules(Constants.CountryCodes.China, Constants.CountryCodes.Australia, Constants.TransportModes.Sea, standardServiceLevel.PK, true);

					var consol = CreateConsol();
					consol.JK_PackageGrouping = packageGrouping;

					var builder = new ShippingOrderBuilder(consol);
					var shippingOrder = builder.Build();

					var errorMessageQuantity = @"There are pack lines with zero (0) quantity.
Please verify in Shipment>Packing>Packs on following Shipments:
SH0001000.";
					var warningMessage = @"There are pack lines with no inner quantity, outer pack quantity has been taken as inners.
Please verify in Shipment>Packing>Packs on following Shipments:
SH0001000.";

					var groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertNoMessageError(groupedPackingLine.QuantityInfo, errorMessageQuantity);

					var firstShipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
					AssertNotNull(firstShipment);

					var firstPackLine = firstShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
					AssertNotNull(firstPackLine);

					firstPackLine.JL_PackageCount = 0;
					shippingOrder = builder.Build();
					groupedPackingLine = shippingOrder.Shipments.First().PackingLines.First();
					AssertHasMessageError(groupedPackingLine.QuantityInfo, errorMessageQuantity);
					AssertHasWarning(groupedPackingLine.QuantityInfo, warningMessage);

					consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;
					shippingOrder = builder.Build();
					var packingLineDO = shippingOrder.Shipments.First().PackingLines.First();
					AssertNoMessageError(packingLineDO.QuantityInfo, errorMessageQuantity);
					AssertHasWarning(packingLineDO.QuantityInfo, warningMessage);
				}
			}
		}

		RefCountryRules CreateRefCountryRules(ZString originCountry, ZString destinationCountry, ZString transportMode, ZGuid serviceLevelPK, ZBool isShowInner)
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();

			rule.R7_RN_NKOrigin = originCountry;
			rule.R7_RN_NKDestination = destinationCountry;
			rule.R7_TransportMode = transportMode;
			rule.R7_RS = serviceLevelPK;
			rule.R7_ShowInner = isShowInner;

			return rule;
		}

		public void TestPacksValidation_DoNotGroup_WhenPackLineHasZeroPackCount()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

				var builder = new ShippingOrderBuilder(consol);
				var shippingOrder = builder.Build();

				var errorMessageQuantity = "Pack quantity is mandatory.";

				var packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertNoMessageError(packingLine.QuantityInfo, errorMessageQuantity);

				var firstShipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
				AssertNotNull(firstShipment);

				var firstPackLine = firstShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
				AssertNotNull(firstPackLine);

				firstPackLine.JL_PackageCount = 0;
				shippingOrder = builder.Build();
				packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertHasMessageError(packingLine.QuantityInfo, errorMessageQuantity);
			}
		}

		public void TestPacksValidation_DoNotGroup_WhenPackLineHasZeroPackWeight()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

				var builder = new ShippingOrderBuilder(consol);
				var shippingOrder = builder.Build();

				var errorMessageWeight = "Total packing line weight is required. Please enter a value.";

				var packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertNoMessageError(packingLine.Weight.ValueInfo, errorMessageWeight);

				var firstShipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
				AssertNotNull(firstShipment);

				var firstPackLine = firstShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
				AssertNotNull(firstPackLine);

				firstPackLine.JL_ActualWeight = 0;
				shippingOrder = builder.Build();
				packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertHasMessageError(packingLine.Weight.ValueInfo, errorMessageWeight);
			}
		}

		public void TestPacksValidation_DoNotGroup_WhenPackLineHasZeroPackVolume()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

				var builder = new ShippingOrderBuilder(consol);
				var shippingOrder = builder.Build();

				var errorMessageVolume = "The total packing line volume is zero. Please enter a value.";

				var packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertNoMessageError(packingLine.Volume.ValueInfo, errorMessageVolume);

				var firstShipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
				AssertNotNull(firstShipment);

				var firstPackLine = firstShipment.OuterPackLines.FirstOrDefault() as ForwardingPackLine;
				AssertNotNull(firstPackLine);

				firstPackLine.JL_ActualVolume = 0;
				shippingOrder = builder.Build();
				packingLine = shippingOrder.Shipments.First().PackingLines.First();
				AssertHasMessageError(packingLine.Volume.ValueInfo, errorMessageVolume);
			}
		}

		#endregion

		#region Charges

		public void TestOtherChargesValidation()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var otherCharges = (OtherCharges)shippingOrder.OtherCharges;

			otherCharges.IsPrepaid = false;
			otherCharges.IsCollect = false;
			otherCharges.IsFree = false;
			otherCharges.IsPayableElsewhere = false;
			otherCharges.IsFirstLinePrepaidLineSecondCollect = false;

			const string paymentDetailsTypeErrorMessage = "At least one Payment Details type must be selected.";

			AssertHasMessageError(otherCharges.RemarksInfo, paymentDetailsTypeErrorMessage);
			AssertHasMessageError(otherCharges.IsPrepaidInfo, paymentDetailsTypeErrorMessage);
			AssertHasMessageError(otherCharges.IsFirstLinePrepaidLineSecondCollectInfo, paymentDetailsTypeErrorMessage);

			otherCharges.Remarks = "These are the remarks";

			AssertNoMessageError(otherCharges.RemarksInfo, paymentDetailsTypeErrorMessage);
			AssertNoMessageError(otherCharges.IsPrepaidInfo, paymentDetailsTypeErrorMessage);
			AssertNoMessageError(otherCharges.IsFirstLinePrepaidLineSecondCollectInfo, paymentDetailsTypeErrorMessage);

			otherCharges.IsFirstLinePrepaidLineSecondCollect = true;
			otherCharges.Remarks = ZString.Empty;

			AssertNoMessageError(otherCharges.RemarksInfo, paymentDetailsTypeErrorMessage);
			AssertNoMessageError(otherCharges.IsPrepaidInfo, paymentDetailsTypeErrorMessage);
			AssertNoMessageError(otherCharges.IsFirstLinePrepaidLineSecondCollectInfo, paymentDetailsTypeErrorMessage);
		}

		public void TestFreightPayableAtValidation()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var freightPayableAt = (Unloco)shippingOrder.FreightPayableAt;

			const string freightPayableAtErrorMessage = "The location of where freight is paid is required.";

			freightPayableAt.Code = ZString.Empty;

			AssertHasMessageError(freightPayableAt.CodeInfo, freightPayableAtErrorMessage);

			freightPayableAt.Code = "AUBNE";

			AssertNoMessageError(freightPayableAt.CodeInfo, freightPayableAtErrorMessage);
		}

		#endregion

		#region Populate SourceID

		public void TestPopulateSourceIdFromCarrierShipperReference()
		{
			var consol = CreateConsol();
			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals("C0000005-V8", shippingOrder.SourceID);
		}

		#endregion

		#region TestPopulateShipper

		public void TestShipper()
		{
			var consol = CreateConsol();
			var builder = new ShippingOrderBuilder(consol).Build();
			var sendingAgentAddress = @"I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222";

			Assert("Precondition", !consol.IsDirect);
			AssertEquals("Shipper without MasterBillShipperOverride", sendingAgentAddress, builder.Shipper.ToAssertString());

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MBSORG";
			org.OH_FullName = "MBS Organisation";
			org.MainAddress.Address1 = "MBS Address 1";
			org.MainAddress.Address2 = "MBS Address 2";
			consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = org.PK;

			builder = new ShippingOrderBuilder(consol).Build();
			var masterBillShipperOverrideAddress = @"MBS Organisation
MBS Address 1
MBS Address 2";

			AssertEquals("Shipper with MasterBillShipperOverride", masterBillShipperOverrideAddress, builder.Shipper.ToAssertString());
		}

		#endregion

		#region TestPopulateConsignee

		public void TestConsignee()
		{
			var consol = CreateConsol();
			var builder = new ShippingOrderBuilder(consol).Build();
			var receivingAgentAddress = @"I'm Receiving Stuff
Unit 399
50 What Lane
Receiver Name
name@receiver.com
3333333
4444444";

			Assert("Precondition", !consol.IsDirect);
			AssertEquals("Consignee without MasterBillConsigneeOverride", receivingAgentAddress, builder.Consignee.ToAssertString());

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MBCORG";
			org.OH_FullName = "MBC Organisation";
			org.MainAddress.Address1 = "MBC Address 1";
			org.MainAddress.Address2 = "MBC Address 2";
			consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = org.PK;

			builder = new ShippingOrderBuilder(consol).Build();
			var masterBillConsigneeOverrideAddress = @"MBC Organisation
MBC Address 1
MBC Address 2";

			AssertEquals("Consignee with MasterBillConsigneeOverride", masterBillConsigneeOverrideAddress, builder.Consignee.ToAssertString());
		}

		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			DocDataObjects.Testing.VerifiedGrossMassTest.InitConsolAndShipmentForShipperCompanyNameTest(Factory, out var consol, out _, out var forwarder, out var consignor);

			AssertEquals("Pre-Condition", false, consol.IsDirect);

			var builder = new ShippingOrderBuilder(consol);
			var shippingOrder = builder.Build();
			AssertEquals($"{forwarder.OH_FullName} {forwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} AAA Lines", shippingOrder.Shipper.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, consol.IsDirect);

			shippingOrder = builder.Build();
			AssertEquals($"{consignor.OH_FullName} {consignor.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} BBB Lines", shippingOrder.Shipper.CompanyName);
		}

		#endregion

		#region TestCarrierMessagingRequirementsValidation

		public void TestCarrierMessagingRequirementsValidation_HSC()
		{
			var errorMessage = "The carrier requires HS code for each pack line.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var creditorRefShippingLine = Factory.New<RefShippingLine>();
			creditor.OH_RSL_ShippingLine = creditorRefShippingLine.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = string.Empty;
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "HSC_CODE";
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PLT";

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = string.Empty;
			packLine3.JL_PackageCount = 1;
			packLine3.JL_F3_NKPackType = "PLT";

			var harmonisedCode1 = packLine3.HarmonisedCodes.AddNew();
			harmonisedCode1.JLH_Code = "HS1";
			harmonisedCode1.JLH_RN_NKCountry = "CN";

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var harmonizedCode1 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.First().HarmonizedCode;
			var harmonizedCode2 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.ElementAt(1).HarmonizedCode;
			var harmonizedCode3 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.ElementAt(2).HarmonizedCode;

			AssertNull(creditorRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode));
			AssertNoMessageError(harmonizedCode1.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode2.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode3.CodeInfo, errorMessage);

			var harmonisedCodeMessagingRequirment = creditorRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			harmonisedCodeMessagingRequirment.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.HarmonisedCode;
			harmonisedCodeMessagingRequirment.RSR_IsShippingOrder = true;

			Assert(creditorRefShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.HarmonisedCode).RSR_IsShippingOrder);

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			harmonizedCode1 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.First().HarmonizedCode;
			harmonizedCode2 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.ElementAt(1).HarmonizedCode;
			harmonizedCode3 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.ElementAt(2).HarmonizedCode;

			AssertHasMessageError(harmonizedCode1.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode2.CodeInfo, errorMessage);
			AssertNoMessageError(harmonizedCode3.CodeInfo, errorMessage);

			var harmonisedCode2 = packLine1.HarmonisedCodes.AddNew();
			harmonisedCode2.JLH_Code = "HS2";
			harmonisedCode2.JLH_RN_NKCountry = "MY";

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			harmonizedCode1 = (HarmonizedCode)shippingOrder.Containers.First().PackingLines.First().HarmonizedCode;

			AssertNoMessageError(harmonizedCode1.CodeInfo, errorMessage);
		}

		public void TestCarrierMessagingRequirementsValidation_IEL()
		{
			var errorMessage = "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Shipping Order.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			shippingLine.RSL_ShippingOrderAvailable = true;
			var carrierMessageData = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			shippingLine.RSL_ShippingOrderAvailable = false;
			carrierMessageData = new ShippingOrderBuilder(consol).Build();
			AssertEquals("TEST NAME", ((Address)carrierMessageData.Carrier).Contact);
			AssertNoMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);

			var shippingLineMessagingRequirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLineMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice;
			shippingLineMessagingRequirement.RSR_IsShippingOrder = true;

			contact.Documents.RemoveAll();

			shippingLine.RSL_ShippingOrderAvailable = true;
			carrierMessageData = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);

			shippingLine.RSL_ShippingOrderAvailable = false;
			carrierMessageData = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError(((Address)carrierMessageData.Carrier).ContactInfo, errorMessage);
		}

		#endregion

		#region TestCarrierLinkShippingLineValidation

		public void TestCarrierLinkShippingLineValidation()
		{
			var errorMessage = "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertHasMessageError((shippingOrder.Carrier as Address).CompanyNameInfo, errorMessage);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertNoMessageError((shippingOrder.Carrier as Address).CompanyNameInfo, errorMessage);
		}

		#endregion

		#region TestContextWithCarrierUnlocoMapping

		public void TestContextWithCarrierUnlocoMapping_Agent()
		{
			var consol = CreateConsol();
			var creditor = Factory.New<OrgHeader>();

			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "CNXXX";
			mapping1.OO_LocalCode = "CNSHA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "CNYYY";
			mapping2.OO_LocalCode = "CNSHA";

			Factory.Save();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(shippingOrder.PlaceOfReceipt.Unlocos);
			AssertEquals("Mapping should be from the carrier's foreign code", "CNXXX", shippingOrder.PlaceOfReceipt.Code);
		}

		public void TestContextWithCarrierUnlocoMapping_Coload()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "CNXXX";
			mapping1.OO_LocalCode = "CNSHA";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "CNYYY";
			mapping2.OO_LocalCode = "CNSHA";

			Factory.Save();

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(shippingOrder.PlaceOfReceipt.Unlocos);
			AssertEquals("Mapping should be from the co-load with's foreign code", "CNYYY", shippingOrder.PlaceOfReceipt.Code);
		}

		#endregion

		public void TestTransportsValidation()
		{
			var errorMessage1 = "Main transport leg mode must be SEA.";
			var errorMessage2 = "Main Sea leg is required.";

			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			var mainTransport = shippingOrder.Transports.Main;

			AssertNoMessageError(((CodeDescription)mainTransport.Mode).CodeInfo, errorMessage1);
			foreach (DocDataObjects.Transport transport in shippingOrder.Transports)
			{
				AssertNoMessageError(((CodeDescription)transport.Type)?.DescriptionInfo, errorMessage2);
			}

			var mainTransportRaw = consol.Transports.Cast<Freight.Business.Transport>().First(t => t.JW_TransportType == Constants.TransportPlanningType.MainVessel);
			mainTransportRaw.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			mainTransportRaw.JW_TransportMode = Constants.TransportModes.Road;
			shippingOrder = new ShippingOrderBuilder(consol).Build();

			mainTransport = shippingOrder.Transports.Main;
			AssertHasMessageError(((CodeDescription)mainTransport.Mode).CodeInfo, errorMessage1);
			foreach (DocDataObjects.Transport transport in shippingOrder.Transports)
			{
				AssertNoMessageError(((CodeDescription)transport.Type)?.DescriptionInfo, errorMessage2);
			}

			mainTransportRaw.JW_TransportType = Constants.TransportPlanningType.Other;
			shippingOrder = new ShippingOrderBuilder(consol).Build();
			foreach (DocDataObjects.Transport transport in shippingOrder.Transports)
			{
				AssertHasMessageError("Main transport leg SEA mandatory error", ((CodeDescription)transport.Type)?.DescriptionInfo, errorMessage2);
			}
		}

		public void TestNVOCCReferenceOfCurrentBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("CHCOMP", true, true, "CNNJI");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			var testCompany = testObjectCreator.CreateNewCompany("CN", "CN", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("NAN", "NanjingBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.NVOCCReference, "NVOCCxxx", Constants.CountryCodes.China);
			Factory.Save();

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol();
				var shippingOrderBuilder = new ShippingOrderBuilder(consol);
				var shippingOrder = shippingOrderBuilder.Build();
				AssertEquals("NVOCC Reference Value", "NVOCCxxx", shippingOrder.NVOCCReference.Value);
				AssertEquals("NVOCC Reference Type", OrgCusCode.CodeTypes.NVOCCReference, shippingOrder.NVOCCReference.Type.Code);
				AssertEquals("NVOCC Reference Country", Constants.CountryCodes.China, shippingOrder.NVOCCReference.CountryOfIssue.Code);
			}
		}

		public void TestNVOCCReferenceOfCurrentCompany()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("CHCOMP", true, true, "CNNJI");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.China;
			var testCompany = testObjectCreator.CreateNewCompany("CN", "CN", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("NAN", "NanjingBranche", testCompany, testOrgProxy);

			testCompany.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testCompany.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.NVOCCReference, "NVOCCxxx", Constants.CountryCodes.China);
			Factory.Save();
			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol();
				var shippingOrderBuilder = new ShippingOrderBuilder(consol);
				var shippingOrder = shippingOrderBuilder.Build();
				AssertEquals("NVOCC Reference Value", "NVOCCxxx", shippingOrder.NVOCCReference.Value);
				AssertEquals("NVOCC Reference Type", OrgCusCode.CodeTypes.NVOCCReference, shippingOrder.NVOCCReference.Type.Code);
				AssertEquals("NVOCC Reference Country", Constants.CountryCodes.China, shippingOrder.NVOCCReference.CountryOfIssue.Code);
			}
		}

		public void TestPopulateIsRequiredSendAttachment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "OrgHeader";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "UNIT05";
			orgHeader.MainAddress.Address2 = "Haha Street";
			orgHeader.MainAddress.City = "AUCKLAND";
			orgHeader.MainAddress.Postcode = "1050";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "NZ";
			orgHeader.MainAddress.OA_Email = "Flah@Floogle.com";

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = orgHeader.MainAddress.PK;

			var refShippingLine = Factory.New<RefShippingLine>();
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);

			var messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsShippingOrder = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingOrder);
			Assert(new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsShippingOrder = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingOrder);
			Assert(!new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			refShippingLine.ShippingLineMessagingRequirements.DeleteAll();

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsShippingOrder = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingOrder);
			Assert(new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsShippingOrder = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsShippingOrder);
			Assert(!new ShippingOrderBuilder(consol).Build().IsRequiredSendAttachment);
		}

		public void TestPacksInContainers_DNG()
		{
			var consol = CreateConsolForGrouping(Constants.PackageGrouping.Codes.DoNotGroup);
			var shippingOrder = new ShippingOrderBuilder(consol);
			var data = shippingOrder.Build();

			var containerDO1 = data.Containers.Cast<Container>().First(x => x.Number == "CONTAINER1");
			var containerDO2 = data.Containers.Cast<Container>().First(x => x.Number == "CONTAINER2");

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(2, containerDO1.PackingLines.Count);
				AssertEquals(60, containerDO1.PackCount);
				AssertEquals(3, containerDO2.PackingLines.Count);
				AssertEquals(130, containerDO2.PackCount);
			}
		}

		public void TestPacksInContainers_Grouping()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsolForGrouping(Constants.PackageGrouping.Codes.GroupByShipment);
				var shippingOrder = new ShippingOrderBuilder(consol);
				var data = shippingOrder.Build();

				var shipmentDO1 = data.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = data.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				AssertEquals(1, shipmentDO1.PackingLines.Count);
				AssertEquals(1, shipmentDO2.PackingLines.Count);
			}
		}

		public void TestUnlocoIsSetToNotLongerModifiable()
		{
			var consol = CreateConsol();
			var shippingOrder = new ShippingOrderBuilder(consol).Build();

			Assert("Port of Loading should be read-only for UNLOCO", shippingOrder.PortOfLoading.Code_DisableModifiable);
			Assert("Port of Discharge should be read-only for UNLOCO", shippingOrder.PortOfDischarge.Code_DisableModifiable);

			Assert("Place of Receipt should be read-only for UNLOCO", shippingOrder.PlaceOfReceipt.Code_DisableModifiable);
			Assert("Place of Issue should be read-only for UNLOCO", shippingOrder.PlaceOfIssue.Code_DisableModifiable);
			Assert("Place of Delivery should be read-only for UNLOCO", shippingOrder.PlaceOfDelivery.Code_DisableModifiable);
		}

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var shippingOrder = new ShippingOrderBuilder(consol).Build();
			AssertEquals(Constants.ContainerModes.FCL, shippingOrder.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, shippingOrder.ContainerMode.Description);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);
			consol.JK_CarrierContractNumber = "11111";

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "BLOOP";
			unpackDepotAddress.OH_RL_NKClosestPort = "USJFK";
			unpackDepotAddress.MainAddress.Address1 = "199 Crab Road";
			unpackDepotAddress.MainAddress.Address2 = "Crabby";
			unpackDepotAddress.MainAddress.City = "New York";
			unpackDepotAddress.MainAddress.Postcode = "10005";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "US";
			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var contractNamedAccount = consol.Numbers.AddNew();
			contractNamedAccount.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount;
			contractNamedAccount.CE_EntryNum = "Contract Named Account";

			var letterOfCredit = consol.Numbers.AddNew();
			letterOfCredit.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.LetterOfCreditNumber;
			letterOfCredit.CE_EntryNum = "22222";

			var sldNumber = consol.Numbers.AddNew();
			sldNumber.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			sldNumber.CE_EntryNum = "12345";

			var note1 = consol.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description;
			note1.ST_NoteText = "forwarding instructions";

			var note2 = consol.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note2.ST_NoteText = "goods handling instructions";

			var note3 = consol.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note3.ST_NoteText = "special instructions";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";

			var transport4 = consol.Transports.AddNew();
			transport4.JW_LegOrder = 4;
			transport4.JW_TransportMode = Constants.TransportModes.Rail;
			transport4.JW_TransportType = Constants.TransportPlanningType.Other;
			transport4.JW_RL_NKLoadPort = "AUSYD";
			transport4.JW_RL_NKDiscPort = "AUMEL";
			transport4.JW_Vessel = "Tommy";
			transport4.JW_VoyageFlight = "444";

			PopulateAddresses(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CY";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 200;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "ABCDE";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var undg = packline1.UNDGs.AddNew();
			undg.LinkDefault(subs);
			undg.Substance.DG_PSN = "DG SHIPPER NAME";
			undg.DI_TechnicalName = "WHATEVER";
			undg.DI_IMOClass = "CLAS";
			undg.Substance.DG_PG = "GRO";
			undg.Substance.DG_SubLabel1 = "sub1";
			undg.Substance.DG_SubLabel2 = "su2";
			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 15.0m;
			undg.DI_MPMarinePollutant = "N";
			undg.DI_DGVolume = 2m;
			undg.DI_UnitOfVolume = "M3";
			undg.DI_DGWeight = 200m;
			undg.DI_UnitOfWeight = "KG";
			undg.DI_IsLimitedQuantity = true;
			undg.DI_PackageCount = 5;
			undg.DI_F3_NKPackType = "BAG";
			undg.DI_OC_DGContact = contact.PK;

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "CN";
			hc.JLH_Code = "1234";

			packline1.HarmonisedCodes.Add(hc);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ExportRefNumber = "BBB";
			packline2.JL_PackageCount = 5;
			packline2.JL_ExportRefNumber = "REF001";
			packline2.JL_DetailedDescription = "pack2";
			packline2.JL_ActualVolume = 100;
			packline2.JL_ActualWeight = 200;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 20000;
			packline3.JL_ActualWeightUQ = "G";
			packline3.JL_ActualVolume = 300;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_HarmonisedCode = "EEEEE";
			packline3.JL_ExportRefNumber = "REF002";
			packline3.JL_DetailedDescription = "pack3";

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);
			container.PackLines.Add(packline3);

			return consol;
		}

		OrgHeader CreateOrgHeader(ZString orgCode, ZString orgName)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = orgName;
			orgHeader.OH_RL_NKClosestPort = "DKAAL";
			orgHeader.MainAddress.Address1 = "Unit 13";
			orgHeader.MainAddress.Address2 = "4 Lost Lane";
			orgHeader.MainAddress.City = "Aalborg";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "DK";

			return orgHeader;
		}

		void PopulateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";

			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingForwarder.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";

			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var handlingAgent = Factory.New<OrgHeader>();
			handlingAgent.OH_FullName = "I'm Handling Stuff";
			handlingAgent.OH_RL_NKClosestPort = "AUSYD";
			handlingAgent.MainAddress.Address1 = "Unit 2";
			handlingAgent.MainAddress.Address2 = "60 What Lane";
			handlingAgent.MainAddress.City = "Sydney";
			handlingAgent.MainAddress.Postcode = "2023";
			handlingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Booking Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying About Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";
		}

		ForwardingConsol CreateConsolForGrouping(string type)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_PackageGrouping = type;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment2.JS_UnitOfVolume = Constants.Volume.MegaLitre;

			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Package, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre);
			var packLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Package, 50, Constants.Weight.Grams, 50, Constants.Volume.MegaLitre);
			var packLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Pallet, 60, Constants.Weight.Grams, 60, Constants.Volume.MegaLitre);
			var packLine4 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 50, Constants.PkgUnit.Pallet, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres);
			var packLine5 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 100, Constants.PkgUnit.Pallet, 110, Constants.Weight.Kilograms, 110, Constants.Volume.CubicMetres);

			container1.PackLines.Add(packLine1);
			container1.PackLines.Add(packLine4);
			container2.PackLines.Add(packLine2);
			container2.PackLines.Add(packLine3);
			container2.PackLines.Add(packLine5);

			return consol;
		}
		PackLine PopulatePackLine(PackLine packLine, ZInt packageCount, ZString packType, ZDecimal weight, ZString unitOfWeight, ZDecimal volume, ZString unitOfVolume, string marksAndNumbers = null, string detailedDescription = null,
			bool requiresTemperatureControl = false, int requiredTemperatureMinimum = -10, int requiredTemperatureMaximum = 10, string requiredTemperatureUnit = Constants.Temperature.Centigrade)
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = unitOfWeight;
			packLine.JL_ActualVolume = volume;
			packLine.JL_ActualVolumeUQ = unitOfVolume;
			packLine.JL_MarksAndNumbers = marksAndNumbers ?? ZString.Empty;
			packLine.JL_DetailedDescription = detailedDescription ?? ZString.Empty;
			packLine.JL_RequiredTemperatureMinimum = requiredTemperatureMinimum;
			packLine.JL_RequiredTemperatureMaximum = requiredTemperatureMaximum;
			packLine.JL_RequiredTemperatureUnit = requiredTemperatureUnit;
			packLine.JL_RequiresTemperatureControl = requiresTemperatureControl;

			return packLine;
		}

		#endregion
	}
}

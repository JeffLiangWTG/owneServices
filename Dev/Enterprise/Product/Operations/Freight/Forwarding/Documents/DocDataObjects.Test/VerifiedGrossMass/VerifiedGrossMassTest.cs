using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(VerifiedGrossMass))]
	sealed class VerifiedGrossMassTest : NonPersistentBusinessObjectTestCase
	{
		#region TestContainerIsEmpty

		public void TestContainerIsEmpty()
		{
			var consol = CreateConsol(Constants.AgentType.Direct);
			var vgm = CreateVGM(consol);

			var forwardingContainers = consol.Containers.OfType<ForwardingContainer>().ToArray();
			var vgmContainers = vgm.Containers.ToArray();

			AssertEquals(forwardingContainers.Length, vgmContainers.Length);

			for (var i = 0; i < forwardingContainers.Length; i++)
			{
				var forwardingContainerIsEmptyContainer = forwardingContainers[i].JC_IsEmptyContainer;
				var vgmContainerIsEmpty = vgmContainers[i].IsEmpty;

				AssertEquals(forwardingContainerIsEmptyContainer, vgmContainerIsEmpty);
			}
		}

		#endregion

		#region TestAddressPopulation_DirectConsol

		public void TestAddressPopulation_DirectConsol()
		{
			var consol = CreateConsol(Constants.AgentType.Direct);
			var shipment = consol.Shipments.OfType<ForwardingShipment>().First();
			var vgm = CreateVGM(consol);

			AssertEquals("FCL", vgm.ContainerMode.Code);
			AssertEquals("Full Container Load", vgm.ContainerMode.Description);

			AssertAddressData(shipment.ConsignorDocumentaryAddress, vgm.Shipper);
			AssertAddressData(shipment.ConsigneeDocumentaryAddress, vgm.Consignee);
			AssertAddressData(consol.ShippingLine, vgm.Carrier);
			AssertAddressData(consol.SendingForwarder, vgm.FreightForwarder);
			AssertAddressData(consol.CarrierHandlingAgent, vgm.CarrierHandlingAgent);
			AssertAddressData(consol.CarrierBookingAgent, vgm.CarrierBookingAgent);
		}

		#endregion

		#region TestPopulateChinaSpecificInfo

		public void TestPopulateChinaSpecificInfo_Sea() => TestPopulateChinaSpecificInfo(Constants.TransportModes.Sea);
		public void TestPopulateChinaSpecificInfo_InlandWaterway() => TestPopulateChinaSpecificInfo(Constants.TransportModes.InlandWaterwayTransport);

		void TestPopulateChinaSpecificInfo(string seaTransportMode)
		{
			var consol = CreateConsol(loadUnloco: "CNNBO", dischUnloco: "SGSIN", seaTransportMode: seaTransportMode);
			var vgm = CreateVGM(consol);

			AssertEquals("TAIKO", vgm.FirstSeaLegForChina.Vessel.Name);
			AssertEquals("8204975", vgm.FirstSeaLegForChina.Vessel.LloydsIMO);
			AssertEquals("123", vgm.FirstSeaLegForChina.VoyageFlightNumber);
			AssertEquals("CNNBO", vgm.OperationalPort.Code);
			AssertEquals("Ningbo", vgm.OperationalPort.Name);
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

			var vgm = CreateVGM(consol);
			vgm.Carrier.ValidateAll();
			AssertHasMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);

			var carrierRefShippingLine = Factory.New<RefShippingLine>();
			carrier.OH_RSL_ShippingLine = carrierRefShippingLine.PK;
			vgm = CreateVGM(consol);
			vgm.Carrier.ValidateAll();
			AssertNoMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);
		}

		public void TestCarrierLinkShippingLineValidation_IsCoLoad()
		{
			var errorMessage = "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var creditor = Factory.New<OrgHeader>();
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var vgm = CreateVGM(consol);
			vgm.Carrier.ValidateAll();
			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			var creditorRefShippingLine = Factory.New<RefShippingLine>();
			creditor.OH_RSL_ShippingLine = creditorRefShippingLine.PK;
			vgm = CreateVGM(consol);
			vgm.Carrier.ValidateAll();
			AssertNoMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);
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
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			var vgm = CreateVGM(consol);
			AssertEquals("test@test.com", vgm.Carrier.Email);
			AssertEquals("TEST NAME", vgm.Carrier.Contact);

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			vgm = CreateVGM(consol);
			AssertEquals("", vgm.Carrier.Email);
			AssertEquals("", vgm.Carrier.Contact);
		}

		public void TestDefaultContactAndEmail_IsCoload()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var contact = creditor.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";

			var vgm = CreateVGM(consol);
			AssertEquals("test@test.com", vgm.FreightForwarder.Email);
			AssertEquals("TEST NAME", vgm.FreightForwarder.Contact);

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			vgm = CreateVGM(consol);
			AssertEquals("", vgm.Carrier.Email);
			AssertEquals("", vgm.Carrier.Contact);
		}

		#endregion

		#region TestAddressPopulation_ColoadConsol

		public void TestAddressPopulation_ColoadConsol()
		{
			var consol = CreateConsol(Constants.AgentType.CoLoad);
			var shipment = consol.Shipments.OfType<ForwardingShipment>().First();
			var vgm = CreateVGM(consol);

			AssertAddressData(consol.SendingForwarder, vgm.Shipper);
			AssertAddressData(consol.ReceivingForwarder, vgm.Consignee);
			AssertAddressData(consol.ShippingLine, vgm.Carrier);
			AssertAddressData(consol.Creditor, vgm.FreightForwarder);
		}

		#endregion

		#region TestValidation

		#region TestAddressValidation

		public void TestAddressValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var vgm = new VGMBuilder(consol, null).Build();

			var shipper = vgm.Shipper;
			var freightForwarder = vgm.FreightForwarder;
			var carrier = vgm.Carrier;

			AssertHasMessageError(shipper.AddressFormattedInfo, "Shipper party name and address information is required.");
			AssertHasMessageError(freightForwarder.AddressFormattedInfo, "Freight Forwarder party name and address information is required.");
			AssertHasMessageError(carrier.AddressFormattedInfo, string.Join(System.Environment.NewLine, "Carrier party name and address information is required.", "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C)."));

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

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Carrier";
			shippingLine.OH_RL_NKClosestPort = "NZAKL";
			shippingLine.MainAddress.Address1 = "UNIT03";
			shippingLine.MainAddress.Address2 = "Something Street";
			shippingLine.MainAddress.City = "AUCKLAND";
			shippingLine.MainAddress.Postcode = "1050";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingLine.MainAddress.OA_Email = "Out@ofideas.com";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			consol.JK_AgentType = Constants.AgentType.Agent;
			vgm = new VGMBuilder(consol, null).Build();

			freightForwarder = vgm.FreightForwarder;
			carrier = vgm.Carrier;

			AssertHasMessageError(carrier.AddressFormattedInfo, "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");
			AssertNoMessageError(freightForwarder.AddressFormattedInfo, "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");

			consol.JK_AgentType = Constants.AgentType.CoLoad;

			creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "UNIT05";
			creditor.MainAddress.Address2 = "Haha Street";
			creditor.MainAddress.City = "AUCKLAND";
			creditor.MainAddress.Postcode = "1050";
			creditor.MainAddress.OA_RN_NKCountryCode = "NZ";
			creditor.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Carrier";
			shippingLine.OH_RL_NKClosestPort = "NZAKL";
			shippingLine.MainAddress.Address1 = "UNIT03";
			shippingLine.MainAddress.Address2 = "Something Street";
			shippingLine.MainAddress.City = "AUCKLAND";
			shippingLine.MainAddress.Postcode = "1050";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingLine.MainAddress.OA_Email = "Out@ofideas.com";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			vgm = new VGMBuilder(consol, null).Build();

			freightForwarder = vgm.FreightForwarder;
			carrier = vgm.Carrier;

			AssertNoMessageError(carrier.AddressFormattedInfo, "This Carrier Organisation is not linked to a Shipping Line record. Please link this Carrier to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");
			AssertHasMessageError(freightForwarder.AddressFormattedInfo, "This Co-Load With Organisation is not linked to a Shipping Line record. Please link this Co-Loader to Shipping Line record on Organization > Carrier > Shipping Line/NVOCC/Agent > Shipping Line.");
		}

		public void TestAddressValidation_CreditorAddress()
		{
			var consol = CreateConsol(Constants.AgentType.CoLoad);

			var errorMessage = "Freight Forwarder SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C).";

			var vgm = CreateVGM(consol);

			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			var cargoWiseOneCarrierCode = consol.CreditorAddress.Header.CustomsCodes.AddNew();
			cargoWiseOneCarrierCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			cargoWiseOneCarrierCode.OK_CustomsRegNo = "ABCD";

			vgm = CreateVGM(consol);

			AssertNoMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			consol.CreditorAddress.Header.CustomsCodes.RemoveAndDeleteAll();

			vgm = CreateVGM(consol);

			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			var usCarrierCode = consol.CreditorAddress.Header.CustomsCodes.AddNew();
			usCarrierCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			usCarrierCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			usCarrierCode.OK_CustomsRegNo = "BDEF";

			vgm = CreateVGM(consol);

			AssertNoMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);
		}

		public void TestDefaultContactAndEmailValidation()
		{
			var errorMessage = "This carrier does not support electronic Verified Gross Container Weight. Contact name and email address are required to send your Verified Gross Container Weight by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group VGM";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "";

			var vgm = CreateVGM(consol);
			AssertEquals("", vgm.Carrier.Email);
			AssertEquals("", vgm.Carrier.Contact);
			AssertHasMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);

			vgm.Carrier.Contact = string.Empty;
			vgm.Carrier.Email = "email";
			AssertHasMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);

			vgm.Carrier.Contact = "ABC";
			vgm.Carrier.Email = string.Empty;
			AssertHasMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);

			vgm.Carrier.Email = "email";
			AssertNoMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			vgm = CreateVGM(consol);
			AssertNoMessageError(vgm.Carrier.AddressFormattedInfo, errorMessage);
		}

		public void TestDefaultContactAndEmailValidation_IsCoload()
		{
			var errorMessage = "This NVOCC does not support electronic Verified Gross Container Weight. Contact name and email address are required to send your Verified Gross Container Weight by email.\nPlease setup contact name and email address on Organization> Contact> Email and Receiving Documents> Group VGM";

			var consol = CreateConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var contact = creditor.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "";

			var vgm = CreateVGM(consol);
			AssertEquals("", vgm.FreightForwarder.Email);
			AssertEquals("", vgm.FreightForwarder.Contact);
			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			vgm.FreightForwarder.Contact = string.Empty;
			vgm.FreightForwarder.Email = "email";
			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			vgm.FreightForwarder.Contact = "ABC";
			vgm.FreightForwarder.Email = string.Empty;
			AssertHasMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);

			vgm.FreightForwarder.Email = "email";
			AssertNoMessageError(vgm.FreightForwarder.AddressFormattedInfo, errorMessage);
		}

		#endregion

		#region TestVGMContainerValidation

		public void TestVGMContainerValidation_AtLeastOneContainer()
		{
			var consol = CreateConsol();
			var vgm = CreateVGM(consol, null);

			AssertHasMessageError(vgm.TitleInfo, "You need at least one non empty container selected in order to send this document.");

			vgm = CreateVGM(consol);

			AssertNoMessageError(vgm.TitleInfo, "You need at least one non empty container selected in order to send this document.");
		}

		public void TestAddCurrentUserValidationForVerifiedGrossMass()
		{
			var saveCurrentUserFullName = GlbStaff.CurrentUser.GS_FullName;
			try
			{
				GlbStaff.CurrentUser.GS_FullName = "ValidName";
				var consol = Factory.New<ForwardingConsol>();
				var vgm = new VGMBuilder(consol, null).Build();
				AssertNoMessageErrors(vgm.ErrorPlaceHolderInfo);

				GlbStaff.CurrentUser.GS_FullName = "Radosavljević";
				var invalidVgmBuilder = new VGMBuilder(consol, null);
				var invalidVgm = invalidVgmBuilder.Build();
				AssertHasMessageError(invalidVgm.ErrorPlaceHolderInfo, "Your username contains invalid characters. Please use basic ASCII characters only.");
			}
			finally
			{
				GlbStaff.CurrentUser.GS_FullName = saveCurrentUserFullName;
			}
		}

		public void TestContainerValidation_ContainerNumberIsValid()
		{
			const string invalidISONumberErrorMessage = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";

			var consol = CreateConsol();
			consol.Containers.RemoveAll();

			var newContainer = consol.Containers.AddNew();
			newContainer.JC_ContainerNum = "X";
			newContainer.JC_DeliveryMode = "CFS/CY";
			newContainer.JC_IsShipperOwned = false;
			newContainer.JC_GrossWeightUQ = "KG";
			newContainer.JC_TareWeight = 1000;
			newContainer.JC_DunnageWeight = 1000;
			newContainer.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			newContainer.JC_IsEmptyContainer = false;

			var packline = consol.Shipments.Cast<CommonShipment>().First().OuterPackLines.AddNew();
			newContainer.PackLines.Add(packline);

			var vgm = CreateVGM(consol);
			var container = vgm.Containers.First();

			AssertHasMessageError(container.NumberInfo, invalidISONumberErrorMessage);

			newContainer.JC_IsShipperOwned = true;
			vgm = CreateVGM(consol);
			container = vgm.Containers.First();

			AssertHasWarning(container.NumberInfo, invalidISONumberErrorMessage);

			newContainer.JC_ContainerNum = "AAAA0000007";
			vgm = CreateVGM(consol);
			container = vgm.Containers.First();
			AssertNoMessageError(container.NumberInfo, invalidISONumberErrorMessage);
			AssertNoWarning(container.NumberInfo, invalidISONumberErrorMessage);
		}

		#endregion

		#region TestC1CCodeValidation

		public void TestC1CCodeValidation_CarrierHandlingAgent()
		{
			var consol = CreateConsol(loadUnloco: "CNNGB");
			var transport = consol.Transports.Cast<Freight.Business.Transport>().FirstOrDefault();
			transport.JW_RL_NKLoadPort = "CNCAN";
			Factory.Save();

			var vgm = CreateVGM(consol);

			AssertHasMessageError(vgm.CarrierHandlingAgent.AddressFormattedInfo, "C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");

			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			code.OK_CustomsRegNo = "123";
			consol.CarrierHandlingAgentDocumentaryAddress.Organisation.CustomsCodes.Add(code);

			vgm = CreateVGM(consol);

			AssertNoMessageError(vgm.CarrierHandlingAgent.AddressFormattedInfo, "C1C Code is missing from Carrier Handling Agent record under Organization > Config > Type C1C.");
		}

		public void TestC1CCodeValidation_CarrierBookingAgent()
		{
			var consol = CreateConsol(loadUnloco: "CNNGB");
			var transport = consol.Transports.Cast<Freight.Business.Transport>().FirstOrDefault();
			transport.JW_RL_NKLoadPort = "CNCAN";
			Factory.Save();

			var vgm = CreateVGM(consol);

			AssertHasMessageError(vgm.CarrierBookingAgent.AddressFormattedInfo, "C1C Code is missing from Carrier Booking Agent record under Organization > Config > Type C1C.");

			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			code.OK_CustomsRegNo = "123";
			consol.CarrierBookingAgentDocumentaryAddress.Organisation.CustomsCodes.Add(code);

			vgm = CreateVGM(consol);

			AssertNoMessageError(vgm.CarrierBookingAgent.AddressFormattedInfo, "C1C Code is missing from Carrier Booking Agent record under Organization > Config > Type C1C.");
		}

		#endregion

		#region TestContainerISOCodeValidation

		public void TestContainerISOCodeValidation()
		{
			var consol = CreateConsol(loadUnloco: "CNNGB");
			var vgm = CreateVGM(consol);
			var containerType = (ContainerType)vgm.Containers.FirstOrDefault(c => c.Number == "CONTAINER4").Type;
			AssertHasMessageError(containerType.ISOCodeInfo, "The container ISO code is required.");

			containerType.ISOCode = "ISO001";
			AssertNoMessageError(containerType.ISOCodeInfo, "The container ISO code is required.");
		}

		#endregion

		#region TestCarrierBookingReferenceBillOfLadingNumberValidation

		public void TestCarrierBookingReferenceBillOfLadingNumberValidation()
		{
			var consol = CreateConsol(loadUnloco: "CNNGB");
			consol.JK_MasterBillNum = "";
			consol.JK_BookingReference = "";

			var vgm = CreateVGM(consol);

			AssertHasMessageError(vgm.CarrierBookingReferenceInfo, "Carrier Booking Reference is required.");
		}

		#endregion

		#region TestNoEmptyContainerValidation

		public void TestNoEmptyContainerValidation()
		{
			var errorMsg = "Container must have at least one non-empty Packline.";
			var consol = CreateConsol();
			var vgm = CreateVGM(consol);
			var containerData = vgm.Containers.First(c => c.Number == "CONTAINER1");
			AssertNoMessageError(containerData.NumberInfo, errorMsg);

			var container = consol.Containers
				.Cast<ForwardingContainer>()
				.First(c => c.JC_ContainerNum == "CONTAINER1");

			container.PackLines.RemoveAll();

			vgm = CreateVGM(consol);
			containerData = vgm.Containers.First(c => c.Number == "CONTAINER1");
			AssertHasMessageError("error since no packlines exist", containerData.NumberInfo, errorMsg);

			var shipment = consol.Shipments[0];
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 0;
			container.PackLines.Add(packLine);
			container.JC_IsEmptyContainer = true;

			vgm = CreateVGM(consol);
			containerData = vgm.Containers.First(c => c.Number == "CONTAINER1");
			AssertHasMessageError("error since packline exists, but is empty", containerData.NumberInfo, errorMsg);
		}

		#endregion

		#region TestUnsupportedMethodValidation

		public void TestUnsupportedMethodValidation()
		{
			var consol = CreateConsol();
			var vgm = CreateVGM(consol);

			var container = vgm.Containers.First();
			AssertVGMUnsupportedMethod(container, Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired);
			AssertVGMUnsupportedMethod(container, Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod);
			AssertVGMUnsupportedMethod(container, Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal);
		}

		void AssertVGMUnsupportedMethod(VGMMessagingContainer container, string methodCode)
		{
			container.VerifiedMethod.Code = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertNoMessageErrors(container.VerifiedMethod.CodeInfo);

			container.VerifiedMethod.Code = methodCode;
			AssertHasMessageError(container.VerifiedMethod.CodeInfo, "This Verified Method cannot be sent electronically.");
		}

		public void TestNoVerifiedMethod()
		{
			var consol = CreateConsol();
			var vgm = CreateVGM(consol);

			var container = vgm.Containers.First();
			AssertNoMessageErrors(container.VerifiedMethod.CodeInfo);

			container.VerifiedMethod.Code = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertHasMessageError(container.VerifiedMethod.CodeInfo, "Container CONTAINER1 requires a Verified Method to be entered.");
		}

		#endregion

		#region TestVesselLloydsIMOValidation

		public void TestVesselLloydsIMOValidation_CNNBO()
		{
			var consol = CreateConsol(loadUnloco: "CNNBO");
			var vgm = CreateVGM(consol);
			var firstSeaLegForChina = vgm.FirstSeaLegForChina;

			AssertNotNull("Pre-condition: first sea leg is from China", firstSeaLegForChina);
			Assert("Pre-condition: Consol load port is Ningbo.", consol.LoadPort.Code.IsNingboPort());
			AssertEquals("8204975", firstSeaLegForChina.Vessel.LloydsIMO);
			AssertNoMessageError(firstSeaLegForChina.Vessel.LloydsIMOInfo, "Vessel's Lloyds/IMO is mandatory.");

			firstSeaLegForChina.Vessel.LloydsIMO = "";
			AssertHasMessageError(firstSeaLegForChina.Vessel.LloydsIMOInfo, "Vessel's Lloyds/IMO is mandatory.");
		}

		public void TestVesselLloydsIMOValidation_NotCNNBO()
		{
			var consol = CreateConsol(loadUnloco: "CNSHA");
			var vgm = CreateVGM(consol);
			var firstSeaLegForChina = vgm.FirstSeaLegForChina;

			AssertNotNull("Pre-condition: first sea leg is from China", firstSeaLegForChina);
			Assert("Pre-condition: Consol load port is NOT Ningbo.", !consol.LoadPort.Code.IsNingboPort());
			AssertEquals("8204975", firstSeaLegForChina.Vessel.LloydsIMO);
			AssertNoMessageErrors(firstSeaLegForChina.Vessel.LloydsIMOInfo);

			firstSeaLegForChina.Vessel.LloydsIMO = "";
			AssertNoMessageErrors(firstSeaLegForChina.Vessel.LloydsIMOInfo);
		}

		#endregion

		#region TestAddVerifiedByContactNameValidationForVerifiedGrossMass

		public void TestAddVerifiedByContactNameValidationForVerifiedGrossMass()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var container = consol.Containers.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packline);
			var vgm = CreateVGM(consol);

			AssertBasicAsciiCharactersValidation("Containers[0].VerifiedByAddress.Contact",
				vgm.Containers.Single().VerifiedByAddress.ContactInfo,
				"Your Contact Name contains invalid characters. Please use basic ASCII characters only.");
		}

		#endregion

		#region TestAddAsciiCharactersValidation

		public void TestAddAsciiCharactersValidation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";
			var container = consol.Containers.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packline);
			var vgm = CreateVGM(consol);

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("Shipper", vgm.Shipper);
				AssertAsciiCharactersValidation("Carrier", vgm.Carrier);
				AssertAsciiCharactersValidation("Consignee", vgm.Consignee);
				AssertAsciiCharactersValidation("FreightForwarder", vgm.FreightForwarder);
				AssertAsciiCharactersValidation("CarrierHandlingAgent", vgm.CarrierHandlingAgent);
				AssertAsciiCharactersValidation("CarrierBookingAgent", vgm.CarrierBookingAgent);
				AssertAsciiCharactersValidation("CurrentUser", vgm.CurrentUser);
				AssertAsciiCharactersValidation("Containers[0].VerifiedByAddress", vgm.Containers.Single().VerifiedByAddress);
				AssertAsciiCharactersValidation("ExportLegFromChina.VoyageFlightNumber", vgm.FirstSeaLegForChina.VoyageFlightNumberInfo);
			});
		}

		#endregion

		#region TestContainerVerifiedGrossWeightValidation

		public void TestContainerVerifiedGrossWeightValidation_ShouldNotBeLessTareWeight()
		{
			var errorMessage = "Verified Gross Weight should not be less than Container tare weight.";

			var consol = CreateConsol(loadUnloco: "CNNGB");
			var vgm = CreateVGM(consol);
			var grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER1").GrossWeight;
			AssertEquals("The weight unit must be KG", grossWeight.Unit.Code, "KG");
			AssertHasWarning(grossWeight.ValueInfo, errorMessage);

			grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER2").GrossWeight;
			AssertNoWarning("Boundary validation", grossWeight.ValueInfo, errorMessage);
		}

		public void TestContainerVerifiedGrossWeightValidation_ExceedsMaxGrossWt()
		{
			var errorMessage = "Verified Gross Weight exceeds the Max Gross Wt. for this container type.";

			var consol = CreateConsol(loadUnloco: "CNNGB");

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_ISOType = "ABCD";
			refContainer2.RC_TareWeight = 1001;
			refContainer2.RC_GrossWeight = 10000;

			var container6 = consol.Containers.AddNew();
			container6.JC_ContainerNum = "CONTAINER6";
			container6.JC_SealNum = "SEAL6";
			container6.JC_RC = refContainer2.PK;
			container6.JC_GrossWeight = 10001;
			container6.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container6.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container6.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container6.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			container6.JC_IsEmptyContainer = false;

			var packingLine6 = consol.Shipments[0].OuterPackLines.AddNew();
			packingLine6.JL_ActualWeight = 10001;
			packingLine6.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packingLine6.JL_ActualVolume = 3;
			packingLine6.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packingLine6.Containers.Add(container6);

			var vgm = CreateVGM(consol);
			var grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER3").GrossWeight;
			AssertEquals("The weight unit must be KG", grossWeight.Unit.Code, "KG");
			AssertNoWarning("Boundary validation", grossWeight.ValueInfo, errorMessage);

			grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER4").GrossWeight;
			AssertNoWarning("Empty container not validated", grossWeight.ValueInfo, errorMessage);

			grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER5").GrossWeight;
			AssertHasWarning(grossWeight.ValueInfo, errorMessage);

			grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER6").GrossWeight;
			AssertNoWarning("Container Type has a valid ISO Type ", grossWeight.ValueInfo, errorMessage);

			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;
			vgm = CreateVGM(consol);
			grossWeight = (Measurement)vgm.Containers.First(c => c.Number == "CONTAINER5").GrossWeight;
			AssertNoWarning("Consol container mode must be FCL", grossWeight.ValueInfo, errorMessage);
		}

		#endregion

		#endregion

		#region TestPopulation

		public void TestPopulation()
		{
			var consol = CreateConsol(Constants.AgentType.CoLoad);
			var vgm = CreateVGM(consol);

			AssertEquals("CONSOL", vgm.FreightForwardersReference);
			AssertEquals("CLD", vgm.ShipmentType.Code);
			AssertEquals("WAYBILL", vgm.BillOfLadingNumber);
			AssertEquals("REF", vgm.CarrierBookingReference);
		}

		public void TestPopulateContainerIsNonOperativeReefer()
		{
			var consol = CreateConsol(Constants.AgentType.CoLoad);
			var container1 = consol.Containers.OfType<ForwardingContainer>().First(x => x.JC_ContainerNum == "CONTAINER1");
			container1.JC_IsNonOperativeReefer = true;
			var container2 = consol.Containers.OfType<ForwardingContainer>().First(x => x.JC_ContainerNum == "CONTAINER2");
			container2.JC_IsNonOperativeReefer = false;

			var vgm = CreateVGM(consol);

			Assert(vgm.Containers.FirstOrDefault(c => c.Number == "CONTAINER1").IsNonOperativeReefer);
			Assert(!vgm.Containers.FirstOrDefault(c => c.Number == "CONTAINER2").IsNonOperativeReefer);
		}

		#endregion

		#region TestPopulateStatement

		public void TestPopulateStatement_FirstLoadInChina()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_SealNum = "SEAL1";
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;

			var vgm = CreateVGM(consol);

			AssertEquals("Show CN statement", "I, the shipper, declare that the verified gross mass information of the packed container in this document is obtained based on the stated method in the International Convention for the Safety of Life at Sea (SOLAS) 1974 Chapter VI Regulation 2.4.2. The weighing instrument in the weighing station has received the certificate from a metrological supervision organization and the date receiving the verified weight is within the validity period of the certificate."
				, vgm.Containers.FirstOrDefault().Statement);
		}

		const string PackedInCNForHKByTruckStatement = "The container stated in this document was packed and sealed in Mainland China then trucked to Hong Kong for shipping overseas under our name as shipper, which was not a business entity registered in Hong Kong. The verified gross mass of the packed container(s) herein declared was obtained in accordance with the requirement, which complied with Method 1/Method 2 stipulated in SOLAS Chapter VI Regulation 2, laid down by China MSA.";
		const string PackedInCNForHKByInlandWaterwayStatement = "The container stated in this document was packed and sealed in Mainland China then transported to Hong Kong by Inland Waterway Transport for shipping overseas under our name as shipper, which was not a business entity registered in Hong Kong. The verified gross mass of the packed container(s) herein declared was obtained in accordance with the requirement, which complied with Method 1/Method 2 stipulated in SOLAS Chapter VI Regulation 2, laid down by China MSA.";

		public void TestPopulateStatement_PackedInChinaForHongKong_ByTruck() => TestPopulateStatement_PackedInChinaForHongKong(Constants.TransportModes.Truck, PackedInCNForHKByTruckStatement);
		public void TestPopulateStatement_PackedInChinaForHongKong_ByInlandWaterway() => TestPopulateStatement_PackedInChinaForHongKong(Constants.TransportModes.InlandWaterwayTransport, PackedInCNForHKByInlandWaterwayStatement);

		void TestPopulateStatement_PackedInChinaForHongKong(string transportModeFromCNToHK, string expectedStatement)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_SealNum = "SEAL1";
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;

			var transport1 = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = transportModeFromCNToHK;
			transport1.JW_TransportType = transportModeFromCNToHK == Constants.TransportModes.InlandWaterwayTransport ? Constants.TransportPlanningType.PreCarriage : Constants.TransportPlanningType.Other;
			transport1.JW_RL_NKLoadPort = "CNSHA";
			transport1.JW_RL_NKDiscPort = "HKHKG";
			transport1.JW_Vessel = "Dragon";
			transport1.JW_VoyageFlight = "111";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "USCHI";
			transport2.JW_Vessel = "R123";
			transport2.JW_VoyageFlight = "222";

			var vgm = CreateVGM(consol);

			AssertEquals(expectedStatement, vgm.Containers.FirstOrDefault().Statement);
		}

		public void TestPopulateStatement_FirstLoadInHongKong()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_SealNum = "SEAL1";
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Gross Weight Verified By";

			container.GrossWeightVerifiedByAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var vgmNumber = container.GrossWeightVerifiedByAddress.Organisation.CustomsCodes.AddNew();
			vgmNumber.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			vgmNumber.OK_RN_NKCodeCountry = Constants.CountryCodes.HongKong;
			vgmNumber.OK_CustomsRegNo = "123";

			var vgm = CreateVGM(consol);

			AssertEquals("The verified gross mass of the packed container(s) declared in this shipping document was obtained in accordance with Method 1 stipulated in SOLAS Chapter VI Regulation 2."
				, vgm.Containers.FirstOrDefault().Statement);
		}

		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			InitConsolAndShipmentForShipperCompanyNameTest(Factory, out var consol, out _, out var forwarder, out var consignor);

			AssertEquals("Pre-Condition", false, consol.IsDirect);

			var builder = new VGMBuilder(consol, null);
			var vgm = builder.Build();
			AssertEquals($"{forwarder.OH_FullName} {forwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} AAA Lines", vgm.Shipper.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, consol.IsDirect);

			vgm = builder.Build();
			AssertEquals($"{consignor.OH_FullName} {consignor.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} BBB Lines", vgm.Shipper.CompanyName);
		}

		public static void InitConsolAndShipmentForShipperCompanyNameTest(BusinessObjectFactory factory, out ForwardingConsol consol, out ForwardingShipment shipment, out OrgHeader forwarder, out OrgHeader consignor)
		{
			shipment = factory.New<ForwardingShipment>();
			consol = shipment.Consols.AddNew();

			forwarder = factory.New<OrgHeader>();
			forwarder.OH_FullName = "Forwarder";
			forwarder.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			forwarder.MiscServ.OM_FWAsAgentName = "AAA Lines";

			consignor = factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsCarrier;
			consignor.MiscServ.OM_FWAsAgentName = "BBB Lines";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
		}

		#endregion

		#region TestPopulateSourceIdFromCarrierShipperReference

		public void TestPopulateSourceIdFromCarrierShipperReference()
		{
			var consol = CreateConsol();
			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C0000005-V8";

			var vgm = CreateVGM(consol);
			AssertEquals("C0000005-V8", ((IDataSourceProvider)vgm).SourceID);
		}

		#endregion

		#region TestContainerSealValidation

		public void TestContainerSealValidation()
		{
			var errorMessage = "Sealed By is mandatory when Seal Number exists.";

			var consol = Factory.New<ForwardingConsol>();
			var containerSrc = consol.Containers.AddNew();
			var containers = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = containers
			};

			var data = CreateVGMBuilder(consol, parameters).Build();
			var container = data.Containers.Cast<VGMMessagingContainer>().First();

			void RefreshExportCode()
			{
				data = CreateVGMBuilder(consol, parameters).Build();
				container = data.Containers.Cast<VGMMessagingContainer>().First();
			}

			AssertNoMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_SealNum = "100";
			RefreshExportCode();

			AssertHasMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_SealParty = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			containerSrc.JC_SealNum = "100";
			RefreshExportCode();

			AssertNoMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);
		}

		#endregion

		#region TestContextWithCarrierUnlocoMapping

		public void TestContextWithCarrierUnlocoMapping_Agent()
		{
			var consol = CreateConsol();
			var mapping1 = consol.ShippingLine.CreatePatternMatchOverrideForTest();

			mapping1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping1.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping1.OO_ForeignCode = "AUXXX";
			mapping1.OO_LocalCode = "AUSYD";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "AUYYY";
			mapping2.OO_LocalCode = "AUSYD";

			Factory.Save();

			var vgm = CreateVGM(consol);
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(vgm.FreightForwarder.Unloco.Unlocos);
			AssertEquals("Mapping should be from the carrier's foreign code", "AUXXX", vgm.FreightForwarder.Unloco.Code);
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
			mapping1.OO_ForeignCode = "AUXXX";
			mapping1.OO_LocalCode = "AUSYD";

			var mapping2 = consol.Creditor.CreatePatternMatchOverrideForTest();

			mapping2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			mapping2.OO_Context = Core.Constants.OrgPatternMatchOverrideContexts.Codes.OceanCarrierMessage;
			mapping2.OO_ForeignCode = "AUYYY";
			mapping2.OO_LocalCode = "AUSYD";

			Factory.Save();

			var vgm = CreateVGM(consol);
			AssertType<RefUNLOCOCollectionWithCarrierMapping>(vgm.FreightForwarder.Unloco.Unlocos);
			AssertEquals("Mapping should be from the co-load with's foreign code", "AUYYY", vgm.FreightForwarder.Unloco.Code);
		}

		#endregion

		#region TestSealNumberValidationErrorMessage

		public void TestVGMSealNumberValidationErrorMessage()
		{
			var consol = Factory.New<ForwardingConsol>();

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "TestOrg";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = organization.PK;
			consol.JK_OA_ShippingLineAddress = address.PK;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			organization.OH_RSL_ShippingLine = shippingLine.PK;

			var sel = Factory.New<RefShippingLineMessagingRequirement>();
			sel.RSR_IsVerifiedGrossContainerWeight = true;
			sel.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.SealNumberMandatory;
			sel.RSR_RSL_ShippingLine = shippingLine.PK;

			var containerSrc = consol.Containers.AddNew();
			var containers = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = containers
			};

			var data = CreateVGMBuilder(consol, parameters).Build();
			var container = data.Containers.Cast<VGMMessagingContainer>().First();

			AssertHasMessageError("Seal number is mandatory if SEL ticked in Shipping Line.", ((CodeDescription)container.SealPartyType).CodeInfo, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForExists);

			void RefreshExportCode()
			{
				data = CreateVGMBuilder(consol, parameters).Build();
				container = data.Containers.Cast<VGMMessagingContainer>().First();
			}
			containerSrc.JC_SealNum = "0123456789123456";
			RefreshExportCode();
			AssertHasMessageError("Seal number cannot exceed 15 characters.", ((CodeDescription)container.SealPartyType).CodeInfo, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
		}

		#endregion

		#region TestPopulateCarrierMessagingRequirements

		public void TestPopulateCarrierMessagingRequirements_FOM()
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
			Assert(!new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);

			var messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsVerifiedGrossContainerWeight = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsVerifiedGrossContainerWeight);
			Assert(new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsVerifiedGrossContainerWeight = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsVerifiedGrossContainerWeight);
			Assert(!new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = orgHeader.MainAddress.PK;

			refShippingLine.ShippingLineMessagingRequirements.DeleteAll();

			AssertNull(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage));
			Assert(!new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);

			messagingRequirement = refShippingLine.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage;
			messagingRequirement.RSR_IsVerifiedGrossContainerWeight = true;

			Assert(refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsVerifiedGrossContainerWeight);
			Assert(new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);

			messagingRequirement.RSR_IsVerifiedGrossContainerWeight = false;

			Assert(!refShippingLine.ShippingLineMessagingRequirements.FirstOrDefault(x => x.RSR_RST_NKType == ShippingLineMessagingRequirement.Types.AttachFormAsPDFInMessage).RSR_IsVerifiedGrossContainerWeight);
			Assert(!new VGMBuilder(consol, null).Build().IsRequiredSendAttachment);
		}

		public void TestCarrierMessagingRequirementsValidation_IEL()
		{
			var errorMessage = "This carrier only supports integration via email to local office. \r\nContact name and email address are required to send Verified Gross Container Weight.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group VGM.";

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

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			var carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNoMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertNullOrEmpty(carrierMessageData.Carrier.Contact);

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;
			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNoMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertNullOrEmpty(carrierMessageData.Carrier.Email);

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNoMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertEquals("test@test.com", carrierMessageData.Carrier.Email);

			contact.Documents.RemoveAll();

			var shippingLineMessagingRequirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLineMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice;
			shippingLineMessagingRequirement.RSR_IsVerifiedGrossContainerWeight = true;

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertHasMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertNullOrEmpty(carrierMessageData.Carrier.Contact);

			document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "VGM";

			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNoMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertEquals("TEST NAME",carrierMessageData.Carrier.Contact);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			contact.Documents.RemoveAll();

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNullOrEmpty(carrierMessageData.Carrier.Contact);
			AssertNoMessageError(carrierMessageData.Carrier.AddressFormattedInfo, errorMessage);
			AssertNoMessageError(carrierMessageData.FreightForwarder.AddressFormattedInfo, errorMessage);

			document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;
			carrierMessageData = new VGMBuilder(consol, null).Build();
			AssertNullOrEmpty(carrierMessageData.Carrier.Contact);
			AssertEquals("TEST NAME", carrierMessageData.FreightForwarder.Contact);
			AssertNoMessageError(carrierMessageData.FreightForwarder.AddressFormattedInfo, errorMessage);
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var verifiedGrossMass = CreateVGM(consol);
			AssertEquals(Constants.ContainerModes.FCL, verifiedGrossMass.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, verifiedGrossMass.ContainerMode.Description);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => CreateVGM();

		VerifiedGrossMass CreateVGM()
		{
			var consol = CreateConsol();
			return CreateVGM(consol);
		}

		VerifiedGrossMass CreateVGM(ForwardingConsol consol)
		{
			var containersToSend = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();

			return CreateVGM(consol, containersToSend);
		}

		VerifiedGrossMass CreateVGM(ForwardingConsol consol, ForwardingContainer[] containersToSend)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				Data = containersToSend
			};

			var builder = new VGMBuilder(consol, parameters);

			return builder.Build();
		}

		ForwardingConsol CreateConsol(string agentType = Core.Constants.AgentType.Agent, string loadUnloco = "AUSYD", string dischUnloco = "NZAKL", string seaTransportMode = Constants.TransportModes.Sea)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "CONSOL";
			consol.JK_BookingReference = "REF";
			consol.JK_MasterBillNum = "WAYBILL";
			consol.JK_AgentType = agentType;
			consol.JK_RL_NKLoadPort = loadUnloco;
			consol.JK_RL_NKDischargePort = dischUnloco;

			var seaTransportLeg = consol.Transports
				.OfType<Freight.Business.Transport>()
				.Single();

			seaTransportLeg.JW_TransportMode = seaTransportMode;
			seaTransportLeg.JW_Vessel = "TAIKO";
			seaTransportLeg.JW_VoyageFlight = "123";
			seaTransportLeg.JW_RL_NKLoadPort = loadUnloco;
			seaTransportLeg.JW_RL_NKDiscPort = dischUnloco;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "BookingRef";
			shipment.JS_HouseBill = "3334444444";
			shipment.JS_RL_NKOrigin = loadUnloco;
			shipment.JS_RL_NKDestination = dischUnloco;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Shipper";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "UNIT01";
			shipper.MainAddress.Address2 = "Whatever Street";
			shipper.MainAddress.City = "SYDNEY";
			shipper.MainAddress.Postcode = "2015";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_Email = "HeyHey@ItsATest.com";
			shipment.ConsignorDocumentaryAddress.E2_Contact = "CONTACT1";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "UNIT02";
			consignee.MainAddress.Address2 = "Yeah nah Street";
			consignee.MainAddress.City = "AUCKLAND";
			consignee.MainAddress.Postcode = "1050";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_Email = "Yesss@AnotherOne.com";
			shipment.ConsigneeDocumentaryAddress.E2_Contact = "CONTACT2";

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "Carrier";
			shippingLine.OH_RL_NKClosestPort = "NZAKL";
			shippingLine.MainAddress.Address1 = "UNIT03";
			shippingLine.MainAddress.Address2 = "Something Street";
			shippingLine.MainAddress.City = "AUCKLAND";
			shippingLine.MainAddress.Postcode = "1050";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingLine.MainAddress.OA_Email = "Out@ofideas.com";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "UNIT04";
			sendingForwarder.MainAddress.Address2 = "Rather Street";
			sendingForwarder.MainAddress.City = "SYDNEY";
			sendingForwarder.MainAddress.Postcode = "1050";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			sendingForwarder.MainAddress.OA_Email = "Blah@Bloogle.com";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.OH_RL_NKClosestPort = "AUMEL";
			receivingForwarder.MainAddress.Address1 = "UNIT04";
			receivingForwarder.MainAddress.Address2 = "Something Street";
			receivingForwarder.MainAddress.City = "Melbourne";
			receivingForwarder.MainAddress.Postcode = "1050";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			receivingForwarder.MainAddress.OA_Email = "Crah@Croogle.com";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var carrierHanlingAgent = Factory.New<OrgHeader>();
			carrierHanlingAgent.OH_FullName = "Carrier Handling Agent";
			carrierHanlingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierHanlingAgent.MainAddress.Address1 = "Unit 990";
			carrierHanlingAgent.MainAddress.Address2 = "245 Drive";
			carrierHanlingAgent.MainAddress.City = "unknown city";
			carrierHanlingAgent.MainAddress.Postcode = "4689";
			carrierHanlingAgent.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = carrierHanlingAgent.MainAddress.PK;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "Carrier Booking Agent";
			carrierBookingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierBookingAgent.MainAddress.Address1 = "Unit 990";
			carrierBookingAgent.MainAddress.Address2 = "245 Drive";
			carrierBookingAgent.MainAddress.City = "unknown city";
			carrierBookingAgent.MainAddress.Postcode = "4689";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "CN";
			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

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

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = "22P1";
			refContainer.RC_TareWeight = 1001;
			refContainer.RC_GrossWeight = 10000;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			container1.JC_SealNum = "SEAL1";
			container1.JC_RC = refContainer.PK;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container1.JC_IsEmptyContainer = false;

			var packingLine1 = shipment.OuterPackLines.AddNew();
			packingLine1.JL_ActualWeight = 1000;
			packingLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packingLine1.JL_ActualVolume = 1;
			packingLine1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packingLine1.Containers.Add(container1);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";
			container2.JC_SealNum = "SEAL2";
			container2.JC_RC = refContainer.PK;
			container2.JC_GrossWeight = 1001;
			container2.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container2.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
			container2.JC_IsEmptyContainer = false;

			var packingLine2 = shipment.OuterPackLines.AddNew();
			packingLine2.JL_ActualWeight = 1001;
			packingLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packingLine2.JL_ActualVolume = 2;
			packingLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packingLine2.Containers.Add(container2);

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";
			container3.JC_SealNum = "SEAL3";
			container3.JC_RC = refContainer.PK;
			container3.JC_GrossWeight = 10000;
			container3.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container3.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container3.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container3.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			container3.JC_IsEmptyContainer = false;

			var packingLine3 = shipment.OuterPackLines.AddNew();
			packingLine3.JL_ActualWeight = 10000;
			packingLine3.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packingLine3.JL_ActualVolume = 3;
			packingLine3.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packingLine3.Containers.Add(container3);

			var container4 = consol.Containers.AddNew();
			container4.JC_ContainerNum = "CONTAINER4";
			container4.JC_SealNum = "SEAL4";
			container4.JC_GrossWeight = 4000;
			container4.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container4.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container4.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container4.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			container4.JC_IsEmptyContainer = true;

			var container5 = consol.Containers.AddNew();
			container5.JC_ContainerNum = "CONTAINER5";
			container5.JC_SealNum = "SEAL5";
			container5.JC_RC = refContainer.PK;
			container5.JC_GrossWeight = 10001;
			container5.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container5.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container5.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container5.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			container5.JC_IsEmptyContainer = false;

			var packingLine5 = shipment.OuterPackLines.AddNew();
			packingLine5.JL_ActualWeight = 10001;
			packingLine5.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packingLine5.JL_ActualVolume = 3;
			packingLine5.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packingLine5.Containers.Add(container5);

			Factory.Save();

			return consol;
		}

		public static void AssertBasicAsciiCharactersValidation(string propertyName, ZPropertyInfo propertyInfo, string errorMessage)
		{
			propertyInfo.Value = (ZString)"天";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"இ";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"æ";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"ß";
			AssertHasMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"1";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);

			propertyInfo.Value = (ZString)"A";
			AssertNoMessageError($"{propertyName}", propertyInfo, errorMessage);
		}

		VGMBuilder CreateVGMBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			return new VGMBuilder(consol, parameters);
		}
		#endregion
	}
}

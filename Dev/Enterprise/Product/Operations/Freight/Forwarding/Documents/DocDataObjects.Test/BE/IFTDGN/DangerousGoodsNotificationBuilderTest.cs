using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using BelgianPortsConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.BelgianPortsConstants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.BE.Testing
{
	sealed class DangerousGoodsNotificationBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertNotNull(dangerousGoodsNotification);

			AssertEquals("ConsolNumber", "C00002000", dangerousGoodsNotification.ConsolNumber);
			AssertEquals("BOL_Reference", consol.JK_MasterBillNum, dangerousGoodsNotification.BillOfLading);
			AssertEquals("BookingReference", consol.JK_BookingReference, dangerousGoodsNotification.CarrierBookingReference);
			AssertEquals("DgnSecurityNumber", "", dangerousGoodsNotification.DgnSecurityNumber);

			AssertAddressData(consol.ShippingLineAddress, dangerousGoodsNotification.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dangerousGoodsNotification.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dangerousGoodsNotification.SendingForwarder);
			AssertAddressData(consol.ArrivalCTOAddress, dangerousGoodsNotification.ArrivalCTO);
			AssertAddressData(consol.DepartureCTOAddress, dangerousGoodsNotification.DepartureCTO);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dangerousGoodsNotification.SendingParty);
		}

		public void TestDataSetValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();
			dangerousGoodsNotification.ValidateAllIncludingChildren();

			var missingDangerousGoodsErrorMessage = "No Dangerous Goods found! Dangerous Goods Notification is not required.";
			AssertHasMessageError("ErrorPlaceHolder should have error message indicating missing Dangerous Goods", dangerousGoodsNotification.ErrorPlaceHolderInfo, missingDangerousGoodsErrorMessage);

			CreateExportTransports(consol, Constants.TransportModes.Sea);
			CreateBasicGoodsAndEquipment(consol);
			dangerousGoodsNotification = builder.Build();
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError("ErrorPlaceHolder should not have error message indicating missing Dangerous Goods", dangerousGoodsNotification.ErrorPlaceHolderInfo, missingDangerousGoodsErrorMessage);
		}

		public void TestPopulateSendingParty()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "BE";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertAddressData(branchProxy.MainAddress, dangerousGoodsNotification.SendingParty);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			dangerousGoodsNotification = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dangerousGoodsNotification.SendingParty);
		}

		public void TestPopulatePortIds()
		{
			var consol = CreateConsol();
			CreateAddresses(consol);

			AssertPopulatePortIds(consol, consol.ShippingLineAddress, "CarrierPortId");
			AssertPopulatePortIds(consol, consol.ReceivingForwarderAddress, "ReceivingForwarderPortId");
			AssertPopulatePortIds(consol, consol.SendingForwarderAddress, "SendingForwarderPortId");
			AssertPopulatePortIds(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartyPortId");
			AssertPopulateEoriAndDuns(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartyEori", "SendingPartyDuns");
			AssertPopulatePortIds(consol, consol.ArrivalCTOAddress, "ArrivalCTOTerminalId");
			AssertPopulatePortIds(consol, consol.DepartureCTOAddress, "DepartureCTOTerminalId");
		}

		void AssertPopulatePortIds(ForwardingConsol consol, OrgAddress address, string portIdPropertyName)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};
			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var portSystemNumber = address.Header.CustomsCodes.AddNew();
			portSystemNumber.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			portSystemNumber.OK_RN_NKCodeCountry = Constants.CountryCodes.Belgium;
			portSystemNumber.OK_CustomsRegNo = "PSN001";

			var dangerousGoodsNotification = builder.Build();

			AssertEquals($"{portIdPropertyName} should be from org", "PSN001", ((RegistrationNumber)dangerousGoodsNotification[portIdPropertyName]).Value);

			var portSystemNumber2 = address.CustomsCodes.AddNew();
			portSystemNumber2.OK_CodeType = OrgCusCode.CodeTypes.PortSystemNumber;
			portSystemNumber2.OK_RN_NKCodeCountry = Constants.CountryCodes.Belgium;
			portSystemNumber2.OK_CustomsRegNo = "PSN002";

			dangerousGoodsNotification = builder.Build();

			AssertEquals($"{portIdPropertyName} should be from org", "PSN002", ((RegistrationNumber)dangerousGoodsNotification[portIdPropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			dangerousGoodsNotification = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)dangerousGoodsNotification[portIdPropertyName]).Value);
		}

		void AssertPopulateEoriAndDuns(ForwardingConsol consol, OrgAddress address, string eoriPropertyName, string dunsPropertyName)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};
			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var eoriNumber = address.Header.CustomsCodes.AddNew();
			eoriNumber.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriNumber.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			eoriNumber.OK_CustomsRegNo = "EORI001";
			var dangerousGoodsNotification = builder.Build();
			AssertEquals($"{eoriPropertyName} should be from org", "EORI001", ((RegistrationNumber)dangerousGoodsNotification[eoriPropertyName]).Value);

			var eoriNumber2 = address.CustomsCodes.AddNew();
			eoriNumber2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eoriNumber2.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			eoriNumber2.OK_CustomsRegNo = "EORI002";
			dangerousGoodsNotification = builder.Build();
			AssertEquals($"{eoriPropertyName} should be from org", "EORI002", ((RegistrationNumber)dangerousGoodsNotification[eoriPropertyName]).Value);

			var dunsNumber = address.Header.CustomsCodes.AddNew();
			dunsNumber.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			dunsNumber.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			dunsNumber.OK_CustomsRegNo = "DUNS001";
			dangerousGoodsNotification = builder.Build();
			AssertEquals($"{dunsPropertyName} should be from org", "DUNS001", ((RegistrationNumber)dangerousGoodsNotification[dunsPropertyName]).Value);

			var dunsNumber2 = address.CustomsCodes.AddNew();
			dunsNumber2.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			dunsNumber2.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			dunsNumber2.OK_CustomsRegNo = "DUNS002";
			dangerousGoodsNotification = builder.Build();
			AssertEquals($"{dunsPropertyName} should be from org", "DUNS002", ((RegistrationNumber)dangerousGoodsNotification[dunsPropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();
			dangerousGoodsNotification = builder.Build();
			AssertEquals(string.Empty, ((RegistrationNumber)dangerousGoodsNotification[eoriPropertyName]).Value);
			AssertEquals(string.Empty, ((RegistrationNumber)dangerousGoodsNotification[dunsPropertyName]).Value);
			AssertHasMessageError("EORI not filled in", ((RegistrationNumber)dangerousGoodsNotification[dunsPropertyName]).ValueInfo, "DUNS/EORI Number is missing from this organization > Config > Registration Numbers/Codes - type DUN/EOR");
		}

		public void TestPopulateContactDetails()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			CreateAddresses(consol);

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertEquals("Contact Name receivingForwarder should be Black Panther", "Black Panther", dangerousGoodsNotification.ReceivingForwarder.Contact);
			AssertEquals("Email receivingForwarder should be black_panther@marvel.com", "black_panther@marvel.com", dangerousGoodsNotification.ReceivingForwarder.Email);
			AssertEquals("Phone receivingForwarder should be 112", "112", dangerousGoodsNotification.ReceivingForwarder.Phone);

			AssertEquals("Contact Name sendingForwarder should be SPIDERMAN", "SPIDERMAN", dangerousGoodsNotification.SendingForwarder.Contact);
			AssertEquals("Email sendingForwarder should be spider@marvel.com", "spider@marvel.com", dangerousGoodsNotification.SendingForwarder.Email);
			AssertEquals("Phone sendingForwarder should be 911", "911", dangerousGoodsNotification.SendingForwarder.Phone);
		}

		public void TestContactValidationImport()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			CreateAddresses(consol);

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var contactErrorMessage = "Contact name is required.";
			var emailErrorMessage = "Email is required.";
			var phoneErrorMessage = "Phone is required.";
			AssertNoMessageError("Contact Name receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.PhoneInfo, phoneErrorMessage);

			AssertNoMessageError("Contact Name sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.PhoneInfo, phoneErrorMessage);

			dangerousGoodsNotification.ReceivingForwarder.Contact = "";
			dangerousGoodsNotification.ReceivingForwarder.Email = "";
			dangerousGoodsNotification.ReceivingForwarder.Phone = "";
			dangerousGoodsNotification.SendingForwarder.Contact = "";
			dangerousGoodsNotification.SendingForwarder.Email = "";
			dangerousGoodsNotification.SendingForwarder.Phone = "";
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError("Contact Name receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.ContactInfo, contactErrorMessage);
			AssertHasMessageError("Email receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.EmailInfo, emailErrorMessage);
			AssertHasMessageError("Phone receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.PhoneInfo, phoneErrorMessage);

			AssertNoMessageError("Contact Name sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.PhoneInfo, phoneErrorMessage);
		}

		public void TestContactValidationExport()
		{
			var consol = CreateConsol("EXPORT");
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			CreateAddresses(consol);

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var contactErrorMessage = "Contact name is required.";
			var emailErrorMessage = "Email is required.";
			var phoneErrorMessage = "Phone is required.";
			AssertNoMessageError("Contact Name receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone receivingForwarder should not have error message", dangerousGoodsNotification.ReceivingForwarder.PhoneInfo, phoneErrorMessage);

			AssertNoMessageError("Contact Name sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.PhoneInfo, phoneErrorMessage);

			dangerousGoodsNotification.ReceivingForwarder.Contact = "";
			dangerousGoodsNotification.ReceivingForwarder.Email = "";
			dangerousGoodsNotification.ReceivingForwarder.Phone = "";
			dangerousGoodsNotification.SendingForwarder.Contact = "";
			dangerousGoodsNotification.SendingForwarder.Email = "";
			dangerousGoodsNotification.SendingForwarder.Phone = "";
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError("Contact Name receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.ContactInfo, contactErrorMessage);
			AssertNoMessageError("Email receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.EmailInfo, emailErrorMessage);
			AssertNoMessageError("Phone receivingForwarder should have error message", dangerousGoodsNotification.ReceivingForwarder.PhoneInfo, phoneErrorMessage);

			AssertHasMessageError("Contact Name sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.ContactInfo, contactErrorMessage);
			AssertHasMessageError("Email sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.EmailInfo, emailErrorMessage);
			AssertHasMessageError("Phone sendingForwarder should not have error message", dangerousGoodsNotification.SendingForwarder.PhoneInfo, phoneErrorMessage);
		}

		public void TestPopulateImportTransports()
		{
			var consol = CreateConsol("IMPORT", Constants.TransportModes.Sea);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertNotNull(dangerousGoodsNotification);
			var mainTransport = (Transport)dangerousGoodsNotification.Transports.FirstOrDefault(t => t.Type.Code == Constants.TransportPlanningType.MainVessel);
			AssertNotNull(mainTransport);

			AssertEquals("Handling Instruction should be LDI (Discharging)", BelgianPortsConstants.HandlingInstructions.Discharge, dangerousGoodsNotification.HandlingInstruction);
			AssertEquals("Handling Date should be empty", ZDateTime.Empty, dangerousGoodsNotification.HandlingDate);
			AssertEquals("Operational Port should be BEANR", "BEANR", dangerousGoodsNotification.OperationalPort.Code);
			AssertEquals("Vessel Stay Start date should be taken from main Transport ETA", mainTransport.ETA, dangerousGoodsNotification.VesselStayStartDate);
			AssertEquals("Vessel Stay End date should be empty", ZDateTime.Empty, dangerousGoodsNotification.VesselStayEndDate);
			AssertEquals("Vessel Stay Reference should be empty", ZString.Empty, dangerousGoodsNotification.VesselStayReference);

			AssertEquals("Port Collection Details transport Mode should be set", Constants.TransportModes.Sea, dangerousGoodsNotification.PreOrOnTransportMode.Code);
			AssertEquals("Port Collection Details Vessel Name should be set", "MSC POOLSTER", dangerousGoodsNotification.PreOrOnVesselName);
			AssertEquals("Port Collection Details Vessel ENINumber should be empty", ZString.Empty, dangerousGoodsNotification.PreOrOnVesselENINumber);
			AssertEquals("Port Collection Details Expected Departure at Port should be set", new ZDateTime(2020, 10, 23, 7, 35, 00), dangerousGoodsNotification.PickupDate);

			AssertEquals("Main transport ETD should be set", new ZDateTime(2020, 10, 6, 8, 45, 00), mainTransport.ETD);
			AssertEquals("Main transport ETA should be set", new ZDateTime(2020, 10, 22, 12, 15, 00), mainTransport.ETA);
			AssertEquals("Main transport Mode should be set", Constants.TransportModes.Sea, mainTransport.Mode.Code);
			AssertEquals("Main transport Type should be set", Constants.TransportPlanningType.MainVessel, mainTransport.Type.Code);
			AssertEquals("Main transport Vessel Name should be set", "COSCO NEBULA", mainTransport.Vessel.Name);
			AssertEquals("Main transport Vessel LloydsIMO should be empty", "9795622", mainTransport.Vessel.LloydsIMO);
			AssertEquals("Main transport Vessel Radio Call Sign should be set", "VRRW8", mainTransport.Vessel.RadioCallSign);
			AssertEquals("Main transport Vessel Type should be set", "CV", mainTransport.Vessel.Type.Code);
			AssertEquals("Main transport Vessel Country of registration should be set", "HK", mainTransport.Vessel.CountryOfRegistration.Code);
			AssertEquals("Main transport Voyage Reference should be set", "85475", mainTransport.VoyageFlightNumber);
			AssertEquals("Main transport Port Of Loading should be set", "CNYTN", mainTransport.PortOfLoading.Code);
			AssertEquals("Main transport Port Of Discharge should be set", "BEANR", mainTransport.PortOfDischarge.Code);
		}

		public void TestPopulateExportTransports()
		{
			var consol = CreateConsol("EXPORT", Constants.TransportModes.Road);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertNotNull(dangerousGoodsNotification);
			var mainTransport = (Transport)dangerousGoodsNotification.Transports.FirstOrDefault(t => t.Type.Code == Constants.TransportPlanningType.MainVessel);

			AssertEquals("Handling Instruction should be LLO (Loading)", BelgianPortsConstants.HandlingInstructions.Loading, dangerousGoodsNotification.HandlingInstruction);
			AssertEquals("Handling Date should be empty", ZDateTime.Empty, dangerousGoodsNotification.HandlingDate);
			AssertEquals("Operational Port should be BEANR", "BEANR", dangerousGoodsNotification.OperationalPort.Code);
			AssertEquals("Vessel Stay Start date should be empty", ZDateTime.Empty, dangerousGoodsNotification.VesselStayStartDate);
			AssertEquals("Vessel Stay End date should be taken from main Transport ETD", mainTransport.ETD, dangerousGoodsNotification.VesselStayEndDate);
			AssertEquals("Vessel Stay Reference should be empty", ZString.Empty, dangerousGoodsNotification.VesselStayReference);

			AssertEquals("Port Delivery Details transport Mode should be set", Constants.TransportModes.Road, dangerousGoodsNotification.PreOrOnTransportMode.Code);
			AssertEquals("Port Delivery Details Vessel Name should be set", "TRAILER", dangerousGoodsNotification.PreOrOnVesselName);
			AssertEquals("Port Delivery Details Vessel ENINumber should be empty", ZString.Empty, dangerousGoodsNotification.PreOrOnVesselENINumber);
			AssertEquals("Port Delivery Details Expected Arrival at Port should be set", new ZDateTime(2020, 10, 4, 21, 25, 00), dangerousGoodsNotification.DeliveryDate);

			AssertEquals("Main transport ETD should be set", new ZDateTime(2020, 10, 6, 8, 45, 00), mainTransport.ETD);
			AssertEquals("Main transport ETA should be set", new ZDateTime(2020, 10, 22, 12, 15, 00), mainTransport.ETA);
			AssertEquals("Main transport Mode should be set", Constants.TransportModes.Sea, mainTransport.Mode.Code);
			AssertEquals("Main transport Type should be set", Constants.TransportPlanningType.MainVessel, mainTransport.Type.Code);
			AssertEquals("Main transport Vessel Name should be set", "COSCO NEBULA", mainTransport.Vessel.Name);
			AssertEquals("Main transport Vessel LloydsIMO should be empty", "9795622", mainTransport.Vessel.LloydsIMO);
			AssertEquals("Main transport Vessel Radio Call Sign should be set", "VRRW8", mainTransport.Vessel.RadioCallSign);
			AssertEquals("Main transport Vessel Type should be set", "CV", mainTransport.Vessel.Type.Code);
			AssertEquals("Main transport Vessel Country of registration should be set", "HK", mainTransport.Vessel.CountryOfRegistration.Code);
			AssertEquals("Main transport Voyage Reference should be set", "85475", mainTransport.VoyageFlightNumber);
			AssertEquals("Main transport Port Of Loading should be set", "BEANR", mainTransport.PortOfLoading.Code);
			AssertEquals("Main transport Port Of Discharge should be set", "CNYTN", mainTransport.PortOfDischarge.Code);
		}

		public void TestTransportValidations()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();
			dangerousGoodsNotification.ValidateAllIncludingChildren();

			var vesselStayReferenceErrorMessage = "Ship's Stay Number is required.";
			AssertHasMessageError("VesselStayReference should have error message", dangerousGoodsNotification.VesselStayReferenceInfo, vesselStayReferenceErrorMessage);

			var operationalPortErrorMessage = "Port of Call is required.";
			AssertNoMessageError("Port Of Call should not have error message", ((Unloco)dangerousGoodsNotification.OperationalPort).CodeInfo, operationalPortErrorMessage);

			dangerousGoodsNotification.VesselStayReference = "V207145";
			dangerousGoodsNotification.OperationalPort.Code = "";
			dangerousGoodsNotification.ValidateAllIncludingChildren();

			AssertNoMessageError("VesselStayReference should not have error message", dangerousGoodsNotification.VesselStayReferenceInfo, vesselStayReferenceErrorMessage);
			AssertHasMessageError("Port Of Call should have error message", ((Unloco)dangerousGoodsNotification.OperationalPort).CodeInfo, operationalPortErrorMessage);
		}

		public void TestRoutingValidationExport()
		{
			var consol = CreateConsol("EXPORT", Constants.TransportModes.Road);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var missingPortDeliveryDetailsTransportModeErrorMessage = "Transport Mode of Port Delivery Details is required.";
			var missingDeliveryDateErrorMessage = "Expected Arrival date at Port is required.";
			AssertNoMessageError("Port Delivery Details should not have error message indicating missing Transport Mode", ((CodeDescription)dangerousGoodsNotification.PreOrOnTransportMode).CodeInfo, missingPortDeliveryDetailsTransportModeErrorMessage);
			AssertNoMessageError("Port Delivery Details should not have error message indicating missing Expected Arrival date at Port", dangerousGoodsNotification.DeliveryDateInfo, missingDeliveryDateErrorMessage);

			dangerousGoodsNotification.PreOrOnTransportMode.Code = "";
			dangerousGoodsNotification.DeliveryDate = ZDateTime.Empty;
			dangerousGoodsNotification.ValidateAllIncludingChildren();

			AssertHasMessageError("Port Delivery Details should have error message indicating missing Transport Mode", ((CodeDescription)dangerousGoodsNotification.PreOrOnTransportMode).CodeInfo, missingPortDeliveryDetailsTransportModeErrorMessage);
			AssertHasMessageError("Port Delivery Details should have error message indicating missing Expected Arrival date at Port", dangerousGoodsNotification.DeliveryDateInfo, missingDeliveryDateErrorMessage);
		}

		public void TestRoutingValidationImport()
		{
			var consol = CreateConsol("IMPORT", Constants.TransportModes.Road);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var missingPortCollectionDetailsTransportModeErrorMessage = "Transport Mode of Port Collection Details is required.";
			var missingPickupDateErrorMessage = "Expected Departure date at Port is required.";
			AssertNoMessageError("Port Collection Details should not have error message indicating missing Transport Mode", ((CodeDescription)dangerousGoodsNotification.PreOrOnTransportMode).CodeInfo, missingPortCollectionDetailsTransportModeErrorMessage);
			AssertNoMessageError("Port Collection Details should not have error message indicating missing Expected Departure date at Port", dangerousGoodsNotification.PickupDateInfo, missingPickupDateErrorMessage);

			dangerousGoodsNotification.PreOrOnTransportMode.Code = "";
			dangerousGoodsNotification.PickupDate = ZDateTime.Empty;
			dangerousGoodsNotification.ValidateAllIncludingChildren();

			AssertHasMessageError("Port Collection Details should have error message indicating missing Transport Mode", ((CodeDescription)dangerousGoodsNotification.PreOrOnTransportMode).CodeInfo, missingPortCollectionDetailsTransportModeErrorMessage);
			AssertHasMessageError("Port Collection Details should have error message indicating missing Expected Departure date at Port", dangerousGoodsNotification.PickupDateInfo, missingPickupDateErrorMessage);
		}

		public void TestPopulateVesselInformation()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertNotNull(dangerousGoodsNotification);

			var mainTransport = (Transport)dangerousGoodsNotification.Transports.FirstOrDefault(t => t.Type.Code == Constants.TransportPlanningType.MainVessel);

			AssertEquals("Vessel Name should be populated", "COSCO NEBULA", mainTransport.Vessel.Name);
			AssertEquals("Vessel LloydsIMO should be populated", "9795622", mainTransport.Vessel.LloydsIMO);
			AssertEquals("Vessel Radio Call Sign should be populated", "VRRW8", mainTransport.Vessel.RadioCallSign);
			AssertEquals("Vessel Type should be populated", "CV", mainTransport.Vessel.Type.Code);
			AssertEquals("Vessel Country of Registration should be populated", "HK", mainTransport.Vessel.CountryOfRegistration.Code);
			AssertEquals("Voyage Reference should be populated", "85475", mainTransport.VoyageFlightNumber);
		}

		public void TestPopulateGoodsAndEquipmentInformation()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PopulateGoodsAndEquipmentInformation(true);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				PopulateGoodsAndEquipmentInformation(false);
			}

			void PopulateGoodsAndEquipmentInformation(bool activateIsCombustibleForDGItems)
			{
				var consol = CreateConsol();
				var parameters = new DummyDocDataObjectParameters
				{
					DataStoreName = "BEDangerousGoodsNotification_IMP"
				};

				var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
				var dangerousGoodsNotification = builder.Build();

				AssertNotNull(dangerousGoodsNotification);

				AssertEquals("DangerousGoodsNotification should contain 2 containers", 2, dangerousGoodsNotification.Containers.Count);

				// Container 1
				var container1 = dangerousGoodsNotification.Containers.ElementAt(0);
				AssertEquals("DangerousGoodsNotification container 1 should contain 3 packinglines", 3, container1.PackingLines.Count);
				AssertEquals("DangerousGoodsNotification container 1 Equipment Number MSCU1245787", "MSCU1245787", container1.Number);
				AssertEquals("DangerousGoodsNotification container 1 should contain 5 packs", 5, container1.PackCount);
				AssertEquals("DangerousGoodsNotification container 1 should have DangerousGoods", true, container1.HasDangerousGoods);

				//Container 1 - Packing Line 1
				var packline1 = container1.PackingLines.ElementAt(0);
				AssertEquals("Description (good 1)", "ROOF COVERING", packline1.GoodsDescription);
				AssertEquals("Packs (good 1)", 1, packline1.Quantity);
				AssertEquals("Weight (good 1)", 18m, packline1.Weight.Value);
				AssertEquals("Weight UM (good 1)", "KG", packline1.Weight.Unit.Code);
				AssertEquals("Volume (good 1)", 0m, packline1.Volume.Value);
				AssertEquals("Volume UM (good 1)", "M3", packline1.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 1)", 0, packline1.DangerousGoods.Count);

				//Container 1 - Packing Line 2
				var packline2 = container1.PackingLines.ElementAt(1);
				AssertEquals("Description (good 2)", "goods description", packline2.GoodsDescription);
				AssertEquals("Packs (good 2)", 1, packline2.Quantity);
				AssertEquals("Weight (good 2)", 15m, packline2.Weight.Value);
				AssertEquals("Weight UM (good 2)", "KG", packline2.Weight.Unit.Code);
				AssertEquals("Volume (good 2)", 0m, packline2.Volume.Value);
				AssertEquals("Volume UM (good 2)", "M3", packline2.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 2)", 2, packline2.DangerousGoods.Count);

				//Container 1 - Dangerous Good 1
				var dangerousGood1 = packline2.DangerousGoods.ElementAt(0);
				AssertEquals("Container DGN Index of Dangerous Goods (good 2 - dg 1)", 1, dangerousGood1.ContainerDGNIndex);
				AssertEquals("UNDG Code of Dangerous Goods (good 2 - dg 1)", "0503b", dangerousGood1.Code);
				AssertEquals("UNDG Code of Dangerous Goods (good 2 - dg 1)", "IMO", dangerousGood1.RegulationStandard);
				AssertEquals("Proper Shipping Name of Dangerous Goods (good 2 - dg 1)", "AIR BAG MODULES", dangerousGood1.ProperShippingName);
				AssertEquals("Technical name of Dangerous Goods (good 2 - dg 1)", "Airbag Mercedes C", dangerousGood1.TechnicalName);
				AssertEquals("IMOClass of Dangerous Goods (good 2 - dg 1)", "1.4G", dangerousGood1.IMOClass);
				AssertEquals("FlashPoint of Dangerous Goods (good 2 - dg 1)", 85.0m, dangerousGood1.FlashPoint.Value);
				AssertEquals("DGWeight of Dangerous Goods (good 2 - dg 1)", 142m, dangerousGood1.Weight.Value);
				AssertEquals("Pack Count of Dangerous Goods (good 2 - dg 1)", 6, dangerousGood1.Quantity);
				AssertEquals("Pack Type of Dangerous Goods (good 2 - dg 1)", "BOX", dangerousGood1.PackageType.Code);
				AssertEquals("PackingGroup of Dangerous Goods (good 2 - dg 1)", "II", dangerousGood1.PackingGroup);
				AssertEquals("PackedInLimitedQty of Dangerous Goods (good 2 - dg 1)", false, dangerousGood1.PackedInLimitedQuantity);
				AssertEquals("PackedInExceptedQty of Dangerous Goods (good 2 - dg 1)", false, dangerousGood1.PackedInExceptedQuantity);
				AssertEquals("Emergency Schedule Fire Code of Dangerous Goods (good 2 - dg 1)", "F-I", dangerousGood1.EmergencyScheduleFire.Code);
				AssertEquals("Emergency Schedule Spillage Code of Dangerous Goods (good 2 - dg 1)", "S-S", dangerousGood1.EmergencyScheduleSpillage.Code);
				AssertEquals("Medical FirstAide Guide of Dangerous Goods (good 2 - dg 1)", "", dangerousGood1.MedicalFirstAidGuide);
				AssertEquals("Radioactive Transport Index of Dangerous Goods (good 2 - dg 1)", 0m, dangerousGood1.RadioactiveTransportIndex);
				AssertEquals("Radioactive Criticality Safety Index of Dangerous Goods (good 2 - dg 1)", 0m, dangerousGood1.RadioactiveCriticalitySafetyIndex);

				//Container 1 - Dangerous Good 2
				var dangerousGood2 = packline2.DangerousGoods.ElementAt(1);
				AssertEquals("Container DGN Index of Dangerous Goods (good 2 - dg 2)", 2, dangerousGood2.ContainerDGNIndex);
				AssertEquals("UNDG Code of Dangerous Goods (good 2 - dg 2)", "0453a", dangerousGood2.Code);
				AssertEquals("UNDG Code of Dangerous Goods (good 2 - dg 2)", "IMO", dangerousGood2.RegulationStandard);
				AssertEquals("Proper Shipping Name of Dangerous Goods (good 2 - dg 2)", "ROCKETS, LINE-THROWING", dangerousGood2.ProperShippingName);
				AssertEquals("Technical name of Dangerous Goods (good 2 - dg 2)", "Ejection Seat", dangerousGood2.TechnicalName);
				AssertEquals("IMOClass of Dangerous Goods (good 2 - dg 2)", "1.4G", dangerousGood2.IMOClass);
				AssertEquals("FlashPoint of Dangerous Goods (good 2 - dg 2)", 670.0m, dangerousGood2.FlashPoint.Value);
				AssertEquals("DGWeight of Dangerous Goods (good 2 - dg 2)", 213m, dangerousGood2.Weight.Value);
				AssertEquals("Pack Count of Dangerous Goods (good 2 - dg 2)", 9, dangerousGood2.Quantity);
				AssertEquals("Pack Type of Dangerous Goods (good 2 - dg 2)", "BOX", dangerousGood2.PackageType.Code);
				AssertEquals("PackingGroup of Dangerous Goods (good 2 - dg 2)", "II", dangerousGood2.PackingGroup);
				AssertEquals("PackedInLimitedQty of Dangerous Goods (good 2 - dg 2)", false, dangerousGood2.PackedInLimitedQuantity);
				AssertEquals("PackedInExceptedQty of Dangerous Goods (good 2 - dg 2)", false, dangerousGood2.PackedInExceptedQuantity);
				AssertEquals("Emergency Schedule Fire Code of Dangerous Goods (good 2 - dg 2)", "F-B", dangerousGood2.EmergencyScheduleFire.Code);
				AssertEquals("Emergency Schedule Spillage Code of Dangerous Goods (good 2 - dg 2)", "S-X", dangerousGood2.EmergencyScheduleSpillage.Code);
				AssertEquals("Medical FirstAide Guide of Dangerous Goods (good 2 - dg 2)", "", dangerousGood2.MedicalFirstAidGuide);
				AssertEquals("Radioactive Transport Index of Dangerous Goods (good 2 - dg 2)", 0m, dangerousGood2.RadioactiveTransportIndex);
				AssertEquals("Radioactive Criticality Safety Index of Dangerous Goods (good 2 - dg 2)", 0m, dangerousGood2.RadioactiveCriticalitySafetyIndex);

				//Container 1 - Packing Line 3
				var packline3 = container1.PackingLines.ElementAt(2);
				AssertEquals("Description (good 3)", "AIR BAG Detailed Description", packline3.GoodsDescription);
				AssertEquals("Packs (good 3)", 3, packline3.Quantity);
				AssertEquals("Weight (good 3)", 2140m, packline3.Weight.Value);
				AssertEquals("Weight UM (good 3)", "KG", packline3.Weight.Unit.Code);
				AssertEquals("Volume (good 3)", 0m, packline3.Volume.Value);
				AssertEquals("Volume UM (good 3)", "M3", packline3.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 3)", 1, packline3.DangerousGoods.Count);

				//Container 1 - Dangerous Good 3
				var dangerousGood3 = packline3.DangerousGoods.ElementAt(0);
				AssertEquals("Container DGN Index of Dangerous Goods (good 3 - dg 1)", 3, dangerousGood3.ContainerDGNIndex);
				AssertEquals("UNDG Code of Dangerous Goods (good 3 - dg 1)", "0503b", dangerousGood3.Code);
				AssertEquals("UNDG Code of Dangerous Goods (good 3 - dg 1)", "IMO", dangerousGood3.RegulationStandard);
				AssertEquals("Proper Shipping Name of Dangerous Goods (good 3 - dg 1)", "AIR BAG MODULES", dangerousGood3.ProperShippingName);
				AssertEquals("Technical name of Dangerous Goods (good 3 - dg 1)", "Airbag Mercedes B", dangerousGood3.TechnicalName);
				AssertEquals("IMOClass of Dangerous Goods (good 3 - dg 1)", "1.4G", dangerousGood3.IMOClass);

				if (activateIsCombustibleForDGItems)
				{
					AssertNull("FlashPoint of Dangerous Goods is null", dangerousGood3.FlashPoint);
				}
				else
				{
					AssertEquals("FlashPoint of Dangerous Goods (good 3 - dg 1)", 210.0m, dangerousGood3.FlashPoint.Value);
				}

				AssertEquals("DGWeight of Dangerous Goods (good 3 - dg 1)", 120m, dangerousGood3.Weight.Value);
				AssertEquals("Pack Count of Dangerous Goods (good 3 - dg 1)", 2, dangerousGood3.Quantity);
				AssertEquals("Pack Type of Dangerous Goods (good 3 - dg 1)", "BOX", dangerousGood3.PackageType.Code);
				AssertEquals("PackingGroup of Dangerous Goods (good 3 - dg 1)", "II", dangerousGood3.PackingGroup);
				AssertEquals("PackedInLimitedQty of Dangerous Goods (good 3 - dg 1)", true, dangerousGood3.PackedInLimitedQuantity);
				AssertEquals("PackedInExceptedQty of Dangerous Goods (good 3 - dg 1)", false, dangerousGood3.PackedInExceptedQuantity);
				AssertEquals("Emergency Schedule Fire Code of Dangerous Goods (good 3 - dg 1)", "F-I", dangerousGood3.EmergencyScheduleFire.Code);
				AssertEquals("Emergency Schedule Spillage Code of Dangerous Goods (good 3 - dg 1)", "S-S", dangerousGood3.EmergencyScheduleSpillage.Code);
				AssertEquals("Medical FirstAide Guide of Dangerous Goods (good 3 - dg 1)", "", dangerousGood3.MedicalFirstAidGuide);
				AssertEquals("Radioactive Transport Index of Dangerous Goods (good 3 - dg 1)", 0m, dangerousGood3.RadioactiveTransportIndex);
				AssertEquals("Radioactive Criticality Safety Index of Dangerous Goods (good 3 - dg 1)", 0m, dangerousGood3.RadioactiveCriticalitySafetyIndex);

				// Container 2
				var container2 = dangerousGoodsNotification.Containers.ElementAt(1);
				AssertEquals("DangerousGoodsNotification container 3 should contain 2 packinglines", 2, container2.PackingLines.Count);
				AssertEquals("DangerousGoodsNotification container 3 Equipment Number MSCU8757656", "MSCU8757656", container2.Number);
				AssertEquals("DangerousGoodsNotification container 3 should contain 6 packs", 6, container2.PackCount);
				AssertEquals("DangerousGoodsNotification container 3 should have DangerousGoods", true, container2.HasDangerousGoods);

				//Container 3 - Packing Line 4
				var packline4 = container2.PackingLines.ElementAt(0);
				AssertEquals("Description (good 4)", "DASHBOARD MERCEDES", packline4.GoodsDescription);
				AssertEquals("Packs (good 4)", 5, packline4.Quantity);
				AssertEquals("Weight (good 4)", 256m, packline4.Weight.Value);
				AssertEquals("Weight UM (good 4)", "KG", packline4.Weight.Unit.Code);
				AssertEquals("Volume (good 4)", 0m, packline4.Volume.Value);
				AssertEquals("Volume UM (good 4)", "M3", packline4.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 4)", 0, packline4.DangerousGoods.Count);

				//Container 2 - Packing Line 5
				var packline5 = container2.PackingLines.ElementAt(1);
				AssertEquals("Description (good 5)", "Experimental Fuel", packline5.GoodsDescription);
				AssertEquals("Packs (good 5)", 1, packline5.Quantity);
				AssertEquals("Weight (good 5)", 56m, packline5.Weight.Value);
				AssertEquals("Weight UM (good 5)", "KG", packline5.Weight.Unit.Code);
				AssertEquals("Volume (good 5)", 0m, packline5.Volume.Value);
				AssertEquals("Volume UM (good 5)", "M3", packline5.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 5)", 1, packline5.DangerousGoods.Count);

				//Container 2 - Dangerous Good 4
				var dangerousGood4 = packline5.DangerousGoods.ElementAt(0);
				AssertEquals("Container DGN Index of Dangerous Goods (good 5 - dg 1)", 1, dangerousGood4.ContainerDGNIndex);
				AssertEquals("UNDG Code of Dangerous Goods (good 5 - dg 1)", "2911b", dangerousGood4.Code);
				AssertEquals("UNDG Code of Dangerous Goods (good 5 - dg 1)", "IMO", dangerousGood4.RegulationStandard);
				AssertEquals("Proper Shipping Name of Dangerous Goods (good 5 - dg 1)", "RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - ARTICLES", dangerousGood4.ProperShippingName);
				AssertEquals("Technical name of Dangerous Goods (good 5 - dg 1)", "Uranium", dangerousGood4.TechnicalName);
				AssertEquals("IMOClass of Dangerous Goods (good 5 - dg 1)", "7", dangerousGood4.IMOClass);

				if (activateIsCombustibleForDGItems)
				{
					AssertNull("FlashPoint of Dangerous Goods is null", dangerousGood4.FlashPoint);
				}
				else
				{
					AssertEquals("FlashPoint of Dangerous Goods (good 5 - dg 1)", 450.0m, dangerousGood4.FlashPoint.Value);
				}

				AssertEquals("DGWeight of Dangerous Goods (good 5 - dg 1)", 55m, dangerousGood4.Weight.Value);
				AssertEquals("Pack Count of Dangerous Goods (good 5 - dg 1)", 12, dangerousGood4.Quantity);
				AssertEquals("Pack Type of Dangerous Goods (good 5 - dg 1)", "BOX", dangerousGood4.PackageType.Code);
				AssertEquals("PackingGroup of Dangerous Goods (good 5 - dg 1)", "II", dangerousGood4.PackingGroup);
				AssertEquals("PackedInLimitedQty of Dangerous Goods (good 5 - dg 1)", false, dangerousGood4.PackedInLimitedQuantity);
				AssertEquals("PackedInExceptedQty of Dangerous Goods (good 5 - dg 1)", false, dangerousGood4.PackedInExceptedQuantity);
				AssertEquals("Emergency Schedule Fire Code of Dangerous Goods (good 5 - dg 1)", "F-I", dangerousGood4.EmergencyScheduleFire.Code);
				AssertEquals("Emergency Schedule Spillage Code of Dangerous Goods (good 5 - dg 1)", "S-S", dangerousGood4.EmergencyScheduleSpillage.Code);
				AssertEquals("Medical FirstAide Guide of Dangerous Goods (good 5 - dg 1)", "", dangerousGood4.MedicalFirstAidGuide);
				AssertEquals("Radioactive Transport Index of Dangerous Goods (good 5 - dg 1)", 0m, dangerousGood4.RadioactiveTransportIndex);
				AssertEquals("Radioactive Criticality Safety Index of Dangerous Goods (good 5 - dg 1)", 0m, dangerousGood4.RadioactiveCriticalitySafetyIndex);
			}
		}

		#region Test Validations

		public void TestMissingContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			var missingContainerErrorMessage = "Container details are required for Dangerous Goods Notification(BE).";
			AssertHasMessageError("ErrorPlaceHolder should have error message indicating missing Container", dangerousGoodsNotification.ErrorPlaceHolderInfo, missingContainerErrorMessage);

			CreateExportTransports(consol, Constants.TransportModes.Sea);
			CreateBasicGoodsAndEquipment(consol);
			dangerousGoodsNotification = builder.Build();

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError("ErrorPlaceHolder should not have error message indicating missing Container", dangerousGoodsNotification.ErrorPlaceHolderInfo, missingContainerErrorMessage);
		}

		public void TestContainerNumberEmpty()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var containerEmptyErrorMessage = "Please enter a Container Number.";
			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var container = dangerousGoodsNotification.Containers.First();
			container.Number = ZString.Empty;

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError(container.NumberInfo, containerEmptyErrorMessage);

			container.Number = "Number";

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError(container.NumberInfo, containerEmptyErrorMessage);
		}

		public void TestDangerousGoodValidation()
		{
			var consol = CreateBasicConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			var dangerousGood = dangerousGoodsNotification.Containers.First().PackingLines.First().DangerousGoods.First();

			var missingUnitOfRadioactivity = "Unit of Radioactivity is required.";
			var missingUnitOfNetExplosiveWeight = "Unit of Net explosive weight is required.";
			var errorMessage = "At most one of the options (Limited Quantity/Excepted Quantity) can be selected.";
			AssertNoMessageError("Dangerous Good PackedInExceptedQuantity should not have error message", dangerousGood.PackedInExceptedQuantityInfo, errorMessage);
			AssertNoMessageError("Dangerous Good PackedInLimitedQuantity should not have error message", dangerousGood.PackedInLimitedQuantityInfo, errorMessage);
			AssertNoMessageError("Dangerous Good Radioactivity Unit Code should not have error message", ((CodeDescription)dangerousGood.Radioactivity.Unit).CodeInfo, missingUnitOfRadioactivity);
			AssertNoMessageError("Dangerous Good Net Explowive Weight Unit Code should not have error message", ((CodeDescription)dangerousGood.NetExplosiveWeight.Unit).CodeInfo, missingUnitOfNetExplosiveWeight);

			dangerousGood.PackedInExceptedQuantity = true;
			dangerousGood.Radioactivity.Value = 12;
			dangerousGood.NetExplosiveWeight.Value = 12;
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError("Dangerous Good PackedInExceptedQuantity should not have error message", dangerousGood.PackedInExceptedQuantityInfo, errorMessage);
			AssertNoMessageError("Dangerous Good PackedInLimitedQuantity should not have error message", dangerousGood.PackedInLimitedQuantityInfo, errorMessage);
			AssertHasMessageError("Dangerous Good Radioactivity Unit Code should have error message", ((CodeDescription)dangerousGood.Radioactivity.Unit).CodeInfo, missingUnitOfRadioactivity);
			AssertHasMessageError("Dangerous Good Net Explosive Weight Unit Code should have error message", ((CodeDescription)dangerousGood.NetExplosiveWeight.Unit).CodeInfo, missingUnitOfNetExplosiveWeight);

			dangerousGood.PackedInExceptedQuantity = false;
			dangerousGood.PackedInLimitedQuantity = true;
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError("Dangerous Good PackedInExceptedQuantity should not have error message", dangerousGood.PackedInExceptedQuantityInfo, errorMessage);
			AssertNoMessageError("Dangerous Good PackedInLimitedQuantity should not have error message", dangerousGood.PackedInLimitedQuantityInfo, errorMessage);

			dangerousGood.PackedInExceptedQuantity = true;
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError("Dangerous Good PackedInExceptedQuantity should have error message", dangerousGood.PackedInExceptedQuantityInfo, errorMessage);
			AssertHasMessageError("Dangerous Good PackedInLimitedQuantity should have error message", dangerousGood.PackedInLimitedQuantityInfo, errorMessage);
		}

		public void TestDangerousGoodTechnicalName()
		{
			var consol = CreateConsol();
			var parameter = new DummyDocDataObjectParameters()
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};
			var errorMessage = "Technical Name is required.";

			var builder = new DangerousGoodsNotificationBuilder(consol, parameter);
			var dangerousGoodsNotification = builder.Build();

			var undg = dangerousGoodsNotification.PackingLines.First(x => x.DangerousGoods.Count > 0).DangerousGoods.First();
			undg.TechnicalName = ZString.Empty;

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError(undg.TechnicalNameInfo, errorMessage);

			undg.TechnicalName = "Technical Name";
			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError(undg.TechnicalNameInfo, errorMessage);
		}

		public void TestDangerousGoodDetails()
		{
			var consol = CreateConsol();
			var parameter = new DummyDocDataObjectParameters()
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};
			var packsErrorMessage = "UNDG Packs is required. (Dangerous Goods > Packs)";
			var packTypeErrorMessage = "UNDG Pack Type is required. (Dangerous Goods > Pack Type)";
			var weightErrorMessage = "UNDG Weight is required. (Dangerous Goods > Weight)";

			var builder = new DangerousGoodsNotificationBuilder(consol, parameter);
			var dangerousGoodsNotification = builder.Build();

			var undg = dangerousGoodsNotification.PackingLines.First(x => x.DangerousGoods.Count > 0).DangerousGoods.First();
			undg.Quantity = ZInt.Zero;
			undg.PackageType.Code = ZString.Empty;
			undg.Weight.Value = ZDecimal.Zero;

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError(undg.QuantityInfo, packsErrorMessage);
			AssertHasMessageError(((CodeDescription)undg.PackageType).CodeInfo, packTypeErrorMessage);
			AssertHasMessageError(((Measurement)undg.Weight).ValueInfo, weightErrorMessage);

			undg.Quantity = 1;
			undg.PackageType.Code = "Code";
			undg.Weight.Value = 1.0M;

			dangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError(undg.QuantityInfo, packsErrorMessage);
			AssertNoMessageError(((CodeDescription)undg.PackageType).CodeInfo, packTypeErrorMessage);
			AssertNoMessageError(((Measurement)undg.Weight).ValueInfo, weightErrorMessage);
		}

		[TestDate(2022, 2, 7)]
		public void TestHandlingDateValidation()
		{
			var consol = CreateConsol();
			var importParameter = new DummyDocDataObjectParameters()
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};
			var exportParameter = new DummyDocDataObjectParameters()
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};

			var unloadingDateErrorMessage = "Unloading Date is required.";
			var loadingDateErrorMessage = "Loading Date is required.";

			var importBuilder = new DangerousGoodsNotificationBuilder(consol, importParameter);
			var importDangerousGoodsNotification = importBuilder.Build();
			var exportBuilder = new DangerousGoodsNotificationBuilder(consol, exportParameter);
			var exportDangerousGoodsNotification = exportBuilder.Build();

			importDangerousGoodsNotification.HandlingDate = ZDateTime.Empty;
			importDangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError(importDangerousGoodsNotification.HandlingDateInfo, unloadingDateErrorMessage);
			exportDangerousGoodsNotification.HandlingDate = ZDateTime.Empty;
			exportDangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertHasMessageError(exportDangerousGoodsNotification.HandlingDateInfo, loadingDateErrorMessage);

			importDangerousGoodsNotification.HandlingDate = ZDateTime.Now;
			importDangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError(importDangerousGoodsNotification.HandlingDateInfo, unloadingDateErrorMessage);
			exportDangerousGoodsNotification.HandlingDate = ZDateTime.Now;
			exportDangerousGoodsNotification.ValidateAllIncludingChildren();
			AssertNoMessageError(exportDangerousGoodsNotification.HandlingDateInfo, loadingDateErrorMessage);
		}

		#endregion

		#region TestPopulateDgnSecurityNumber

		public void TestPopulateDgnSecurityNumber()
		{
			var consol = CreateBasicConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_EXP"
			};
			var builder = new DangerousGoodsNotificationBuilder(consol, parameters);
			var dangerousGoodsNotification = builder.Build();

			AssertEquals("DgnSecurityNumber should be empty when no messages have been sent yet", "", dangerousGoodsNotification.DgnSecurityNumber);

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTimeOffset(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));
			dangerousGoodsNotification = builder.Build();
			AssertEquals("DgnSecurityNumber should be empty when the IFTDGN request was sent", "", dangerousGoodsNotification.DgnSecurityNumber);

			CreateLog(consol,
				Events.InterchangeSent,
				new ZDateTimeOffset(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));
			dangerousGoodsNotification = builder.Build();
			AssertEquals("DgnSecurityNumber should still be empty when the IFTDGN interchange message was sent", "", dangerousGoodsNotification.DgnSecurityNumber);

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTimeOffset(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "Example DGN Security Number"));
			dangerousGoodsNotification = builder.Build();
			AssertEquals("DgnSecurityNumber should contain a DGN Security Number", "Example DGN Security Number", dangerousGoodsNotification.DgnSecurityNumber);

			CreateLog(consol,
				Events.MessageWithdrawCancelRequest,
				new ZDateTimeOffset(2020, 02, 02),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, "CAM - Cancellation because of mistake"));
			dangerousGoodsNotification = builder.Build();
			AssertEquals("DgnSecurityNumber should still contain a DGN Security Number after sending WithdrawCancelRequest", "Example DGN Security Number", dangerousGoodsNotification.DgnSecurityNumber);

			CreateLog(consol,
				Events.MessageWithdrawCancelAccepted,
				new ZDateTimeOffset(2020, 02, 03),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNExport),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, "Example DGN Security Number"));
			dangerousGoodsNotification = builder.Build();
			AssertEquals("DgnSecurityNumber should be empty when Message Cancellation accepted", "", dangerousGoodsNotification.DgnSecurityNumber);
		}

		void CreateLog(ForwardingConsol consol, Event @event, ZDateTimeOffset time, params KeyValuePair<string, string>[] parameters)
		{
			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time, "", parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#endregion TestPopulateDgnSecurityNumber

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "BEDangerousGoodsNotification_IMP"
			};

			var data = new DangerousGoodsNotificationBuilder(consol, parameters).Build();
			AssertEquals(Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			CreateRefVessels();
		}

		#region Implement
		ForwardingConsol CreateConsol(string messageType = "IMPORT", string transportMode = Core.Constants.TransportModes.Road)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateAddresses(consol);

			if (messageType.Equals("IMPORT"))
			{
				CreateImportTransports(consol, transportMode);
			}
			else if (messageType.Equals("EXPORT"))
			{
				CreateExportTransports(consol, transportMode);
			}

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "MSCU1245787";
			container1.JC_DeliveryMode = "CY/CY";
			container1.JC_IsShipperOwned = false;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 1000;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "MSCU8757656";
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_IsShipperOwned = false;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 1000;
			container2.JC_DunnageWeight = 1000;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "MSCU1247856";
			container3.JC_DeliveryMode = "CY/CY";
			container3.JC_IsShipperOwned = false;
			container3.JC_GrossWeightUQ = "KG";
			container3.JC_TareWeight = 1000;
			container3.JC_DunnageWeight = 1000;
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_HouseBill = "S00001816";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
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

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 0;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 1;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 15;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 0;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 4;
			packline2.JL_Calc_ContainerNumber = "MSCU1245787";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline2.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_Standard = "IMO";

			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_IsCombustible = true;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "0453", "a", "IMO").FirstOrDefault();
			if (subs2 == null)
			{
				subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_UNNO = "0453";
				subs2.DG_Variant = "a";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg2 = packline2.UNDGs.AddNew();
			undg2.LinkDefault(subs2);
			undg2.Substance.DG_ExceptedQuantityCode = "E1";
			undg2.Substance.DG_PG = "II";
			undg2.Substance.DG_PSN = "ROCKETS, LINE-THROWING";
			undg2.Substance.DG_EMS = "F-B,S-X";
			undg2.Substance.DG_Class = "1.4G";
			undg2.Substance.DG_Standard = "IMO";

			undg2.DI_TechnicalName = "Ejection Seat";
			undg2.DI_DGFlashPoint = 670.0m;
			undg2.DI_IsCombustible = true;
			undg2.DI_MPMarinePollutant = "";
			undg2.DI_DGVolume = 0m;
			undg2.DI_UnitOfVolume = "M3";
			undg2.DI_DGWeight = 213m;
			undg2.DI_UnitOfWeight = "KG";
			undg2.DI_IsLimitedQuantity = false;
			undg2.DI_PackageCount = 9;
			undg2.DI_F3_NKPackType = "BOX";
			undg2.DI_OC_DGContact = contact.PK;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 2140;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 0;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Description = "AIR BAG";
			packline3.JL_DetailedDescription = "AIR BAG Detailed Description";
			packline3.JL_ContainerPackingOrder = 1;
			packline3.JL_Calc_ContainerNumber = "MSCU1245787";

			var undg3 = packline3.UNDGs.AddNew();
			undg3.LinkDefault(subs);
			undg3.Substance.DG_ExceptedQuantityCode = "E0";
			undg3.Substance.DG_PG = "II";
			undg3.Substance.DG_PSN = "AIR BAG MODULES";
			undg3.Substance.DG_EMS = "F-I,S-S";
			undg3.Substance.DG_Class = "1.4G";
			undg3.Substance.DG_Standard = "IMO";

			undg3.DI_TechnicalName = "Airbag Mercedes B";
			undg3.DI_IMOClass = "1.4G";
			undg3.DI_DGFlashPoint = 210.0m;
			undg3.DI_IsCombustible = false;
			undg3.DI_MPMarinePollutant = "";
			undg3.DI_DGVolume = 0m;
			undg3.DI_UnitOfVolume = "M3";
			undg3.DI_DGWeight = 120m;
			undg3.DI_UnitOfWeight = "KG";
			undg3.DI_IsLimitedQuantity = true;
			undg3.DI_PackageCount = 2;
			undg3.DI_F3_NKPackType = "BOX";
			undg3.DI_OC_DGContact = contact.PK;

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 5;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 256;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 0;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_Description = "DASHBOARD MERCEDES";
			packline4.JL_DetailedDescription = "";
			packline4.JL_ContainerPackingOrder = 2;
			packline4.JL_Calc_ContainerNumber = "MSCU8757656";

			var packline5 = shipment.OuterPackLines.AddNew();
			packline5.JL_PackageCount = 1;
			packline5.JL_F3_NKPackType = "PLT";
			packline5.JL_ActualWeight = 56;
			packline5.JL_ActualWeightUQ = "KG";
			packline5.JL_ActualVolume = 0;
			packline5.JL_ActualVolumeUQ = "M3";
			packline5.JL_Description = "Experimental Fuel";
			packline5.JL_DetailedDescription = "";
			packline5.JL_ContainerPackingOrder = 5;
			packline5.JL_Calc_ContainerNumber = "MSCU8757656";

			var subs3 = UNDGSubstanceLoader.LoadSubstances(Factory, "2911", "b", "IMO").FirstOrDefault();
			if (subs3 == null)
			{
				subs3 = Factory.New<UNDGSubstance>();
				subs3.DG_UNNO = "2911";
				subs3.DG_Variant = "b";
				subs3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg4 = packline5.UNDGs.AddNew();
			undg4.LinkDefault(subs3);
			undg4.Substance.DG_ExceptedQuantityCode = "E0";
			undg4.Substance.DG_SubLabel1 = "SP290";
			undg4.Substance.DG_SubLabel2 = "";
			undg4.Substance.DG_PG = "II";
			undg4.Substance.DG_PSN = "RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - ARTICLES";
			undg4.Substance.DG_EMS = "F-I,S-S";
			undg4.Substance.DG_Class = "7";
			undg4.Substance.DG_Standard = "IMO";

			undg4.DI_TechnicalName = "Uranium";
			undg4.DI_DGFlashPoint = 450.0m;
			undg4.DI_IsCombustible = false;
			undg4.DI_MPMarinePollutant = "M";
			undg4.DI_DGVolume = 0m;
			undg4.DI_UnitOfVolume = "M3";
			undg4.DI_DGWeight = 55m;
			undg4.DI_UnitOfWeight = "KG";
			undg4.DI_IsLimitedQuantity = false;
			undg4.DI_PackageCount = 12;
			undg4.DI_F3_NKPackType = "BOX";
			undg4.DI_OC_DGContact = contact.PK;

			var packline6 = shipment.OuterPackLines.AddNew();
			packline6.JL_PackageCount = 12;
			packline6.JL_F3_NKPackType = "PLT";
			packline6.JL_ActualWeight = 18;
			packline6.JL_ActualWeightUQ = "KG";
			packline6.JL_ActualVolume = 0;
			packline6.JL_ActualVolumeUQ = "M3";
			packline6.JL_Description = "Manual";
			packline6.JL_DetailedDescription = "";
			packline6.JL_ContainerPackingOrder = 6;
			packline6.JL_Calc_ContainerNumber = "MSCU1247856";

			container1.PackLines.Add(packline1);
			container1.PackLines.Add(packline2);
			container1.PackLines.Add(packline3);
			container2.PackLines.Add(packline4);
			container2.PackLines.Add(packline5);
			container3.PackLines.Add(packline6);

			return consol;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BEANR";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_ContactName = "SPIDERMAN";
			sendingForwarderContact.OC_Email = "spider@marvel.com";
			sendingForwarderContact.OC_Phone = "911";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BEANR";
			receivingForwarder.MainAddress.Address1 = "Unit 2";
			receivingForwarder.MainAddress.Address2 = "60 What Lane";
			receivingForwarder.MainAddress.City = "Antwerp";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			receivingForwarderContact.OC_ContactName = "Black Panther";
			receivingForwarderContact.OC_Email = "black_panther@marvel.com";
			receivingForwarderContact.OC_Phone = "112";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.OH_FullName = "I'm Handling the Stuff to be received";
			arrivalCTOAddress.OH_RL_NKClosestPort = "BEANR";
			arrivalCTOAddress.MainAddress.Address1 = "Unit 200";
			arrivalCTOAddress.MainAddress.Address2 = "55 Why Lane";
			arrivalCTOAddress.MainAddress.City = "Antwerp";
			arrivalCTOAddress.MainAddress.Postcode = "2000";
			arrivalCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;
		}

		ForwardingConsol CreateBasicConsol(string messageType = "IMPORT", string transportMode = Core.Constants.TransportModes.Road)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateAddresses(consol);

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "BEANR";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = transportMode;
			transport2.JW_TransportType = Constants.TransportPlanningType.OnForwarding;
			transport2.JW_RL_NKLoadPort = "BEANR";
			transport2.JW_RL_NKDiscPort = "BEWJG";
			transport2.JW_ETD = new ZDateTime(2020, 10, 23, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 25, 15, 55, 00);
			transport2.JW_Vessel = "TRAILER";
			transport2.JW_VoyageFlight = "1-TRU-CK1";

			CreateBasicGoodsAndEquipment(consol);

			return consol;
		}

		void CreateBasicGoodsAndEquipment(ForwardingConsol consol)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "MSCU1245787";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsShipperOwned = false;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP").PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_HouseBill = "S00001816";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
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

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 0;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline1.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_Standard = "IMO";

			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			container.PackLines.Add(packline1);
		}

		void CreateRefVessels()
		{
			var vesselSea = Factory.NewWithValidTestData<RefVessel>();
			vesselSea.RV_Name = "COSCO NEBULA";
			vesselSea.RV_LloydsNumber = "9795622";
			vesselSea.RV_VesselType = "CV";
			vesselSea.RV_RN_NKCountryOfReg = "HK";
			vesselSea.RV_RadioCallSign = "VRRW8";

			var vesselBarge = Factory.NewWithValidTestData<RefVessel>();
			vesselBarge.RV_Name = "MSC POOLSTER";
			vesselBarge.RV_VesselType = "BA";
			vesselBarge.RV_RN_NKCountryOfReg = "BE";
			vesselBarge.RV_RadioCallSign = "OT5325";
		}

		void CreateImportTransports(ForwardingConsol consol, ZString transportMode)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "BEANR";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = transportMode;
			transport2.JW_TransportType = Constants.TransportPlanningType.OnForwarding;
			transport2.JW_RL_NKLoadPort = "BEANR";
			transport2.JW_RL_NKDiscPort = "BEWJG";
			transport2.JW_ETD = new ZDateTime(2020, 10, 23, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 25, 15, 55, 00);
			switch (transportMode)
			{
				case Constants.TransportModes.Road:
					transport2.JW_Vessel = "TRAILER";
					transport2.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Constants.TransportModes.Rail:
					transport2.JW_Vessel = "JOURNEY REF";
					transport2.JW_VoyageFlight = "124575";
					break;
				case Constants.TransportModes.Sea:
					transport2.JW_Vessel = "MSC POOLSTER";
					transport2.JW_VoyageFlight = "124";
					break;
			}
		}

		void CreateExportTransports(ForwardingConsol consol, ZString transportMode)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 2;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "BEANR";
			transport.JW_RL_NKDiscPort = "CNYTN";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = transportMode;
			transport2.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			transport2.JW_RL_NKLoadPort = "BEWJG";
			transport2.JW_RL_NKDiscPort = "BEANR";
			transport2.JW_ETD = new ZDateTime(2020, 10, 2, 11, 43, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 4, 21, 25, 00);
			switch (transportMode)
			{
				case Constants.TransportModes.Road:
					transport2.JW_Vessel = "TRAILER";
					transport2.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Constants.TransportModes.Rail:
					transport2.JW_Vessel = "JOURNEY REF";
					transport2.JW_VoyageFlight = "124575";
					break;
				case Constants.TransportModes.Sea:
					transport2.JW_Vessel = "MSC POOLSTER";
					transport2.JW_VoyageFlight = "124";
					break;
			}
		}
		#endregion
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class ETerminalReleaseManifestBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateFromNewConsolDoesNotThrowException

		[TestDate(2018, 1, 1)]
		public void TestPopulateFromNewConsolDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNoExceptionThrown(() => new ETerminalReleaseManifestBuilder(consol).Build());
		}

		public void TestPopulateFromForwardingConsol()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("SourceType", "ForwardingConsol", eTerminalReleaseManifest.SourceType);
				AssertEquals("SourceID", "C00001234", eTerminalReleaseManifest.SourceID);
				AssertEquals("DocumentName", "eTerminalReleaseManifest", eTerminalReleaseManifest.DocumentName);

				AssertEquals("ShipperReference", "AgentRef001", eTerminalReleaseManifest.ShipperReference);
				AssertEquals("BillOfLadingNumber", "MasterBillNumber001", eTerminalReleaseManifest.BillOfLadingNumber);
				AssertEquals("CarrierBookingReference", "BookingRef001", eTerminalReleaseManifest.CarrierBookingReference);
				AssertEquals("CarrierContractNumber", "CarrierContractNumber001", eTerminalReleaseManifest.CarrierContractNumber);

				AssertEquals("NumberOfOriginals", 11, eTerminalReleaseManifest.NumberOfOriginals);
				AssertEquals("NumberOfCopies", 22, eTerminalReleaseManifest.NumberOfCopies);
				AssertEquals("RequestedDateOfIssue", new ZDateTime(2018, 10, 1), eTerminalReleaseManifest.RequestedDateOfIssue);
				AssertEquals("IsDoorPickup", true, eTerminalReleaseManifest.IsDoorPickup);
				AssertEquals("IsDoorDelivery", false, eTerminalReleaseManifest.IsDoorDelivery);
				AssertEquals("IsFreightCollect", false, eTerminalReleaseManifest.IsFreightCollect);
				AssertEquals("IsFreightPrepaid", true, eTerminalReleaseManifest.IsFreightPrepaid);
				AssertEquals("SpecialInstructions", "consol special instructions", eTerminalReleaseManifest.SpecialInstructions);

				AssertEquals("ReleaseType.Code", "BOL", eTerminalReleaseManifest.ReleaseType.Code);
				AssertEquals("ReleaseType.Description", "BOL Original", eTerminalReleaseManifest.ReleaseType.Description);

				AssertEquals("ContainerMode.Code", "FCL", eTerminalReleaseManifest.ContainerMode.Code);
				AssertEquals("ContainerMode.Description", "Full Container Load", eTerminalReleaseManifest.ContainerMode.Description);

				AssertEquals("PortOfLoading.Code", "CNSHA", eTerminalReleaseManifest.PortOfLoad.Code);
				AssertEquals("PortOfDischarge.Code", "SGSIN", eTerminalReleaseManifest.PortOfDischarge.Code);
				AssertEquals("PlaceOfReceipt.Code", "CNSHA", eTerminalReleaseManifest.PlaceOfReceipt.Code);
				AssertEquals("PlaceOfIssue.Code", "DKAAL", eTerminalReleaseManifest.PlaceOfIssue.Code);
				AssertEquals("PlaceOfDelivery.Code", "SGSIN", eTerminalReleaseManifest.PlaceOfDelivery.Code);
				AssertEquals("FreightPayableAt.Code", "CNSHA", eTerminalReleaseManifest.FreightPayableAt.Code);

				AssertEquals("Transports.Count", 3, eTerminalReleaseManifest.Transports.Count);
				AssertEquals("Vessel", "CHINA HONGKONG Vessel", eTerminalReleaseManifest.Transports.Main.Vessel.Name);
				AssertEquals("Lloyds/IMO", "", eTerminalReleaseManifest.Transports.Main.Vessel.LloydsIMO);
				AssertEquals("Voyage", "111", eTerminalReleaseManifest.Transports.Main.VoyageFlightNumber);

				AssertEquals("Containers.Count", 2, eTerminalReleaseManifest.Containers.Count);

				AssertBookings(eTerminalReleaseManifest);
				AssertContainers(eTerminalReleaseManifest);
				AssertTransports(eTerminalReleaseManifest);
			});

			AssertAddresses(eTerminalReleaseManifest, consol);
		}

		public void TestPopulateFromForwardingConsol_Direct()
		{
			var consol = CreateConsol(true);

			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("NumberOfOriginals", 11, eTerminalReleaseManifest.NumberOfOriginals);
				AssertEquals("NumberOfCopies", 22, eTerminalReleaseManifest.NumberOfCopies);
				AssertEquals("SpecialInstructions", "consol special instructions", eTerminalReleaseManifest.SpecialInstructions);
				AssertEquals("ShipperReference", "BKG00000S1", eTerminalReleaseManifest.ShipperReference);
			});

			AssertAddresses(eTerminalReleaseManifest, consol.Shipments[0]);
		}

		public void TestCountrySpecificHarmonizedCode()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var packLine = eTerminalReleaseManifest.Containers.First().PackingLines.First();
			AssertEquals("HS (SG): 1234", packLine.ImportHarmonizedCode.ToString());

			eTerminalReleaseManifest.PortOfDischarge.Code = "USLAX";
			AssertEquals(string.Empty, packLine.ImportHarmonizedCode.ToString());
		}

		void AssertTransports(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			AssertContainsExactElementsInAnyOrder("Transports",
				new[]
				{
					"CNSHA -> HKHKG",
					"HKHKG -> SGSIN",
					"SGSIN -> SGQPG"
				},
				eTerminalReleaseManifest.Transports.Select(t => $"{t.PortOfLoading.Code} -> {t.PortOfDischarge.Code}"));
		}

		void AssertAddresses(ETerminalReleaseManifest eTerminalReleaseManifest, ForwardingConsol consol)
		{
			AssertionHelper.AssertAddressData(consol.ShippingLineAddress, eTerminalReleaseManifest.Carrier);
			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, eTerminalReleaseManifest.Shipper);
			AssertionHelper.AssertAddressData(consol.ReceivingForwarder, eTerminalReleaseManifest.Consignee);
			AssertionHelper.AssertAddressData(consol.NotifyPartyDocumentaryAddress, eTerminalReleaseManifest.NotifyParty);
			AssertionHelper.AssertAddressData(consol.NotifyParty2DocumentaryAddress, eTerminalReleaseManifest.NotifyParty2);
			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, eTerminalReleaseManifest.Forwarder);
			AssertionHelper.AssertCurrentUserAddressData(eTerminalReleaseManifest.CurrentUser);
		}

		void AssertAddresses(ETerminalReleaseManifest eTerminalReleaseManifest, ForwardingShipment shipment)
		{
			AssertionHelper.AssertAddressData(shipment.ConsignorDocumentaryAddress, eTerminalReleaseManifest.Shipper);
			AssertionHelper.AssertAddressData(shipment.ConsigneeDocumentaryAddress, eTerminalReleaseManifest.Consignee);
			AssertionHelper.AssertAddressData(shipment.NotifyPartyDocumentaryAddress, eTerminalReleaseManifest.NotifyParty);
			AssertionHelper.AssertAddressData(shipment.NotifyParty2DocumentaryAddress, eTerminalReleaseManifest.NotifyParty2);
		}

		void AssertBookings(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			AssertEquals("Bookings.Count", 3, eTerminalReleaseManifest.Bookings.Count);

			var booking1Formatted = ToAssertString(eTerminalReleaseManifest.Bookings.ElementAt(0));
			var booking2Formatted = ToAssertString(eTerminalReleaseManifest.Bookings.ElementAt(1));
			var booking3Formatted = ToAssertString(eTerminalReleaseManifest.Bookings.ElementAt(2));

			AssertMultilineASCIIEquals("booking1",
				@"ExportRef001
   CONT1111111|11|WU8CM2QZNK|
      11 PLT|pack1-1|ExportRef001
         DG SHIPPER NAME
         SG|1234
   CONT2222222|21|DY5OAGEIAG|
      21 PLT|pack2-1|ExportRef001
",
				booking1Formatted);

			AssertMultilineASCIIEquals("booking2",
				@"ExportRef002
   CONT1111111|12|WU8CM2QZNK|
      12 PLT|pack1-2|ExportRef002",
				booking2Formatted);

			AssertMultilineASCIIEquals("booking3",
				@"ShippingOrderNumber002
   CONT2222222|22|DY5OAGEIAG|
      22 PLT|pack2-2|ShippingOrderNumber002",
				booking3Formatted);
		}

		void AssertContainers(ETerminalReleaseManifest eTerminalReleaseManifest)
		{
			AssertEquals("Containsers.Count", 2, eTerminalReleaseManifest.Containers.Count);

			var container1Formatted = eTerminalReleaseManifest.Containers.ElementAt(0).ToAssertString();
			var container2Formatted = eTerminalReleaseManifest.Containers.ElementAt(1).ToAssertString();

			AssertMultilineASCIIEquals("container1",
				@"CONT1111111|23|WU8CM2QZNK|
   11 PLT|pack1-1|ExportRef001
      DG SHIPPER NAME
      SG|1234
   12 PLT|pack1-2|ExportRef002",
				container1Formatted);

			AssertMultilineASCIIEquals("container2",
				@"CONT2222222|43|DY5OAGEIAG|
   21 PLT|pack2-1|ExportRef001
   22 PLT|pack2-2|ShippingOrderNumber002",
				container2Formatted);
		}

		string ToAssertString(IBooking booking, int indentLevel = 0)
		{
			const string newLine = "\r\n";
			var indent = new string(' ', indentLevel * 3);

			var res = string.Concat(indent, booking.BookingNumber);

			var containers = booking
				.Containers
				.Select(container => container.ToAssertString(indentLevel + 1))
				.ToArray();

			return containers.Any()
				? string.Concat(res, newLine, string.Join(newLine, containers))
				: res;
		}

		#region ContainerMode

		public void TestContainerMode()
		{
			var consol = Factory.New<ForwardingConsol>();

			var modeMap = new Dictionary<string, string>
			{
				[Constants.ContainerModes.FCL] = Constants.ContainerModes.FCL,
				[Constants.ContainerModes.Groupage] = Constants.ContainerModes.FCL,
				[Constants.ContainerModes.BuyersConsol] = Constants.ContainerModes.FCL,
				[Constants.ContainerModes.Other] = Constants.ContainerModes.LCL,
				[Constants.ContainerModes.LCL] = Constants.ContainerModes.LCL
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
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertEquals($"Mode mapped {consolMode} -> {expecedContainerMode}", eTerminalReleaseManifest.ContainerMode.Code, expecedContainerMode);
		}

		#endregion

		#endregion

		#region Test Address Validation

		public void TestCarrierSCACIsRequired()
		{
			var carrier = CreateOrgHeader("MSK", "MAERSK");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			const string errorMessage = "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC.";

			AssertHasMessageError(eTerminalReleaseManifest.Carrier.CompanyNameInfo, errorMessage);

			carrier.CustomsCodes.AddNew("CCC", "SUDU", "US");

			eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertNoMessageError(eTerminalReleaseManifest.Carrier.CompanyNameInfo, errorMessage);
		}

		public void TestToOrderIsAllowedForConsignee()
		{
			var consol = Factory.New<ForwardingConsol>();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			const string errorMessage = "Consignee party name and address information is required.";

			AssertHasMessageError(eTerminalReleaseManifest.Consignee.CompanyNameInfo, errorMessage);

			eTerminalReleaseManifest.Consignee.CompanyName = "TO ORDER";
			AssertNoMessageError(eTerminalReleaseManifest.Consignee.CompanyNameInfo, errorMessage);
		}

		public void TestEmptySONumberValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_ExportRefNumber = ZString.Empty;
			packline.Containers.Add(container);

			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			var booking = eTerminalReleaseManifest.Bookings.First();
			AssertHasMessageError(booking.BookingNumberInfo, "Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number).");

			booking.BookingNumber = "111222";
			AssertNoMessageError(booking.BookingNumberInfo, "Shipping Order Number is required. Enter it on Shipment level (Reference Type SLD) or packs (Shipping Order/Shi Lian Dan Number).");
		}

		public void TestNotifyPartyPostcodeValidationForUSImports()
		{
			var usPostcodeErrorMessage = "Notify Party postcode is required for US imports.";

			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ETerminalReleaseManifestBuilder(consol).Build();

			var notifyParty = shippingOrder.NotifyParty;
			AssertNoMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);

			notifyParty.Country.Code = "US";
			AssertHasMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);

			notifyParty.Postcode = "50101";
			AssertNoMessageError(notifyParty.PostcodeInfo, usPostcodeErrorMessage);
		}

		public void TestConsigneeValidation_ToOrder()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shippingOrder = new ETerminalReleaseManifestBuilder(consol).Build();
			var consignee = shippingOrder.Consignee;

			AssertAddressToOrder("Consignee", consignee);
		}

		#endregion

		#region UNDG Contact Validation

		public void TestUNDGContactValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			var packline = eTerminalReleaseManifest.Bookings.First().Containers.First().PackingLines.First();
			var dangerousGood = packline.DangerousGoods.First();

			AssertNoMessageError(dangerousGood.Contact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertNoMessageError(dangerousGood.Contact.PhoneInfo, "Contact Phone is required for dangerous goods.");

			dangerousGood.Contact.FullName = "";
			dangerousGood.Contact.Phone = "";

			AssertHasMessageError(dangerousGood.Contact.FullNameInfo, "Contact Name is required for dangerous goods.");
			AssertHasMessageError(dangerousGood.Contact.PhoneInfo, "Contact Phone is required for dangerous goods.");
		}

		#endregion

		#region Test References Validation

		public void TestUseMasterBillOrCarrierBkgRefAsSONumberValidation()
		{
			var errorMessage = "At least one of these checkboxes must be ticked to indicate which the carrier uses as the master SO#";

			var consol = Factory.New<ForwardingConsol>();
			var eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();

			eTerminalReleaseManifestBuilder.UseBkgRefAsMasterSO = false;
			eTerminalReleaseManifestBuilder.UseMasterBillAsMasterSO = true;

			AssertNoMessageError(eTerminalReleaseManifestBuilder.UseBkgRefAsMasterSOInfo, errorMessage);
			AssertNoMessageError(eTerminalReleaseManifestBuilder.UseMasterBillAsMasterSOInfo, errorMessage);

			eTerminalReleaseManifestBuilder.UseMasterBillAsMasterSO = false;

			AssertHasMessageError(eTerminalReleaseManifestBuilder.UseBkgRefAsMasterSOInfo, errorMessage);
			AssertHasMessageError(eTerminalReleaseManifestBuilder.UseMasterBillAsMasterSOInfo, errorMessage);

			eTerminalReleaseManifestBuilder.UseBkgRefAsMasterSO = true;

			AssertNoMessageError(eTerminalReleaseManifestBuilder.UseBkgRefAsMasterSOInfo, errorMessage);
			AssertNoMessageError(eTerminalReleaseManifestBuilder.UseMasterBillAsMasterSOInfo, errorMessage);
		}

		#endregion

		#region Test Port Validation

		public void TestPlaceAndDateOfIssueValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();

			var placeOfIssue = eTerminalReleaseManifestBuilder.PlaceOfIssue;

			const string placeOfIssueErrorMessage = "Place of Issue is required.";
			const string dateOfIssueErrorMessage = "Date of Issue is required if Place of Issue is entered.";

			AssertHasMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertNoMessageError(eTerminalReleaseManifestBuilder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);

			placeOfIssue.Code = "AUSYD";

			AssertNoMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertHasMessageError(eTerminalReleaseManifestBuilder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);

			eTerminalReleaseManifestBuilder.RequestedDateOfIssue = ZDateTime.Today;

			AssertNoMessageError(placeOfIssue.CodeInfo, placeOfIssueErrorMessage);
			AssertNoMessageError(eTerminalReleaseManifestBuilder.RequestedDateOfIssueInfo, dateOfIssueErrorMessage);
		}

		#endregion

		#region	Test Transports Validation

		public void TestVesselNameAndETDValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();

			var transport = (DocDataObjects.Transport)eTerminalReleaseManifestBuilder.Transports.First();

			var vessel = transport.Vessel;
			AssertionHelper.AssertMessageErrorIfEmpty(transport.Vessel.NameInfo, "Vessel Name is required.");
		}

		public void TestPortOfDischargeValidation()
		{
			const string errorMessage = "Port of Discharge is required.";

			var consol = CreateConsol();
			var eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();

			eTerminalReleaseManifestBuilder.PortOfDischarge.Code = "";
			AssertHasMessageError(eTerminalReleaseManifestBuilder.PortOfDischarge.CodeInfo, errorMessage);

			eTerminalReleaseManifestBuilder.PortOfDischarge.Code = "AUSYD";
			AssertNoMessageError(eTerminalReleaseManifestBuilder.PortOfDischarge.CodeInfo, errorMessage);

			var transport = (DocDataObjects.Transport)eTerminalReleaseManifestBuilder.Transports.First();

			transport.PortOfDischarge.Code = "";
			AssertHasMessageError(transport.PortOfDischarge.CodeInfo, errorMessage);

			transport.PortOfDischarge.Code = "AUSYD";
			AssertNoMessageError(transport.PortOfDischarge.CodeInfo, errorMessage);
		}

		public void TestTransportsPortOfLoadingValidation()
		{
			const string errorMessage = "eTerminal Release Manifest can only be sent when there is at least one sea leg Loading from Ningbo port.";

			var consol = CreateConsol();
			var eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();
			AssertHasMessageError(eTerminalReleaseManifestBuilder.ErrorPlaceHolderInfo, errorMessage);

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 4;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			transport.JW_RL_NKLoadPort = "CNNBO";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_Vessel = "CHINA HONGKONG Vessel";
			transport.JW_VoyageFlight = "444";
			eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();
			AssertHasMessageError(eTerminalReleaseManifestBuilder.ErrorPlaceHolderInfo, errorMessage);

			transport.JW_LegOrder = 4;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "CNSHA";
			eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();
			AssertHasMessageError(eTerminalReleaseManifestBuilder.ErrorPlaceHolderInfo, errorMessage);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "CNNBO";
			eTerminalReleaseManifestBuilder = new ETerminalReleaseManifestBuilder(consol).Build();
			AssertNoMessageError(eTerminalReleaseManifestBuilder.ErrorPlaceHolderInfo, errorMessage);
		}

		#endregion

		#region TestVesselVoyageNameValidation

		public void TestVesselVoyageNameValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertionHelper.AssertMessageErrorIfEmpty(eTerminalReleaseManifest.Transports.Main.Vessel.NameInfo, "Vessel Name is required.");
			AssertionHelper.AssertMessageErrorIfEmpty(eTerminalReleaseManifest.Transports.Main.Vessel.LloydsIMOInfo, "Vessel's Lloyds/IMO is mandatory.");
			AssertionHelper.AssertMessageErrorIfEmpty(eTerminalReleaseManifest.Transports.Main.VoyageFlightNumberInfo, "Voyage is required.");

			var otherTransport = (DocDataObjects.Transport)eTerminalReleaseManifest.Transports.ElementAt(2);
			AssertNoMessageError(otherTransport.Vessel.NameInfo, "Vessel Name is required.");
			AssertNoMessageError(otherTransport.Vessel.LloydsIMOInfo, "Vessel's Lloyds/IMO is mandatory.");
			AssertNoMessageError(otherTransport.VoyageFlightNumberInfo, "Voyage is required.");
		}

		#endregion

		#region Test Containers Validation

		public void TestContainerNumberValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var container = eTerminalReleaseManifest.Bookings.First().Containers.First();
			AssertionHelper.AssertMessageErrorIfEmpty(container.NumberInfo, "Please enter a Container Number.");
			AssertionHelper.AssertMessageErrorIfEmpty(container.SealInfo, "Seal number is mandatory.");
		}

		public void TestContainerISOCodeValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var container = eTerminalReleaseManifest.Bookings.First().Containers.First();
			var errorMessage = "Container type entered does not have a valid ISO code.";
			AssertNoMessageError(container.Type.ISOCodeInfo, errorMessage);

			container.Type.ISOCode = "";
			AssertHasMessageError(container.Type.ISOCodeInfo, errorMessage);
		}

		#endregion

		#region Test Pack Line Validation

		public void TestPackLineValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var packLine = eTerminalReleaseManifest.Containers.First().PackingLines.First();

			var packCountErrorMessage = "Pack count is mandatory.";
			var packWeightErrorMessage = "Pack weight is mandatory.";
			var packGoodsDescriptionErrorMessage = "Goods Description is mandatory.";
			var packMarksAndNumbersErrorMessage = "Marks are mandatory.";
			var harmonisedCodeErrorMessage = "Harmonised Code is required.";

			packLine.Quantity = 0;
			packLine.Weight.Value = 0;
			packLine.GoodsDescription = "";
			packLine.MarksAndNumbers = "";
			packLine.HarmonizedCode.Code = "";

			AssertHasMessageError(packLine.QuantityInfo, packCountErrorMessage);
			AssertHasMessageError(packLine.Weight.ValueInfo, packWeightErrorMessage);
			AssertHasMessageError(packLine.GoodsDescriptionInfo, packGoodsDescriptionErrorMessage);
			AssertHasMessageError(packLine.MarksAndNumbersInfo, packMarksAndNumbersErrorMessage);
			AssertHasMessageError(((HarmonizedCode)packLine.HarmonizedCode).CodeInfo, harmonisedCodeErrorMessage);

			packLine.Quantity = 5;
			packLine.Weight.Value = 10;
			packLine.GoodsDescription = "Some Goods";
			packLine.MarksAndNumbers = "Some Marks";
			packLine.HarmonizedCode.Code = "HS0001";

			AssertNoMessageError(packLine.QuantityInfo, packCountErrorMessage);
			AssertNoMessageError(packLine.Weight.ValueInfo, packWeightErrorMessage);
			AssertNoMessageError(packLine.GoodsDescriptionInfo, packGoodsDescriptionErrorMessage);
			AssertNoMessageError(packLine.MarksAndNumbersInfo, packMarksAndNumbersErrorMessage);
			AssertNoMessageError(((HarmonizedCode)packLine.HarmonizedCode).CodeInfo, harmonisedCodeErrorMessage);
		}

		#endregion

		#region Test Charges Validation

		public void TestOtherChargesValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var otherCharges = eTerminalReleaseManifest.OtherCharges;

			var errorMessage = "At least one type must be selected for Payment Details.";

			otherCharges.IsPrepaid = false;
			otherCharges.IsCollect = false;
			otherCharges.IsFree = false;
			otherCharges.IsPayableElsewhere = false;
			otherCharges.IsFirstLinePrepaidLineSecondCollect = false;
			otherCharges.Remarks = "";

			AssertHasMessageError(otherCharges.IsPrepaidInfo, errorMessage);
			AssertHasMessageError(otherCharges.IsCollectInfo, errorMessage);
			AssertHasMessageError(otherCharges.IsFreeInfo, errorMessage);
			AssertHasMessageError(otherCharges.IsPayableElsewhereInfo, errorMessage);
			AssertHasMessageError(otherCharges.IsFirstLinePrepaidLineSecondCollectInfo, errorMessage);
			AssertHasMessageError(otherCharges.RemarksInfo, errorMessage);

			otherCharges.IsPayableElsewhere = true;

			AssertNoMessageError(otherCharges.IsPrepaidInfo, errorMessage);
			AssertNoMessageError(otherCharges.IsCollectInfo, errorMessage);
			AssertNoMessageError(otherCharges.IsFreeInfo, errorMessage);
			AssertNoMessageError(otherCharges.IsPayableElsewhereInfo, errorMessage);
			AssertNoMessageError(otherCharges.IsFirstLinePrepaidLineSecondCollectInfo, errorMessage);
			AssertNoMessageError(otherCharges.RemarksInfo, errorMessage);
		}

		public void TestPrepaidCollectValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var otherCharges = eTerminalReleaseManifest.OtherCharges;

			var errorMessage = "Either Prepaid or Collect payment type must be selected.";

			eTerminalReleaseManifest.IsFreightPrepaid = true;
			eTerminalReleaseManifest.IsFreightCollect = false;

			AssertNoMessageError(eTerminalReleaseManifest.IsFreightPrepaidInfo, errorMessage);
			AssertNoMessageError(eTerminalReleaseManifest.IsFreightCollectInfo, errorMessage);

			eTerminalReleaseManifest.IsFreightPrepaid = false;

			AssertHasMessageError(eTerminalReleaseManifest.IsFreightPrepaidInfo, errorMessage);
			AssertHasMessageError(eTerminalReleaseManifest.IsFreightCollectInfo, errorMessage);

			eTerminalReleaseManifest.IsFreightCollect = true;

			AssertNoMessageError(eTerminalReleaseManifest.IsFreightPrepaidInfo, errorMessage);
			AssertNoMessageError(eTerminalReleaseManifest.IsFreightCollectInfo, errorMessage);
		}

		public void TestFreightPayableAtValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			var errorMessage = "The location of where freight is paid is required.";

			eTerminalReleaseManifest.FreightPayableAt.Code = "";

			AssertHasMessageError(eTerminalReleaseManifest.FreightPayableAt.CodeInfo, errorMessage);

			eTerminalReleaseManifest.FreightPayableAt.Code = "CNNGB";

			AssertNoMessageError(eTerminalReleaseManifest.FreightPayableAt.CodeInfo, errorMessage);
		}

		public void TestAirVentFlowValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			var bookingContainer = eTerminalReleaseManifest.Bookings.First().Containers.First();
			bookingContainer.Type.Type.Code = Constants.ContainerTypes.Refrigerated;

			bookingContainer.AirVentFlow.Value = 0;
			bookingContainer.AirVentFlow.Unit.Code = "2L";
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, "No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab.");

			bookingContainer.AirVentFlow.Unit.Code = "";
			AssertHasMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, "No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab.");

			foreach (ForwardingContainer container in consol.Containers)
			{
				container.JC_IsNonOperativeReefer = true;
			}
			eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			bookingContainer = eTerminalReleaseManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.AirVentFlow.Unit).CodeInfo, "No Air Vent Setting measurement type exists. Ensure each temperature controlled container has one entered on the Containers > Refrigeration tab.");
		}

		public void TestSetTemperatureValidation()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			var bookingContainer = eTerminalReleaseManifest.Bookings.First().Containers.First();
			bookingContainer.Type.Type.Code = Constants.ContainerTypes.Refrigerated;

			bookingContainer.SetTemperature.Value = 0;
			bookingContainer.SetTemperature.Unit.Code = "C";
			AssertNoMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");

			bookingContainer.SetTemperature.Unit.Code = "";
			AssertHasMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");

			foreach (ForwardingContainer container in consol.Containers)
			{
				container.JC_IsNonOperativeReefer = true;
			}
			eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			bookingContainer = eTerminalReleaseManifest.Bookings.First().Containers.First();
			AssertNoMessageError(((CodeDescription)bookingContainer.SetTemperature.Unit).CodeInfo, "Temperature is required when container is a reefer.");
		}

		#endregion

		#region Test Ascii Characters Validation

		public void TestAsciiCharactersValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNNBO";
			consol.JK_ConsolMode = "FCL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Containers.AddNew();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertAsciiCharactersValidation("Shipper", eTerminalReleaseManifest.Shipper);
				AssertAsciiCharactersValidation("Carrier", eTerminalReleaseManifest.Carrier);
				AssertAsciiCharactersValidation("Consignee", eTerminalReleaseManifest.Consignee);
				AssertAsciiCharactersValidation("NotifyParty", eTerminalReleaseManifest.NotifyParty);
				AssertAsciiCharactersValidation("NotifyParty2", eTerminalReleaseManifest.NotifyParty2);
				AssertAsciiCharactersValidation("Forwarder", eTerminalReleaseManifest.Forwarder);
				AssertAsciiCharactersValidation("CurrentUser", eTerminalReleaseManifest.CurrentUser);

				AssertAsciiCharactersValidation("PortOfLoading", eTerminalReleaseManifest.PortOfLoad);
				AssertAsciiCharactersValidation("PortOfDischarge", eTerminalReleaseManifest.PortOfDischarge);
				AssertAsciiCharactersValidation("PlaceOfReceipt", eTerminalReleaseManifest.PlaceOfReceipt);
				AssertAsciiCharactersValidation("PlaceOfDelivery", eTerminalReleaseManifest.PlaceOfDelivery);
				AssertAsciiCharactersValidation("PlaceOfIssue", eTerminalReleaseManifest.PlaceOfIssue);
				AssertAsciiCharactersValidation("FreightPayableAt", eTerminalReleaseManifest.FreightPayableAt);

				AssertAsciiCharactersValidation("OtherChargesRemarks", eTerminalReleaseManifest.OtherCharges.RemarksInfo);
				AssertAsciiCharactersValidation("Transports[0].VoyageFlightNumber", ((DocDataObjects.Transport)eTerminalReleaseManifest.Transports.ToArray()[0]).VoyageFlightNumberInfo);
				AssertAsciiCharactersValidation("Transports[0].Vessel", ((DocDataObjects.Transport)eTerminalReleaseManifest.Transports.ToArray()[0]).Vessel);
				AssertAsciiCharactersValidation("SpecialInstructions", eTerminalReleaseManifest.SpecialInstructionsInfo);
			});
		}
		#endregion

		#region TestPopulateShipperCompanyName

		public void TestPopulateShipperCompanyName()
		{
			DocDataObjects.Testing.VerifiedGrossMassTest.InitConsolAndShipmentForShipperCompanyNameTest(Factory, out var consol, out _, out var forwarder, out var consignor);

			AssertEquals("Pre-condition", false, consol.IsDirect);

			var builder = new ETerminalReleaseManifestBuilder(consol);
			var eTerminalReleaseManifest = builder.Build();
			AssertEquals($"{forwarder.OH_FullName} {forwarder.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} AAA Lines", eTerminalReleaseManifest.Shipper.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, consol.IsDirect);

			eTerminalReleaseManifest = builder.Build();
			AssertEquals($"{consignor.OH_FullName} {consignor.MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} BBB Lines", eTerminalReleaseManifest.Shipper.CompanyName);
		}

		#endregion

		#region TestPorts

		public void TestPorts()
		{
			var consol = CreateConsol();
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertEquals("PlaceOfIssue", "DKAAL|Aalborg", eTerminalReleaseManifest.PlaceOfIssue.ToAssertString());
			AssertEquals("PortOfLoad", "CNSHA|Shanghai Hongqiao International Apt", eTerminalReleaseManifest.PortOfLoad.ToAssertString());
			AssertEquals("PortOfDischarge", "SGSIN|Singapore", eTerminalReleaseManifest.PortOfDischarge.ToAssertString());
			AssertEquals("PlaceOfReceipt", "CNSHA|Shanghai Hongqiao International Apt", eTerminalReleaseManifest.PlaceOfReceipt.ToAssertString());
			AssertEquals("PlaceOfDelivery", "SGSIN|Singapore", eTerminalReleaseManifest.PlaceOfDelivery.ToAssertString());
			AssertEquals("OperationalPort", "CNSHA|Shanghai Hongqiao International Apt", eTerminalReleaseManifest.OperationalPort.ToAssertString());
		}

		#endregion

		#region TestSameContainerAndBookingHaveMultiplePackines

		public void TestSameContainerAndBookingHaveMultiplePackines()
		{
			#region setup

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "BRRIO";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CNT0001";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 10;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 100;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 100;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_HarmonisedCode = "";
			packline1.JL_ExportRefNumber = "REF001";
			packline1.JL_DetailedDescription = "pack1";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 20;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 200;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 200;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_HarmonisedCode = "";
			packline2.JL_ExportRefNumber = "REF002";
			packline2.JL_DetailedDescription = "pack2";

			container.PackLines.Add(packline1);
			container.PackLines.Add(packline2);

			#endregion

			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertEquals(2, eTerminalReleaseManifest.Bookings.Count);

			var expectedPackCounts = new ZInt[] { 10, 20 };
			var containers = eTerminalReleaseManifest.Bookings.SelectMany(a => a.Containers).OfType<BookingContainer>();

			AssertContainsExactElementsInAnyOrder(
				"Each Booking should have it's own PackCount based on container packlines",
				expectedPackCounts,
				containers.Select(c => c.PackCount)
			);

			AssertEquals(
				"Each Booking container should have a unique ID",
				2,
				containers.Select(c => c.Identifier).Distinct().Count()
			);

			AssertEquals(
				"When wrapping the containers in dynamic data, it should still have unique identifiers",
				2,
				containers.Select(c => c.MakeDocDataDynamic().Value.As<BookingContainer>().Identifier).Distinct().Count()
			);
		}

		#endregion

		#region PopulateContact

		public void TestPoplulateContactInformation()
		{
			var consol = CreateConsol(false, true);
			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();

			AssertEquals("Contact Name sendingForwarder should be SPIDERMAN", "SPIDERMAN", eTerminalReleaseManifest.Shipper.Contact);
			AssertEquals("Email sendingForwarder should be spider@marvel.com", "spider@marvel.com", eTerminalReleaseManifest.Shipper.Email);
			AssertEquals("Phone sendingForwarder should be 911", "911", eTerminalReleaseManifest.Shipper.Phone);

			AssertEquals("Contact Name receivingForwarder should be Black Panther", "Black Panther", eTerminalReleaseManifest.Consignee.Contact);
			AssertEquals("Email receivingForwarder should be black_panther@marvel.com", "black_panther@marvel.com", eTerminalReleaseManifest.Consignee.Email);
			AssertEquals("Phone receivingForwarder should be 112", "112", eTerminalReleaseManifest.Consignee.Phone);

			AssertEquals("Contact Name sendingForwarder should be SPIDERMAN", "SPIDERMAN", eTerminalReleaseManifest.Forwarder.Contact);
			AssertEquals("Email sendingForwarder should be spider@marvel.com", "spider@marvel.com", eTerminalReleaseManifest.Forwarder.Email);
			AssertEquals("Phone sendingForwarder should be 911", "911", eTerminalReleaseManifest.Forwarder.Phone);
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var eTerminalReleaseManifest = new ETerminalReleaseManifestBuilder(consol).Build();
			AssertEquals(Constants.ContainerModes.FCL, eTerminalReleaseManifest.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, eTerminalReleaseManifest.ContainerMode.Description);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol(bool isDirect = false, bool includeContactDetail = false)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = isDirect ? Constants.AgentType.Direct : Constants.AgentType.Agent;
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_NoOriginalBills = 11;
			consol.JK_NoCopyBills = 22;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_BookingReference = "BookingRef001";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);
			consol.JK_MasterBillNum = "MasterBillNumber001";
			consol.JK_AgentsReference = "AgentRef001";
			consol.JK_UniqueConsignRef = "C00001234";
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_CarrierContractNumber = "CarrierContractNumber001";

			PopulateAddresses(consol, includeContactDetail);

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_Vessel = "CHINA HONGKONG Vessel";
			transport.JW_VoyageFlight = "111";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "HKHKG";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_Vessel = "HONG KONG SINGAPORE VESSEL";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Road;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "SGSIN";
			transport3.JW_RL_NKDiscPort = "SGQPG";

			var consolInstruction = consol.Notes.AddNew();
			consolInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			consolInstruction.ST_NoteText = "consol special instructions";

			#region Containers

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_ISOType = "22P1";

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_ISOType = "40RE";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";
			container1.JC_DeliveryMode = "CFS/CY";
			container1.JC_IsShipperOwned = true;
			container1.JC_GrossWeightUQ = "KG";
			container1.JC_TareWeight = 1000;
			container1.JC_DunnageWeight = 1000;
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";
			container2.JC_DeliveryMode = "CY/CY";
			container2.JC_IsShipperOwned = false;
			container2.JC_GrossWeightUQ = "KG";
			container2.JC_TareWeight = 2000;
			container2.JC_DunnageWeight = 2000;
			container2.JC_RC = refContainer2.PK;
			container2.JC_IsControlledAtmosphere = true;
			container2.JC_SetPointTemp = -18.0m;
			container2.JC_SetPointTempUnit = "C";
			container2.JC_HumidityPercent = 50;
			container2.JC_AirVentFlow = 90m;
			container2.JC_AirVentFlowRateUnit = AirFlowRateUnits.Codes.Percent;

			#endregion

			#region Shipment1

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S001000011";
			shipment1.JS_HouseBill = "HBL 001";
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment1.JS_RL_NKOrigin = "CNSHA";
			shipment1.JS_RL_NKDestination = "SGSIN";
			shipment1.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment1.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment1.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment1.JS_ShippedOnBoard = "SHP";
			shipment1.JS_ShippedOnBoardDate = ZDate.Today;
			shipment1.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment1.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment1.JS_GoodsDescription = "goods description S1";
			shipment1.JS_MarksAndNumbers = "marks & numbers S1";
			shipment1.JS_BookingReference = "BKG00000S1";
			shipment1.JS_NoOriginalBills = 3;
			shipment1.JS_NoCopyBills = 3;

			PopulateShipmentAddresses(shipment1);

			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var shipmentInstruction = shipment1.Notes.AddNew();
			shipmentInstruction.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			shipmentInstruction.ST_NoteText = "shipment special instructions";

			var number = shipment1.Numbers.AddNew();
			number.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
			number.CE_RN_NKCountryCode = Constants.CountryCodes.China;
			number.CE_EntryNum = "ShippingOrderNumber001";

			var packline11 = shipment1.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 11;
			packline11.JL_F3_NKPackType = "PLT";
			packline11.JL_ActualWeight = 111;
			packline11.JL_ActualWeightUQ = "KG";
			packline11.JL_ActualVolume = 311;
			packline11.JL_ActualVolumeUQ = "M3";
			packline11.JL_HarmonisedCode = "HS11";
			packline11.JL_DetailedDescription = "pack1-1";
			packline11.JL_ExportRefNumber = "ExportRef001";

			var hc = Factory.New<JobPackLineHarmonisedCode>();
			hc.JLH_RN_NKCountry = "SG";
			hc.JLH_Code = "1234";

			packline11.HarmonisedCodes.Add(hc);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "6666";
			subs.DG_Variant = "E";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = packline11.UNDGs.AddNew();
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

			var packline12 = shipment1.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 12;
			packline12.JL_F3_NKPackType = "PLT";
			packline12.JL_ActualWeight = 112;
			packline12.JL_ActualWeightUQ = "KG";
			packline12.JL_ActualVolume = 312;
			packline12.JL_ActualVolumeUQ = "M3";
			packline12.JL_HarmonisedCode = "HS12";
			packline12.JL_DetailedDescription = "pack1-2";
			packline12.JL_ExportRefNumber = "ExportRef002";

			container1.PackLines.Add(packline11);
			container1.PackLines.Add(packline12);

			#endregion

			#region Shipment2

			if (!isDirect)
			{
				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S001000022";
				shipment2.JS_HouseBill = "HBL 002";
				shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment2.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
				shipment2.JS_RL_NKOrigin = "CNSHA";
				shipment2.JS_RL_NKDestination = "SGSIN";
				shipment2.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
				shipment2.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CY;
				shipment2.JS_INCO = Constants.IncoTerms.CostAndFreight;
				shipment2.JS_ShippedOnBoard = "SHP";
				shipment2.JS_ShippedOnBoardDate = ZDate.Today;
				shipment2.JS_E_DEP = ZDate.Today.AddDays(1);
				shipment2.JS_E_ARV = ZDate.Today.AddDays(2);
				shipment2.JS_GoodsDescription = "goods description S2";
				shipment2.JS_MarksAndNumbers = "marks & numbers S2";
				shipment2.JS_BookingReference = "BKG00000S2";

				shipment2.OuterPackLines.RemoveAndDeleteAll();

				var num1 = shipment2.Numbers.AddNew();
				num1.CE_EntryType = ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber;
				num1.CE_EntryNum = "ShippingOrderNumber002";

				var packline21 = shipment2.OuterPackLines.AddNew();
				packline21.JL_PackageCount = 21;
				packline21.JL_F3_NKPackType = "PLT";
				packline21.JL_ActualWeight = 121;
				packline21.JL_ActualWeightUQ = "KG";
				packline21.JL_ActualVolume = 321;
				packline21.JL_ActualVolumeUQ = "M3";
				packline21.JL_HarmonisedCode = "HS21";
				packline21.JL_DetailedDescription = "pack2-1";
				packline21.JL_ExportRefNumber = "ExportRef001";

				var packline22 = shipment2.OuterPackLines.AddNew();
				packline22.JL_PackageCount = 22;
				packline22.JL_F3_NKPackType = "PLT";
				packline22.JL_ActualWeight = 122;
				packline22.JL_ActualWeightUQ = "KG";
				packline22.JL_ActualVolume = 322;
				packline22.JL_ActualVolumeUQ = "M3";
				packline22.JL_HarmonisedCode = "HS22";
				packline22.JL_DetailedDescription = "pack2-2";

				container2.PackLines.Add(packline21);
				container2.PackLines.Add(packline22);
			}

			#endregion

			Factory.Save();

			return consol;
		}

		void PopulateAddresses(ForwardingConsol consol, bool includeContactDetail)
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
			if (includeContactDetail)
			{
				var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
				sendingForwarderContact.OC_ContactName = "SPIDERMAN";
				sendingForwarderContact.OC_Email = "spider@marvel.com";
				sendingForwarderContact.OC_Phone = "911";
			}
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			if (includeContactDetail)
			{
				var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
				receivingForwarderContact.OC_ContactName = "Black Panther";
				receivingForwarderContact.OC_Email = "black_panther@marvel.com";
				receivingForwarderContact.OC_Phone = "112";
			}
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "I'm Notifying 1";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying 2";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2023";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
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

		#endregion
	}
}

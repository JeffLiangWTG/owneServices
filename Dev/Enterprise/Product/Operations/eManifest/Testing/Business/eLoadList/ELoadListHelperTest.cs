namespace Enterprise.eManifest.Testing.Business
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.eManifest.Business;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	public class ELoadListHelperTest : TestCaseWithFactory
	{
		#region FromELoadList

		public void TestCreateConsol_eLoadListIsNull_ThrowArgumentNullException()
		{
			var eLoadListHelper = new ELoadListHelper();

			AssertExceptionThrown<ArgumentNullException>(() => eLoadListHelper.CreateConsol(null));
		}

		public void TestCreateConsol_eLoadListIsSpecified_GenerateConsol()
		{
			var originDepot = Factory.NewWithValidTestData<OrgHeader>().WithPort("UAIEV");
			var originAddress1 = originDepot.Addresses.AddNew().WithPort("UAIEV").WithAddress("111");
			var originAddress2 = originDepot.Addresses.AddNew().WithPort("USLAX").WithAddress("222");

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>().WithPort("AUSYD");
			var destinationAddress1 = destinationDepot.Addresses.AddNew().WithPort("AUMEL").WithAddress("333");
			var destinationAddress2 = destinationDepot.Addresses.AddNew().WithPort("AUSYD").WithAddress("444");

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_IsActive = true;
			sendingAgent.OH_IsForwarder = true;

			var sendingAppointedPort = sendingAgent.AppointedAgentPorts.AddNew();
			sendingAppointedPort.O5_AgentDirection = "BTH";
			sendingAppointedPort.O5_AirAgentStatus = "PUB";
			sendingAppointedPort.O5_PortOrCountry = "USLAX";
			sendingAppointedPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_IsActive = true;
			receivingAgent.OH_IsForwarder = true;

			var receivingAppointedPort = receivingAgent.AppointedAgentPorts.AddNew();
			receivingAppointedPort.O5_AgentDirection = "BTH";
			receivingAppointedPort.O5_AirAgentStatus = "PUB";
			receivingAppointedPort.O5_PortOrCountry = "AUMEL";
			receivingAppointedPort.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;

			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_TransportMode = Constants.TransportModes.Air;
			eLoadList.DO_OA_OriginDepot = originAddress2.PK;
			eLoadList.DO_OA_DestinationDepot = destinationAddress1.PK;
			eLoadList.DO_E_ARV = new ZDateTime(2013, 06, 22);
			eLoadList.DO_E_DEP = new ZDateTime(2013, 06, 24);
			eLoadList.DO_VoyageFlight = "SV333";
			eLoadList.DO_RV_NKVessel = "Titanic";
			eLoadList.DO_MasterBillNumber = "MCLAREN";

			Factory.Save();

			var eLoadListHelperToTest = new ELoadListHelper();

			var consol = (ForwardingConsol)eLoadListHelperToTest.CreateConsol(eLoadList);
			AssertEquals("JK_TransportMode", Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals("JK_OA_PackDepotAddress", originAddress2.PK, consol.JK_OA_PackDepotAddress);
			AssertEquals("JK_OA_UnpackDepotAddress", destinationAddress1.PK, consol.JK_OA_UnpackDepotAddress);
			AssertEquals("JK_OA_SendingForwarderAddress", sendingAgent.MainAddress.PK, consol.JK_OA_SendingForwarderAddress);
			AssertEquals("JK_OA_ReceivingForwarderAddress", receivingAgent.MainAddress.PK, consol.JK_OA_ReceivingForwarderAddress);
			AssertEquals("JK_RL_NKDischargePort", "AUMEL", consol.JK_RL_NKDischargePort);
			AssertEquals("JK_RL_NKLoadPort", "USLAX", consol.JK_RL_NKLoadPort);
			AssertEquals("JK_ConsolMode", Constants.ContainerModes.Loose, consol.JK_ConsolMode);

			AssertEquals("Transports", 1, consol.Transports.Count);
			AssertEquals("transport.JW_ETA", new ZDateTime(2013, 06, 22), consol.Transports[0].JW_ETA);
			AssertEquals("transport.JW_ETD", new ZDateTime(2013, 06, 24), consol.Transports[0].JW_ETD);
			AssertEquals("transport.JW_VoyageFlight", "SV333", consol.Transports[0].JW_VoyageFlight);
			AssertEquals("transport.JW_Vessel", "Titanic", consol.Transports[0].JW_Vessel);
			AssertEquals("JW_RL_NKDiscPort", "AUMEL", consol.Transports[0].JW_RL_NKDiscPort);
			AssertEquals("JW_RL_NKLoadPort", "USLAX", consol.Transports[0].JW_RL_NKLoadPort);

			eLoadList.DO_TransportMode = Constants.TransportModes.Sea;
			consol = (ForwardingConsol)eLoadListHelperToTest.CreateConsol(eLoadList);
			AssertEquals("JK_ConsolMode", Constants.ContainerModes.Groupage, consol.JK_ConsolMode);
		}

		public void TestCreateConsol_CreateCarrierFromELoadList()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsActive = true;

			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_TransportMode = Constants.TransportModes.Sea;
			eLoadList.DO_OH_Carrier = carrier.PK;

			Factory.Save();

			var eLoadListHelperToTest = new ELoadListHelper();
			var consol = eLoadListHelperToTest.CreateConsol(eLoadList) as ForwardingConsol;

			AssertEquals(carrier.PK, consol.ShippingLine.PK);
		}

		public void TestCreateConsol_CarrierIsObtainedFromFlightDetails()
		{
			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_TransportMode = Constants.TransportModes.Air;
			eLoadList.DO_VoyageFlight = "SV333";

			Factory.Save();

			Assert("Prerequisite", eLoadList.DO_OH_Carrier.IsEmpty);

			OrgHeader svCarrier = Factory.New<OrgHeader>();
			svCarrier.OH_IsShippingLine = true;
			svCarrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirline2LetterCode(Factory, "SV").PK;

			var eLoadListHelperToTest = new ELoadListHelper();
			var consol = (ForwardingConsol)eLoadListHelperToTest.CreateConsol(eLoadList);

			AssertEquals("Carrier should be defaulted from Airline", svCarrier.PK, consol.ShippingLine.PK);
		}

		#endregion

		#region eLoadListContainsSendingAgentWithCFSAsRelatedParty

		public void TestCreateConsol_eLoadListContainsSendingAgentWithCFSAsRelatedParty_PopulateCFSFromELoadList()
		{
			var originDepot = Factory.NewWithValidTestData<OrgHeader>().WithPort("UAIEV");
			var originAddress1 = originDepot.Addresses.AddNew().WithPort("USLAX").WithAddress("222");

			var sendingForwarderRelatedParty = Factory.NewWithValidTestData<OrgHeader>().WithPort("USLAX");

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_IsActive = true;
			sendingAgent.OH_IsForwarder = true;

			sendingAgent.AddRelatedParty(sendingForwarderRelatedParty.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			var sendingAppointedPort = sendingAgent.AppointedAgentPorts.AddNew();
			sendingAppointedPort.O5_AgentDirection = "BTH";
			sendingAppointedPort.O5_AirAgentStatus = "PUB";
			sendingAppointedPort.O5_PortOrCountry = "USLAX";
			sendingAppointedPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;

			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_TransportMode = Constants.TransportModes.Air;
			eLoadList.DO_OA_OriginDepot = originAddress1.PK;

			Factory.Save();

			var eLoadListHelperToTest = new ELoadListHelper();

			var consol = (ForwardingConsol)eLoadListHelperToTest.CreateConsol(eLoadList);
			AssertEquals("JK_OA_PackDepotAddress", originAddress1.PK, consol.JK_OA_PackDepotAddress);
		}

		#endregion

		#region eLoadListContainsReceivingAgentWithCFSAsRelatedParty

		public void TestTestCreateConsol_eLoadListContainsReceivingAgentWithCFSAsRelatedParty_PopulateCFSFromELoadList()
		{
			var receivingForwarderRelatedParty = Factory.NewWithValidTestData<OrgHeader>().WithPort("AUSYD");

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>().WithPort("AUSYD");
			var destinationAddress = destinationDepot.Addresses.AddNew().WithPort("AUMEL").WithAddress("333");

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_IsActive = true;
			receivingAgent.OH_IsForwarder = true;

			receivingAgent.AddRelatedParty(receivingForwarderRelatedParty.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);

			var receivingAppointedPort = receivingAgent.AppointedAgentPorts.AddNew();
			receivingAppointedPort.O5_AgentDirection = "BTH";
			receivingAppointedPort.O5_AirAgentStatus = "PUB";
			receivingAppointedPort.O5_PortOrCountry = "AUMEL";
			receivingAppointedPort.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;

			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_TransportMode = Constants.TransportModes.Air;
			eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;

			Factory.Save();

			var eLoadListHelperToTest = new ELoadListHelper();

			var consol = (ForwardingConsol)eLoadListHelperToTest.CreateConsol(eLoadList);
			AssertEquals("JK_OA_UnpackDepotAddress", destinationAddress.PK, consol.JK_OA_UnpackDepotAddress);
		}

		#endregion

		#region AttachELoadLists

		public void TestAttachELoadLists_ConsolIsNull_ThrowArgumentNullException()
		{
			var eLoadList = Factory.New<ELoadList>();
			var eLoadListHelperToTest = new ELoadListHelper();

			AssertExceptionThrown<ArgumentNullException>(() => eLoadListHelperToTest.AttachELoadLists(null, new[] { eLoadList }));
		}

		public void TestAttachToConsol_eLoadListsCollectionIsNull_ThrowArgumentNullException()
		{
			var consol = Factory.New<ForwardingConsol>();
			var eLoadListHelperToTest = new ELoadListHelper();

			AssertExceptionThrown<ArgumentNullException>(() => eLoadListHelperToTest.AttachELoadLists(consol, null));
		}

		public void TestAttachToConsol_ShipmentsAreCreatedFromBookingHeadersGroupedByConsignorAddress()
		{
			var eLoadList1 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList1.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList1.DO_UniqueReference = "KADUMHCIVOKUNAY";

			var eLoadList2 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList2.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList2.DO_UniqueReference = "LEZOKVORAZA";

			var consignorA = Factory.NewWithValidTestData<OrgHeader>();
			var consignorB = Factory.NewWithValidTestData<OrgHeader>();
			var consignorC = Factory.NewWithValidTestData<OrgHeader>();
			var bookedByOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var consignmentBroker = Factory.NewWithValidTestData<OrgHeader>();

			var bookedBy = Factory.NewWithValidTestData<OrgContact>();
			bookedBy.OC_OH = bookedByOrganisation.PK;
			bookedBy.OC_ContactName = "Test Contact";

			var bookingHeader1 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader1.DH_OA_Consignor = consignorA.MainAddress.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader2.DH_OA_Consignor = consignorA.MainAddress.PK;
			bookingHeader2.DH_OH_ConsignmentBroker = consignmentBroker.PK;

			var bookingHeader3 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader3.DH_OA_Consignor = consignorB.MainAddress.PK;

			var bookingHeader4 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader4.DH_OA_Consignor = consignorA.MainAddress.PK;

			var bookingHeader5 = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader5.DH_OA_Consignor = consignorC.MainAddress.PK;
			bookingHeader5.DH_OC_BookedBy = bookedBy.PK;

			var line11 = Factory.NewWithValidTestData<SupplierBookingLine>();
			line11.DL_DH_BookingHeader = bookingHeader1.PK;
			line11.DL_DO_LoadList = eLoadList1.PK;

			var line21 = Factory.NewWithValidTestData<SupplierBookingLine>();
			line21.DL_DH_BookingHeader = bookingHeader2.PK;
			line21.DL_DO_LoadList = eLoadList1.PK;

			var line31 = Factory.NewWithValidTestData<SupplierBookingLine>();
			line31.DL_DH_BookingHeader = bookingHeader3.PK;
			line31.DL_DO_LoadList = eLoadList1.PK;

			var line42 = Factory.NewWithValidTestData<SupplierBookingLine>();
			line42.DL_DH_BookingHeader = bookingHeader4.PK;
			line42.DL_DO_LoadList = eLoadList2.PK;

			var line52 = Factory.NewWithValidTestData<SupplierBookingLine>();
			line52.DL_DH_BookingHeader = bookingHeader5.PK;
			line52.DL_DO_LoadList = eLoadList2.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;

			Factory.Save();

			var eLoadLists = new[] { eLoadList1, eLoadList2 };
			new ELoadListHelper().AttachELoadLists(consol, eLoadLists);

			var attachedShipments = consol.Shipments.Cast<ForwardingShipment>().ToList();
			AssertEquals("Shipments should be attached", 3, attachedShipments.Count);

			Assert("HLS shipments created", attachedShipments.All(shipment => shipment.IsHighVolumeLowValueLegacy));
			Assert("Goods description", attachedShipments.All(shipment => shipment.JS_GoodsDescription == "Various Cargo"));

			Assert("TransportMode on shipments should be the same as on consol",
				attachedShipments.All(shipment => shipment.JS_TransportMode == Constants.TransportModes.Air));

			Assert("ContainerType on shipments should be LSE when transport mode is Air",
				attachedShipments.All(shipment => shipment.JS_PackingMode == Constants.ContainerModes.Loose));

			Assert("Consignee's address on shipments should be the same as consol's unpack depot address",
				attachedShipments.All(shipment => shipment.ConsigneeDocumentaryAddress.E2_OA_Address == consol.JK_OA_UnpackDepotAddress));

			Assert("All eLoadLists must be marked as consolidated", eLoadLists.All(e => e.DO_Status == Constants.ELoadListStatuses.Consolidated));

			var eventLog1 = consol.Logs.MostRecentLogByEventTime(
				Events.ELoadListConsolidated,
				string.Format("eLoadList {0} has been consolidated", "KADUMHCIVOKUNAY"));

			var eventLog2 = consol.Logs.MostRecentLogByEventTime(
				Events.ELoadListConsolidated,
				string.Format("eLoadList {0} has been consolidated", "LEZOKVORAZA"));

			AssertNotNull("ELoadListConsolidated event for KADUMHCIVOKUNAY", eventLog1);
			AssertNotNull("ELoadListConsolidated event for LEZOKVORAZA", eventLog2);

			var shipment1 = attachedShipments.First(shipment => shipment.ConsignorPK == consignorA.PK);
			Assert("Lines attached to shipment", new[] { line11, line21, line42 }.All(line => line.DL_JS_ApprovedShipment == shipment1.PK));
			AssertEquals("PickupAgent was set from ConsignmentBroker", consignmentBroker.PK, shipment1.PickupAgent.PK);

			var shipment2 = attachedShipments.First(shipment => shipment.ConsignorPK == consignorB.PK);
			Assert("Lines attached to shipment", new[] { line31 }.All(line => line.DL_JS_ApprovedShipment == shipment2.PK));

			var shipment3 = attachedShipments.First(shipment => shipment.ConsignorPK == consignorC.PK);
			Assert("Lines attached to shipment", new[] { line52 }.All(line => line.DL_JS_ApprovedShipment == shipment3.PK));
			AssertEquals("Booking Party Documentary Address was set from BookedBy", bookedByOrganisation.MainAddress.PK, shipment3.BookingPartyDocumentaryAddress.E2_OA_Address);
			AssertEquals("Contact about Booking Party Documentary Address was set from Header With BookedBy", bookedBy.OC_ContactName, shipment3.BookingPartyDocumentaryAddress.E2_Contact);
		}

		public void TestAttachToConsol_AttachContainersAndPackLines()
		{
			#region Currencies

			"AUD".SetSellRate(Factory, 1);
			"USD".SetSellRate(Factory, 2);
			"EUR".SetSellRate(Factory, 4);
			"UAH".SetSellRate(Factory, 5);

			#endregion

			#region eLoadLists

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "UAIEV";

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			var destinationAddress = destinationDepot.Addresses.AddNew().WithPort("UAIEV").WithAddress("222");

			var eLoadList1 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList1.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList1.DO_ContainerNumber = "PER";
			eLoadList1.DO_OA_DestinationDepot = destinationAddress.PK;

			var eLoadList2 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList2.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList2.DO_ContainerNumber = "BUT";

			var line1 = bookingHeader.CreateLine(eLoadList1, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");
			var line2 = bookingHeader.CreateLine(eLoadList1, 176.573m, Constants.Volume.CubicFeet, 10000, Constants.Weight.Grams, 10, "EUR");
			var line3 = bookingHeader.CreateLine(eLoadList2, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "USD");
			var line4 = bookingHeader.CreateLine(eLoadList2, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "USD");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			consol.JK_OA_UnpackDepotAddress = consignee.MainAddress.PK;

			#endregion

			Factory.Save();

			var eLoadLists = new[] { eLoadList1, eLoadList2 };
			var eLoadListHelperToTest = new ELoadListHelper();
			eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

			AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);

			var actualContainers = consol.Containers.Cast<CommonContainer>().Select(c => c.JC_ContainerNum.ToString());
			AssertContainsExactElementsInAnyOrder("Containers on consol", new[] { "PER", "BUT" }, actualContainers);

			var shipment = (CommonShipment)consol.Shipments.First();
			AssertEquals("Packlines on shipment", 2, shipment.OuterPackLines.Count);

			var container = consol.Containers.Cast<CommonContainer>().FirstOrDefault(c => c.JC_ContainerNum == "PER");
			var packline = shipment.OuterPackLines.Cast<PackLine>().FirstOrDefault(p => p.JL_JC == container.PK);
			AssertNotNull("Packline for PER container", container);
			AssertEquals("packLine.JL_ActualVolume", 10m, Utilities.Round(packline.JL_ActualVolume, 1));
			AssertEquals("packLine.JL_ActualVolumeUQ", Constants.Volume.CubicMetres, packline.JL_ActualVolumeUQ);
			AssertEquals("packLine.JL_ActualWeight", 20m, Utilities.Round(packline.JL_ActualWeight, 1));
			AssertEquals("packLine.JL_ActualWeightUQ", Constants.Weight.Kilograms, packline.JL_ActualWeightUQ);
			AssertEquals("shipment.JS_ActualVolume", 20m, Utilities.Round(shipment.JS_ActualVolume, 1));
			AssertEquals("shipment.JL_ActualWeight", 40m, Utilities.Round(shipment.JS_ActualWeight, 1));
			AssertEquals("shipment.JS_OuterPacks", 4, shipment.JS_OuterPacks);
			AssertEquals("shipment.JS_F3_NKPackType", Constants.PkgUnit.Package, shipment.JS_F3_NKPackType);

			// Different currencies
			AssertEquals("shipment.JS_RX_NKGoodsValueCurr", "UAH", shipment.JS_RX_NKGoodsValueCurr);
			AssertEquals("shipment.JS_GoodsValue", 112.5m, shipment.JS_GoodsValue);

			// Single currency
			consol.Shipments.RemoveAndDeleteAll();
			line1.DL_RX_NKGoodsValueCurrency = "EUR";
			line2.DL_RX_NKGoodsValueCurrency = "EUR";
			line3.DL_RX_NKGoodsValueCurrency = "EUR";
			line4.DL_RX_NKGoodsValueCurrency = "EUR";

			eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

			shipment = (CommonShipment)consol.Shipments.First();
			AssertEquals("shipment.JS_RX_NKGoodsValueCurr", "EUR", shipment.JS_RX_NKGoodsValueCurr);
			AssertEquals("shipment.JS_GoodsValue", 40m, shipment.JS_GoodsValue);
		}

		public void TestAttachToConsol_OriginAndDestinationOnShipmentArePopulatedFromeLoadList()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "AUSYD";
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "UAIEV";
			var agent = Factory.NewWithValidTestData<OrgHeader>();

			var originDepot = Factory.NewWithValidTestData<OrgHeader>();
			var originAddress = originDepot.Addresses.AddNew().WithPort("NZAKL").WithAddress("111");

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			var destinationAddress = destinationDepot.Addresses.AddNew().WithPort("AUMEL").WithAddress("222");

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();

			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList.DO_OA_OriginDepot = originAddress.PK;
			eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
			var line1 = bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_PackDepotAddress = consignor.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = consignee.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;

			Factory.Save();

			var eLoadLists = new[] { eLoadList };
			var eLoadListHelperToTest = new ELoadListHelper();
			eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

			AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
			AssertEquals("JS_RL_NKOrigin", "NZAKL", consol.Shipments[0].JS_RL_NKOrigin);
			AssertEquals("JS_RL_NKDestination", "AUMEL", consol.Shipments[0].JS_RL_NKDestination);
			AssertEquals("Shipment’s consignee set from consol’s receiving agent", agent.MainAddress.PK, consol.Shipments[0].ConsigneeDocumentaryAddress.E2_OA_Address);
		}

		public void TestAttachToConsol_CreateJobHeaderForShipment()
		{
			var originDepot = Factory.NewWithValidTestData<OrgHeader>();
			var originAddress = originDepot.Addresses.AddNew().WithPort("SGSIN").WithAddress("111");

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			var destinationAddress = destinationDepot.Addresses.AddNew().WithPort("AUMEL").WithAddress("222");

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_OA_OriginDepot = originAddress.PK;
			eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
			bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchSYD = company.Branches.AddNew();
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchMEL = company.Branches.AddNew();
			branchMEL.GB_Code = "AUM";
			branchMEL.GB_RL_NKHomePort = "AUMEL";

			var branchAKL = company.Branches.AddNew();
			branchAKL.GB_Code = "NZA";
			branchAKL.GB_RL_NKHomePort = "NZAKL";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchSYD.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (new ZArchitecture.Environment.User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var eLoadLists = new[] { eLoadList };
				var eLoadListHelperToTest = new ELoadListHelper();
				eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);
				AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
				AssertNotNull(consol.Shipments[0].Job);
				AssertEquals("should change the branch accroding to the unloco of destination", branchMEL.PK, consol.Shipments[0].Job.JH_GB);

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				destinationAddress.OA_RL_NKRelatedPortCode = "NZBBB";
				eLoadList = Factory.NewWithValidTestData<ELoadList>();
				eLoadList.DO_OA_OriginDepot = originAddress.PK;
				eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
				bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");
				Factory.Save();
				eLoadLists = new[] { eLoadList };
				eLoadListHelperToTest = new ELoadListHelper();
				eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

				AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
				AssertEquals("should change the branch accroding to the country of destination", branchAKL.PK, consol.Shipments[0].Job.JH_GB);

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				eLoadList = Factory.NewWithValidTestData<ELoadList>();
				eLoadList.DO_OA_OriginDepot = originAddress.PK;
				eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
				bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");
				Factory.Save();
				var rule = new ImportBranchRule()
				{
					UseBranchFromXml = true,
					DefaultToBranchRelatedToOriginLoadPort = 0,
					DefaultToBranchRelatedToDestinationDischargePort = 0,
					FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate
				};

				SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
				eLoadLists = new[] { eLoadList };
				eLoadListHelperToTest = new ELoadListHelper();
				eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

				AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
				AssertNull("Can NOT create JobHeader as registry is \"Not Create\"", consol.Shipments[0].Job);

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				eLoadList = Factory.NewWithValidTestData<ELoadList>();
				eLoadList.DO_OA_OriginDepot = originAddress.PK;
				eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
				bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");
				Factory.Save();
				rule = new ImportBranchRule()
				{
					UseBranchFromXml = true,
					DefaultToBranchRelatedToOriginLoadPort = 0,
					DefaultToBranchRelatedToDestinationDischargePort = 0,
					FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny
				};

				SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
				eLoadLists = new[] { eLoadList };
				eLoadListHelperToTest = new ELoadListHelper();
				eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

				AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
				AssertEquals("default to the current branch as registry is \"Default to Any\"", branchSYD.PK, consol.Shipments[0].Job.JH_GB);

				Factory.Save();
				rule = new ImportBranchRule()
				{
					UseBranchFromXml = true,
					DefaultToBranchRelatedToOriginLoadPort = 0,
					DefaultToBranchRelatedToDestinationDischargePort = 0,
					FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate
				};

				SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

				consol = Factory.NewWithValidTestData<ForwardingConsol>();
				destinationAddress.OA_RL_NKRelatedPortCode = "CNSHA";
				eLoadList = Factory.NewWithValidTestData<ELoadList>();
				eLoadList.DO_OA_OriginDepot = originAddress.PK;
				eLoadList.DO_OA_DestinationDepot = destinationAddress.PK;
				bookingHeader.CreateLine(eLoadList, 5, Constants.Volume.CubicMetres, 10, Constants.Weight.Kilograms, 10, "AUD");
				Factory.Save();
				eLoadLists = new[] { eLoadList };
				eLoadListHelperToTest = new ELoadListHelper();
				eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

				AssertEquals("Shipments should be attached", 1, consol.Shipments.Count);
				AssertNull("Can NOT create JobHeader as the branch are NOT matched", consol.Shipments[0].Job);
			}
		}

		public void TestAttachToConcol_SetDefaultIncoterm()
		{
			var eLoadList = Factory.NewWithValidTestData<ELoadList>();
			eLoadList.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList.DO_UniqueReference = "KADUMHCIVOKUNAY";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var bookingHeader = Factory.NewWithValidTestData<SupplierBookingHeader>();
			bookingHeader.DH_OA_Consignor = consignor.MainAddress.PK;

			var line = Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_DH_BookingHeader = bookingHeader.PK;
			line.DL_DO_LoadList = eLoadList.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			Factory.Save();

			var eLoadLists = new[] { eLoadList };
			new ELoadListHelper().AttachELoadLists(consol, eLoadLists);

			var attachedShipments = consol.Shipments.Cast<ForwardingShipment>().ToList();
			AssertEquals("Shipments should be attached", 1, attachedShipments.Count);
			Assert("Default incoterm should be delivered duty paid", attachedShipments.All(shipment => shipment.JS_INCO == Constants.IncoTerms.DeliveredDutyPaid));
		}

		public void TestAttachToConcol_SetContainerType()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.ULD;

			var refContainer20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var eLoadList1 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList1.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList1.DO_ContainerNumber = "C00001";
			eLoadList1.DO_RC_ContainerType = refContainer20GP.PK;

			var eLoadList2 = Factory.NewWithValidTestData<ELoadList>();
			eLoadList2.DO_Status = Constants.ELoadListStatuses.Lodged;
			eLoadList2.DO_ContainerNumber = "C00002";
			eLoadList2.DO_RC_ContainerType = refContainer40GP.PK;

			Factory.Save();

			var eLoadLists = new[] { eLoadList1, eLoadList2 };
			var eLoadListHelperToTest = new ELoadListHelper();
			eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);

			var container20GP = consol.Containers.Cast<CommonContainer>().FirstOrDefault(c => c.JC_ContainerNum == "C00001");
			var container40GP = consol.Containers.Cast<CommonContainer>().FirstOrDefault(c => c.JC_ContainerNum == "C00002");

			AssertEquals(2, consol.Containers.Count);
			AssertEquals(refContainer20GP.PK, container20GP.JC_RC);
			AssertEquals(Constants.ContainerModes.Groupage, container20GP.JC_ContainerMode);
			AssertEquals(refContainer40GP.PK, container40GP.JC_RC);
			AssertEquals(Constants.ContainerModes.Groupage, container20GP.JC_ContainerMode);

			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Containers.RemoveAll();

			eLoadListHelperToTest.AttachELoadLists(consol, eLoadLists);
			Assert(consol.Containers.Cast<CommonContainer>().All(container => container.JC_ContainerMode == Constants.ContainerModes.ULD));
		}

		#endregion
	}
}

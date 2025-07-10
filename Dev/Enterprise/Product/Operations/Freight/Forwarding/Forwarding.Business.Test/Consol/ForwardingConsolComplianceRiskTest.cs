using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardingConsolComplianceRiskTest : ComplianceRiskBusinessObjectTestCase
	{
		public void TestCompliancePartiesAndCountries()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertNotNull("Precondition: Consol should be IComplianceItemRiskStatusProvider", consol);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKFirstForeignPort = "ADALV";
			consol.JK_RL_NKLastForeignPort = "AEAJP";
			consol.JK_RL_NKPortOfFirstArrival = "AFASH";
			consol.JK_RL_NKLoadPort = "FIAAI";
			consol.JK_RL_NKDischargePort = "FJBFJ";

			var container = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "USORD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "FKFBE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "FMEAU";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "TestVessel1";
			transport1.JW_Vessel = vessel1.RV_Code;
			var transportCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCarrier1.OH_RL_NKClosestPort = "AGANU";
			transport1.CarrierPK = transportCarrier1.PK;
			var transportCreditor1 = Factory.NewWithValidTestData<OrgHeader>();
			transportCreditor1.OH_RL_NKClosestPort = "AIAXA";
			transport1.CreditorPK = transportCreditor1.PK;
			var departOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			departOrg1.OH_RL_NKClosestPort = "ALARG";
			transport1.JW_OA_DepartureLocation = departOrg1.MainAddress.PK;
			var arriveOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			arriveOrg1.OH_RL_NKClosestPort = "AMARG";
			transport1.JW_OA_ArrivalLocation = arriveOrg1.MainAddress.PK;
			transport1.JW_IsLinked = true;
			transport1.JW_RL_NKLoadPort = "AOANL";
			transport1.JW_RL_NKDiscPort = "AQBEL";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Road;
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Code = "TestVessel2";
			transport2.JW_Vessel = vessel2.RV_Code;
			var transportCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCarrier2.OH_RL_NKClosestPort = "ARABA";
			transport2.CarrierPK = transportCarrier2.PK;
			var transportCreditor2 = Factory.NewWithValidTestData<OrgHeader>();
			transportCreditor2.OH_RL_NKClosestPort = "ASACC";
			transport2.CreditorPK = transportCreditor2.PK;
			var departOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			departOrg2.OH_RL_NKClosestPort = "ATA9Z";
			transport2.JW_OA_DepartureLocation = departOrg2.MainAddress.PK;
			var arriveOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			arriveOrg2.OH_RL_NKClosestPort = "AUALO";
			transport2.JW_OA_ArrivalLocation = arriveOrg2.MainAddress.PK;
			transport2.JW_IsLinked = false;
			transport2.JW_RL_NKLoadPort = "AQBEL";
			transport2.JW_RL_NKDiscPort = "AWBAR";

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_RL_NKClosestPort = "AZKAZ";
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_RL_NKClosestPort = "BABIR";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "BBSML";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_RL_NKClosestPort = "BDAKH";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			departureCTO.OH_RL_NKClosestPort = "BE2HW";
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var packDepot = Factory.NewWithValidTestData<OrgHeader>();
			packDepot.OH_RL_NKClosestPort = "BFABD";
			consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;

			var emptyPickup = Factory.NewWithValidTestData<OrgHeader>();
			emptyPickup.OH_RL_NKClosestPort = "BGAIO";
			consol.JK_OA_ContainerYardEmptyPickupAddress = emptyPickup.MainAddress.PK;

			var departurePortTransport = Factory.NewWithValidTestData<OrgHeader>();
			departurePortTransport.OH_RL_NKClosestPort = "BHADA";
			consol.JK_OA_DeparturePackCFSTransportAddress = departurePortTransport.MainAddress.PK;

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCTO.OH_RL_NKClosestPort = "BIBBZ";
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			var unpackDepot = Factory.NewWithValidTestData<OrgHeader>();
			unpackDepot.OH_RL_NKClosestPort = "BJAGT";
			consol.JK_OA_UnpackDepotAddress = unpackDepot.MainAddress.PK;

			var emptyReturn = Factory.NewWithValidTestData<OrgHeader>();
			emptyReturn.OH_RL_NKClosestPort = "BM5PE";
			consol.JK_OA_ContainerYardEmptyReturnAddress = emptyReturn.MainAddress.PK;

			var arrivalPortTransport = Factory.NewWithValidTestData<OrgHeader>();
			arrivalPortTransport.OH_RL_NKClosestPort = "BNTAS";
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalPortTransport.MainAddress.PK;

			var container1 = consol.Containers.AddNew();
			var departureContainerYard1 = Factory.NewWithValidTestData<OrgHeader>();
			departureContainerYard1.OH_RL_NKClosestPort = "BOBJO";
			container1.JC_Calc_DepartureContainerYardAddressOrg = departureContainerYard1.PK;
			var arrivalContainerYard1 = Factory.NewWithValidTestData<OrgHeader>();
			arrivalContainerYard1.OH_RL_NKClosestPort = "BQDTK";
			container1.JC_Calc_ArrivalContainerYardAddressOrg = arrivalContainerYard1.PK;

			var container2 = consol.Containers.AddNew();
			var departureContainerYard2 = Factory.NewWithValidTestData<OrgHeader>();
			departureContainerYard2.OH_RL_NKClosestPort = "BRAAI";
			container2.JC_Calc_DepartureContainerYardAddressOrg = departureContainerYard2.PK;
			var arrivalContainerYard2 = Factory.NewWithValidTestData<OrgHeader>();
			arrivalContainerYard2.OH_RL_NKClosestPort = "BSASD";
			container2.JC_Calc_ArrivalContainerYardAddressOrg = arrivalContainerYard2.PK;

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(container.PK);
			var whs1 = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			var whsLoc1 = (IWhsLocation)helper.CreateRowAndGenerateLocations(whs1, "R1").Locations[0];
			AssertNotEquals("Precondition", ZGuid.Empty, whsLoc1.WLV_WW_Whs);
			var whsOrg1 = (OrgHeader)whs1.WarehouseAddress.Header;
			whsOrg1.OH_RL_NKClosestPort = "BTPBH";
			whsLoc1.WLV_WA_PutawayArea = helper.CreateWhsArea(whs1.PK, "A1").PK;
			var packLoc1 = packLine1.PackLocations.AddNew();
			packLoc1.JQ_WL = whsLoc1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(container.PK);
			var whs2 = (IWhsWarehouse)helper.CreateWarehouse("WH2");
			var whsLoc2 = (IWhsLocation)helper.CreateRowAndGenerateLocations(whs2, "R2").Locations[0];
			AssertNotEquals("Precondition", ZGuid.Empty, whsLoc2.WLV_WW_Whs);
			var whsOrg2 = (OrgHeader)whs2.WarehouseAddress.Header;
			whsOrg2.OH_RL_NKClosestPort = "BWBBK";
			whsLoc2.WLV_WA_PutawayArea = helper.CreateWhsArea(whs2.PK, "A2").PK;
			var packLoc2 = packLine2.PackLocations.AddNew();
			packLoc2.JQ_WL = whsLoc2.PK;

			var contractor1a = Factory.NewWithValidTestData<OrgHeader>();
			contractor1a.OH_RL_NKClosestPort = "CACAL";
			var service1a = container1.Services.AddNew();
			service1a.ES_OH_Contractor = contractor1a.PK;

			var contractor1b = Factory.NewWithValidTestData<OrgHeader>();
			contractor1b.OH_RL_NKClosestPort = "CCCCK";
			var service1b = container1.Services.AddNew();
			service1b.ES_OH_Contractor = contractor1b.PK;

			var contractor2a = Factory.NewWithValidTestData<OrgHeader>();
			contractor2a.OH_RL_NKClosestPort = "CDANG";
			var service2a = container2.Services.AddNew();
			service2a.ES_OH_Contractor = contractor2a.PK;

			var contractor2b = Factory.NewWithValidTestData<OrgHeader>();
			contractor2b.OH_RL_NKClosestPort = "CFAIG";
			var service2b = container2.Services.AddNew();
			service2b.ES_OH_Contractor = contractor2b.PK;

			var serviceLocationOrg1a = Factory.NewWithValidTestData<OrgHeader>();
			serviceLocationOrg1a.OH_RL_NKClosestPort = "CGANJ";
			service1a.ES_OA_Location = serviceLocationOrg1a.MainAddress.PK;

			var serviceLocationOrg1b = Factory.NewWithValidTestData<OrgHeader>();
			serviceLocationOrg1b.OH_RL_NKClosestPort = "CHADO";
			service1b.ES_OA_Location = serviceLocationOrg1b.MainAddress.PK;

			var serviceLocationOrg2a = Factory.NewWithValidTestData<OrgHeader>();
			serviceLocationOrg2a.OH_RL_NKClosestPort = "CIABO";
			service2a.ES_OA_Location = serviceLocationOrg2a.MainAddress.PK;

			var serviceLocationOrg2b = Factory.NewWithValidTestData<OrgHeader>();
			serviceLocationOrg2b.OH_RL_NKClosestPort = "CKARU";
			service2b.ES_OA_Location = serviceLocationOrg2b.MainAddress.PK;

			var consolCostCreditorOrg = Factory.NewWithValidTestData<OrgHeader>();
			consolCostCreditorOrg.OH_RL_NKClosestPort = "CLABB";
			CreateConsolCost(consol, consolCostCreditorOrg);
			AssertEquals("Precondition", true, consol.HasConsolCosts(GlbCompany.CurrentCompany));

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_RL_NKClosestPort = "DEBES";
			consol.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			var notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_RL_NKClosestPort = "DJCLA";
			consol.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty.PK;

			var notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.OH_RL_NKClosestPort = "DK2DH";
			consol.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty.PK;

			var masterBillIssuingParty = Factory.NewWithValidTestData<OrgHeader>();
			masterBillIssuingParty.OH_RL_NKClosestPort = "DMBEL";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = masterBillIssuingParty.PK;

			var carrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			carrierBookingAgent.OH_RL_NKClosestPort = "DOAZU";
			consol.CarrierBookingAgentDocumentaryAddress.OrganisationPK = carrierBookingAgent.PK;

			var masterBillShipperOverride = Factory.NewWithValidTestData<OrgHeader>();
			masterBillShipperOverride.OH_RL_NKClosestPort = "DZBMR";
			consol.MasterBillShipperOverrideDocumentaryAddress.OrganisationPK = masterBillShipperOverride.PK;

			var masterBillConsigneeOverride = Factory.NewWithValidTestData<OrgHeader>();
			masterBillConsigneeOverride.OH_RL_NKClosestPort = "ECARE";
			consol.MasterBillConsigneeOverrideDocumentaryAddress.OrganisationPK = masterBillConsigneeOverride.PK;

			var carrierExportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			carrierExportCreditor.OH_RL_NKClosestPort = "EEAAR";
			consol.CarrierExportCreditorAddress.OrganisationPK = carrierExportCreditor.PK;

			var carrierImportCreditor = Factory.NewWithValidTestData<OrgHeader>();
			carrierImportCreditor.OH_RL_NKClosestPort = "EGABE";
			consol.CarrierImportCreditorAddress.OrganisationPK = carrierImportCreditor.PK;

			Factory.Save();

			var parties = ((ICompliancePartyRiskStatusProvider)consol).Parties.ToArray();
			var countries = ((IComplianceLocationRiskStatusProvider)consol).Locations.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsVessel(transport1, parties);
				AssertContainsVessel(transport2, parties, false);

				AssertContainsComplianceParty("Sending Agent", sendingAgent, parties);
				AssertContainsComplianceParty("Receiving Agent", receivingAgent, parties);
				AssertContainsComplianceParty("Carrier", carrier, parties);
				AssertContainsComplianceParty("Creditor", creditor, parties);
				AssertContainsComplianceParty("Departure CTO", departureCTO, parties);
				AssertContainsComplianceParty("Pack Depot", packDepot, parties);
				AssertContainsComplianceParty("Pickup Container Yard", emptyPickup, parties);
				AssertContainsComplianceParty("Departure Port Transport", departurePortTransport, parties);
				AssertContainsComplianceParty("Arrival CTO", arrivalCTO, parties);
				AssertContainsComplianceParty("Unpack Depot", unpackDepot, parties);
				AssertContainsComplianceParty("Return Container Yard", emptyReturn, parties);
				AssertContainsComplianceParty("Arrival Port Transport", arrivalPortTransport, parties);
				AssertContainsComplianceParty("Carrier", transportCarrier1, parties);
				AssertContainsComplianceParty("Carrier", transportCarrier2, parties);
				AssertContainsComplianceParty("Creditor", transportCreditor1, parties);
				AssertContainsComplianceParty("Creditor", transportCreditor2, parties);
				AssertContainsComplianceParty("Depart From", departOrg1, parties);
				AssertContainsComplianceParty("Depart From", departOrg2, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg1, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg2, parties);
				AssertContainsComplianceParty("Empty Pickup From", departureContainerYard1, parties);
				AssertContainsComplianceParty("Empty Return To", arrivalContainerYard1, parties);
				AssertContainsComplianceParty("Warehouse", whsOrg1, parties);
				AssertContainsComplianceParty("Warehouse", whsOrg2, parties);
				AssertContainsComplianceParty("Contractor", contractor1a, parties);
				AssertContainsComplianceParty("Contractor", contractor1b, parties);
				AssertContainsComplianceParty("Contractor", contractor2a, parties);
				AssertContainsComplianceParty("Contractor", contractor2b, parties);
				AssertContainsComplianceParty("Service Location", serviceLocationOrg1a, parties);
				AssertContainsComplianceParty("Service Location", serviceLocationOrg1b, parties);
				AssertContainsComplianceParty("Service Location", serviceLocationOrg2a, parties);
				AssertContainsComplianceParty("Service Location", serviceLocationOrg2b, parties);
				AssertContainsComplianceParty("Consol Costing Creditor", consolCostCreditorOrg, parties);

				AssertContainsComplianceParty("Consignor Documentary Address", consignor, parties);
				AssertContainsComplianceParty("Consignee Documentary Address", consignee, parties);

				AssertContainsComplianceCountry("First Load Country", consol.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Last Discharge Country", consol.DischargePort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", transport1.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Discharge Country", transport1.DiscPort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", transport2.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Discharge Country", transport2.DiscPort.Country, countries);
				AssertContainsComplianceCountry("First Foreign Country", consol.FirstForeignPort.Country, countries);
				AssertContainsComplianceCountry("Last Foreign Country", consol.LastForeignPort.Country, countries);
				AssertContainsComplianceCountry("First Arrival Country", consol.PortOfFirstArrival.Country, countries);
				AssertContainsComplianceCountry("Sending Agent", sendingAgent.Country, countries);
				AssertContainsComplianceCountry("Receiving Agent", receivingAgent.Country, countries);
				AssertContainsComplianceCountry("Carrier", carrier.Country, countries);
				AssertContainsComplianceCountry("Creditor", creditor.Country, countries);
				AssertContainsComplianceCountry("Departure CTO", departureCTO.Country, countries);
				AssertContainsComplianceCountry("Pack Depot", packDepot.Country, countries);
				AssertContainsComplianceCountry("Pickup Container Yard", emptyPickup.Country, countries);
				AssertContainsComplianceCountry("Departure Port Transport", departurePortTransport.Country, countries);
				AssertContainsComplianceCountry("Arrival CTO", arrivalCTO.Country, countries);
				AssertContainsComplianceCountry("Unpack Depot", unpackDepot.Country, countries);
				AssertContainsComplianceCountry("Return Container Yard", emptyReturn.Country, countries);
				AssertContainsComplianceCountry("Arrival Port Transport", arrivalPortTransport.Country, countries);
				AssertContainsComplianceCountry("Carrier", transportCarrier1.Country, countries);
				AssertContainsComplianceCountry("Carrier", transportCarrier2.Country, countries);
				AssertContainsComplianceCountry("Creditor", transportCreditor1.Country, countries);
				AssertContainsComplianceCountry("Creditor", transportCreditor2.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg1.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg2.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg1.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg2.Country, countries);
				AssertContainsComplianceCountry("Empty Pickup From", departureContainerYard1.Country, countries);
				AssertContainsComplianceCountry("Empty Return To", arrivalContainerYard1.Country, countries);
				AssertContainsComplianceCountry("Warehouse", whsOrg1.Country, countries);
				AssertContainsComplianceCountry("Warehouse", whsOrg2.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor1a.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor1b.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor2a.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor2b.Country, countries);
				AssertContainsComplianceCountry("Service Location", serviceLocationOrg1a.Country, countries);
				AssertContainsComplianceCountry("Service Location", serviceLocationOrg1b.Country, countries);
				AssertContainsComplianceCountry("Service Location", serviceLocationOrg2a.Country, countries);
				AssertContainsComplianceCountry("Service Location", serviceLocationOrg2b.Country, countries);
				AssertContainsComplianceCountry("Consol Costing Creditor", consolCostCreditorOrg.Country, countries);

				AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
				AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);
				AssertContainsComplianceCountry("Origin Country", shipment.Origin.Country, countries);

				AssertEquals(0, consol.AWBHeaderManager.Count);
				AssertEquals(0, shipment.AWBHeaderManager.Count);
			});
		}

		public void TestComplianceWhenOverrideAddressesCountryIncludedInLocationRisk()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_RL_NKFirstForeignPort = "ADALV";
			consol.JK_RL_NKLastForeignPort = "AEAJP";
			consol.JK_RL_NKPortOfFirstArrival = "AFASH";
			consol.JK_RL_NKLoadPort = "FIAAI";
			consol.JK_RL_NKDischargePort = "FJBFJ";

			var shipment = consol.Shipments.AddNew();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "FKFBE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "FMEAU";

			var countries = ((IComplianceLocationRiskStatusProvider)consol).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "GB";

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "IR";

			countries = ((IComplianceLocationRiskStatusProvider)consol).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", shipment.ConsignorDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", shipment.ConsigneeDocumentaryAddress.Country, countries);
		}

		public void TestComplianceWhenOverrideAddressesCountryInAWBIncludedInLocationRisk()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKFirstForeignPort = "ADALV";
			consol.JK_RL_NKLastForeignPort = "AEAJP";
			consol.JK_RL_NKPortOfFirstArrival = "AFASH";
			consol.JK_RL_NKLoadPort = "FIAAI";
			consol.JK_RL_NKDischargePort = "FJBFJ";

			var shipment = consol.Shipments.AddNew();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "FKFBE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "FMEAU";

			var notifyOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyOrg.PK;
			notifyOrg.OH_RL_NKClosestPort = "AUSYD";

			var countries = ((IComplianceLocationRiskStatusProvider)consol).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);
			AssertContainsComplianceCountry("Notify Party", notifyOrg.Country, countries);

			var consigneeCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "BD");
			var shipperCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "BB");
			var notifyCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CU");

			AssertNotContainsComplianceCountry("Consignee Overridden Country", consigneeCountry, countries);
			AssertNotContainsComplianceCountry("Shipper Overridden Country", shipperCountry, countries);
			AssertNotContainsComplianceCountry("Notify Overridden Country", notifyCountry, countries);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "GB";

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "IR";

			consol.AWBHeader.EH_IsConsigneeOverriden = true;
			consol.AWBHeader.EH_ConsigneeCountryCode = "BD";
			consol.AWBHeader.EH_IsShipperOverriden = true;
			consol.AWBHeader.EH_ShipperCountryCode = "BB";
			consol.AWBHeader.EH_IsNotifyOverriden = true;
			consol.AWBHeader.EH_AlsoNotifyCountryCode = "CU";

			countries = ((IComplianceLocationRiskStatusProvider)consol).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", shipment.ConsignorDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", shipment.ConsigneeDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Notify Party", notifyOrg.Country, countries);
			AssertContainsComplianceCountry("Consignee Overridden Country", consigneeCountry, countries);
			AssertContainsComplianceCountry("Shipper Overridden Country", shipperCountry, countries);
			AssertContainsComplianceCountry("Notify Overridden Country", notifyCountry, countries);
		}

		public void TestComplianceCommodities()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertNotNull("Precondition: Consol should be IComplianceItemRiskStatusProvider", consol);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "840140";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "880260";

			Factory.Save();

			var commodities = ((IComplianceCommodityRiskStatusProvider)consol).Commodities.ToArray();

			AssertEquals("Precondition: Commodities Count", 2, commodities.Length);

			var commodity = commodities.FirstOrDefault(x => x.HarmonizedCode == packLine1.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 1", commodity);
			AssertEquals("Commodity from PackLine 1", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 1", "WCO", commodity.GroupingOrCountry);

			commodity = commodities.FirstOrDefault(x => x.HarmonizedCode == packLine2.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 2", commodity);
			AssertEquals("Commodity from PackLine 2", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 2", "WCO", commodity.GroupingOrCountry);
		}

		public void TestComplianceRiskSupport()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals(false, ((IComplianceItemRiskStatusProvider)consol).ComplianceRiskSupport.IsSupportInitialization());
			AssertEquals(true, ((IComplianceItemRiskStatusProvider)consol).ComplianceRiskSupport.IsSupportSubCompliances());
		}

		public void TestSubComplianceRiskStatusProviders()
		{
			var consol = Factory.New<ForwardingConsol>();
			var provider = consol as IComplianceItemRiskStatusProvider;
			AssertEquals("PreCondition", 0, provider.SubComplianceRiskStatusProviders.Count());

			var ship1 = consol.GridShipments.AddNew();
			var ship2 = consol.GridShipments.AddNew();

			AssertEquals(2, provider.SubComplianceRiskStatusProviders.Count());
			Assert(provider.SubComplianceRiskStatusProviders.Contains(ship1));
			Assert(provider.SubComplianceRiskStatusProviders.Contains(ship2));
		}

		public void TestParentComplianceRiskStatusProviders()
		{
			var consol = Factory.New<ForwardingConsol>();
			var provider = consol as IComplianceItemRiskStatusProvider;
			AssertEquals(0, provider.ParentComplianceRiskStatusProviders.Count());
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol, OrgHeader creditor)
		{
			var consolCostType = ObjectFactory.GetType<IJobConsolCost>();
			BusinessObject consolCost = Factory.NewWithValidTestData(consolCostType);
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
				consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
				consolCost[JobConsolCostSchema.E6_OH_Creditor] = creditor.PK;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			return consolCost;
		}

		[TestDate(2022, 10, 15)]
		public void TestComplianceEffectiveDate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			AssertNotNull("Precondition: Consol should be IComplianceItemRiskStatusProvider", consol);

			AssertEquals("Precondition", ZDateTime.Empty, consol.JK_DepartureForFirstExportTransport);
			AssertEquals("Precondition", ZDateTime.Empty, consol.JK_SystemCreateTimeUtc);
			AssertEquals("Effective Date should be Now", ZDateTime.Now, ((IComplianceCommodityRiskStatusProvider)consol).EffectiveDate);

			consol.JK_SystemCreateTimeUtc = new ZDateTime(2022, 1, 9);
			AssertEquals("Effective Date should be consol creation time", new ZDateTime(2022, 1, 9).ToLocalBranchTime(), ((IComplianceCommodityRiskStatusProvider)consol).EffectiveDate);

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "RUMOW";
			transport1.JW_RL_NKDiscPort = "SARUH";
			transport1.JW_ETD = new ZDateTime(2022, 1, 2);
			transport1.JW_ETA = new ZDateTime(2022, 1, 3);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SARUH";
			transport2.JW_RL_NKDiscPort = "CNSHA";
			transport2.JW_ETD = new ZDateTime(2022, 1, 4);
			transport2.JW_ETA = new ZDateTime(2022, 1, 5);

			AssertEquals("Effective Date should be Consol ETD", new ZDateTime(2022, 1, 2), ((IComplianceCommodityRiskStatusProvider)consol).EffectiveDate);
		}

		public void TestAssessmentPointPairInfo()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var assessmentPointPairInfo = ((IComplianceCommodityRiskStatusProvider)consol).AssessmentPointPairInfo;
			AssertEquals(false, assessmentPointPairInfo.SupportAssessmentByBorderWise);
			AssertNull(assessmentPointPairInfo.PointPairs);
		}

		void AssertContainsComplianceParty(string description, OrgHeader orgHeader, IScreeningParty[] complianceParties)
		{
			var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Where(p =>
				p.Description == description).ToList();
			AssertGreaterThan($"{description} - {orgHeader.OH_Code}", parties.Count, 0);

			var party = parties.FirstOrDefault(p => orgHeader.OH_Code == ((p?.Header?.OH_Code ?? p?.OrgCode) ?? ZString.Empty));
			AssertEquals($"{description} - {orgHeader.OH_Code}", orgHeader.OH_Code, party?.OrgCode);
		}

		void AssertContainsVessel(Transport transport, IScreeningParty[] complianceParties, bool contains = true)
		{
			var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson));
			AssertEquals($"Vessel - {transport.JW_Vessel}",
				contains,
				parties.Any(o => o.NotLinkedVessel != null && (o.NotLinkedVessel as Transport).PK == transport.PK)
				|| parties.Any(o => o.Vessel != null && o.Vessel.RV_Code == transport.JW_Vessel));
		}

		void AssertContainsComplianceCountry(string description, RefCountry expectedCountry, IComplianceLocation[] complianceCountries)
		{
			AssertNotNull($"Precondition: {description} - expectedCountry", expectedCountry);

			var countries = complianceCountries.Where(p => p.ParentsDescription.Contains(description)).ToList();
			AssertGreaterThan($"{description} - {expectedCountry}", countries.Count, 0);

			var country = countries.FirstOrDefault(c => expectedCountry.Code == (c?.Code ?? ZString.Empty));
			AssertEquals($"{description} - {expectedCountry}", expectedCountry.Code, country?.Code);
		}

		void AssertNotContainsComplianceCountry(string description, RefCountry expectedCountry, IComplianceLocation[] complianceCountries)
		{
			AssertNotNull($"Precondition: {description} - expectedCountry", expectedCountry);

			var countries = complianceCountries.Where(p => p.ParentsDescription.Contains(description)).ToList();
			AssertEquals($"{description} - {expectedCountry}", countries.Count, 0);

			var country = countries.FirstOrDefault(c => expectedCountry.Code == (c?.Code ?? ZString.Empty));
			AssertNotEquals($"{description} - {expectedCountry}", expectedCountry.Code, country?.Code);
		}

		public void TestForwardingConsol_ComplianceRiskStatusObject()
		{
			Test("OVR", "PSK", "CLR", "CLR");
			Test("OVR", "CLR", "CLR", "PSK");
			Test("PSK", "PSK", "PSK", "CLR");
			Test("CLR", "CLR", "CLR", "CLR");

			void Test(ZString overallRisk, ZString commodityRisk, ZString partyRisk, ZString locationRisk)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var consol = Factory.New<ForwardingConsol>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = consol.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = consol.PK;

				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRisk;
				instance[ComplianceRiskStatusSchema.COR_CommodityRisk] = commodityRisk;
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = partyRisk;
				instance[ComplianceRiskStatusSchema.COR_LocationRisk] = locationRisk;

				Factory.Save();

				AssertEquals(overallRisk, consol.ComplianceRiskStatus.JobRisk);
				AssertEquals(partyRisk, consol.ComplianceRiskStatus.PartyRisk);
				AssertEquals(locationRisk, consol.ComplianceRiskStatus.LocationRisk);
				AssertEquals(commodityRisk, consol.ComplianceRiskStatus.CommodityRisk);
			}
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatus()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "FIAAI";
			transport.JW_RL_NKDiscPort = "FJBFJ";
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			consol.JK_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			Factory.Save();

			Assert(!((IComplianceItemRiskStatusProvider)consol).JobTime.IsCurrent);

			transport.JW_ETD = new ZDateTime(2024, 08, 02);
			Assert(((IComplianceItemRiskStatusProvider)consol).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithShipments()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			consol.JK_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			Factory.Save();

			Assert(!((IComplianceItemRiskStatusProvider)consol).JobTime.IsCurrent);

			var shipment = consol.Shipments.AddNew();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "FKFBE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "FMEAU";

			shipment.JS_E_ARV = new ZDateTime(2024, 08, 02);
			Assert(((IComplianceItemRiskStatusProvider)consol).JobTime.IsCurrent);
		}

		#region Implementation

		protected override IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider() =>
			Factory.NewWithValidTestData<ForwardingConsol>();

		protected override ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.SupportSubCompliances;

		#endregion
	}
}

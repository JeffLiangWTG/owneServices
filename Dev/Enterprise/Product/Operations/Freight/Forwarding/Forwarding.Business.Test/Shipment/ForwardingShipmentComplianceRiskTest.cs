using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentComplianceRiskTest : ComplianceRiskBusinessObjectTestCase
	{
		public void TestCompliancePartiesAndCountries()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertNotNull("Precondition: Shipment should be ICompliancePartyRiskStatusProvider", shipment);
			AssertNotNull("Precondition: Shipment should be ICompliancePartyRiskStatusProvider", shipment);

			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "ATVIE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "BDCGP";

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			var plannedCarrier = Factory.NewWithValidTestData<OrgHeader>();
			plannedCarrier.OH_RL_NKClosestPort = "BNBWN";
			shipment.BookedShippingLinePK = plannedCarrier.PK;

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_RL_NKClosestPort = "BRITJ";
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			var contractor1 = Factory.NewWithValidTestData<OrgHeader>();
			contractor1.OH_RL_NKClosestPort = "CACAL";
			shipment.Services.AddNew();
			shipment.Services[0].ES_OH_Contractor = contractor1.PK;

			var contractor2 = Factory.NewWithValidTestData<OrgHeader>();
			contractor2.OH_RL_NKClosestPort = "CHZRH";
			shipment.Services.AddNew();
			shipment.Services[1].ES_OH_Contractor = contractor2.PK;

			var provider1 = Factory.NewWithValidTestData<OrgHeader>();
			provider1.OH_RL_NKClosestPort = "CZPRG";
			shipment.Services[0].ServiceProviderPK = provider1.PK;

			var provider2 = Factory.NewWithValidTestData<OrgHeader>();
			provider2.OH_RL_NKClosestPort = "AUMEL";
			shipment.Services[1].ServiceProviderPK = provider2.PK;

			var gateway1 = Factory.NewWithValidTestData<OrgHeader>();
			gateway1.OH_RL_NKClosestPort = "CNSHA";
			shipment.Gateways.AddNew();
			shipment.Gateways[0].ForwarderPK = gateway1.PK;

			var gateway2 = Factory.NewWithValidTestData<OrgHeader>();
			gateway2.OH_RL_NKClosestPort = "CLARI";
			shipment.Gateways.AddNew();
			shipment.Gateways[1].ForwarderPK = gateway2.PK;

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "RUMOW";
			transport1.JW_RL_NKDiscPort = "SARUH";
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Code = "TestVessel1";
			transport1.JW_Vessel = vessel1.RV_Code;
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_RL_NKClosestPort = "CRSJO";
			transport1.CarrierPK = carrier1.PK;
			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_RL_NKClosestPort = "CUGER";
			transport1.CreditorPK = creditor1.PK;
			var departOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			departOrg1.OH_RL_NKClosestPort = "DEBER";
			transport1.JW_OA_DepartureLocation = departOrg1.MainAddress.PK;
			var arriveOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			arriveOrg1.OH_RL_NKClosestPort = "DKCPH";
			transport1.JW_OA_ArrivalLocation = arriveOrg1.MainAddress.PK;

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "JPTYO";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Code = "TestVessel2";
			transport2.JW_Vessel = vessel2.RV_Code;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_RL_NKClosestPort = "ESBCN";
			transport2.CarrierPK = carrier2.PK;
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_RL_NKClosestPort = "ETADD";
			transport2.CreditorPK = creditor2.PK;
			var departOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			departOrg2.OH_RL_NKClosestPort = "FIHEL";
			transport2.JW_OA_DepartureLocation = departOrg2.MainAddress.PK;
			var arriveOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			arriveOrg2.OH_RL_NKClosestPort = "FJLTK";
			transport2.JW_OA_ArrivalLocation = arriveOrg2.MainAddress.PK;

			var transitWarehouse1 = Factory.NewWithValidTestData<OrgHeader>();
			transitWarehouse1.OH_RL_NKClosestPort = "FRFOS";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouse1.MainAddress.PK;

			var transitWarehouse2 = Factory.NewWithValidTestData<OrgHeader>();
			transitWarehouse2.OH_RL_NKClosestPort = "GBAVO";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_OA_LastKnownTransitWarehouseAddress = transitWarehouse2.MainAddress.PK;

			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			exportBroker.OH_RL_NKClosestPort = "GRATH";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var pickupCartage = Factory.NewWithValidTestData<OrgHeader>();
			pickupCartage.OH_RL_NKClosestPort = "GUGUM";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = pickupCartage.MainAddress.PK;

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_RL_NKClosestPort = "HKHKG";
			shipment.PickupAgentPK = pickupAgent.PK;

			var pickupFrom = Factory.NewWithValidTestData<OrgHeader>();
			pickupFrom.OH_RL_NKClosestPort = "IDBEJ";
			shipment.ConsignorPickupAddress.OrganisationPK = pickupFrom.PK;

			var exportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>();
			exportReceivingDepot.OH_RL_NKClosestPort = "IEDUB";
			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_RL_NKClosestPort = "INBOM";
			shipment.JS_OH_ImportBroker = importBroker.PK;

			var deliveryCartage = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCartage.OH_RL_NKClosestPort = "ISREY";
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = deliveryCartage.MainAddress.PK;

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_RL_NKClosestPort = "ITFLR";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var deliveryTo = Factory.NewWithValidTestData<OrgHeader>();
			deliveryTo.OH_RL_NKClosestPort = "JPTYO";
			shipment.ConsigneeDeliveryAddress.OrganisationPK = deliveryTo.PK;

			var importReleaseDepot = Factory.NewWithValidTestData<OrgHeader>();
			importReleaseDepot.OH_RL_NKClosestPort = "KRINC";
			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;

			var undgContact = Factory.NewWithValidTestData<OrgHeader>();
			undgContact.OH_RL_NKClosestPort = "LKCMB";
			var dgContact = undgContact.Contacts.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			var undg = packline.UNDGs.AddNew();
			undg.DI_OC_DGContact = dgContact.PK;

			var declarationSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Supplier = declarationSupplier.PK;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_RL_NKClosestPort = "MYPGU";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			Factory.Save();

			var parties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToArray();
			var countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsVessel(transport1, parties);
				AssertContainsVessel(transport2, parties, false);

				AssertContainsComplianceParty("Consignor Documentary Address", consignor, parties);
				AssertContainsComplianceParty("Consignee Documentary Address", consignee, parties);
				AssertContainsComplianceParty("Local Client", localClient, parties);
				AssertContainsComplianceParty("Overseas Agent", overseasAgent, parties);
				AssertContainsComplianceParty("Planned Carrier", plannedCarrier, parties);
				AssertContainsComplianceParty("Notify Party", notifyParty, parties);
				AssertContainsComplianceParty("Contractor", contractor1, parties);
				AssertContainsComplianceParty("Contractor", contractor2, parties);
				AssertContainsComplianceParty("Gateway Forwarder", gateway1, parties);
				AssertContainsComplianceParty("Gateway Forwarder", gateway2, parties);
				AssertContainsComplianceParty("Carrier", carrier1, parties);
				AssertContainsComplianceParty("Carrier", carrier2, parties);
				AssertContainsComplianceParty("Creditor", creditor1, parties);
				AssertContainsComplianceParty("Creditor", creditor2, parties);
				AssertContainsComplianceParty("Depart From", departOrg1, parties);
				AssertContainsComplianceParty("Depart From", departOrg2, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg1, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg2, parties);
				AssertContainsComplianceParty("Transit Warehouse", transitWarehouse1, parties);
				AssertContainsComplianceParty("Transit Warehouse", transitWarehouse2, parties);
				AssertContainsComplianceParty("Export Broker", exportBroker, parties);
				AssertContainsComplianceParty("Pickup Local Transport Company", pickupCartage, parties);
				AssertContainsComplianceParty("Pickup Agent", pickupAgent, parties);
				AssertContainsComplianceParty("Consignor Pickup/Delivery Address", pickupFrom, parties);
				AssertContainsComplianceParty("Export CFS", exportReceivingDepot, parties);
				AssertContainsComplianceParty("Import Broker", importBroker, parties);
				AssertContainsComplianceParty("Delivery Local Transport Company", deliveryCartage, parties);
				AssertContainsComplianceParty("Delivery Agent", deliveryAgent, parties);
				AssertContainsComplianceParty("Consignee Pickup/Delivery Address", deliveryTo, parties);
				AssertContainsComplianceParty("Import CFS", importReleaseDepot, parties);
				AssertContainsComplianceParty("UNDG Contact Name", undgContact, parties);
				AssertNotNull("Contains compliance parties from declarations", parties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Single(x => x.Header == declarationSupplier));
				AssertContainsComplianceParty("Controlling Customer", controllingCustomer, parties);
				AssertContainsComplianceParty("Service Provider", provider1, parties);
				AssertContainsComplianceParty("Service Provider", provider2, parties);

				AssertContainsComplianceCountry("Origin Country", shipment.Origin.Country, countries);
				AssertContainsComplianceCountry("Destination Country", shipment.Destination.Country, countries);
				AssertContainsComplianceCountry("Planned Load Country", shipment.LoadPort.Country, countries);
				AssertContainsComplianceCountry("Planned Discharge Country", shipment.DischargePort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", shipment.Transports[0].LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", shipment.Transports[1].LoadPort.Country, countries);
				AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
				AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);
				AssertContainsComplianceCountry("Local Client", localClient.Country, countries);
				AssertContainsComplianceCountry("Overseas Agent", overseasAgent.Country, countries);
				AssertContainsComplianceCountry("Planned Carrier", plannedCarrier.Country, countries);
				AssertContainsComplianceCountry("Notify Party", notifyParty.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor1.Country, countries);
				AssertContainsComplianceCountry("Contractor", contractor2.Country, countries);
				AssertContainsComplianceCountry("Gateway Forwarder", gateway1.Country, countries);
				AssertContainsComplianceCountry("Gateway Forwarder", gateway2.Country, countries);
				AssertContainsComplianceCountry("Carrier", carrier1.Country, countries);
				AssertContainsComplianceCountry("Carrier", carrier2.Country, countries);
				AssertContainsComplianceCountry("Creditor", creditor1.Country, countries);
				AssertContainsComplianceCountry("Creditor", creditor2.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg1.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg2.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg1.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg2.Country, countries);
				AssertContainsComplianceCountry("Transit Warehouse", transitWarehouse1.Country, countries);
				AssertContainsComplianceCountry("Transit Warehouse", transitWarehouse2.Country, countries);
				AssertContainsComplianceCountry("Export Broker", exportBroker.Country, countries);
				AssertContainsComplianceCountry("Pickup Local Transport Company", pickupCartage.Country, countries);
				AssertContainsComplianceCountry("Pickup Agent", pickupAgent.Country, countries);
				AssertContainsComplianceCountry("Consignor Pickup/Delivery Address", pickupFrom.Country, countries);
				AssertContainsComplianceCountry("Export CFS", exportReceivingDepot.Country, countries);
				AssertContainsComplianceCountry("Import Broker", importBroker.Country, countries);
				AssertContainsComplianceCountry("Delivery Local Transport Company", deliveryCartage.Country, countries);
				AssertContainsComplianceCountry("Delivery Agent", deliveryAgent.Country, countries);
				AssertContainsComplianceCountry("Consignee Pickup/Delivery Address", deliveryTo.Country, countries);
				AssertContainsComplianceCountry("Import CFS", importReleaseDepot.Country, countries);
				AssertContainsComplianceCountry("UNDG Contact Name", undgContact.Country, countries);
				AssertContainsComplianceCountry("Controlling Customer", controllingCustomer.Country, countries);
				AssertContainsComplianceCountry("Service Provider", provider1.Country, countries);
				AssertContainsComplianceCountry("Service Provider", provider2.Country, countries);

				AssertEquals(0, shipment.AWBHeaderManager.Count);
			});
		}

		public void TestCompliancePartiesContainsHVLVConsignmentsParties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;
			Factory.Save();

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.Header.OH_Code = "SHIPPER";

			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Header.OH_Code = "CONSIGNEE";

			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.Header.OH_Code = "DEPOT";

			var returnLocationAddress = Factory.NewWithValidTestData<OrgAddress>();
			returnLocationAddress.Header.OH_Code = "RETURN";

			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_Code = "CARRIER";

			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.OH_Code = "AGENT";

			consignment[HVLVConsignmentSchema.HVC_OA_ShipperAddress] = shipperAddress.PK;
			consignment[HVLVConsignmentSchema.HVC_OA_ConsigneeAddress] = consigneeAddress.PK;
			consignment[HVLVConsignmentSchema.HVC_OA_DestinationDepot] = destinationDepotAddress.PK;
			consignment[HVLVConsignmentSchema.HVC_OA_ReturnLocation] = returnLocationAddress.PK;
			consignment[HVLVConsignmentSchema.HVC_OH_LastMileCarrier] = lastMileCarrier.PK;
			consignment[HVLVConsignmentSchema.HVC_OH_LastMileCarrierBookingAgent] = lastMileCarrierBookingAgent.PK;

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var parties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToArray();

				CombineAssertions(() =>
				{
					AssertEquals(true, parties.All(u => (u as ScreeningParty).Parents.Contains(shipment)));
					AssertContainsComplianceParty("Shipper", shipperAddress, parties);
					AssertContainsComplianceParty("Consignee", consigneeAddress, parties);
					AssertContainsComplianceParty("Destination Depot", destinationDepotAddress, parties);
					AssertContainsComplianceParty("Return Location", returnLocationAddress, parties);
					AssertContainsComplianceParty("Last Mile Carrier", lastMileCarrier, parties);
					AssertContainsComplianceParty("Last Mile Carrier Booking Agent", lastMileCarrierBookingAgent, parties);
				});
			}
		}

		public void TestCompliancePartiesContainHVLVConsignmentsFreeTextParties()
		{
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			var returnLocationAddress = Factory.NewWithValidTestData<OrgAddress>();

			Factory.CleanUp();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;
			Factory.Save();

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

			consignment[HVLVConsignmentSchema.HVC_ConsigneeName] = consigneeAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeAddress1] = consigneeAddress.OA_Address1;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeAddress2] = consigneeAddress.OA_Address2;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeCity] = consigneeAddress.OA_City;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeState] = consigneeAddress.OA_State;
			consignment[HVLVConsignmentSchema.HVC_ConsigneePostcode] = consigneeAddress.OA_PostCode;
			consignment[HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode] = consigneeAddress.OA_RN_NKCountryCode;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeContact] = consigneeAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeEmail] = consigneeAddress.OA_Email;
			consignment[HVLVConsignmentSchema.HVC_ConsigneePhone] = consigneeAddress.OA_Phone;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeMobile] = consigneeAddress.OA_Mobile;
			consignment[HVLVConsignmentSchema.HVC_ConsigneeFax] = consigneeAddress.OA_Fax;

			consignment[HVLVConsignmentSchema.HVC_ShipperName] = shipperAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ShipperAddress1] = shipperAddress.OA_Address1;
			consignment[HVLVConsignmentSchema.HVC_ShipperAddress2] = shipperAddress.OA_Address2;
			consignment[HVLVConsignmentSchema.HVC_ShipperCity] = shipperAddress.OA_City;
			consignment[HVLVConsignmentSchema.HVC_ShipperState] = shipperAddress.OA_City;
			consignment[HVLVConsignmentSchema.HVC_ShipperPostcode] = shipperAddress.OA_PostCode;
			consignment[HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode] = shipperAddress.OA_RN_NKCountryCode;
			consignment[HVLVConsignmentSchema.HVC_ShipperContact] = shipperAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ShipperEmail] = shipperAddress.OA_Email;
			consignment[HVLVConsignmentSchema.HVC_ShipperPhone] = shipperAddress.OA_Phone;
			consignment[HVLVConsignmentSchema.HVC_ShipperMobile] = shipperAddress.OA_Mobile;
			consignment[HVLVConsignmentSchema.HVC_ShipperFax] = shipperAddress.OA_Fax;

			consignment[HVLVConsignmentSchema.HVC_ReturnName] = returnLocationAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ReturnAddress1] = returnLocationAddress.OA_Address1;
			consignment[HVLVConsignmentSchema.HVC_ReturnAddress2] = returnLocationAddress.OA_Address2;
			consignment[HVLVConsignmentSchema.HVC_ReturnCity] = returnLocationAddress.OA_City;
			consignment[HVLVConsignmentSchema.HVC_ReturnState] = returnLocationAddress.OA_State;
			consignment[HVLVConsignmentSchema.HVC_ReturnPostcode] = returnLocationAddress.OA_PostCode;
			consignment[HVLVConsignmentSchema.HVC_RN_NKReturnCountryCode] = returnLocationAddress.OA_RN_NKCountryCode;
			consignment[HVLVConsignmentSchema.HVC_ReturnContact] = returnLocationAddress.CompanyName;
			consignment[HVLVConsignmentSchema.HVC_ReturnEmail] = returnLocationAddress.OA_Email;
			consignment[HVLVConsignmentSchema.HVC_ReturnPhone] = returnLocationAddress.OA_Phone;
			consignment[HVLVConsignmentSchema.HVC_ReturnMobile] = returnLocationAddress.OA_Mobile;
			consignment[HVLVConsignmentSchema.HVC_ReturnFax] = returnLocationAddress.OA_Fax;

			Factory.Save();

			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var parties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToArray();

				CombineAssertions(() =>
				{
					Assert(parties.Cast<ScreeningParty>().All((u) => u.Parents.Contains(shipment)));
					AssertContainsFreeTextComplianceParty("Consignee", consigneeAddress, parties);
					AssertContainsFreeTextComplianceParty("Shipper", shipperAddress, parties);
					AssertContainsFreeTextComplianceParty("Return Location", returnLocationAddress, parties);
				});
			}
		}

		public void TestComplianceWhenOverrideAddressesCountryIncludedInLocationRisk()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "ATVIE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "BDCGP";

			var countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "GB";

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "IR";

			countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", shipment.ConsignorDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", shipment.ConsigneeDocumentaryAddress.Country, countries);
		}

		public void TestComplianceWhenOverrideAddressesCountryInAWBIncludedInLocationRisk()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "ATVIE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "BDCGP";

			var notifyOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyOrg.PK;
			notifyOrg.OH_RL_NKClosestPort = "AUSYD";

			var countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

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

			shipment.AWBHeader.EH_IsConsigneeOverriden = true;
			shipment.AWBHeader.EH_ConsigneeCountryCode = "BD";
			shipment.AWBHeader.EH_IsShipperOverriden = true;
			shipment.AWBHeader.EH_ShipperCountryCode = "BB";
			shipment.AWBHeader.EH_IsNotifyOverriden = true;
			shipment.AWBHeader.EH_AlsoNotifyCountryCode = "CU";

			countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", shipment.ConsignorDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", shipment.ConsigneeDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Notify Party", notifyOrg.Country, countries);
			AssertContainsComplianceCountry("Consignee Overridden Country", consigneeCountry, countries);
			AssertContainsComplianceCountry("Shipper Overridden Country", shipperCountry, countries);
			AssertContainsComplianceCountry("Notify Overridden Country", notifyCountry, countries);
		}

		public void TestCompliancePartiesWhenHaveChildShipments()
		{
			var shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertCompliancePartiesWhenHaveChildShipments(false, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.BuyersConsol, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertCompliancePartiesWhenHaveChildShipments(true, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.FCL, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertCompliancePartiesWhenHaveChildShipments(true, false);

			void AssertCompliancePartiesWhenHaveChildShipments(bool shouldHaveSecondLayerScreeningParties, bool shouldHaveThirdLayerScreeningParties)
			{
				var screeningParties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToList();

				var secondLayerScreeningParties = ((ICompliancePartyRiskStatusProvider)shipment.CoLoadShipments.FirstOrDefault())?.Parties.ToList();
				var thirdLayerScreeningParties = ((ICompliancePartyRiskStatusProvider)(shipment.CoLoadShipments.FirstOrDefault() as ForwardingShipment)?.CoLoadShipments.FirstOrDefault())?.Parties.ToList();

				CombineAssertions(() =>
				{
					if (shouldHaveThirdLayerScreeningParties)
					{
						AssertEquals("Third layer should not have 'Local Client'", 0, thirdLayerScreeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("Third layer should have 'Export CFS'", 1, thirdLayerScreeningParties.Count(x => x.Description == "Export CFS"));
					}

					if (shouldHaveSecondLayerScreeningParties && shouldHaveThirdLayerScreeningParties)
					{
						AssertEquals("Second layer should have 'Local Client', it comes from itself.", 1, secondLayerScreeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("Second layer should have 'Export CFS', it comes from third layer.", 1, secondLayerScreeningParties.Count(x => x.Description == "Export CFS"));

						AssertEquals("First layer should have 'Local Client', it comes from second layer.", 1, screeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("First layer should have 'Export CFS', it comes from third layer.", 1, screeningParties.Count(x => x.Description == "Export CFS"));
					}
					else if (shouldHaveSecondLayerScreeningParties)
					{
						AssertEquals("Second layer should have 'Local Client', it comes from itself.", 1, secondLayerScreeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("Second layer should not have 'Export CFS'.", 0, secondLayerScreeningParties.Count(x => x.Description == "Export CFS"));

						AssertEquals("First layer should have 'Local Client', it comes from second layer.", 1, screeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("First layer should not have 'Export CFS'.", 0, screeningParties.Count(x => x.Description == "Export CFS"));
					}
					else
					{
						AssertEquals("First layer should not have 'Local Client'", 0, screeningParties.Count(x => x.Description == "Local Client"));
						AssertEquals("First layer should not have 'Export CFS'", 0, screeningParties.Count(x => x.Description == "Export CFS"));
					}
				});
			}
		}

		public void TestComplianceCountriesWhenHaveChildShipments()
		{
			var shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertComplianceCountriesWhenHaveChildShipments(false, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.BuyersConsol, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertComplianceCountriesWhenHaveChildShipments(true, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.FCL, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertComplianceCountriesWhenHaveChildShipments(true, false);

			void AssertComplianceCountriesWhenHaveChildShipments(bool shouldHaveSecondLayerCountries, bool shouldHaveThirdLayerCountries)
			{
				var countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToList();

				var secondLayerCountries = ((IComplianceLocationRiskStatusProvider)shipment.CoLoadShipments.FirstOrDefault())?.Locations.ToList();
				var thirdLayerCountries = ((IComplianceLocationRiskStatusProvider)(shipment.CoLoadShipments.FirstOrDefault() as ForwardingShipment)?.CoLoadShipments.FirstOrDefault())?.Locations.ToList();

				CombineAssertions(() =>
				{
					if (shouldHaveThirdLayerCountries)
					{
						AssertEquals("Third layer should not have 'Origin Country'", 0, thirdLayerCountries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("Third layer should have 'Destination Country'", 1, thirdLayerCountries.Count(x => x.ParentsDescription.Contains("Destination Country")));
					}

					if (shouldHaveSecondLayerCountries && shouldHaveThirdLayerCountries)
					{
						AssertEquals("Second layer should have 'Origin Country', it comes from itself.", 1, secondLayerCountries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("Second layer should have 'Destination Country', it comes from third layer.", 1, secondLayerCountries.Count(x => x.ParentsDescription.Contains("Destination Country")));

						AssertEquals("First layer should have 'Origin Country', it comes from second layer.", 1, countries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("First layer should have 'Destination Country', it comes from third layer.", 1, countries.Count(x => x.ParentsDescription.Contains("Destination Country")));
					}
					else if (shouldHaveSecondLayerCountries)
					{
						AssertEquals("Second layer should have 'Origin Country', it comes from itself.", 1, secondLayerCountries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("Second layer should not have 'Destination Country'.", 0, secondLayerCountries.Count(x => x.ParentsDescription.Contains("Destination Country")));

						AssertEquals("First layer should have 'Origin Country', it comes from second layer.", 1, countries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("First layer should not have 'Destination Country'.", 0, countries.Count(x => x.ParentsDescription.Contains("Destination Country")));
					}
					else
					{
						AssertEquals("First layer should not have 'Origin Country'", 0, countries.Count(x => x.ParentsDescription.Contains("Origin Country")));
						AssertEquals("First layer should not have 'Destination Country'", 0, countries.Count(x => x.ParentsDescription.Contains("Destination Country")));
					}
				});
			}
		}

		public void TestComplianceTariffCodeShowJI_TariffByDefault()
		{
			AssertComplianceTariffCodeShowJI_TariffByDefault(Constants.CountryCodes.UnitedStates, "111111", "8482100000");
			AssertComplianceTariffCodeShowJI_TariffByDefault(Constants.CountryCodes.UnitedKingdom, "111111", "8482100000");
		}

		void AssertComplianceTariffCodeShowJI_TariffByDefault(string countryCode, string expectedCode1, string expectedCode2)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RN_NKCountryCode = countryCode;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;

				var invoiceHeader = declaration.Invoices.AddNew();

				var invoiceLine1 = invoiceHeader.AddNewInvoiceLine();
				invoiceLine1.JI_Tariff = "8482100000";

				var invoiceLine2 = invoiceHeader.AddNewInvoiceLine();
				invoiceLine2.JI_Tariff = "111111";
				Factory.Save();

				var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities.ToArray();
				CombineAssertions("There are 2 commodities with formatted tariff code.", () =>
				{
					AssertEquals(2, complianceCommodities.Length);
					Assert(complianceCommodities.Any(v => v.HarmonizedCode == expectedCode1 && v.CommoditySource == "Brokerage"));
					Assert(complianceCommodities.Any(v => v.HarmonizedCode == expectedCode2 && v.CommoditySource == "Brokerage"));
				});
			}
		}

		public void TestComplianceCommoditiesDoesNotIncludeEmptyInvoiceLines()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.AddNewInvoiceLine();
			invoiceLine1.JI_Tariff = " ";

			var invoiceLine2 = invoiceHeader.AddNewInvoiceLine();
			invoiceLine2.JI_Tariff = "1111.11.11";
			Factory.Save();

			var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities.ToArray();
			AssertEquals("There is only 1 commodity.", 1, complianceCommodities.Length);
			Assert("The commodity's harmonized code is 11111111 and Commodity Source is 'Brokerage'.", complianceCommodities.Any(v => v.HarmonizedCode == invoiceLine2.JI_Tariff && v.CommoditySource == "Brokerage"));
		}

		public void TestComplianceCommoditiesWithCommercialInvoiceLines()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var header = Factory.NewWithValidTestData<GlobalCommercialInvoiceHeader>();
				header.GIH_ParentID = shipment.PK;
				header.GIH_ParentTableCode = shipment.TablePrefix;
				var line = Factory.New<GlobalCommercialInvoiceLine>();
				line.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
				line.GIL_GIH_Header = header.PK;
				line.GIL_Description = "Test";
				line.GIL_Tariff1 = "1111.11";
				line.GIL_Tariff2 = "2222.22";
				Factory.Save();
				var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities.ToArray();
				AssertEquals("There is 2 commodities.", 2, complianceCommodities.Length);
				Assert("The commodity's harmonized code is 111111 and Commodity Source is 'Commercial Invoice' from GIL_Tariff1.", complianceCommodities.Any(v => v.HarmonizedCode == "1111.11" && v.CommoditySource == "Commercial Invoice"));
				Assert("The commodity's harmonized code is 222222 and Commodity Source is 'Commercial Invoice' from GIL_Tariff2.", complianceCommodities.Any(v => v.HarmonizedCode == "2222.22" && v.CommoditySource == "Commercial Invoice"));
			}
		}

		public void TestComplianceCommoditiesWithPackingLines()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertNotNull("Precondition: Shipment should be IComplianceCommodityRiskStatusProvider", shipment);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "840140";
			packLine1.JL_RN_NKOrigin = "AU";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "880260";

			Factory.Save();

			var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities.ToArray();

			AssertEquals("Precondition: Commodities Count", 2, complianceCommodities.Length);

			var commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine1.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 1", commodity);
			AssertEquals("Commodity from PackLine 1", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 1", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 1", ((IComplianceCommodityRiskStatusProvider)shipment).ParentID, commodity.ParentJobID);
			AssertEquals("Commodity from PackLine 1", "AU", commodity.Origin);
			AssertEquals("Commodity from PackLine 1", "Packing", commodity.CommoditySource);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine2.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 2", commodity);
			AssertEquals("Commodity from PackLine 2", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 2", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 2", ((IComplianceCommodityRiskStatusProvider)shipment).ParentID, commodity.ParentJobID);
			AssertEquals("Commodity from PackLine 2", ZString.Empty, commodity.Origin);
			AssertEquals("Commodity from PackLine 2", "Packing", commodity.CommoditySource);
		}

		public void TestComplianceCommoditiesWhenHaveChildShipments()
		{
			var shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertComplianceCommoditiesWhenHaveChildShipments(false, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.BuyersConsol, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertComplianceCommoditiesWhenHaveChildShipments(true, false);

			shipment = CreateShipmentWithChildren(Constants.ShipmentTypes.AssemblyMaster, Constants.ContainerModes.FCL, Constants.ShipmentTypes.StandardHouse, Constants.ContainerModes.BuyersConsol, string.Empty, string.Empty);
			AssertComplianceCommoditiesWhenHaveChildShipments(true, false);

			void AssertComplianceCommoditiesWhenHaveChildShipments(bool shouldHaveSecondLayerCountries, bool shouldHaveThirdLayerCountries)
			{
				var harmonizedCode1 = "840140";
				var harmonizedCode2 = "880260";

				var commodities = ((IComplianceCommodityRiskStatusProvider)shipment).Commodities.ToList();

				var secondLayerCommodities = ((IComplianceCommodityRiskStatusProvider)shipment.CoLoadShipments.FirstOrDefault())?.Commodities.ToList();
				var thirdLayerCommodities = ((IComplianceCommodityRiskStatusProvider)(shipment.CoLoadShipments.FirstOrDefault() as ForwardingShipment)?.CoLoadShipments.FirstOrDefault())?.Commodities.ToList();

				CombineAssertions(() =>
				{
					if (shouldHaveThirdLayerCountries)
					{
						AssertEquals("Third layer should not have 'Harmonized Code 1'", 0, thirdLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("Third layer should have 'Harmonized Code 2'", 1, thirdLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode2));
					}

					if (shouldHaveSecondLayerCountries && shouldHaveThirdLayerCountries)
					{
						AssertEquals("Second layer should have 'Harmonized Code 1, it comes from itself.", 1, secondLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("Second layer should have 'Harmonized Code 2', it comes from third layer.", 1, secondLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode2));

						AssertEquals("First layer should have 'Harmonized Code 1, it comes from second layer.", 1, commodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("First layer should have 'Harmonized Code 2', it comes from third layer.", 1, commodities.Count(x => x.HarmonizedCode == harmonizedCode2));
					}
					else if (shouldHaveSecondLayerCountries)
					{
						AssertEquals("Second layer should have 'Harmonized Code 1, it comes from itself.", 1, secondLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("Second layer should not have 'Harmonized Code 2'.", 0, secondLayerCommodities.Count(x => x.HarmonizedCode == harmonizedCode2));

						AssertEquals("First layer should have 'Harmonized Code 1, it comes from second layer.", 1, commodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("First layer should not have 'Harmonized Code 2'.", 0, commodities.Count(x => x.HarmonizedCode == harmonizedCode2));
					}
					else
					{
						AssertEquals("First layer should not have 'Harmonized Code 1", 0, commodities.Count(x => x.HarmonizedCode == harmonizedCode1));
						AssertEquals("First layer should not have 'Harmonized Code 2'", 0, commodities.Count(x => x.HarmonizedCode == harmonizedCode2));
					}
				});
			}
		}

		[TestDate(2022, 10, 15)]
		public void TestComplianceEffectiveDate()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertNotNull("Precondition: Shipment should be IComplianceCommodityRiskStatusProvider", shipment);

			AssertNull("Precondition", shipment.DepartureConsol);
			AssertEquals("Precondition", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("Precondition", ZDateTime.Empty, shipment.JS_SystemCreateTimeUtc);
			AssertEquals("Effective Date should be Now", ZDateTime.Now, ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);

			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2022, 1, 8);
			AssertEquals("Effective Date should be shipment creation time", new ZDateTime(2022, 1, 8).ToLocalBranchTime(), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);

			shipment.JS_E_DEP = new ZDateTime(2022, 1, 9);
			AssertEquals("Effective Date should be shipment ETD", new ZDateTime(2022, 1, 9), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			AssertNotNull("Precondition", shipment.DepartureConsol);

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

			AssertEquals("Effective Date should be Departure Consol ETD", new ZDateTime(2022, 1, 2), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);
		}

		public void TestComplianceRiskSupport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(true, ((IComplianceItemRiskStatusProvider)shipment).ComplianceRiskSupport.IsSupportInitialization());
			AssertEquals(true, ((IComplianceItemRiskStatusProvider)shipment).ComplianceRiskSupport.IsSupportSubCompliances());
		}

		public void TestSubComplianceRiskStatusProviders()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var provider = shipment as IComplianceItemRiskStatusProvider;
			AssertEquals("PreCondition", 0, provider.SubComplianceRiskStatusProviders.Count());

			var ship1 = shipment.CoLoadShipments.AddNew();
			var ship2 = shipment.CoLoadShipments.AddNew();

			AssertEquals(2, provider.SubComplianceRiskStatusProviders.Count());
			Assert(provider.SubComplianceRiskStatusProviders.Contains(ship1));
			Assert(provider.SubComplianceRiskStatusProviders.Contains(ship2));
		}

		public void TestParentComplianceRiskStatusProviders()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = Factory.New<ForwardingConsol>();

			var ship1 = shipment.CoLoadShipments.AddNew();
			var ship2 = shipment.CoLoadShipments.AddNew();
			consol.GridShipments.Add(ship1);

			var provider1 = ship1 as IComplianceItemRiskStatusProvider;
			var provider2 = ship2 as IComplianceItemRiskStatusProvider;

			AssertEquals(2, provider1.ParentComplianceRiskStatusProviders.Count());
			AssertEquals(1, provider2.ParentComplianceRiskStatusProviders.Count());
			Assert(provider1.ParentComplianceRiskStatusProviders.Contains(shipment));
			Assert(provider1.ParentComplianceRiskStatusProviders.Contains(consol));
			Assert(provider2.ParentComplianceRiskStatusProviders.Contains(shipment));
		}

		public void TestAssessmentPointPairInfo()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			shipment.JS_E_ARV = new ZDateTime(2024, 1, 12);
			shipment.JS_E_DEP = new ZDateTime(2024, 1, 2);

			var assessmentPointPairInfo = ((IComplianceCommodityRiskStatusProvider)shipment).AssessmentPointPairInfo;
			AssertContainsExactElementsInAnyOrder(new[] { ("AU", "AUSYD", "Origin", "US", "USORD", "Destination", shipment.JS_E_ARV, shipment.JS_E_DEP, shipment.JS_TransportMode.ToString()) },
				assessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint.Country, u.OriginPoint.UNLOCO, u.OriginPoint.MovementDescription, u.DestinationPoint.Country, u.DestinationPoint.UNLOCO, u.DestinationPoint.MovementDescription, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithBilling()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;

			job.JH_Status = JobHeaderStatus.Complete.Code;
			Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
			AssertEquals("Job End Date should be JS_E_DEP", ((IComplianceItemRiskStatusProvider)shipment).JobTime.JobEndDate, shipment.JS_E_DEP);
			job.JH_Status = JobHeaderStatus.CustomsProcessActive.Code;
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
			AssertEquals("Job End Date should be JS_E_DEP", ((IComplianceItemRiskStatusProvider)shipment).JobTime.JobEndDate, shipment.JS_E_DEP);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithETDOrETALessThanOneWeek()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Codes.Working;

			Factory.Save();

			shipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 25);
			shipment.JS_E_ARV = ZDateTime.Empty;
			Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 8, 8);
			shipment.JS_E_ARV = ZDateTime.Empty;
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 8, 7);
			shipment.JS_E_ARV = ZDateTime.Empty;
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = new ZDateTime(2024, 8, 7);
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = new ZDateTime(2024, 8, 8);
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithAddEventTime()
		{
			using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_RL_NKClosestPort = "BEBRU";
				var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
				overseasAgent.OH_RL_NKClosestPort = "FIATS";

				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_ParentID = shipment.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
				job.JH_JobNum = "ABC123";
				job.JH_Status = JobHeaderStatus.Codes.Working;

				shipment.JS_E_DEP = ZDateTime.Empty;
				shipment.JS_E_ARV = ZDateTime.Empty;

				Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

				Factory.Save();

				var addEvent = shipment.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
				Assert(!addEvent.SL_EventTime.IsEmpty);

				shipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				Factory.Save();
				Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

				shipment.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-11);
				Factory.Save();
				Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
			}
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithRouting()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Codes.Working;

			shipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			Factory.Save();

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "FIATS";
			transport.JW_RL_NKDiscPort = "BEBRU";
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ETA = new ZDateTime(2024, 08, 02);
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);

			transport.JW_ETA = new ZDateTime(2024, 07, 02);
			Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithCoLoadShipment()
		{
			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Codes.Working;
			shipment.JS_E_ARV = new ZDateTime(2024, 08, 02);

			masterShipment.JS_E_ARV = new ZDateTime(2024, 07, 02);
			masterShipment.CoLoadShipments.Add(shipment);

			Factory.Save();

			Assert(((IComplianceItemRiskStatusProvider)masterShipment).JobTime.IsCurrent);
		}

		void AssertContainsComplianceParty(string description, OrgHeader orgHeader, IScreeningParty[] complianceParties)
		{
			var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Where(p =>
				p.Description == description).ToList();
			AssertGreaterThan($"{description} - {orgHeader.OH_Code}", parties.Count, 0);

			var party = parties.FirstOrDefault(p => orgHeader.OH_Code == ((p?.Header?.OH_Code ?? p?.OrgCode) ?? ZString.Empty));
			AssertEquals($"{description} - {orgHeader.OH_Code}", orgHeader.OH_Code, party.OrgCode);
		}

		void AssertContainsComplianceParty(string description, OrgAddress orgAddress, IScreeningParty[] complianceParties)
		{
			var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Where(p =>
				p.Description == description).ToList();

			AssertGreaterThan($"{description} - {orgAddress.Header.OH_Code}", parties.Count, 0);

			var party = parties.FirstOrDefault(p => orgAddress.Header.OH_Code == ((p?.Header?.OH_Code ?? p?.OrgCode) ?? ZString.Empty));
			AssertEquals($"{description} - {orgAddress.Header.OH_Code}", orgAddress.Header.OH_Code, party.OrgCode);
		}

		void AssertContainsFreeTextComplianceParty(string description, OrgAddress orgAddress, IScreeningParty[] complianceParties)
		{
			var parties = complianceParties
				.Select((o) => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson))
				.Where((p) =>	p.Description == description)
				.ToList();

			AssertEquals($"{description} {nameof(parties.Count)} is 1", parties.Count, 1);

			var party = parties[0];
			AssertNotNull($"{description} {nameof(party.NaturalPerson)} exists", party.NaturalPerson);
			AssertEquals($"{description} {nameof(party.NaturalPerson)} {nameof(party.NaturalPerson.BusinessEntityKey)} is {nameof(party)} {nameof(party.Key)}", party.Key, party.NaturalPerson.BusinessEntityKey);
			AssertEquals($"{nameof(party.NaturalPerson)} {nameof(party.NaturalPerson.Name)}", orgAddress.EffectiveCompanyName, party.NaturalPerson.Name);
			AssertEquals($"{nameof(party.NaturalPerson)} {nameof(party.NaturalPerson.Address1)}", orgAddress.Address1, party.NaturalPerson.Address1);
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

		ForwardingShipment CreateShipmentWithChildren(string shipmentType, string packingMode, string subShipmentType, string subPackingMode, string thirdShipmentType, string thirdPackingMode)
		{
			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			masterShipment.JS_ShipmentType = shipmentType;
			masterShipment.JS_PackingMode = packingMode;

			ForwardingShipment secondLayerShipment = null;

			if (!string.IsNullOrEmpty(subShipmentType))
			{
				secondLayerShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				secondLayerShipment.JS_RL_NKOrigin = "AUSYD";
				secondLayerShipment.JS_ShipmentType = shipmentType;
				secondLayerShipment.JS_PackingMode = packingMode;
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_ParentID = secondLayerShipment.PK;
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
				job.JH_JobNum = "ABC123";

				var packLine = secondLayerShipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = "840140";

				masterShipment.CoLoadShipments.Add(secondLayerShipment);
			}

			if (secondLayerShipment != null && !string.IsNullOrEmpty(thirdShipmentType))
			{
				var thirdLayerShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				thirdLayerShipment.JS_RL_NKDestination = "CNSHA";
				thirdLayerShipment.JS_ShipmentType = thirdShipmentType;
				thirdLayerShipment.JS_PackingMode = thirdPackingMode;
				var exportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>();
				thirdLayerShipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

				var packLine = thirdLayerShipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = "880260";

				secondLayerShipment.CoLoadShipments.Add(thirdLayerShipment);
			}

			Factory.Save();

			return masterShipment;
		}

		#region Implementation

		protected override IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider() =>
			Factory.NewWithValidTestData<ForwardingShipment>();

		protected override ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.FullySupported;

		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	public class AgencyShipmentComplianceRiskTest : ComplianceRiskBusinessObjectTestCase
	{
		public void TestAgencyShipmentCompliancePartiesAndCountries()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertNotNull("Precondition: Shipment should be ICompliancePartyRiskStatusProvider,IComplianceLocationRiskStatusProvider", shipment);

			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_RL_NKClosestPort = "BNBWN";
			shipment.JS_OH_DeliveryAgent = principal.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "ATVIE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "BDCGP";

			var plannedCarrier = Factory.NewWithValidTestData<OrgHeader>();
			plannedCarrier.OH_RL_NKClosestPort = "BNBWN";
			shipment.BookedShippingLinePK = plannedCarrier.PK;

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_RL_NKClosestPort = "BRITJ";
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_RL_NKClosestPort = "HKHKG";
			shipment.PickupAgentPK = pickupAgent.PK;

			var transport1 = shipment.Transports.AddNew();
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

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_RL_NKClosestPort = "MYPGU";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;

			var loadPort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			shipment.JS_RL_NKPlaceOfReceipt = loadPort.RL_Code;

			var dischargePort = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "HKHKG");
			shipment.JS_RL_NKPlaceOfDischarge = dischargePort.RL_Code;

			var container1 = shipment.BookedContainers.AddNew();
			var departureYard1 = Factory.NewWithValidTestData<OrgHeader>();
			departureYard1.OH_RL_NKClosestPort = "BHALA";
			container1.JC_OA_DepartureContainerYardAddress = departureYard1.MainAddress.PK;

			var container2 = shipment.BookedContainers.AddNew();
			var departureYard2 = Factory.NewWithValidTestData<OrgHeader>();
			departureYard2.OH_RL_NKClosestPort = "BJKDC";
			container2.JC_OA_DepartureContainerYardAddress = departureYard2.MainAddress.PK;
			shipment.RealContainers.Add(container2);

			var port1 = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			port1.O5_PortOrCountry = "AUMEL";
			port1.O5_OA_AgentOfficeAddress = org1.MainAddress.PK;

			var port2 = principal.CarrierAppointedAgentPorts_Agency.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			port2.O5_PortOrCountry = "NZAKL";
			port2.O5_OA_AgentOfficeAddress = org2.MainAddress.PK;

			var container = shipment.RealContainers.AddNew();
			var verifiedBy = Factory.NewWithValidTestData<OrgHeader>();
			container.GrossWeightVerifiedByAddress.OrganisationPK = verifiedBy.PK;
			shipment.Confirm();

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var charges = (IBusinessObjectCollection)job["Charges"];
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charges.Add(charge);
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_OH_CostAccount = creditor.PK;

			var parties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToArray();
			var countries = ((IComplianceLocationRiskStatusProvider)shipment).Locations.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsComplianceParty("Consignor Documentary Address", consignor, parties);
				AssertContainsComplianceParty("Consignee Documentary Address", consignee, parties);
				AssertContainsComplianceParty("Notify Party", notifyParty, parties);
				AssertContainsComplianceParty("Pickup Agent", pickupAgent, parties);
				AssertContainsComplianceParty("Controlling Customer", controllingCustomer, parties);
				AssertContainsComplianceParty("Planned Carrier", plannedCarrier, parties);
				AssertContainsComplianceParty("Principal", principal, parties);
				AssertContainsComplianceParty("Sending Agent", org1, parties);
				AssertContainsComplianceParty("Receiving Agent", org2, parties);
				AssertContainsComplianceParty("Departure Container Yard", departureYard1, parties);
				AssertContainsComplianceParty("Departure Container Yard", departureYard2, parties);
				AssertContainsComplianceParty("Carrier", carrier1, parties);
				AssertContainsComplianceParty("Carrier", carrier2, parties);
				AssertContainsComplianceParty("Creditor", creditor1, parties);
				AssertContainsComplianceParty("Creditor", creditor2, parties);
				AssertContainsComplianceParty("Depart From", departOrg1, parties);
				AssertContainsComplianceParty("Depart From", departOrg2, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg1, parties);
				AssertContainsComplianceParty("Arrival At", arriveOrg2, parties);
				AssertContainsComplianceParty("Verified By", verifiedBy, parties);
				AssertContainsComplianceParty("Local Client", localClient, parties);
				AssertContainsComplianceParty("Creditor", creditor, parties);
				AssertContainsComplianceParty("Debtor", debtor, parties);

				AssertContainsComplianceCountry("Origin Country", shipment.Origin.Country, countries);
				AssertContainsComplianceCountry("Destination Country", shipment.Destination.Country, countries);
				AssertContainsComplianceCountry("Place of Receipt", shipment.PlaceOfReceipt.Country, countries);
				AssertContainsComplianceCountry("Place of Delivery", shipment.PlaceOfDischarge.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", shipment.Transports[0].LoadPort.Country, countries);
				AssertContainsComplianceCountry("Routing Load Country", shipment.Transports[1].LoadPort.Country, countries);
				AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
				AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);
				AssertContainsComplianceCountry("Notify Party", notifyParty.Country, countries);
				AssertContainsComplianceCountry("Pickup Agent", pickupAgent.Country, countries);
				AssertContainsComplianceCountry("Controlling Customer", controllingCustomer.Country, countries);
				AssertContainsComplianceCountry("Planned Carrier", plannedCarrier.Country, countries);
				AssertContainsComplianceCountry("Principal", principal.Country, countries);
				AssertContainsComplianceCountry("Carrier", carrier1.Country, countries);
				AssertContainsComplianceCountry("Carrier", carrier2.Country, countries);
				AssertContainsComplianceCountry("Creditor", creditor1.Country, countries);
				AssertContainsComplianceCountry("Creditor", creditor2.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg1.Country, countries);
				AssertContainsComplianceCountry("Depart From", departOrg2.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg1.Country, countries);
				AssertContainsComplianceCountry("Arrival At", arriveOrg2.Country, countries);
				AssertContainsComplianceCountry("Departure Container Yard", departureYard1.Country, countries);
				AssertContainsComplianceCountry("Departure Container Yard", departureYard2.Country, countries);
				AssertContainsComplianceCountry("Local Client", localClient.Country, countries);
			});
		}

		public void TestAgencyBookingCompliancePartiesAndCountries()
		{
			var shipment = Factory.New<AgencyBooking>();
			AssertNotNull("Precondition: Shipment should be ICompliancePartyRiskStatusProvider", shipment);

			var bookingParty = Factory.New<OrgHeader>();
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var parties = ((ICompliancePartyRiskStatusProvider)shipment).Parties.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsComplianceParty("Booking Party Documentary Address", bookingParty, parties);
			});
		}

		public void TestBillOfLadingCompliancePartiesAndCountries()
		{
			var bill = Factory.New<BillOfLading>();
			AssertNotNull("Precondition: Bill should be ICompliancePartyRiskStatusProvider,IComplianceLocationRiskStatusProvider", bill);

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "UAIEV";
			origin.JA_JV = jobVoyage.PK;

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "CLESR";
			destination.JB_JV = jobVoyage.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			bill.JS_JX = sailing.PK;

			var container = bill.RealContainers.AddNew();
			var arrivalYard = Factory.NewWithValidTestData<OrgHeader>();
			arrivalYard.OH_RL_NKClosestPort = "BGBEL";
			container.JC_OA_ArrivalContainerYardAddress = arrivalYard.MainAddress.PK;

			var departureYard = Factory.NewWithValidTestData<OrgHeader>();
			departureYard.OH_RL_NKClosestPort = "BHALA";
			container.JC_OA_DepartureContainerYardAddress = departureYard.MainAddress.PK;

			var parties = ((ICompliancePartyRiskStatusProvider)bill).Parties.ToArray();
			var countries = ((IComplianceLocationRiskStatusProvider)bill).Locations.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsVessel(sailing, parties);
				AssertContainsComplianceParty("Empty Pickup From", departureYard, parties);
				AssertContainsComplianceParty("Empty Return To", arrivalYard, parties);

				AssertContainsComplianceCountry("Empty Pickup From", departureYard.Country, countries);
				AssertContainsComplianceCountry("Empty Return To", arrivalYard.Country, countries);
			});
		}

		public void TestAgencyBookingComplianceCommodities()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			AssertNotNull("Precondition: Booking should be IComplianceCommodityRiskStatusProvider", booking);

			var packLine1 = booking.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "840140";
			packLine1.JL_RN_NKOrigin = "AU";
			var packLine2 = booking.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "880260";

			Factory.Save();

			var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)booking).Commodities.ToArray();

			AssertEquals("Precondition: Commodities Count", 2, complianceCommodities.Length);

			var commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine1.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 1", commodity);
			AssertEquals("Commodity from PackLine 1", booking.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 1", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 1", "AU", commodity.Origin);
			AssertEquals("Details > Packs", commodity.CommoditySource);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine2.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 2", commodity);
			AssertEquals("Commodity from PackLine 2", booking.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 2", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 1", ZString.Empty, commodity.Origin);
			AssertEquals("Details > Packs", commodity.CommoditySource);

			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var vehicle = booking.Vehicles.AddNew();
			vehicle.JC_HarmonisedCode = "190590";
			Factory.Save();

			complianceCommodities = ((IComplianceCommodityRiskStatusProvider)booking).Commodities.ToArray();
			AssertEquals("Precondition: Commodities Count", 1, complianceCommodities.Length);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == vehicle.JC_HarmonisedCode);
			AssertNotNull("Commodity from Vehicle", commodity);
			AssertEquals("Commodity from Vehicle", booking.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from Vehicle", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from Vehicle", ZString.Empty, commodity.Origin);
			AssertEquals("Details > Vehicles", commodity.CommoditySource);

			booking.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var packline3 = booking.TopLevelPacks.AddNew();
			packline3.JC_HarmonisedCode = "123456";
			Factory.Save();

			complianceCommodities = ((IComplianceCommodityRiskStatusProvider)booking).Commodities.ToArray();
			AssertEquals("Precondition: Commodities Count", 1, complianceCommodities.Length);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packline3.JC_HarmonisedCode);
			AssertNotNull("Commodity from TopLevelPacks", commodity);
			AssertEquals("Commodity from TopLevelPacks", booking.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from TopLevelPacks", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from TopLevelPacks", ZString.Empty, commodity.Origin);
			AssertEquals("Details > Packs", commodity.CommoditySource);
		}

		public void TestBillOfLadingComplianceCommodities()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			AssertNotNull("Precondition: Bill should be IComplianceCommodityRiskStatusProvider", bill);

			bill.JS_PackingMode = Constants.ContainerModes.FCL;
			var packLine1 = bill.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "840140";
			var packLine2 = bill.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "880260";

			Factory.Save();

			var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)bill).Commodities.ToArray();
			AssertEquals("Precondition: Commodities Count", 2, complianceCommodities.Length);

			var commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine1.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 1", commodity);
			AssertEquals("Commodity from PackLine 1", bill.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 1", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Containers > Pack Lines", commodity.CommoditySource);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine2.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 2", commodity);
			AssertEquals("Commodity from PackLine 2", bill.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 2", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Containers > Pack Lines", commodity.CommoditySource);

			bill.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var vehicle = bill.Vehicles.AddNew();
			vehicle.JC_HarmonisedCode = "190590";
			Factory.Save();

			complianceCommodities = ((IComplianceCommodityRiskStatusProvider)bill).Commodities.ToArray();
			AssertEquals("Precondition: Commodities Count", 1, complianceCommodities.Length);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == vehicle.JC_HarmonisedCode);
			AssertNotNull("Commodity from Vehicle", commodity);
			AssertEquals("Commodity from Vehicle", bill.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from Vehicle", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from Vehicle", ZString.Empty, commodity.Origin);
			AssertEquals("Vehicles", commodity.CommoditySource);

			bill.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			var packline3 = bill.TopLevelPacks.AddNew();
			packline3.JC_HarmonisedCode = "123456";
			Factory.Save();

			complianceCommodities = ((IComplianceCommodityRiskStatusProvider)bill).Commodities.ToArray();
			AssertEquals("Precondition: Commodities Count", 1, complianceCommodities.Length);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packline3.JC_HarmonisedCode);
			AssertNotNull("Commodity from TopLevelPacks", commodity);
			AssertEquals("Commodity from TopLevelPacks", bill.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from TopLevelPacks", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from TopLevelPacks", ZString.Empty, commodity.Origin);
			AssertEquals("Packs", commodity.CommoditySource);
		}

		[TestDate(2022, 10, 15)]
		public void TestComplianceEffectiveDate()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertEquals("Precondition", ZDateTime.Empty, shipment.JS_E_DEP);
			AssertEquals("Precondition", ZDateTime.Empty, shipment.JS_SystemCreateTimeUtc);
			AssertNotNull("Precondition: Shipment should be IComplianceCommodityRiskStatusProvider", shipment);

			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2022, 1, 8);
			AssertEquals("Effective Date should be shipment creation time", new ZDateTime(2022, 1, 8).ToLocalBranchTime(), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);

			shipment.JS_E_DEP = new ZDateTime(2022, 1, 9);
			AssertEquals("Effective Date should be shipment ETD", new ZDateTime(2022, 1, 9), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);
		}

		public void TestComplianceRiskStatusObject()
		{
			Test("OVR", "PSK", "CLR", "CLR");
			Test("OVR", "CLR", "CLR", "PSK");
			Test("PSK", "PSK", "PSK", "CLR");
			Test("CLR", "CLR", "CLR", "CLR");

			void Test(ZString overallRisk, ZString commodityRisk, ZString partyRisk, ZString locationRisk)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var shipment = Factory.New<AgencyShipment>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = shipment.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = shipment.PK;

				instance[ComplianceRiskStatusSchema.COR_CommodityRisk] = commodityRisk;
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = partyRisk;
				instance[ComplianceRiskStatusSchema.COR_LocationRisk] = locationRisk;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRisk;

				Factory.Save();

				AssertEquals(overallRisk, shipment.ComplianceRiskStatus.JobRisk);
				AssertEquals(partyRisk, shipment.ComplianceRiskStatus.PartyRisk);
				AssertEquals(commodityRisk, shipment.ComplianceRiskStatus.CommodityRisk);
				AssertEquals(locationRisk, shipment.ComplianceRiskStatus.LocationRisk);
			}
		}

		public void TestAssessmentPointPairInfo()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			AssertNotNull("Precondition: Booking should be IComplianceCommodityRiskStatusProvider", booking);

			booking.JS_TransportMode = "SEA";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "USORD";
			booking.JS_E_ARV = new ZDateTime(2024, 1, 12);
			booking.JS_E_DEP = new ZDateTime(2024, 1, 2);

			var assessmentPointPairInfo = ((IComplianceCommodityRiskStatusProvider)booking).AssessmentPointPairInfo;
			AssertContainsExactElementsInAnyOrder(new[] { ("AU", "AUSYD", "Origin", "US", "USORD", "Destination", booking.JS_E_ARV, booking.JS_E_DEP, booking.JS_TransportMode.ToString()) },
				assessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint.Country, u.OriginPoint.UNLOCO, u.OriginPoint.MovementDescription, u.DestinationPoint.Country, u.DestinationPoint.UNLOCO, u.DestinationPoint.MovementDescription, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithBilling()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();

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

			job.JH_Status = JobHeaderStatus.Closed.Code;
			Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
			job.JH_Status = JobHeaderStatus.Complete.Code;
			Assert(!((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
			job.JH_Status = JobHeaderStatus.CustomsProcessActive.Code;
			Assert(((IComplianceItemRiskStatusProvider)shipment).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithETDOrETALessThanOneWeek()
		{
			var shipment = Factory.NewWithValidTestData<AgencyShipment>();

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
				var shipment = Factory.NewWithValidTestData<AgencyShipment>();

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

		void AssertContainsComplianceParty(string description, OrgHeader orgHeader, IScreeningParty[] complianceParties)
		{
			var parties = complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Where(p =>
				p.Description == description).ToList();
			AssertGreaterThan($"{description} - {orgHeader.OH_Code}", parties.Count, 0);

			var party = parties.FirstOrDefault(p => orgHeader.OH_Code == ((p?.Header?.OH_Code ?? p?.OrgCode) ?? ZString.Empty));
			AssertEquals($"{description} - {orgHeader.OH_Code}", orgHeader.OH_Code, party.OrgCode);
		}

		void AssertContainsComplianceCountry(string description, RefCountry expectedCountry, IComplianceLocation[] complianceCountries)
		{
			AssertNotNull($"Precondition: {description} - expectedCountry", expectedCountry);

			var countries = complianceCountries.Where(p => p.ParentsDescription.Contains(description)).ToList();
			AssertGreaterThan($"{description} - {expectedCountry}", countries.Count, 0);

			var country = countries.FirstOrDefault(c => expectedCountry.Code == (c?.Code ?? ZString.Empty));
			AssertEquals($"{description} - {expectedCountry}", expectedCountry.Code, country?.Code);
		}

		void AssertContainsVessel(JobSailing sailing, IScreeningParty[] complianceParties)
		{
			Assert($"Vessel - {sailing.JX_JV_NKVessel}", complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Any(o => o.Vessel != null && o.Vessel.RV_Code == sailing.JX_JV_NKVessel));
		}

		#region Implementation

		protected override IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider() =>
			Factory.NewWithValidTestData<AgencyShipment>();

		protected override ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.SupportInitialization;

		#endregion
	}
}

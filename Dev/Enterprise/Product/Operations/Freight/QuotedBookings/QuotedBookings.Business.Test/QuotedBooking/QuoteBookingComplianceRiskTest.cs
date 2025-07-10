using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Test;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuoteBookingComplianceRiskTest : ComplianceRiskBusinessObjectTestCase
	{
		public void TestCompliancePartiesContainPartyWithNonExistentVessel()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertNotNull("Precondition: QuotedBooking should be ICompliancePartyRiskStatusProvider", quotedBooking);

			SetUpQuotedBooking(quotedBooking, "non-existent vessel", "CLR");

			var complianceParties = ((ICompliancePartyRiskStatusProvider)quotedBooking).Parties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).ToArray();
			Assert(complianceParties.Any(o => o.NotLinkedVessel != null && o.NotLinkedVessel.Code == "non-existent vessel"));
		}

		public void TestQuotedBookingComplianceLocation()
		{
			var booking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			booking.Quote.CurrentOneOffQuote.TT_RL_NKViaLocation = "GBLBA";
			var complianceCountries = ((IComplianceLocationRiskStatusProvider)booking).Locations.ToArray();
			AssertContainsComplianceCountry("Via Country", booking.Quote.CurrentOneOffQuote.ViaLocation.Country, complianceCountries);
		}

		public void TestCompliancePartiesAndCountries()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertNotNull("Precondition: QuotedBooking should be IComplianceItemRiskStatusProvider", quotedBooking);

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

			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			quotedBooking.ClientAddrPK_ZAddress.OrgPK = client.PK;
			AssertNotNull("Precondition", quotedBooking.ClientAddrPK_ZAddress.OrgHeader);
			client.OH_RL_NKClosestPort = "ADALV";

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
			job.JH_ParentID = quotedBooking.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
			job.JH_JobNum = "ABC123";

			quotedBooking.Job.LocalChargesPK = localClient.PK;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_RL_NKClosestPort = "BFNAT";
			quotedBooking.OH_Carrier = carrier.PK;

			var container1 = quotedBooking.QuotedBookingContainers.AddNew();
			var arrivalYard1 = Factory.NewWithValidTestData<OrgHeader>();
			arrivalYard1.OH_RL_NKClosestPort = "BGBEL";
			container1.JC_OA_ArrivalContainerYardAddress = arrivalYard1.MainAddress.PK;
			var departureYard1 = Factory.NewWithValidTestData<OrgHeader>();
			departureYard1.OH_RL_NKClosestPort = "BHALA";
			container1.JC_OA_DepartureContainerYardAddress = departureYard1.MainAddress.PK;

			var container2 = quotedBooking.QuotedBookingContainers.AddNew();
			var arrivalYard2 = Factory.NewWithValidTestData<OrgHeader>();
			arrivalYard2.OH_RL_NKClosestPort = "BIBBZ";
			container2.JC_OA_ArrivalContainerYardAddress = arrivalYard2.MainAddress.PK;
			var departureYard2 = Factory.NewWithValidTestData<OrgHeader>();
			departureYard2.OH_RL_NKClosestPort = "BJKDC";
			container2.JC_OA_DepartureContainerYardAddress = departureYard2.MainAddress.PK;

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_RL_NKClosestPort = "BLGUS";
			quotedBooking.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty.PK;

			var pickupCTO = Factory.NewWithValidTestData<OrgHeader>();
			pickupCTO.OH_RL_NKClosestPort = "BMBDA";
			quotedBooking.ExportReceivingDepot = pickupCTO.MainAddress.PK;

			var deliveryCTO = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCTO.OH_RL_NKClosestPort = "BABUG";
			quotedBooking.ImportReleaseDepot = deliveryCTO.MainAddress.PK;

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_RL_NKClosestPort = "HKHKG";
			shipment.PickupAgentPK = pickupAgent.PK;

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_RL_NKClosestPort = "ITFLR";
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			exportBroker.OH_RL_NKClosestPort = "GRATH";
			shipment.JS_OH_ExportBroker = exportBroker.PK;

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_RL_NKClosestPort = "INBOM";
			shipment.JS_OH_ImportBroker = importBroker.PK;

			var portTransport = Factory.NewWithValidTestData<OrgHeader>();
			portTransport.OH_RL_NKClosestPort = "GUGUM";
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = portTransport.MainAddress.PK;

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_RL_NKClosestPort = "GTMAT";
			shipment.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent.PK;

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

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_RL_NKClosestPort = "BRITJ";
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_RL_NKClosestPort = "USLAX";
			shipment.JS_OH_Creditor = creditor.PK;

			Factory.Save();

			var complianceParties = ((ICompliancePartyRiskStatusProvider)quotedBooking).Parties.ToArray();
			var complianceCountries = ((IComplianceLocationRiskStatusProvider)quotedBooking).Locations.ToArray();

			CombineAssertions(() =>
			{
				AssertContainsVessel(sailing, complianceParties);

				AssertContainsComplianceParty("Client", client, complianceParties);
				AssertContainsComplianceParty("Consignor Documentary Address", consignor, complianceParties);
				AssertContainsComplianceParty("Consignee Documentary Address", consignee, complianceParties);
				AssertContainsComplianceParty("Local Client", localClient, complianceParties);
				AssertContainsComplianceParty("Overseas Agent", overseasAgent, complianceParties);
				AssertContainsComplianceParty("Planned Carrier", carrier, complianceParties);
				AssertContainsComplianceParty("Arrival Container Yard", arrivalYard1, complianceParties);
				AssertContainsComplianceParty("Departure Container Yard", departureYard1, complianceParties);
				AssertContainsComplianceParty("Arrival Container Yard", arrivalYard2, complianceParties);
				AssertContainsComplianceParty("Departure Container Yard", departureYard2, complianceParties);
				AssertContainsComplianceParty("Booking Party Documentary Address", bookingParty, complianceParties);
				AssertContainsComplianceParty("Export CFS", pickupCTO, complianceParties);
				AssertContainsComplianceParty("Import CFS", deliveryCTO, complianceParties);
				AssertContainsComplianceParty("Pickup Agent", pickupAgent, complianceParties);
				AssertContainsComplianceParty("Delivery Agent", deliveryAgent, complianceParties);
				AssertContainsComplianceParty("Export Broker", exportBroker, complianceParties);
				AssertContainsComplianceParty("Import Broker", importBroker, complianceParties);
				AssertContainsComplianceParty("Pickup Local Transport Company", portTransport, complianceParties);
				AssertContainsComplianceParty("Controlling Agent", controllingAgent, complianceParties);
				AssertContainsComplianceParty("Contractor", contractor1, complianceParties);
				AssertContainsComplianceParty("Contractor", contractor2, complianceParties);
				AssertContainsComplianceParty("Notify Party", notifyParty, complianceParties);
				AssertContainsComplianceParty("Creditor", creditor, complianceParties);
				AssertContainsComplianceParty("Service Provider", provider1, complianceParties);
				AssertContainsComplianceParty("Service Provider", provider2, complianceParties);

				AssertContainsComplianceCountry("Client", client.Country, complianceCountries);
				AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, complianceCountries);
				AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, complianceCountries);
				AssertContainsComplianceCountry("Local Client", localClient.Country, complianceCountries);
				AssertContainsComplianceCountry("Overseas Agent", overseasAgent.Country, complianceCountries);
				AssertContainsComplianceCountry("Planned Carrier", carrier.Country, complianceCountries);
				AssertContainsComplianceCountry("Arrival Container Yard", arrivalYard1.Country, complianceCountries);
				AssertContainsComplianceCountry("Departure Container Yard", departureYard1.Country, complianceCountries);
				AssertContainsComplianceCountry("Arrival Container Yard", arrivalYard2.Country, complianceCountries);
				AssertContainsComplianceCountry("Departure Container Yard", departureYard2.Country, complianceCountries);
				AssertContainsComplianceCountry("Booking Party Documentary Address", bookingParty.Country, complianceCountries);
				AssertContainsComplianceCountry("Export CFS", pickupCTO.Country, complianceCountries);
				AssertContainsComplianceCountry("Import CFS", deliveryCTO.Country, complianceCountries);
				AssertContainsComplianceCountry("Pickup Agent", pickupAgent.Country, complianceCountries);
				AssertContainsComplianceCountry("Delivery Agent", deliveryAgent.Country, complianceCountries);
				AssertContainsComplianceCountry("Export Broker", exportBroker.Country, complianceCountries);
				AssertContainsComplianceCountry("Import Broker", importBroker.Country, complianceCountries);
				AssertContainsComplianceCountry("Pickup Local Transport Company", portTransport.Country, complianceCountries);
				AssertContainsComplianceCountry("Controlling Agent", controllingAgent.Country, complianceCountries);
				AssertContainsComplianceCountry("Contractor", contractor1.Country, complianceCountries);
				AssertContainsComplianceCountry("Contractor", contractor2.Country, complianceCountries);
				AssertContainsComplianceCountry("Notify Party", notifyParty.Country, complianceCountries);
				AssertContainsComplianceCountry("Creditor", creditor.Country, complianceCountries);
				AssertContainsComplianceCountry("Service Provider", provider1.Country, complianceCountries);
				AssertContainsComplianceCountry("Service Provider", provider2.Country, complianceCountries);
				AssertContainsComplianceCountry("Sailing Load Country", origin.Country, complianceCountries);
				AssertContainsComplianceCountry("Sailing Discharge Country", destination.Country, complianceCountries);
			});

			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			complianceParties = ((ICompliancePartyRiskStatusProvider)quotedBooking).Parties.ToArray();
			AssertContainsVessel(sailing, complianceParties, false);
		}

		public void TestComplianceWhenOverrideAddressesCountryIncludedInLocationRisk()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKLoadPort = "BZSPR";
			shipment.JS_RL_NKDischargePort = "CLESR";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_RL_NKClosestPort = "FKFBE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_RL_NKClosestPort = "FMEAU";

			var countries = ((IComplianceLocationRiskStatusProvider)quotedBooking).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", consignor.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", consignee.Country, countries);

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "GB";

			shipment.ConsignorDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "IR";

			countries = ((IComplianceLocationRiskStatusProvider)quotedBooking).Locations.ToArray();

			AssertContainsComplianceCountry("Consignor Documentary Address", shipment.ConsignorDocumentaryAddress.Country, countries);
			AssertContainsComplianceCountry("Consignee Documentary Address", shipment.ConsigneeDocumentaryAddress.Country, countries);
		}

		public void TestComplianceCommodities()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertNotNull("Precondition: QuotedBooking should be IComplianceCommodityRiskStatusProvider", quotedBooking);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "840140";
			packLine1.JL_RN_NKOrigin = "AU";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "880260";

			Factory.Save();

			var complianceCommodities = ((IComplianceCommodityRiskStatusProvider)quotedBooking).Commodities.ToArray();

			AssertEquals("Precondition: Commodities Count", 2, complianceCommodities.Length);

			var commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine1.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 1", commodity);
			AssertEquals("Commodity from PackLine 1", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 1", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 1", ((IComplianceCommodityRiskStatusProvider)quotedBooking).ParentID, commodity.ParentJobID);
			AssertEquals("Commodity from PackLine 1", "AU", commodity.Origin);
			AssertEquals("Commodity from PackLine 1", "Packing", commodity.CommoditySource);

			commodity = complianceCommodities.FirstOrDefault(x => x.HarmonizedCode == packLine2.JL_HarmonisedCode);
			AssertNotNull("Commodity from PackLine 2", commodity);
			AssertEquals("Commodity from PackLine 2", shipment.JS_UniqueConsignRef, commodity.Source);
			AssertEquals("Commodity from PackLine 2", "WCO", commodity.GroupingOrCountry);
			AssertEquals("Commodity from PackLine 2", ((IComplianceCommodityRiskStatusProvider)quotedBooking).ParentID, commodity.ParentJobID);
			AssertEquals("Commodity from PackLine 2", ZString.Empty, commodity.Origin);
			AssertEquals("Commodity from PackLine 2", "Packing", commodity.CommoditySource);
		}

		[TestDate(2022, 10, 15)]
		public void TestComplianceEffectiveDate()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertNotNull("Precondition: QuotedBooking should be IComplianceCommodityRiskStatusProvider", quotedBooking);

			AssertEquals("Precondition", ZDateTime.Empty, quotedBooking.Booking.JS_E_DEP);
			AssertEquals("Effective Date should equals Now", ZDateTime.Now, ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);

			quotedBooking.Booking.JS_E_DEP = new ZDateTime(2022, 1, 9);
			AssertEquals("Effective Date should be Booking ETD", new ZDateTime(2022, 1, 9), ((IComplianceCommodityRiskStatusProvider)shipment).EffectiveDate);
		}

		public void TestComplianceRiskSupport()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			AssertEquals(true, ((IComplianceItemRiskStatusProvider)quotedBooking).ComplianceRiskSupport.IsSupportInitialization());
			AssertEquals(false, ((IComplianceItemRiskStatusProvider)quotedBooking).ComplianceRiskSupport.IsSupportSubCompliances());
		}

		public void TestSubComplianceRiskStatusProviders()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var provider = quotedBooking as IComplianceItemRiskStatusProvider;

			AssertEquals(0, provider.SubComplianceRiskStatusProviders.Count());
		}

		public void TestParentComplianceRiskStatusProviders()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var provider = quotedBooking as IComplianceItemRiskStatusProvider;

			AssertEquals(0, provider.ParentComplianceRiskStatusProviders.Count());
		}

		public void TestAssessmentPointPairInfo()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			shipment.JS_E_ARV = new ZDateTime(2024, 1, 12);
			shipment.JS_E_DEP = new ZDateTime(2024, 1, 2);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
			var provider = quotedBooking as IComplianceCommodityRiskStatusProvider;

			var assessmentPointPairInfo = provider.AssessmentPointPairInfo;
			AssertContainsExactElementsInAnyOrder(new[] { ("AU", "AUSYD", "Origin", "US", "USORD", "Destination", quotedBooking.ETA, quotedBooking.ETD, quotedBooking.TransportMode.ToString()) },
					assessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint.Country, u.OriginPoint.UNLOCO, u.OriginPoint.MovementDescription, u.DestinationPoint.Country, u.DestinationPoint.UNLOCO, u.DestinationPoint.MovementDescription, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithBilling()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = quotedBooking.PK;
			job.JH_JobNum = "ABC123";

			quotedBooking.ETD = new ZDateTime(2024, 7, 26);
			quotedBooking.ETA = ZDateTime.Empty;

			job.JH_Status = JobHeaderStatus.Complete.Code;
			Assert(!((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Assert(!((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);
			job.JH_Status = JobHeaderStatus.CustomsProcessActive.Code;
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithWithETDOrETALessThanOneWeek()
		{
			var shipment = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = quotedBooking.PK;
			job.JH_JobNum = "ABC123";

			Factory.Save();

			var relatedBizOs = ((IStmALogParent)quotedBooking).BusinessObjectsWithRelatedEvents.Append(quotedBooking);
			foreach (var bizO in relatedBizOs.Where(item => item is IAuditDetails))
			{
				if (bizO is AutoJobDocsAndCartage autoJobDocsAndCartage)
				{
					autoJobDocsAndCartage.JP_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
				else if (bizO is AutoJobHeader autoJobHeader)
				{
					autoJobHeader.JH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
				else if (bizO is AutoJobShipment autoJobShipment)
				{
					autoJobShipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
			}

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 25);
			shipment.JS_E_ARV = ZDateTime.Empty;
			shipment.JS_SystemLastEditTimeUtc = ZDateTime.Now.AddMonths(-13);
			Assert(!((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			shipment.JS_SystemLastEditTimeUtc = ZDateTime.Now.AddMonths(-13);
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 8, 8);
			shipment.JS_E_ARV = ZDateTime.Empty;
			shipment.JS_SystemLastEditTimeUtc = ZDateTime.Now.AddMonths(-13);
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 8, 7);
			shipment.JS_E_ARV = ZDateTime.Empty;
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = new ZDateTime(2024, 8, 7);
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = new ZDateTime(2024, 8, 8);
			Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);
		}

		[TestDate(2024, 08, 01)]
		public void TestComplianceCurrentJobStatusWithAddEventTime()
		{
			using (OrganisationsDataRegistry.Instance.JobUpdatePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 12))
			{
				var shipment = QuotedBooking.CreateNewBooking(Factory);
				var quotedBooking = CreateNewQuotedBooking(ZGuid.Empty, shipment.PK);
				shipment.JS_E_DEP = ZDateTime.Empty;
				shipment.JS_E_ARV = ZDateTime.Empty;

				var job = Factory.NewJobForTesting<JobHeader>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_ParentID = quotedBooking.PK;
				job.JH_JobNum = "ABC123";

				Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

				Factory.Save();

				AssertEquals("Logs count", 1, shipment.Logs.GetAllLogs().Count);

				var addEvent = shipment.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
				Assert(!addEvent.SL_EventTime.IsEmpty);

				shipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

				Factory.Save();
				Assert(!((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);

				shipment.JS_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-11);

				Factory.Save();
				Assert(((IComplianceItemRiskStatusProvider)quotedBooking).JobTime.IsCurrent);
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

		void AssertContainsVessel(JobSailing sailing, IScreeningParty[] complianceParties, bool contains = true)
		{
			AssertEquals($"Vessel - {sailing.JX_JV_NKVessel}",
				contains,
				complianceParties.Select(o => ScreeningParty.ConvertFromComplianceParty(o.Parent, o.Description, o.Party, o.NaturalPerson)).Any(o => o.Vessel != null && o.Vessel.RV_Code == sailing.JX_JV_NKVessel));
		}

		void AssertContainsComplianceCountry(string description, RefCountry expectedCountry, IComplianceLocation[] complianceCountries)
		{
			AssertNotNull($"Precondition: {description} - expectedCountry", expectedCountry);

			var countries = complianceCountries.Where(p => p.ParentsDescription.Contains(description)).ToList();
			AssertGreaterThan($"{description} - {expectedCountry}", countries.Count, 0);

			var country = countries.FirstOrDefault(c => expectedCountry.Code == (c?.Code ?? ZString.Empty));
			AssertEquals($"{description} - {expectedCountry}", expectedCountry.Code, country?.Code);
		}

		#region Implementation

		protected override IComplianceItemRiskStatusProvider GetComplianceItemRiskStatusProvider()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var currentTestQuotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);

			return currentTestQuotedBooking;
		}

		protected virtual QuotedBooking CreateNewQuotedBooking(ZGuid quotePK, ZGuid bookingPK)
		{
			return QuotedBooking.New(quotePK, bookingPK, Factory);
		}

		protected void SetUpQuotedBooking(QuotedBooking quotedBooking, ZString vesselName, ZString screeningStatus)
		{
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
			((ISailingChooserParent)quotedBooking).SailingJX = sailing.PK;

			jobVoyage.JV_RV_NKVessel = vesselName;
			quotedBooking.Booking.JS_BookedVesselScreeningStatus = screeningStatus;
		}

		protected override ComplianceRiskSupport SupporterInfo_DoNotModifyThisBeforeCheckingBaseImplementation => ComplianceRiskSupport.SupportInitialization;

		#endregion
	}
}

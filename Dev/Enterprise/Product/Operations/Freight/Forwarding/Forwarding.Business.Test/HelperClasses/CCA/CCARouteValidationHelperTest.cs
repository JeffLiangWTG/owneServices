using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CCARouteValidationHelperTest : TestCaseWithFactory
	{
		public void TestIsRouteAssignmentMissingCarrierContract()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			Assert("Precondition: consol has no carrier contract number", consol.JK_CarrierContractNumber.IsEmpty);

			AssertEquals("Route is assigned wth no Contract Number", true, CCARouteValidationHelper.IsRouteAssignmentMissingCarrierContract(consol));

			var contract = Factory.New<IRatingContract>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			contract.RCT_ContractNumber = "BLAHAJ";
			contract.RCT_ContractType = "PRO";
			contract.RCT_OH = carrier.PK;

			consol.JK_CarrierContractNumber = "BLAHAJ";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Consol is now linked to valid Contract too", false, CCARouteValidationHelper.IsRouteAssignmentMissingCarrierContract(consol));
		}

		public void TestIsInvalidAllocationRouteAssigned()
		{
			AssertEquals("No Allocation Route belongs to GUID assigned", true, CCARouteValidationHelper.IsInvalidAllocationRouteAssigned(ZGuid.NewZGuid(), Factory));

			var pk = Factory.New<IRatingContractAllocationLine>().PK;
			AssertEquals("Allocation Route belongs to GUID assigned", false, CCARouteValidationHelper.IsInvalidAllocationRouteAssigned(pk, Factory));
		}

		public void TestIsRouteAssignedUnderParentContractAssigned()
		{
			var route = Factory.New<IRatingContractAllocationLine>();
			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "WAH";
			contract.RCT_ContractType = "PRO";
			var carrier = Factory.New<OrgHeader>();
			contract.RCT_OH = carrier.PK;

			consol.JK_RCA_AllocationLine = route.PK;
			consol.JK_CarrierContractNumber = "WAH";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Route and Contract are unaligned", false, CCARouteValidationHelper.IsRouteAssignedUnderParentContractAssigned(consol.JK_RCA_AllocationLine, consol.CarrierContract));

			route.RCA_RCT_RatingContract = contract.PK;

			AssertEquals("Route and Contract are aligned", true, CCARouteValidationHelper.IsRouteAssignedUnderParentContractAssigned(consol.JK_RCA_AllocationLine, consol.CarrierContract));
		}

		public void TestIsETDBeforeRouteStartDate()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			allocationRoute.RCA_StartDate = new ZDate(2022, 4, 2);

			AssertEquals("ETD of Consol is before the Start Date of the Allocation Route.", true, CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, consol));

			transport.JW_ETD = new ZDateTime(2022, 4, 6);
			AssertEquals("ETD of Consol is after the Start Date of the Allocation Route.", false, CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, consol));
		}

		public void TestIsETDAfterRouteExpiryDate()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			allocationRoute.RCA_StartDate = new ZDate(2020, 1, 2);
			allocationRoute.RCA_ExpiryDate = new ZDate(2020, 2, 12);

			AssertEquals("ETD of Consol is after the Expiry Date of the Allocation Route.", true, CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, consol));

			transport.JW_ETD = new ZDateTime(2020, 1, 6);
			AssertEquals("ETD of Consol is before the Expiry Date of the Allocation Route.", false, CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, consol));
		}

		public void TestIsETDBeforeRouteStartDate_ShouldFallbackIfBlank()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			allocationRoute.RCA_StartDate = ZDate.Empty;
			contract.RCT_StartDate = new ZDate(2022, 4, 2);

			AssertEquals("ETD of Consol is before the Start Date of the Allocation Route.", true, CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, consol));

			transport.JW_ETD = new ZDateTime(2022, 4, 6);
			AssertEquals("ETD of Consol is after the Start Date of the Allocation Route.", false, CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, consol));
		}

		public void TestIsETDAfterRouteExpiryDate_ShouldFallbackIfBlank()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			allocationRoute.RCA_StartDate = new ZDate(2020, 1, 2);
			allocationRoute.RCA_ExpiryDate = ZDate.Empty;
			contract.RCT_EndDate = new ZDate(2020, 2, 12);

			AssertEquals("ETD of Consol is after the Expiry Date of the Allocation Route.", true, CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, consol));

			transport.JW_ETD = new ZDateTime(2020, 1, 6);
			AssertEquals("ETD of Consol is before the Expiry Date of the Allocation Route.", false, CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, consol));
		}

		public void TestIsLoadPortNotCoveredByRouteLoadLocation()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "ACCIO";
			allocationRoute.RCA_LoadLocation = "LUMOS";

			AssertEquals("Load Port of Consol should not match Allocation Route Load Port.", true, CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(allocationRoute, consol));

			transport.JW_RL_NKLoadPort = "LUMOS";
			AssertEquals("Load Port of Consol should match Allocation Route Load Port.", false, CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(allocationRoute, consol));
		}

		public void TestIsDischargePortNotCoveredByRouteDischargeLocation()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKDiscPort = "ACCIO";
			allocationRoute.RCA_DischargeLocation = "LUMOS";

			AssertEquals("Discharge Port of Consol should not match Allocation Route Discharge Port.", true, CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(allocationRoute, consol));

			transport.JW_RL_NKDiscPort = "LUMOS";
			AssertEquals("Discharge Port of Consol should match Allocation Route Discharge Port.", false, CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(allocationRoute, consol));
		}

		public void TestIsVoyageNumberMismatch()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_VoyageFlight = "CRUCIO";
			allocationRoute.RCA_VoyageNumber = "ALOHOMORA";

			AssertEquals("Voyage of Allocation Route should not match with Voyage of Consol.", true, CCARouteValidationHelper.IsVoyageNumberMismatch(allocationRoute, consol));

			transport.JW_VoyageFlight = "ALOHOMORA";

			AssertEquals("Voyage of Allocation Route should match with Voyage of Consol.", false, CCARouteValidationHelper.IsVoyageNumberMismatch(allocationRoute, consol));
		}

		public void TestIsVesselMismatch()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_Vessel = "WEASLEY";
			allocationRoute.RCA_RV_NKVessel = "BLOTTS";

			AssertEquals("Vessel of Allocation Route should not match with Vessel of Consol.", true, CCARouteValidationHelper.IsVesselMismatch(allocationRoute, consol));

			transport.JW_Vessel = "BLOTTS";
			AssertEquals("Vessel of Allocation Route should match with Vessel of Consol.", false, CCARouteValidationHelper.IsVesselMismatch(allocationRoute, consol));
		}

		public void TestIsGatewayAgentAssigned()
		{
			consol.JK_SendingForwarderHandlingType = "YAA";
			consol.JK_ReceivingForwarderHandlingType = "AAH";

			AssertEquals("Consol does not have a Gateway Agent assigned", false, CCARouteValidationHelper.IsGatewayAgentAssigned(consol));

			consol.JK_ReceivingForwarderHandlingType = "GTT";
			AssertEquals("Consol does have a Gateway Agent assigned", true, CCARouteValidationHelper.IsGatewayAgentAssigned(consol));
		}

		public void TestIsValidSendingOrReceivingAgent()
		{
			var randomOrg = Factory.NewWithValidTestData<OrgHeader>();
			var agent = Factory.NewWithValidTestData<OrgHeader>();

			allocationRoute.AgentPivots.AddRelatedIfNotExist(agent);

			consol.JK_OA_SendingForwarderAddress = randomOrg.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg.MainAddress.PK;
			AssertEquals("Sending and Receiving Agent does not match with the Agents specified on Allocation Route", false, CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute));

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			AssertEquals("Sending Agent matches with the Agents specified on Allocation Route", true, CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute));

			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			AssertEquals("Receiving Agent matches with the Agents specified on Allocation Route", true, CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute));

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.GC_OH_OrgProxy = agent.PK;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;
			AssertEquals("Sending Agent matches with the Agents specified on Allocation Route", true, CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute));

			consol.JK_OA_SendingForwarderAddress = randomOrg.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = branchProxy.MainAddress.PK;
			AssertEquals("Receiving Agent matches with the Agents specified on Allocation Route", true, CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute));
		}

		public void TestIsValidContainerOwnerAssigned()
		{
			allocationRoute.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;

			var container = consol.Containers.AddNew();
			container.JC_IsShipperOwned = false;
			container.JC_RCA_AllocationLine = allocationRoute.PK;

			AssertEquals("Container is not Shipper Owned", false, CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container));

			container.JC_IsShipperOwned = true;
			AssertEquals("Container is Shipper Owned", true, CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container));

			allocationRoute.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			AssertEquals("Container is Shipper Owned", false, CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container));

			container.JC_IsShipperOwned = false;
			AssertEquals("Container is not Shipper Owned", true, CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container));
		}

		public void TestJobHasContainersNotMatchingAllocationContainerType()
		{
			var container = consol.Containers.AddNew();
			var refContainer1 = Factory.New<RefContainer>();
			var refContainer2 = Factory.New<RefContainer>();
			container.JC_RC = refContainer1.PK;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RC_ContainerType = refContainer1.PK;

			AssertEquals("Container Type Matches", false, CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, consol));

			container.JC_RC = refContainer2.PK;

			AssertEquals("Container Type does not Match", true, CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, consol));
		}

		public void TestIsContainerTypeInvalidForContract()
		{
			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContainerType = "AAA";

			var container = consol.Containers.AddNew();
			var refContainer = Factory.New<RefContainer>();
			container.JC_RC = refContainer.PK;
			refContainer.RC_ContainerType = "AAA";

			AssertEquals("Container type is valid", false, CCARouteValidationHelper.IsContainerTypeInvalidForContract(contract, container));

			refContainer.RC_ContainerType = "BBB";

			AssertEquals("Container type is invalid", true, CCARouteValidationHelper.IsContainerTypeInvalidForContract(contract, container));
		}

		public void TestHasNoMatchingServiceStringTransportLeg()
		{
			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_ServiceString = "SNAPE";
			allocationRoute.RCA_ServiceLoop = "GRANGER";

			AssertEquals("Service String of Allocation Route does not match with Service String of Consol.", true, CCARouteValidationHelper.HasNoMatchingServiceStringTransportLeg(allocationRoute, consol));

			transport.JW_ServiceString = "GRANGER";
			AssertEquals("Service String of Allocation Route does match with Service String of Consol.", false, CCARouteValidationHelper.HasNoMatchingServiceStringTransportLeg(allocationRoute, consol));
		}

		public void TestIsConsolInvalidForAllocationRouteNamedAccounts()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "GINNY";

			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			contract.RCT_OH = carrier.PK;

			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			var message = CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, consol);
			AssertEquals("Attached shipment does not have matching Named Account.", true, CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, consol));

			shipment.ConsigneePK = namedAccount.PK;
			AssertEquals("Attached shipment has matching Named Account.", false, CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, consol));
		}

		public void TestIsBookingInvalidForAllocationRouteNamedAccounts()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			var namedAccount = allocationRoute.NamedAccountPivots.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			namedAccount.RNP_OH_NamedAccount = orgHeader.PK;

			var bookingMock = new Mock<IQuotedBooking>();
			AssertEquals("Booking does not have Named Accounts setup", true, CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, bookingMock.Object));

			var orgMock = new Mock<IOrgHeader>();
			orgMock.Setup(org => org.PK).Returns(orgHeader.PK);

			bookingMock.Setup(booking => booking.ControllingCustomer).Returns(orgMock.Object);

			AssertEquals("Booking does have Named Accounts setup", false, CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, bookingMock.Object));
		}

		public void TestIsBookingInvalidForAllocationRouteNamedAccounts_WithMultiple()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			var namedAccount1 = allocationRoute.NamedAccountPivots.AddNew();
			var namedAccount2 = allocationRoute.NamedAccountPivots.AddNew();
			var namedAccount3 = allocationRoute.NamedAccountPivots.AddNew();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			namedAccount1.RNP_OH_NamedAccount = orgHeader1.PK;
			namedAccount2.RNP_OH_NamedAccount = orgHeader2.PK;
			namedAccount3.RNP_OH_NamedAccount = orgHeader3.PK;

			var bookingMock = new Mock<IQuotedBooking>();
			AssertEquals("Booking does not have Named Accounts setup", true, CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, bookingMock.Object));

			var orgMock = new Mock<IOrgHeader>();
			orgMock.Setup(org => org.PK).Returns(orgHeader2.PK);

			bookingMock.Setup(booking => booking.ControllingCustomer).Returns(orgMock.Object);

			AssertEquals("Booking does have Named Accounts setup", false, CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, bookingMock.Object));
		}

		public void TestIsBookingInvalidForAllocationRouteNamedAccounts_WithNone()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();

			var bookingMock = new Mock<IQuotedBooking>();

			AssertEquals("Booking does not have any Named Accounts", false, CCARouteValidationHelper.IsBookingInvalidForAllocationRouteNamedAccounts(allocationRoute, bookingMock.Object));
		}

		public void TestDoAnyConsolTransportLegsMatchAllocationRouteSchedule()
		{
			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			var voyage = Factory.New<JobVoyage>();
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var carrier = Factory.New<OrgHeader>();

			voyage.JV_VoyageFlight = "WOO";
			voyage.JV_RV_NKVessel = vessel.RV_Code;
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var today = ZDateTime.Today;
			origin.JA_E_DEP = today;

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_ServiceString = "The coolest route";

			allocationRoute.RCA_JX_SailingSchedule = sailing.PK;

			var transportLeg1 = consol.Transports.AddNew();
			var transportLeg2 = consol.Transports.AddNew();

			AssertEquals("No transport legs match", false, CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol));

			transportLeg1.JW_ETD = today;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";
			transportLeg1.JW_VoyageFlight = "WOO";
			transportLeg1.JW_Vessel = vessel.RV_Code;
			transportLeg1.JW_ServiceString = "The coolest route";
			transportLeg1.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			AssertEquals("Transport leg match", true, CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteLoadLocation()
		{
			var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
			var seaLeg = consolidation.Transports[0];
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			seaLeg.JW_ETD = ZDateTime.Today;

			var contract = Factory.New<IRatingContract>();
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_StartDate = ZDateTime.Today.AddDays(-1).Date;
			allocationRoute.RCA_ExpiryDate = ZDateTime.Today.AddDays(1).Date;
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			seaLeg.JW_RL_NKLoadPort = "AUSYD";

			Assert("Consol has valid route for allocation route", CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consolidation));

			seaLeg.JW_RL_NKLoadPort = "AUBNE";
			consolidation.JK_RL_NKLoadPort = "AUSYD";

			Assert("Consol has first load matching", CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consolidation));

			consolidation.JK_RL_NKLoadPort = "AUBNE";

			Assert("Consol does not match load port", !CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consolidation));
		}

		public void TestIsConsolValidForRouteLoadLocation_WithRouteSets()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "AUPER";

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var modeValidLeg = consol.Transports[0];
			modeValidLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			modeValidLeg.JW_RL_NKLoadPort = "AUMEL";
			modeValidLeg.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals("Precondition", 1, modeValidLeg.RouteSetNumber);
			AssertEquals(
				"Route set does not contain load valid leg",
				false,
				CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consol));

			var loadValidLeg = consol.Transports.AddNew();
			loadValidLeg.JW_TransportMode = Core.Constants.TransportModes.Rail;
			loadValidLeg.JW_RL_NKLoadPort = "AUSYD";
			loadValidLeg.JW_RL_NKDiscPort = "AUPER";

			Assert("Precondition", consol.Transports.All(t => ((Transport)t).RouteSetNumber == 1));
			AssertEquals(
				"Route set contains a mode valid leg and a load valid leg",
				true,
				CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteDischargeLocation()
		{
			var consolidation = Factory.NewWithValidTestData<ForwardingConsol>();
			var seaLeg = consolidation.Transports[0];
			seaLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;

			var contract = Factory.New<IRatingContract>();
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			var allocationRoute = Factory.New<IRatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			allocationRoute.RCA_StartDate = ZDateTime.Today.AddDays(-1).Date;
			allocationRoute.RCA_ExpiryDate = ZDateTime.Today.AddDays(1).Date;
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			seaLeg.JW_RL_NKDiscPort = "NZAKL";

			Assert("Consol has valid route for allocation route", CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consolidation));

			seaLeg.JW_RL_NKLoadPort = "AUBNE";
			consolidation.JK_RL_NKDischargePort = "NZAKL";

			Assert("Consol has last discharge matching", CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consolidation));

			consolidation.JK_RL_NKDischargePort = "AUBNE";

			Assert("Consol does not have discharge matching", !CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consolidation));
		}

		public void TestIsConsolValidForRouteDischargeLocation_WithRouteSets()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "AUPER";

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var modeValidLeg = consol.Transports[0];
			modeValidLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			modeValidLeg.JW_RL_NKLoadPort = "AUMEL";
			modeValidLeg.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals("Precondition", 1, modeValidLeg.RouteSetNumber);
			AssertEquals(
				"Route set does not contain discharge valid leg",
				false,
				CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consol));

			var discValidLeg = consol.Transports.AddNew();
			discValidLeg.JW_TransportMode = Core.Constants.TransportModes.Rail;
			discValidLeg.JW_RL_NKLoadPort = "AUSYD";
			discValidLeg.JW_RL_NKDiscPort = "AUPER";

			Assert("Precondition", consol.Transports.All(t => ((Transport)t).RouteSetNumber == 1));
			AssertEquals(
				"Route set contains a mode valid leg and a discharge valid leg",
				true,
				CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consol));
		}

		public void TestIsConsolFirstLoadValidForPlaceOfReceipt()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			allocationRoute.RCA_PlaceOfReceipt = "AUSYD";
			allocationRoute.RCA_PlaceOfDelivery = "CNSHA";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				Assert(
					"The Place Of Receipt matches the 1st Load Location",
					CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, consol));

				consol.JK_RL_NKLoadPort = "AU";
				Assert(
					"The Place Of Receipt matches the 1st Load Location",
					CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, consol));

				consol.JK_RL_NKLoadPort = "CNSHA";
				Assert(
					"The Place Of Receipt does not match the 1st Load Location",
					!CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, consol));

				consol.JK_RL_NKLoadPort = "CN";
				Assert(
					"The Place Of Receipt does not match the 1st Load Location",
					!CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, consol));
			}
		}

		public void TestIsConsolDischargeValidForPlaceOfDelivery()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			allocationRoute.RCA_PlaceOfReceipt = "AUSYD";
			allocationRoute.RCA_PlaceOfDelivery = "CNSHA";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_RL_NKDischargePort = "CNSHA";
				Assert(
					"The Place Of Delivery matches the Last Discharge Location",
					CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, consol));

				consol.JK_RL_NKDischargePort = "CN";
				Assert(
					"The Place Of Delivery matches the Last Discharge Location",
					CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, consol));

				consol.JK_RL_NKDischargePort = "AUSYD";
				Assert(
					"The Place Of Delivery does not match the Last Discharge Location",
					!CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, consol));

				consol.JK_RL_NKDischargePort = "AU";
				Assert(
					"The Place Of Delivery does not match the Last Discharge Location",
					!CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, consol));
			}
		}

		public void TestIsConsolValidForRouteValidDateRanges_NoDateAndModeValidRouteSets()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var dateLoadValidLeg = consol.Transports[0];
			dateLoadValidLeg.JW_TransportMode = Core.Constants.TransportModes.Rail;
			dateLoadValidLeg.JW_RL_NKLoadPort = "AUSYD";
			dateLoadValidLeg.JW_RL_NKDiscPort = "AUPER";
			dateLoadValidLeg.JW_ETD = allocationRoute.RCA_StartDate.AddDays(1);

			var modeDiscValidLeg = consol.Transports.AddNew();
			modeDiscValidLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			modeDiscValidLeg.JW_RL_NKLoadPort = "NZAKL";
			modeDiscValidLeg.JW_RL_NKDiscPort = "CNSHA";
			modeDiscValidLeg.JW_ETD = allocationRoute.RCA_ExpiryDate.AddDays(1);

			AssertEquals(1, dateLoadValidLeg.RouteSetNumber);
			AssertEquals(2, modeDiscValidLeg.RouteSetNumber);
			Assert(
				"should return false when route sets match date and mode",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			modeDiscValidLeg.JW_RL_NKLoadPort = "AUPER";
			AssertEquals(1, dateLoadValidLeg.RouteSetNumber);
			AssertEquals(1, modeDiscValidLeg.RouteSetNumber);
			Assert(CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteValidDateRanges_ShouldNotConsiderTime()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CNSHA";

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = new ZDate(2001, 9, 10);
			allocationRoute.RCA_ExpiryDate = new ZDate(2001, 9, 12);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var validLeg = consol.Transports[0];
			validLeg.JW_TransportMode = Core.Constants.TransportModes.Sea;
			validLeg.JW_RL_NKLoadPort = "AUSYD";
			validLeg.JW_RL_NKDiscPort = "CNSHA";
			validLeg.JW_ETD = new ZDateTime(2001, 9, 12);

			Assert("Should be valid since same ETD as end date", CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			validLeg.JW_ETD = new ZDateTime(2001, 9, 12, 0, 0, 1);
			Assert("Time should not be considered when validating date ranges", CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteValidDateRanges_LoadPort_IsAttachingFalse()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			consol.JK_RL_NKLoadPort = "NZTRG";

			var discSetLeg1 = consol.Transports[0];
			discSetLeg1.JW_ETD = allocationRoute.RCA_StartDate.AddDays(3);
			discSetLeg1.JW_RL_NKLoadPort = "NZAKL";
			discSetLeg1.JW_RL_NKDiscPort = "NZCHC";

			var discSetLeg2 = consol.Transports.AddNew();
			discSetLeg2.JW_TransportMode = contract.RCT_TransportMode;
			discSetLeg2.JW_ETD = allocationRoute.RCA_StartDate.AddDays(4);
			discSetLeg2.JW_RL_NKLoadPort = "NZCHC";
			discSetLeg2.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;

			AssertEquals(1, discSetLeg1.RouteSetNumber);
			AssertEquals(1, discSetLeg2.RouteSetNumber);

			Assert(
				"should return false when no matching load port",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			consol.JK_RL_NKLoadPort = "AUSYD";
			Assert(
				"should return true when consol has matching load port",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			consol.JK_RL_NKLoadPort = ZString.Empty;
			Assert(
				"should return false when no matching load port and consol load is not going to being populated",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			var loadSetLeg1 = consol.Transports.AddNew();
			loadSetLeg1.JW_TransportMode = Core.Constants.TransportModes.Rail;
			loadSetLeg1.JW_ETD = allocationRoute.RCA_StartDate.AddDays(5);
			loadSetLeg1.JW_RL_NKLoadPort = "AUPER";
			loadSetLeg1.JW_RL_NKDiscPort = "AUSYD";

			var loadSetLeg2 = consol.Transports.AddNew();
			loadSetLeg2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			loadSetLeg2.JW_TransportMode = contract.RCT_TransportMode;
			loadSetLeg2.JW_ETD = allocationRoute.RCA_StartDate.AddDays(5);
			loadSetLeg2.JW_RL_NKLoadPort = "AUSYD";
			loadSetLeg2.JW_RL_NKDiscPort = "AUMEL";

			AssertEquals(2, loadSetLeg1.RouteSetNumber);
			AssertEquals(2, loadSetLeg2.RouteSetNumber);
			Assert(
				"should return false when matching load route set found but is after matching discharge set",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			loadSetLeg1.JW_ETD = discSetLeg1.JW_ETD.AddDays(-1);
			Assert(
				"should return true when matching load route set found that is before matching discharge set",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteValidDateRanges_LoadPort_IsAttachingTrue()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AU";
			allocationRoute.RCA_DischargeLocation = "CNSHA";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var validDiscLeg = consol.Transports[0];
			validDiscLeg.JW_TransportMode = contract.RCT_TransportMode;
			validDiscLeg.JW_ETD = allocationRoute.RCA_StartDate;
			validDiscLeg.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;

			consol.JK_RL_NKLoadPort = "NZAKL";
			Assert(
				"should return false when consol load does not match and no matching load route sets",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			consol.JK_RL_NKLoadPort = ZString.Empty;
			Assert(
				"should return false when consol load is empty and isAttaching is true but route load is not a UNLOCO",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			allocationRoute.RCA_LoadLocation = "AUSYD";
			Assert(
				"should return true when consol load is empty and isAttaching is true and route load is a UNLOCO",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			consol.Transports.AddNew();
			Assert(
				"should return false when consol has more than one transport",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));
		}

		public void TestIsConsolValidForRouteValidDateRanges_DiscPort_IsAttachingFalse()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "AUPER";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var leg1 = consol.Transports[0];
			leg1.JW_TransportMode = contract.RCT_TransportMode;
			leg1.JW_ETD = allocationRoute.RCA_StartDate.AddDays(3);
			leg1.JW_RL_NKLoadPort = allocationRoute.RCA_LoadLocation;
			leg1.JW_RL_NKDiscPort = "AUABP";

			consol.JK_RL_NKDischargePort = "NZAKL";
			Assert(
				"should return false when no matching discharge port",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			consol.JK_RL_NKDischargePort = allocationRoute.RCA_DischargeLocation;
			Assert(
				"should return true when matching consol discharge port",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			consol.JK_RL_NKDischargePort = ZString.Empty;
			Assert(
				"should return false when no matching disc port and not isAttaching",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			leg1.JW_RL_NKDiscPort = "AUABP";

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = contract.RCT_TransportMode;
			leg2.JW_ETD = leg1.JW_ETD.AddDays(-1);
			leg2.JW_RL_NKLoadPort = "AUABX";
			leg2.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;

			AssertEquals(1, leg1.RouteSetNumber);
			AssertEquals(1, leg2.RouteSetNumber);
			Assert(
				"should return false because matching load leg departs after matching disc leg",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			allocationRoute.RCA_DischargeLocation = "CNSHA";
			leg2.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;

			Assert(leg1.RouteSetNumber != leg2.RouteSetNumber);
			Assert(
				"should return false because matching load leg list departs after matching disc leg",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));

			leg2.JW_ETD = leg1.JW_ETD.AddDays(1);
			Assert("should return true because matching load leg list precedes matching disc leg",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol));
		}

		public void TestIsConsolValidForRouteValidDateRanges_DiscPort_IsAttachingTrue()
		{
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "CN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);

			AssertEquals("Precondition", 1, consol.Transports.Count);

			var leg1 = consol.Transports[0];
			leg1.JW_TransportMode = contract.RCT_TransportMode;
			leg1.JW_ETD = allocationRoute.RCA_StartDate.AddDays(3);
			leg1.JW_RL_NKLoadPort = allocationRoute.RCA_LoadLocation;

			consol.JK_RL_NKDischargePort = "NZAKL";
			Assert(
				"should return false when consol discharge does not match and no matching discharge route sets",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			consol.JK_RL_NKDischargePort = ZString.Empty;
			Assert(
				"should return false when consol discharge is empty and isAttaching is true but route discharge is not a UNLOCO",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			allocationRoute.RCA_DischargeLocation = "CNSHA";
			Assert(
				"should return true when consol discharge is empty and isAttaching is true and route discharge is a UNLOCO",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			consol.JK_RL_NKLoadPort = ZString.Empty;
			consol.JK_RL_NKDischargePort = "NZAKL";
			leg1.JW_RL_NKLoadPort = "AUPER";
			leg1.JW_RL_NKDiscPort = allocationRoute.RCA_DischargeLocation;

			Assert(
				"should return true when consol load is empty and single leg has valid discharge",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			leg1.JW_TransportMode = Core.Constants.TransportModes.Rail;
			Assert(
				"should return false when consol load is empty but no date + mode valid leg departs on-or-before valid discharge leg",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			leg1.JW_TransportMode = contract.RCT_TransportMode;
			consol.Transports.AddNew();
			Assert(
				"should return false when multiple transports prevent empty consol load being populated" +
				"and there is no load-valid leg departing on-or-before valid discharge leg",
				!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));

			leg1.JW_RL_NKLoadPort = allocationRoute.RCA_LoadLocation;
			Assert(
				"should return true when single leg satisfies all requirements",
				CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, true));
		}

		protected override void SetUp()
		{
			base.SetUp();

			contract = (IRatingContract)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContract>());
			contract.RCT_ContractNumber = "R2D2";
			allocationRoute = (IRatingContractAllocationLine)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContractAllocationLine>());
			allocationRoute.RCA_AllocationLineID = "STUPEFY";
			allocationRoute.RCA_RCT_RatingContract = contract.PK;
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "EXPELLIARMUS";
		}

		IRatingContract contract;
		IRatingContractAllocationLine allocationRoute;
		ForwardingConsol consol;
	}
}

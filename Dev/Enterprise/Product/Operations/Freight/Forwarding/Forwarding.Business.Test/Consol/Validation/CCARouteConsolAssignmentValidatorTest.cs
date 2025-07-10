using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CCARouteConsolAssignmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, true);
			AssertEquals(notification, null);
		}

		public void TestCheckRoute_StartDate()
		{
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			route.RCA_StartDate = new ZDate(2022, 4, 2);

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Route Start Date is later than the Consol's ETD.", NotificationType.Error, notification.Type);
			AssertEquals($"None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date ({route.RCA_StartDate.ToShortDateString()}) and the Expiry Date ({route.RCA_ExpiryDate.ToShortDateString()}) of Allocation Route STUPEFY.", notification.Message);
		}

		public void TestCheckRoute_EndDate()
		{
			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 3, 1);
			route.RCA_StartDate = new ZDate(2020, 1, 2);
			route.RCA_ExpiryDate = new ZDate(2020, 2, 12);

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Route Expiry Date is earlier than Consol's ETD.", NotificationType.Error, notification.Type);
			AssertEquals("None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date (02-Jan-20) and the Expiry Date (12-Feb-20) of Allocation Route STUPEFY.", notification.Message);
		}

		public void TestCheckLoadPort()
		{
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ACCIO";
			route.RCA_LoadLocation = "LUMOS";
			consol.JK_RL_NKLoadPort = "ACCIO";

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Load Port of Allocation Route does not match with Load Port of Consol.", NotificationType.Error, notification.Type);
			AssertEquals("The Load Port (LUMOS) of Allocation Route STUPEFY does not match the First Load Port nor any Load Port of Consol EXPELLIARMUS. The First Load Port or the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.", notification.Message);
		}

		public void TestCheckDischargePort()
		{
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "ACCIO";
			route.RCA_DischargeLocation = "LUMOS";
			consol.JK_RL_NKDischargePort = "ACCIO";

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Discharge Port of Allocation Route does not match with Discharge Port of Consol.", NotificationType.Error, notification.Type);
			AssertEquals("The Discharge Port (LUMOS) of Allocation Route STUPEFY does not match the Last Discharge Port nor any Discharge Port of Consol EXPELLIARMUS. The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.", notification.Message);
		}

		public void TestScheduleDetails()
		{
			var transport = consol.Transports[0];

			route.RCA_RV_NKVessel = "VESSEL";
			route.RCA_VoyageNumber = "VOYAGE";
			route.RCA_ServiceLoop = "GRANGER";

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, true);

			transport.JW_Vessel = "DORYA";

			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, false);

			AssertEquals("Error when not all details are empty / match.", NotificationType.Error, notification.Type);
			AssertEquals("There is no relevant Routing Leg on Consol EXPELLIARMUS that match the Voyage, Vessel, and Service String of Allocation Route STUPEFY.", notification.Message);
		}

		public void TestCheckContainer()
		{
			var container = Factory.New<ForwardingContainer>();

			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "VIKTOR";

			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "FLEUR";
			route.RCA_RC_ContainerType = refContainer2.PK;
			container.JC_RC = refContainer1.PK;
			consol.Containers.Add(container);

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when the Ref Container and the Route Ref Container is different", NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on this Consol share the same Container Code (FLEUR) of the selected Allocation Route STUPEFY. Consol Container Codes should match the selected Allocation Route's Container Code.", notification.Message);

			route.RCA_RC_ContainerType = refContainer1.PK;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, true);
			AssertEquals("Shows no error when Ref Container and Route Ref Container is equal", null, notification);

			refContainer1.RC_StorageClass = "SNI";
			refContainer1.RC_FreightRateClass = "TCH";
			route.RCA_StorageOrFreightRateClass = "POT";
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when the Ref Container Storage and Freight Rate Class and the Route Storage or Freight Rate Class is different", NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on this Consol share the same Container Code (VIKTOR) of the selected Allocation Route STUPEFY. Consol Container Codes should match the selected Allocation Route's Container Code.", notification.Message);
		}

		public void TestCheckContainerOwner()
		{
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;

			var container = consol.Containers.AddNew();
			container.JC_RCA_AllocationLine = route.PK;
			container.JC_IsShipperOwned = false;
			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is not Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Shipper Owned Containers are eligible to consume Allocation Route STUPEFY but one or more Container(s) to be allocated are not Shipper Owned.", notification.Message);

			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			container.JC_IsShipperOwned = true;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Non-Shipper Owned Containers are eligible to consume Allocation Route STUPEFY but one or more Container(s) to be allocated are Shipper Owned.", notification.Message);
		}

		public void TestCheckContainerOwnerOnMultipleContainers()
		{
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;

			var container1 = consol.Containers.AddNew();
			container1.JC_RCA_AllocationLine = route.PK;
			container1.JC_IsShipperOwned = false;

			var container2 = consol.Containers.AddNew();
			container2.JC_RCA_AllocationLine = route.PK;
			container2.JC_IsShipperOwned = false;

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(true, result);

			container2.JC_IsShipperOwned = true;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is not Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Non-Shipper Owned Containers are eligible to consume Allocation Route STUPEFY but one or more Container(s) to be allocated are Shipper Owned.", notification.Message);

			container1.JC_IsShipperOwned = true;
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(true, result);

			container2.JC_IsShipperOwned = false;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Shipper Owned Containers are eligible to consume Allocation Route STUPEFY but one or more Container(s) to be allocated are not Shipper Owned.", notification.Message);
		}

		public void TestCheckBookingLimit()
		{
			var refCont = Factory.New<RefContainer>();
			refCont.RC_Code = "PATRONUS";
			refCont.RC_TEU = 3.14;

			route.RCA_HasBookingLimit = true;
			route.RCA_AllocatedQuantity = 1;
			route.RCA_BookingVariance = 1;
			route.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
			route.RCA_RC_ContainerType = refCont.PK;

			var container = Factory.New<IForwardingContainer>();
			container.JC_ContainerCount = 2;
			container.JC_RCA_AllocationLine = route.PK;
			container.JC_RC = refCont.PK;

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Allocation Route Unit Quantity is TU and Number of TEUs exceeds capacity.", NotificationType.Error, notification.Type);
			AssertEquals("Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route STUPEFY by 5.27 TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.", notification.Message);

			route.RCA_AllocatedUQ = string.Empty;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Allocation Route Unit Quantity is not TU and Number of TEUs exceeds capacity.", NotificationType.Error, notification.Type);
			AssertEquals("Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route STUPEFY by 0.99 TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.", notification.Message);

			route.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Allocation Route Unit Quantity is CN and Number of Containers exceeds capacity.", NotificationType.Error, notification.Type);
			AssertEquals("Number of Containers to be allocated exceeds available capacity of the selected Allocation Route STUPEFY by 0.99. Number of Containers should be within the Booking Limit of the selected Allocation Route.", notification.Message);
		}

		public void TestCheckGatewayConsol()
		{
			route.RCA_AllowGatewayConsolOnly = true;

			consol.JK_RCA_AllocationLine = route.PK;
			consol.JK_SendingForwarderHandlingType = "YAA";
			consol.JK_ReceivingForwarderHandlingType = "AAH";

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			const string errorMessage = "Allocation Route STUPEFY is restricted to Gateway Consols only, but this Consol has no assigned Gateway Agent. Either the Sending or Receiving Agent of this Consol must be assigned as a Gateway Agent.";

			AssertEquals(false, result);
			AssertEquals("Shows error when Consol does not have a Gateway Agent assigned.", NotificationType.Error, notification.Type);
			AssertEquals(errorMessage, notification.Message);
		}

		public void TestCheckGroupageContainerMode()
		{
			route.RCA_AllowGroupageOnly = true;

			consol.JK_RCA_AllocationLine = route.PK;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			const string errorMessage = "Allocation Route STUPEFY is restricted to Consols with Groupage container mode. The Consol's current container mode is FCL.";

			AssertEquals(false, result);
			AssertEquals("Error when Consol's container mode is not GRP.", NotificationType.Error, notification.Type);
			AssertEquals(errorMessage, notification.Message);
		}

		public void TestCheckAllocationRouteNamedAccountsForConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "EvilInc01";

			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			contract.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			contract.RCT_OH = carrier.PK;

			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when attached shipment don't have matching Named Account.", NotificationType.Warning, notification.Type);
			AssertEquals("All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract OBLIVIATE and Allocation Route STUPEFY (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).", notification.Message);
		}

		public void TestCheckAllocationRouteAgentsForConsol()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "DENNIS";
			var randomOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg1.OH_Code = "STEVE";
			var randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg2.OH_Code = "GARRETT";
			route.AgentPivots.AddRelatedIfNotExist(agent);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_RCA_AllocationLine = route.PK;
			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;

			const string errorMessage = "Neither Sending Agent STEVE nor Receiving Agent GARRETT of the Consol matches with the Agents specified on Allocation Route STUPEFY under Carrier Contract OBLIVIATE.";

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(false, result);
			AssertEquals("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent", NotificationType.Error, notification.Type);
			AssertEquals(errorMessage, notification.Message);

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);
			AssertEquals(true, result);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);
			AssertEquals(result, true);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.GC_OH_OrgProxy = agent.PK;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = branchProxy.MainAddress.PK;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);
			AssertEquals(result, true);

			consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg1.MainAddress.PK;
			result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);
			AssertEquals(result, true);
		}

		public void TestCheckAllocationRouteScheduleMismatch()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.New<RatingContract>();
			carrierContract.RCT_ContractNumber = "BLAHAJ";
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var refVessel = Factory.NewWithValidTestData<RefVessel>();

			route.RCA_AllocationLineID = "KRAMIG";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "JATTELIK";
			voyage.JV_RV_NKVessel = refVessel.RV_Code;
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var schedule = Factory.New<JobSailing>();
			schedule.JX_JA = origin.PK;
			schedule.JX_JB = destination.PK;
			schedule.JX_ServiceString = "AFTONSPARV";
			route.RCA_JX_SailingSchedule = schedule.PK;

			var today = ZDateTime.Today;
			origin.JA_E_DEP = today;

			var transportLeg = consol.Transports[0];
			transportLeg.JW_IsLinked = false;
			transportLeg.JW_ETD = today;
			transportLeg.JW_RL_NKLoadPort = "AUMEL";
			transportLeg.JW_RL_NKDiscPort = "NZWEL";
			transportLeg.JW_VoyageFlight = "JATTELIK";
			transportLeg.JW_Vessel = refVessel.RV_Code;
			transportLeg.JW_ServiceString = "AFTONSPARV";
			transportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows warning when attaching route with no matching legs from consol form.", NotificationType.Warning, notification.Type);
			AssertEquals("Allocation Route KRAMIG is linked to Schedule . However, this Consol has no legs matching Schedule details.", notification.Message);

			var scheduleMismatchErrorHelper = new CCARouteConsolAssignmentValidator(true);
			result = scheduleMismatchErrorHelper.IsAllowedToAllocateToRoute(consol, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when attaching route with no matching legs from consol dashboard form.", NotificationType.Error, notification.Type);
			AssertEquals("Allocation Route KRAMIG is linked to Schedule . However, this Consol has no legs matching Schedule details.", notification.Message);
		}

		public void TestCheckAllocationRoutePlaceofReceiptDelivery()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.New<RatingContract>();
			carrierContract.RCT_ContractNumber = "BLAHAJ";
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				route.RCA_PlaceOfReceipt = "AUMEL";
				route.RCA_PlaceOfDelivery = "CNFUZ";

				var errorMessage = "Mismatch: ‘Place of receipt’ (AUMEL) must match ‘1st Load’ (AUSYD) in ‘Allocated consol’ (EXPELLIARMUS).";
				var result = helper.IsAllowedToAllocateToRoute(consol, route, out var notification);

				AssertEquals(false, result);
				AssertEquals("Error when Place of Receipt is not matching with 1st Load", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "CNFUZ";

				errorMessage = "Mismatch: ‘Place of delivery’ (CNFUZ) must match ‘Last  discharge’ (NZAKL) in ‘Allocated consol’ (EXPELLIARMUS).";
				result = helper.IsAllowedToAllocateToRoute(consol, route, out notification);

				AssertEquals(false, result);
				AssertEquals("Error when Place of Receipt is not matching with 1st Load", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "NZAKL";

				result = helper.IsAllowedToAllocateToRoute(consol, route, out  notification);

				AssertEquals(true, result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new CCARouteConsolAssignmentValidator();

			contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractNumber = "OBLIVIATE";
			contract.RCT_TransportMode = Core.Constants.TransportModes.Sea;

			route = Factory.NewWithValidTestData<AllocationRouteForUtilizationSimulation>();
			route.RCA_AllocationLineID = "STUPEFY";
			route.RCA_RCT_RatingContract = contract.PK;
			route.RCA_StartDate = ZDateTime.Today.Date;
			route.RCA_ExpiryDate = ZDateTime.Today.AddDays(5).Date;
			route.RCA_LoadLocation = "AUSYD";
			route.RCA_DischargeLocation = "NZAKL";

			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "EXPELLIARMUS";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today.AddDays(1);
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
		}

		CCARouteConsolAssignmentValidator helper;
		CarrierContractForUtilizationSimulation contract;
		AllocationRouteForUtilizationSimulation route;
		ForwardingConsol consol;
	}
}

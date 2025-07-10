using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	sealed class CCARouteBookingAssignmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, true);
			AssertEquals(notification, null);
		}

		public void TestCheckRoute_StartDate()
		{
			quotedBooking.ETD = new ZDateTime(2020, 3, 1);
			route.RCA_StartDate = new ZDate(2022, 4, 2);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Route Start Date is later than the Booking's ETD.", NotificationType.Error, notification.Type);
			AssertEquals("Start Date (02-Apr-22) of Allocation Route 830RES is later than the ETD (01-Mar-20) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.", notification.Message);
		}

		public void TestCheckRoute_EndDate()
		{
			quotedBooking.ETD = new ZDateTime(2020, 3, 1);
			route.RCA_StartDate = new ZDate(2020, 1, 2);
			route.RCA_ExpiryDate = new ZDate(2020, 2, 12);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Route Expiry Date is earlier than Booking's ETD.", NotificationType.Error, notification.Type);
			AssertEquals("Expiry Date (12-Feb-20) of Allocation Route 830RES is earlier than the ETD (01-Mar-20) of this Booking. Booking departure should be within the validity period of the selected Allocation Route.", notification.Message);
		}

		public void TestCheckLoadPort()
		{
			quotedBooking.LoadPort = "AUSYD";
			route.RCA_LoadLocation = "NZAKL";

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Load Port of Allocation Route does not match with Load Port of Booking.", NotificationType.Error, notification.Type);
			AssertEquals("Load Port (NZAKL) of Allocation Route 830RES does not match the Load Port (AUSYD) of this Booking. Booking Load Port should match the selected Allocation Route's Load Port.", notification.Message);
		}

		public void TestCheckDischargePort()
		{
			quotedBooking.DischargePort = "AUSYD";
			route.RCA_DischargeLocation = "NZAKL";

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Discharge Port of Allocation Route does not match with Discharge Port of Booking.", NotificationType.Error, notification.Type);
			AssertEquals("Discharge Port (NZAKL) of Allocation Route 830RES does not match the Discharge Port (AUSYD) of this Booking. Booking Discharge Port should match the selected Allocation Route's Discharge Port.", notification.Message);
		}

		public void TestCheckVoyage()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = "BRUH";

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JA = origin.PK;

			quotedBooking.Booking.JS_JX = sailing.PK;
			route.RCA_VoyageNumber = "LMAO";

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Voyage of Allocation Route does not match with Voyage of Booking.", NotificationType.Error, notification.Type);
			AssertEquals("Voyage (LMAO) of Allocation Route 830RES does not match the Voyage (BRUH) of this Booking. Booking Voyage should match the selected Allocation Route's Voyage.", notification.Message);
		}

		public void TestCheckVessel()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = "BRUH";

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JA = origin.PK;

			quotedBooking.Booking.JS_JX = sailing.PK;
			route.RCA_RV_NKVessel = "LMAO";

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when Vessel of Allocation Route does not match with Vessel of Booking.", NotificationType.Error, notification.Type);
			AssertEquals("Vessel (LMAO) of Allocation Route 830RES does not match the Vessel (BRUH) of this Booking. Booking Vessel should match the selected Allocation Route's Vessel.", notification.Message);
		}

		public void TestCheckBookingNamedAccounts()
		{
			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			route.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, false);
			AssertEquals("Shows warning when Quoted Booking doesn't have matching Named Account with Route.", CargoWise.ComponentModel.NotificationType.Warning, notification.Type);
			AssertEquals($"At least one of the Booking clients should match a Named Account of Carrier Contract DORSIA and Allocation Route 830RES (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers).", notification.Message);
		}

		public void TestCheckContainer()
		{
			var refCont = Factory.New<RefContainer>();
			refCont.RC_Code = "GOD";
			refCont.RC_StorageClass = "GOD";

			var container = Factory.New<IForwardingContainer>();
			container.JC_RC = refCont.PK;
			route.RCA_RC_ContainerType = refCont.PK;

			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(result, true);
			AssertEquals("Shows no error when Booking Container Type and Allocation Route Container Type is equal", null, notification);

			var refCont2 = Factory.New<RefContainer>();
			refCont2.RC_Code = "LOL";
			refCont2.RC_StorageClass = "LOL";

			route.RCA_RC_ContainerType = refCont2.PK;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when the Booking Container Type and the Allocation Route Container Type is different", NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on this Booking share the same Container Code (LOL) of the selected Allocation Route 830RES. Booking Container Codes should match the selected Allocation Route's Container Code.", notification.Message);

			route.RCA_StorageOrFreightRateClass = "HUH";
			route.RCA_RC_ContainerType = Guid.Empty;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(result, false);
			AssertEquals("Shows error when the Ref Container Storage and Freight Rate Class and the Route Storage or Freight Rate Class is different", NotificationType.Error, notification.Type);
			AssertEquals("NOT all Containers on this Booking share the same Container Class (HUH) of the selected Allocation Route 830RES. Container Class of Booking Containers should match the selected Allocation Route's Container Class.", notification.Message);

			route.RCA_StorageOrFreightRateClass = "GOD";
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(result, true);
			AssertEquals("Shows no error when Booking Container Class and Allocation Route Container Class is equal", null, notification);
		}

		public void TestCheckContainerOwner()
		{
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;

			var container = Factory.New<IForwardingContainer>();
			container.JC_RCA_AllocationLine = route.PK;
			container.JC_IsShipperOwned = false;

			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is not Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Shipper Owned Containers are eligible to consume Allocation Route 830RES but one or more Container(s) to be allocated are not Shipper Owned.", notification.Message);

			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			container.JC_IsShipperOwned = true;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Non-Shipper Owned Containers are eligible to consume Allocation Route 830RES but one or more Container(s) to be allocated are Shipper Owned.", notification.Message);
		}

		public void TestCheckContainerOwnerOnMultipleContainers()
		{
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.CarrierOwned;

			var container1 = Factory.New<IForwardingContainer>();
			container1.JC_RCA_AllocationLine = route.PK;
			container1.JC_IsShipperOwned = false;

			var container2 = Factory.New<IForwardingContainer>();
			container2.JC_RCA_AllocationLine = route.PK;
			container2.JC_IsShipperOwned = false;

			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container1);
			quotedBooking.QuotedBookingContainers.Add((CargoWise.EntityFramework.BusinessObject)container2);

			var result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out var notification);

			AssertEquals(true, result);

			container2.JC_IsShipperOwned = true;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is not Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Non-Shipper Owned Containers are eligible to consume Allocation Route 830RES but one or more Container(s) to be allocated are Shipper Owned.", notification.Message);

			container1.JC_IsShipperOwned = true;
			route.RCA_ContainerOwner = Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(true, result);

			container2.JC_IsShipperOwned = false;
			result = validator.IsAllowedToAllocateToRoute(quotedBooking, route, out notification);

			AssertEquals(false, result);
			AssertEquals("Shows error when Container is Shipper Owned.", NotificationType.Error, notification.Type);
			AssertEquals("Only Shipper Owned Containers are eligible to consume Allocation Route 830RES but one or more Container(s) to be allocated are not Shipper Owned.", notification.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			validator = new CCARouteBookingAssignmentValidator();

			contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractNumber = "DORSIA";

			route = Factory.NewWithValidTestData<AllocationRouteForUtilizationSimulation>();
			route.RCA_AllocationLineID = "830RES";
			route.RCA_RCT_RatingContract = contract.PK;

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_UniqueConsignRef = "PAULALLEN";
			quotedBooking.TransportMode = "SEA";
		}

		CCARouteBookingAssignmentValidator validator;
		CarrierContractForUtilizationSimulation contract;
		AllocationRouteForUtilizationSimulation route;
		QuotedBooking quotedBooking;
	}
}

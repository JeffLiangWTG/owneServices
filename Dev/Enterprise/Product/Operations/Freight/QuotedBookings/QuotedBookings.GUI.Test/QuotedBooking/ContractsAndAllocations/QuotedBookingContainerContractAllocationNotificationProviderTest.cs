using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContainerContractAllocationNotificationProviderTest : TestCaseWithFactory
	{
		public void TestGetContractAllocationNotification_ReturnsNullIfNoError()
		{
			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GOBLINJR";

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNull(result);
		}

		public void TestGetContractAllocationNotification_ReturnsNotificationIfError()
		{
			quotedBooking.TransportMode = "AIR";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "OUTAMI";

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestGetRouteAllocationNotification_ReturnsNullIfNoError()
		{
			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "GOBLINJR";

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestGetRouteAllocationNotification_ReturnsNotificationIfError()
		{
			quotedBooking.LoadPort = "LMAOO";
			route.RCA_LoadLocation = "BROOO";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "OUTAMI";

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestContainerOwnerNotification_SHP_IsShipperOwnedTrue_ReturnsNullIfNoError()
		{
			route.RCA_ContainerOwner = "SHP";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_IsShipperOwned = true;

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestContainerOwnerNotification_CAR_IsShipperOwnedFalse_ReturnsNullIfNoError()
		{
			route.RCA_ContainerOwner = "CAR";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_IsShipperOwned = false;

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestContainerOwnerNotification_CAR_IsShipperOwnedTrue_ReturnsNotificationIfError()
		{
			route.RCA_ContainerOwner = "CAR";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_IsShipperOwned = true;

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestContainerOwnerNotification_SHP_IsShipperOwnedFalse_ReturnsNullIfNoError()
		{
			route.RCA_ContainerOwner = "SHP";

			var container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_IsShipperOwned = false;

			var notificationProvider = new QuotedBookingContainerContractAllocationNotificationProvider(container);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractNumber = "DORSIA";

			route = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			route.RCA_AllocationLineID = "830RES";
			route.RCA_RCT_RatingContract = contract.PK;

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_UniqueConsignRef = "PAULALLEN";
			quotedBooking.TransportMode = "SEA";
		}

		RatingContract contract;
		RatingContractAllocationLine route;
		QuotedBooking quotedBooking;
	}
}

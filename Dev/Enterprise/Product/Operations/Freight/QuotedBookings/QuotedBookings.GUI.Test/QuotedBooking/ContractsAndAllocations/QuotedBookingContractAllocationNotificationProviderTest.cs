using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	sealed class QuotedBookingContractAllocationNotificationProviderTest : TestCaseWithFactory
	{
		public void TestGetContractAllocationNotification_ReturnsNullIfNoError()
		{
			var notificationProvider = new QuotedBookingContractAllocationNotificationProvider(quotedBooking);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNull(result);
		}

		public void TestGetContractAllocationNotification_ReturnsNotificationIfError()
		{
			quotedBooking.TransportMode = "AIR";

			var notificationProvider = new QuotedBookingContractAllocationNotificationProvider(quotedBooking);

			var result = notificationProvider.GetContractAllocationNotification(contract);
			AssertNotNull(result);
			AssertNotNullOrEmpty(result.Message);
		}

		public void TestGetRouteAllocationNotification_ReturnsNullIfNoError()
		{
			var notificationProvider = new QuotedBookingContractAllocationNotificationProvider(quotedBooking);

			var result = notificationProvider.GetRouteAllocationNotification(route);
			AssertNull(result);
		}

		public void TestGetRouteAllocationNotification_ReturnsNotificationIfError()
		{
			quotedBooking.LoadPort = "PBATE";
			route.RCA_LoadLocation = "LEWIS";

			var notificationProvider = new QuotedBookingContractAllocationNotificationProvider(quotedBooking);

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

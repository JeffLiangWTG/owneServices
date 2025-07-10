using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	sealed class CCARouteBookingContainerAssignmentValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var result = validator.IsAllowedToAllocateToRoute(container, route, out var notification);

			AssertEquals(result, true);
			AssertEquals(notification, null);
		}

		public void TestCheckPlaceOfReceiptDelivery_HasLordAndDischarge()
		{
			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				route.RCA_AllowRelatedPorts = true;
				quotedBooking.LoadPort = "AUSYD";
				quotedBooking.DischargePort = "CNSHA";

				route.RCA_PlaceOfReceipt = "CNSHA";
				route.RCA_PlaceOfDelivery = "AUMEL";
				var errorMessage = "Mismatch: ‘Place of receipt’ (CNSHA) must match ‘Load port’ (AUSYD) or ‘Origin’ (AUSYD) in ‘Allocated booking’ (PAULALLEN).";
				var result = validator.IsAllowedToAllocateToRoute(container, route, out var notification);
				AssertEquals(result, false);
				AssertEquals("Shows error when Place of receipt/delivery is not matching with loadport/origin/discharge port/destination.", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "AUMEL";
				errorMessage = "Mismatch: ‘Place of delivery’ (AUMEL) must match ‘Discharge port’ (CNSHA) or ‘Destination’ (CNSHA) in ‘Allocated booking’ (PAULALLEN).";
				result = validator.IsAllowedToAllocateToRoute(container, route, out notification);
				AssertEquals(result, false);
				AssertEquals("Shows error when Place of receipt/delivery is not matching with loadport/origin/discharge port/destination.", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "CNSHA";
				result = validator.IsAllowedToAllocateToRoute(container, route, out notification);
				AssertEquals(result, true);
			}
		}

		public void TestCheckPlaceOfReceiptDelivery_DoesNotHaveLordAndDischarge()
		{
			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				route.RCA_AllowRelatedPorts = true;

				quotedBooking.Origin = "AUSYD";
				quotedBooking.Destination = "CNSHA";

				quotedBooking.LoadPort = ZString.Empty;
				quotedBooking.DischargePort = ZString.Empty;

				route.RCA_PlaceOfReceipt = "CNSHA";
				route.RCA_PlaceOfDelivery = "AUMEL";
				var errorMessage = "Mismatch: ‘Place of receipt’ (CNSHA) must match ‘Load port’ () or ‘Origin’ (AUSYD) in ‘Allocated booking’ (PAULALLEN).";
				var result = validator.IsAllowedToAllocateToRoute(container, route, out var notification);
				AssertEquals(result, false);
				AssertEquals("Shows error when Place of receipt/delivery is not matching with loadport/origin/discharge port/destination.", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "AUMEL";
				errorMessage = "Mismatch: ‘Place of delivery’ (AUMEL) must match ‘Discharge port’ () or ‘Destination’ (CNSHA) in ‘Allocated booking’ (PAULALLEN).";
				result = validator.IsAllowedToAllocateToRoute(container, route, out notification);
				AssertEquals(result, false);
				AssertEquals("Shows error when Place of receipt/delivery is not matching with loadport/origin/discharge port/destination.", NotificationType.Error, notification.Type);
				AssertEquals(errorMessage, notification.Message);

				route.RCA_PlaceOfReceipt = "AUSYD";
				route.RCA_PlaceOfDelivery = "CNSHA";
				result = validator.IsAllowedToAllocateToRoute(container, route, out notification);
				AssertEquals(result, true);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			validator = new CCARouteBookingContainerAssignmentValidator();

			contract = Factory.NewWithValidTestData<CarrierContractForUtilizationSimulation>();
			contract.RCT_ContractNumber = "DORSIA";

			route = Factory.NewWithValidTestData<AllocationRouteForUtilizationSimulation>();
			route.RCA_AllocationLineID = "830RES";
			route.RCA_RCT_RatingContract = contract.PK;

			quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.Booking.JS_UniqueConsignRef = "PAULALLEN";
			quotedBooking.TransportMode = "SEA";
			quotedBooking.AllocationLinePK = route.PK;

			container = quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_RCA_AllocationLine = route.PK;
		}

		CCARouteBookingContainerAssignmentValidator validator;
		CarrierContractForUtilizationSimulation contract;
		AllocationRouteForUtilizationSimulation route;
		ForwardingContainer container;
		QuotedBooking quotedBooking;
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class BookingStatusesTest : TestCaseWithFactory
	{
		public void TestBookingStatuses()
		{
			var bookingStatuses = new BookingStatuses();
			var transportStatuses = new TransportStatuses();
			var expectedExcludedCodes = new string[] { TransportStatuses.Codes.Allocated, TransportStatuses.Codes.Booked, TransportStatuses.Codes.DeliveryAllocated, TransportStatuses.Codes.Incomplete, TransportStatuses.Codes.PickUpAllocated, TransportStatuses.Codes.PickUpCommenced, TransportStatuses.Codes.PickUpConfirmed };
			var actualExcludedCodes = transportStatuses.GetAllCodes().Except(bookingStatuses.GetAllCodes());

			var bookingStatusesOnlyCodes = bookingStatuses.GetAllCodes().Except(transportStatuses.GetAllCodes());

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("BookingStatuses should not have codes that aren't relevant for a transport booking's status", expectedExcludedCodes, actualExcludedCodes);
				AssertContainsExactElementsInAnyOrder(
					"BookingStatuses should not have any codes that aren't mentioned in TransportStatuses. Rationale: If that happens, then methods providing a description of a given booking status will have to be modified, like DtbTransport.StatusDescription.",
					System.Array.Empty<string>(),
					bookingStatusesOnlyCodes
				);
			});
		}
	}
}

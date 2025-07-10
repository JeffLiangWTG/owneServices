using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class BookingConsolidationStatusesTest : TestCaseWithFactory
	{
		public void TestBookingConsolidationStatuses()
		{
			var bookingConsolidationStatuses = new BookingConsolidationStatuses();
			var bookingStatuses = new BookingStatuses();
			var expectedExcludedCodes = new string[] { TransportStatuses.Codes.Deactivated, TransportStatuses.Codes.Quote };
			var actualExcludedCodes = bookingStatuses.GetAllCodes().Except(bookingConsolidationStatuses.GetAllCodes());

			var bookingConsolidationStatusesOnlyCodes = bookingConsolidationStatuses.GetAllCodes().Except(bookingStatuses.GetAllCodes());

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("BookingConsolidationStatuses should not have codes that aren't relevant for a booking consolidation's status", expectedExcludedCodes, actualExcludedCodes);
				AssertContainsExactElementsInAnyOrder(
					"BookingConsolidationStatuses should not have any codes that aren't mentioned in TransportStatuses or BookingStatuses. Rationale: If that happens, then methods providing a description of a given booking consolidation status will have to be modified",
					Array.Empty<string>(),
					bookingConsolidationStatusesOnlyCodes
				);
			});
		}
	}
}

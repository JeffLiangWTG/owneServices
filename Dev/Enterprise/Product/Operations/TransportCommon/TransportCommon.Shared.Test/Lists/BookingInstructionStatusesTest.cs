using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Shared.Testing
{
	public class BookingInstructionStatusesTest : TestCaseWithFactory
	{
		public void TestBookingInstructionStatuses()
		{
			var bookingInstructionStatuses = new BookingInstructionStatuses();
			var bookingStatuses = new BookingStatuses();
			var expectedExcludedCodes = new string[] { TransportStatuses.Codes.Deactivated, TransportStatuses.Codes.DeliveredEmptyNotReturned, TransportStatuses.Codes.ActionRequired, TransportStatuses.Codes.Held, TransportStatuses.Codes.Quote, TransportStatuses.Codes.ServiceCommenced };
			var actualExcludedCodes = bookingStatuses.GetAllCodes().Except(bookingInstructionStatuses.GetAllCodes());

			var bookingInstructionStatusesOnlyCodes = bookingInstructionStatuses.GetAllCodes().Except(bookingStatuses.GetAllCodes());

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("BookingInstructionStatuses should not have codes that aren't relevant for a transport booking instruction's status", expectedExcludedCodes, actualExcludedCodes);
				AssertContainsExactElementsInAnyOrder(
					"BookingInstructionStatuses should not have any codes that aren't mentioned in TransportStatuses or BookingStatuses. Rationale: If that happens, then methods providing a description of a given booking instruction status will have to be modified.",
					System.Array.Empty<string>(),
					bookingInstructionStatusesOnlyCodes
				);
			});
		}
	}
}

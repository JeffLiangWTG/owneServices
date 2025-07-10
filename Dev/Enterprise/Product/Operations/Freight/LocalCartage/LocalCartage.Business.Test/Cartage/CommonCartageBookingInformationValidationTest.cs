using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageBookingInformationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBookingStatus()
		{
			var cartage = Factory.New<CommonCartage>();
			var bookingInformation = cartage.BookingInformation;
			bookingInformation.BookingStatus = "ABC";
			AssertNoErrors("Should not have errors as CanSelectStatus is false.", bookingInformation.BookingStatusInfo);
			cartage.Logs.AddNew(Events.StatusUpdated, FreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted + "-" + FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted); // For CanSelectStatus check
			AssertEquals("Precondition: Can select status", true, bookingInformation.CanSelectStatus);
			bookingInformation.BookingStatus = "ABC"; // POKE
			AssertHasError(bookingInformation.BookingStatusInfo, "Enter a valid selection.");
			bookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced;
			AssertNoErrors(bookingInformation.BookingStatusInfo);
			bookingInformation.BookingStatus = "";
			AssertNoErrors("Should not add error to empty BookingStatus.", bookingInformation.BookingStatusInfo);
		}
	}
}

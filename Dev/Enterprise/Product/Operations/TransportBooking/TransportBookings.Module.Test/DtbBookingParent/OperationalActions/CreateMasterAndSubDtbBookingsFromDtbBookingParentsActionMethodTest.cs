using System;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod))]
	sealed class CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethodTest : BaseCreateDtbBookingsFromDtbBookingParentsActionMethodTest<CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod>
	{
		protected override string ExpectedNameAndDescription => "Create Master Transport Booking and Sub Bookings";

		protected override Type ExpectedApplicatorType => typeof(CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator);

		protected override CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod NewMethod() => new CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod();
	}
}

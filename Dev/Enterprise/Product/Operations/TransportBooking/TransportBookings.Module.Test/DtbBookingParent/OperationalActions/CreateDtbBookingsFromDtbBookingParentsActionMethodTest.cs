using System;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateDtbBookingsFromDtbBookingParentsActionMethod))]
	sealed class CreateDtbBookingsFromDtbBookingParentsActionMethodTest : BaseCreateDtbBookingsFromDtbBookingParentsActionMethodTest<CreateDtbBookingsFromDtbBookingParentsActionMethod>
	{
		protected override string ExpectedNameAndDescription => "Create Transport Bookings";

		protected override Type ExpectedApplicatorType => typeof(CreateDtbBookingsFromDtbBookingParentsApplicator);

		protected override CreateDtbBookingsFromDtbBookingParentsActionMethod NewMethod() => new CreateDtbBookingsFromDtbBookingParentsActionMethod();
	}
}

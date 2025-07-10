using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public sealed class CreateDtbBookingsFromDtbBookingParentsActionMethod : BaseCreateDtbBookingsFromDtbBookingParentsActionMethod
	{
		public CreateDtbBookingsFromDtbBookingParentsActionMethod() : base(new Guid("05795463-98bd-4d59-a1cf-9a72ddfd22a0"))
		{
		}

		protected override string ActionMethodName => Res.GetString("340e5953-70c1-4da5-b262-73f8bc70a031", "Create Transport Bookings");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new CreateDtbBookingsFromDtbBookingParentsApplicator(factory);
	}
}

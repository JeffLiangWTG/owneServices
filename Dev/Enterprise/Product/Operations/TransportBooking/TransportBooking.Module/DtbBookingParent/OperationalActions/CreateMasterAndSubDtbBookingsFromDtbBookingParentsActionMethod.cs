using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.TransportBookings.Module
{
	public sealed class CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod : BaseCreateDtbBookingsFromDtbBookingParentsActionMethod
	{
		public CreateMasterAndSubDtbBookingsFromDtbBookingParentsActionMethod() : base(new Guid("c6657c29-0e2d-4442-a893-217457db19ad"))
		{
		}

		protected override string ActionMethodName => Res.GetString("cf15000a-ff91-424a-b9e6-82ce7e5f4a33", "Create Master Transport Booking and Sub Bookings");

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator(factory);
	}
}

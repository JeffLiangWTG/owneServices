using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Module
{
	public class HoldBookingChangeActionMethodApplicator : BookingStatusChangeActionMethodApplicator
	{
		public HoldBookingChangeActionMethodApplicator()
			: base(Res.GetString("95ea7990-67df-478d-8591-b1189bea84ba", "Hold Booking"))
		{
		}

		protected override IEnumerable<ZString> StatusesToChangeFrom
		{
			get { return new List<ZString> { TransportStatuses.Codes.Available }; }
		}

		protected override ZString StatusToChangeTo
		{
			get { return TransportStatuses.Codes.Held; }
		}
	}
}

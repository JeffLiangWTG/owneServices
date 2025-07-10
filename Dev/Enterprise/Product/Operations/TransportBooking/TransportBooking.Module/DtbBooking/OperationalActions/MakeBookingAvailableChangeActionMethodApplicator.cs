using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Module
{
	public class MakeBookingAvailableChangeActionMethodApplicator : BookingStatusChangeActionMethodApplicator
	{
		public MakeBookingAvailableChangeActionMethodApplicator()
			: base(Res.GetString("d7fd7b9b-cd7d-4338-9148-ae61857f612f", "Make Available"))
		{
		}

		protected override IEnumerable<ZString> StatusesToChangeFrom
		{
			get { return new List<ZString> { TransportStatuses.Codes.Incomplete, TransportStatuses.Codes.Quote, TransportStatuses.Codes.ActionRequired }; }
		}

		protected override ZString StatusToChangeTo
		{
			get { return TransportStatuses.Codes.Available; }
		}
	}
}

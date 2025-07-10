using System;
using CargoWise.Types;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class HBLBookingStatusEventArgs : EventArgs
	{
		public HBLBookingStatusEventArgs(ZString status)
		{
			HBLBookingStatus = status;
			StatusUpdatedReason = ZString.Empty;
		}

		public ZString HBLBookingStatus { get; }

		public ZString StatusUpdatedReason { get; set; }
	}
}

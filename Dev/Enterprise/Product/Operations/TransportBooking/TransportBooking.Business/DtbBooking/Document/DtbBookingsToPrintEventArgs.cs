using System;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingsToPrintEventArgs : EventArgs
	{
		public DtbBookingsToPrintEventArgs(DocumentDtbBookingCollection bookingsToSelectFrom)
		{
			BookingsToSelectFrom = bookingsToSelectFrom;
		}

		public readonly DocumentDtbBookingCollection BookingsToSelectFrom;
		public bool ContinueToPrint = true;
	}
}

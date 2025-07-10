using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public DtbBookingRateLineConditionsSupporter(DtbBooking transportBooking)
			: base(transportBooking)
		{
		}

		DtbBooking booking;
		DtbBooking Booking
		{
			get { return booking ?? (booking = ObjectToWrap as DtbBooking); }
		}

		protected override OrgHeader GetExportBroker()
		{
			return null;
		}

		protected override OrgHeader GetImportBroker()
		{
			return null;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return null;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return null;
		}

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		protected override OrgHeader GetDepartureCFS()
		{
			return null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			return null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return Booking != null && Booking.KM_IsHazardous;
		}
	}
}

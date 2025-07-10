using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationProcessTaskCollection : ProcessTaskCollection
	{
		public DtbBookingConsolidationProcessTaskCollection(DtbBookingConsolidation transportBooking)
			: base(transportBooking)
		{
		}

		public new DtbBookingConsolidationProcessTask this[int index]
		{
			get { return (DtbBookingConsolidationProcessTask)Elements[index]; }
		}

		public new DtbBookingConsolidationProcessTask AddNew()
		{
			return (DtbBookingConsolidationProcessTask)base.AddNew();
		}
	}
}

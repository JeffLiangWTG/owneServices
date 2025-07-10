using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingProcessTaskCollection : ProcessTaskCollection
	{
		public DtbBookingProcessTaskCollection(DtbBooking transportBooking)
			: base(transportBooking)
		{
		}

		public DtbBookingProcessTaskCollection(DtbBooking transportBooking, ZQuery additionalFilter)
			: base(transportBooking, additionalFilter)
		{
		}

		public new DtbBookingProcessTask this[int index]
		{
			get { return (DtbBookingProcessTask)Elements[index]; }
		}

		public new DtbBookingProcessTask AddNew()
		{
			return (DtbBookingProcessTask)base.AddNew();
		}
	}
}

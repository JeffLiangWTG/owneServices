namespace Enterprise.TransportBookings.Business
{
	using Enterprise.MasterFiles.Business;

	public class DtbBookingConfirmationProcessTaskCollection : ProcessTaskCollection
	{
		public DtbBookingConfirmationProcessTaskCollection(DtbBookingConfirmation transportBooking)
			: base(transportBooking)
		{
		}

		public new DtbBookingConfirmationProcessTask this[int index]
		{
			get { return (DtbBookingConfirmationProcessTask)Elements[index]; }
		}

		public new DtbBookingConfirmationProcessTask AddNew()
		{
			return (DtbBookingConfirmationProcessTask)base.AddNew();
		}
	}
}

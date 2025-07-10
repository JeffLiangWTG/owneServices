using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionProcessTaskCollection : ProcessTaskCollection
	{
		public DtbBookingInstructionProcessTaskCollection(DtbBookingInstruction transportBooking)
			: base(transportBooking)
		{
		}

		public new DtbBookingInstructionProcessTask this[int index]
		{
			get { return (DtbBookingInstructionProcessTask)Elements[index]; }
		}

		public new DtbBookingInstructionProcessTask AddNew()
		{
			return (DtbBookingInstructionProcessTask)base.AddNew();
		}
	}
}

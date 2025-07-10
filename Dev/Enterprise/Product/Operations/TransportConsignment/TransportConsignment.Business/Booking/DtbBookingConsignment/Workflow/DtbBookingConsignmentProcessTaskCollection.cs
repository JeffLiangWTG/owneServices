using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentProcessTaskCollection : ProcessTaskCollection
	{
		public DtbBookingConsignmentProcessTaskCollection(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		public new DtbBookingConsignmentProcessTask this[int index]
		{
			get { return (DtbBookingConsignmentProcessTask)Elements[index]; }
		}

		public new DtbBookingConsignmentProcessTask AddNew()
		{
			return (DtbBookingConsignmentProcessTask)base.AddNew();
		}
	}
}

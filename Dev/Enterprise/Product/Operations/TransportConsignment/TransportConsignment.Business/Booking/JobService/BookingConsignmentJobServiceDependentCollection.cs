using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class BookingConsignmentJobServiceDependentCollection : JobServiceDependentCollection
	{
		public BookingConsignmentJobServiceDependentCollection(DtbBookingConsignment dtbConsignment, BusinessObjectFactory factory)
			: base(dtbConsignment, factory)
		{
		}

		public new BookingConsignmentJobService this[int index]
		{
			get { return (BookingConsignmentJobService)base[index]; }
		}

		public new BookingConsignmentJobService AddNew()
		{
			return (BookingConsignmentJobService)base.AddNew();
		}
	}
}

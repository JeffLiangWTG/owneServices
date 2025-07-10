using CargoWise.EntityFramework;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobSupplierBookingCollection : ActiveBusinessObjectCollection<JobSupplierBooking>, IJobSupplierBookingCollection
	{
		public JobSupplierBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobSupplierBookingCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}

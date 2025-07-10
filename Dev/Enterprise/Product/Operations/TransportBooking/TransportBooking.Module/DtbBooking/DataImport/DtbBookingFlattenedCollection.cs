using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingFlattenedCollection : NonPersistentBusinessObjectCollection<DtbBookingFlattened>
	{
		public DtbBookingFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DtbBookingFlattened();
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingTmplCollection : ActiveBusinessObjectCollection<DtbBookingTmpl>
	{
		public DtbBookingTmplCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DtbBookingTmplCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

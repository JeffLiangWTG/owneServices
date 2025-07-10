using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteBookingCollection : ActiveBusinessObjectCollection<GteBooking>
	{
		public GteBookingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}

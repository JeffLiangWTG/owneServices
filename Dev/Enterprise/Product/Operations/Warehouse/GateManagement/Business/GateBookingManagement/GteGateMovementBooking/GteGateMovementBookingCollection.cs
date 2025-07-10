using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementBookingCollection : ActiveBusinessObjectCollection<GteGateMovementBooking>
	{
		public GteGateMovementBookingCollection(GteBooking booking)
			: base(booking.Factory, booking, null, GteGateMovementBookingSchema.GBM_GBK_Booking)
		{
		}

		public GteGateMovementBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

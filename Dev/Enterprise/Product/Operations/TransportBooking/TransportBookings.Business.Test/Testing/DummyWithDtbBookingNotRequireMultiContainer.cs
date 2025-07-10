using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DummyWithDtbBookingNotRequireMultiContainer : DummyWithDtbBooking
	{
		public DummyWithDtbBookingNotRequireMultiContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		public override bool RequiresMultiContainerBooking => false;
	}
}

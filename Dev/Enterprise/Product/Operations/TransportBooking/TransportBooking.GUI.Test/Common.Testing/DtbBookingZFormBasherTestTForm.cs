using Enterprise.TransportBookings.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.TransportBookings.GUI
{
	public abstract class DtbBookingZFormBasherTest : ZFormBasherTest
	{
		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}

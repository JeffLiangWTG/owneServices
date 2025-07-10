using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingConsolidationModuleForTest : DtbBookingConsolidationModule
	{
		public IFilterControl GetNewFilterControlForTest()
		{
			return GetNewFilterControl();
		}
	}
}

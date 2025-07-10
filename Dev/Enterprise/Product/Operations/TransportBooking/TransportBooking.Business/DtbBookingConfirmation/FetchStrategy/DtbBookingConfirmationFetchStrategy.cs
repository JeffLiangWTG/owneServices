using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConfirmationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingConfirmationFetchStrategy(DtbBookingConfirmation confirmation)
			: base(confirmation)
		{
		}
	}
}

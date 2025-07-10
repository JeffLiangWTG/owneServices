using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingInstructionFetchStrategy(DtbBookingInstruction instruction)
			: base(instruction)
		{
		}
	}
}

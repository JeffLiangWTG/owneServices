using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Business
{
	public class DtbTransportInstructionPkgDivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		/// <summary>
		/// Testeded in both DtbBooking and DtbConsignment.
		/// </summary>
		public DtbTransportInstructionPkgDivotFetchStrategy(DtbTransportInstructionPkgDivot divot)
			: base(divot)
		{
		}
	}
}

using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Business
{
	/// <summary>
	/// Tested in both DtbBooking and DtbConsignment.
	/// </summary>
	public class DtbTransportInstructionFetchStrategy<TInstructionPkgDivot, TConfirmation> : EnterpriseBusinessObjectFetchStrategy
		where TInstructionPkgDivot : DtbTransportInstructionPkgDivot
		where TConfirmation : DtbTransportConfirmation
	{
		public DtbTransportInstructionFetchStrategy(DtbTransportInstruction instruction)
			: base(instruction)
		{
		}
	}
}

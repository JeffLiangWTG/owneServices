using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionPkgDivotValidation : DtbBookingInstructionPkgDivotValidation
	{
		protected DtbTransportInstructionPkgDivotValidation(DtbTransportInstructionPkgDivot parent)
			: base(parent)
		{
		}
	}
}

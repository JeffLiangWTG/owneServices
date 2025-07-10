using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionPkgDivotLookups : DtbBookingInstructionPkgDivotLookups
	{
		protected DtbTransportInstructionPkgDivotLookups(DtbTransportInstructionPkgDivot parent)
			: base(parent)
		{
		}
	}
}

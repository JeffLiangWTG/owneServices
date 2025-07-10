using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionTmplLookups : DtbBookingInstructionTmplLookups
	{
		protected DtbTransportInstructionTmplLookups(DtbTransportInstructionTmpl parent)
			: base(parent)
		{
		}
	}
}

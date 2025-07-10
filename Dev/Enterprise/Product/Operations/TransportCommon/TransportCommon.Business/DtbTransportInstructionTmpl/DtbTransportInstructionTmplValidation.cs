using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionTmplValidation : DtbBookingInstructionTmplValidation
	{
		protected DtbTransportInstructionTmplValidation(DtbTransportInstructionTmpl parent)
			: base(parent)
		{
		}
	}
}

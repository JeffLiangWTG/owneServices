using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportTmplValidation : DtbBookingTmplValidation
	{
		protected DtbTransportTmplValidation(DtbTransportTmpl parent)
			: base(parent)
		{
		}
	}
}

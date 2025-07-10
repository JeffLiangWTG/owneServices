using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportTmplLookups : DtbBookingTmplLookups
	{
		protected DtbTransportTmplLookups(DtbTransportTmpl parent)
			: base(parent)
		{
		}
	}
}

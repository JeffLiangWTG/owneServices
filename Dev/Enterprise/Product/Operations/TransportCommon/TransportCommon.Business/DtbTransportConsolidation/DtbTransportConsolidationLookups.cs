using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConsolidationLookups : DtbBookingConsolidationLookups
	{
		protected DtbTransportConsolidationLookups(DtbTransportConsolidation parent)
			: base(parent)
		{
		}
	}
}

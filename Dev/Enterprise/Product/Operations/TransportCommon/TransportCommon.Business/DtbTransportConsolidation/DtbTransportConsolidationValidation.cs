using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConsolidationValidation : DtbBookingConsolidationValidation
	{
		protected DtbTransportConsolidationValidation(DtbTransportConsolidation parent)
			: base(parent)
		{
		}
	}
}

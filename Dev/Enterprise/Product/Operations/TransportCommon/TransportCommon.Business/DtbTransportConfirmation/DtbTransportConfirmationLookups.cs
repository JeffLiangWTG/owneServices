using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConfirmationLookups : DtbBookingConfirmationLookups
	{
		protected DtbTransportConfirmationLookups(DtbTransportConfirmation parent)
			: base(parent)
		{
		}
	}
}

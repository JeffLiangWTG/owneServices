using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConfirmationValidation : DtbBookingConfirmationValidation
	{
		protected DtbTransportConfirmationValidation(DtbTransportConfirmation parent)
			: base(parent)
		{
		}
	}
}

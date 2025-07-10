//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingTmplValidation
//
//    This class should be used for overriding validation in AutoDtbBookingTmplValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingTmplValidation : AutoDtbBookingTmplValidation
	{
		internal DtbBookingTmplValidation(AutoDtbBookingTmpl parent)
			: base(parent)
		{
		}
	}
}

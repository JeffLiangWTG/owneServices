//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingConsolidationValidation
//
//    This class should be used for overriding validation in AutoDtbBookingConsolidationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingConsolidationValidation : AutoDtbBookingConsolidationValidation
	{
		internal DtbBookingConsolidationValidation(AutoDtbBookingConsolidation parent)
			: base(parent)
		{
		}
	}
}

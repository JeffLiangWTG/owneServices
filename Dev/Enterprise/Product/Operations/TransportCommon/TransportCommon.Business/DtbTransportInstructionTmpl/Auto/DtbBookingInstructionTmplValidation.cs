//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingInstructionTmplValidation
//
//    This class should be used for overriding validation in AutoDtbBookingInstructionTmplValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingInstructionTmplValidation : AutoDtbBookingInstructionTmplValidation
	{
		internal DtbBookingInstructionTmplValidation(AutoDtbBookingInstructionTmpl parent)
			: base(parent)
		{
		}
	}
}

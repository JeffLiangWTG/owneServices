//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingInstructionValidation
//
//    This class should be used for overriding validation in AutoDtbBookingInstructionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingInstructionValidation : AutoDtbBookingInstructionValidation
	{
		internal DtbBookingInstructionValidation(AutoDtbBookingInstruction parent)
			: base(parent)
		{
		}
	}
}

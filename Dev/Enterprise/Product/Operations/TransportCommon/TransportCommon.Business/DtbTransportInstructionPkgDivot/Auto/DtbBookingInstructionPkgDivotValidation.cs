//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbBookingInstructionPkgDivotValidation
//
//    This class should be used for overriding validation in AutoDtbBookingInstructionPkgDivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportCommon.Business.Common
{
	public class DtbBookingInstructionPkgDivotValidation : AutoDtbBookingInstructionPkgDivotValidation
	{
		internal DtbBookingInstructionPkgDivotValidation(AutoDtbBookingInstructionPkgDivot parent)
			: base(parent)
		{
		}
	}
}

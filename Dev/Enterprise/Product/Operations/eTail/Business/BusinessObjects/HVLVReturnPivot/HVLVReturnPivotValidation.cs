//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVReturnPivotValidation
//
//    This class should be used for overriding validation in AutoHVLVReturnPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eTail.Business
{
	public class HVLVReturnPivotValidation : AutoHVLVReturnPivotValidation
	{
		public HVLVReturnPivotValidation(AutoHVLVReturnPivot parent)
			: base(parent)
		{
		}
	}
}


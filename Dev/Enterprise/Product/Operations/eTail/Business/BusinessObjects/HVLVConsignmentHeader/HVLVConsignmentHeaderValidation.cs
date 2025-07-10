//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHVLVConsignmentHeaderValidation
//
//    This class should be used for overriding validation in AutoHVLVConsignmentHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.eTail.Business
{
	public class HVLVConsignmentHeaderValidation : AutoHVLVConsignmentHeaderValidation
	{
		public HVLVConsignmentHeaderValidation(AutoHVLVConsignmentHeader parent) : base(parent)
		{
		}
	}
}

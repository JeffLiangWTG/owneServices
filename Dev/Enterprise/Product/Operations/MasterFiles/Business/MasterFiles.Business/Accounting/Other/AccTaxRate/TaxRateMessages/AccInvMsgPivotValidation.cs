//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccInvMsgPivotValidation
//
//    This class should be used for overriding validation in AutoAccInvMsgPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccInvMsgPivotValidation : AutoAccInvMsgPivotValidation
	{
		public AccInvMsgPivotValidation(AutoAccInvMsgPivot parent) : base(parent)
		{
		}
	}
}

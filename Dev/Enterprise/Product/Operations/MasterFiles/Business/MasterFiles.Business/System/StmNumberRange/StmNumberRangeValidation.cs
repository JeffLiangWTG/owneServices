//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmNumberRangeValidation
//
//    This class should be used for overriding validation in AutoStmNumberRangeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmNumberRangeValidation : AutoStmNumberRangeValidation
	{
		public StmNumberRangeValidation(AutoStmNumberRange parent) : base(parent)
		{
		}
	}
}

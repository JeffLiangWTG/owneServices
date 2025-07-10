//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuMenuPivotValidation
//
//    This class should be used for overriding validation in AutoStmMenuMenuPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuMenuPivotValidation : AutoStmMenuMenuPivotValidation
	{
		public StmMenuMenuPivotValidation(AutoStmMenuMenuPivot parent) : base(parent)
		{
		}
	}
}

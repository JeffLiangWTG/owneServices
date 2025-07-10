//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuTemplatePivotValidation
//
//    This class should be used for overriding validation in AutoStmMenuTemplatePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmMenuTemplatePivotValidation : AutoStmMenuTemplatePivotValidation
	{
		public StmMenuTemplatePivotValidation(AutoStmMenuTemplatePivot parent) : base(parent)
		{
		}
	}
}

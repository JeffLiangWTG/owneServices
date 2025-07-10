//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmChangeLogValidation
//
//    This class should be used for overriding validation in AutoStmChangeLogValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmChangeLogValidation : AutoStmChangeLogValidation
	{
		public StmChangeLogValidation(AutoStmChangeLog parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmTemplateValidation
//
//    This class should be used for overriding validation in AutoStmTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmTemplateValidation : AutoStmTemplateValidation
	{
		public StmTemplateValidation(AutoStmTemplate parent) : base(parent)
		{
		}
	}
}

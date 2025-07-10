//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDocDataOverrideValidation
//
//    This class should be used for overriding validation in AutoStmDocDataOverrideValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmDocDataOverrideValidation : AutoStmDocDataOverrideValidation
	{
		public StmDocDataOverrideValidation(AutoStmDocDataOverride parent) : base(parent)
		{
		}
	}
}

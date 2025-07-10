//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmAccessTokenValidation
//
//    This class should be used for overriding validation in AutoStmAccessTokenValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class StmAccessTokenValidation : AutoStmAccessTokenValidation
	{
		public StmAccessTokenValidation(AutoStmAccessToken parent) : base(parent)
		{
		}
	}
}

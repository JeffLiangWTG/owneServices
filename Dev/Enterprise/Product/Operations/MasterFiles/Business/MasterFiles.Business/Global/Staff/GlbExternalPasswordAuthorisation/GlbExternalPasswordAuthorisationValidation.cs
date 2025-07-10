//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbExternalPasswordAuthorisationValidation
//
//    This class should be used for overriding validation in AutoGlbExternalPasswordAuthorisationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordAuthorisationValidation : AutoGlbExternalPasswordAuthorisationValidation
	{
		public GlbExternalPasswordAuthorisationValidation(AutoGlbExternalPasswordAuthorisation parent) : base(parent)
		{
		}
	}
}

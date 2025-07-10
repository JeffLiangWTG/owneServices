//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefLanguageTextValidation
//
//    This class should be used for overriding validation in AutoRefLanguageTextValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefLanguageTextValidation : AutoRefLanguageTextValidation
	{
		public RefLanguageTextValidation(AutoRefLanguageText parent) : base(parent)
		{
		}
	}
}

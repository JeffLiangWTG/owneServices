//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusPreferenceLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusPreferenceLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusPreferenceLanguageValidation : AutoRefCusPreferenceLanguageValidation
	{
		public RefCusPreferenceLanguageValidation(AutoRefCusPreferenceLanguage parent) : base(parent)
		{
		}
	}
}

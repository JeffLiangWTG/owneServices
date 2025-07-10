//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProfileQuestionLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusProfileQuestionLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestionLanguageValidation : AutoRefCusProfileQuestionLanguageValidation
	{
		public RefCusProfileQuestionLanguageValidation(AutoRefCusProfileQuestionLanguage parent) : base(parent)
		{
		}
	}
}

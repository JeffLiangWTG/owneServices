//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeTypeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeTypeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeTypeLanguageValidation : AutoRefCusCodeTypeLanguageValidation
	{
		public RefCusCodeTypeLanguageValidation(AutoRefCusCodeTypeLanguage parent) : base(parent)
		{
		}
	}
}

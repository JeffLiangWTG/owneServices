//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusRateTypeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusRateTypeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusRateTypeLanguageValidation : AutoRefCusRateTypeLanguageValidation
	{
		public RefCusRateTypeLanguageValidation(AutoRefCusRateTypeLanguage parent) : base(parent)
		{
		}
	}
}

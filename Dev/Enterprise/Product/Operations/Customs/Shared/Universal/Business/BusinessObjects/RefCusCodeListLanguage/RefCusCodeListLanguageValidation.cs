//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeListLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeListLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListLanguageValidation : AutoRefCusCodeListLanguageValidation
	{
		public RefCusCodeListLanguageValidation(AutoRefCusCodeListLanguage parent) : base(parent)
		{
		}
	}
}

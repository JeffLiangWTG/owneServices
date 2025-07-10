//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffLanguageValidation : AutoRefCusTariffLanguageValidation
	{
		public RefCusTariffLanguageValidation(AutoRefCusTariffLanguage parent) : base(parent)
		{
		}
	}
}

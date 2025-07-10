//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffAdditionalCodeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffAdditionalCodeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffAdditionalCodeLanguageValidation : AutoRefCusTariffAdditionalCodeLanguageValidation
	{
		public RefCusTariffAdditionalCodeLanguageValidation(AutoRefCusTariffAdditionalCodeLanguage parent)
			: base(parent)
		{
		}
	}
}

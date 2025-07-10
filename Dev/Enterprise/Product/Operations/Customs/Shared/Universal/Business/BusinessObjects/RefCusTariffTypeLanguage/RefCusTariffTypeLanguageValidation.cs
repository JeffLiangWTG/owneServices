//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffTypeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffTypeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffTypeLanguageValidation : AutoRefCusTariffTypeLanguageValidation
	{
		public RefCusTariffTypeLanguageValidation(AutoRefCusTariffTypeLanguage parent) : base(parent)
		{
		}
	}
}

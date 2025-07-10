//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTaxOrFeeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusTaxOrFeeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTaxOrFeeLanguageValidation : AutoRefCusTaxOrFeeLanguageValidation
	{
		public RefCusTaxOrFeeLanguageValidation(AutoRefCusTaxOrFeeLanguage parent) : base(parent)
		{
		}
	}
}

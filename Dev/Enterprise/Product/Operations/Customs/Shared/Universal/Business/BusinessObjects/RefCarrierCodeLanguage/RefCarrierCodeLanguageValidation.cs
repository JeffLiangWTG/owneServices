//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCarrierCodeLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCarrierCodeLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCarrierCodeLanguageValidation : AutoRefCarrierCodeLanguageValidation
	{
		public RefCarrierCodeLanguageValidation(AutoRefCarrierCodeLanguage parent) : base(parent)
		{
		}
	}
}

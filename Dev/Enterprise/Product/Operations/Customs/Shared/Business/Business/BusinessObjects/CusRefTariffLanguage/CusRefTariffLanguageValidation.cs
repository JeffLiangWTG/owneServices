//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTariffLanguageValidation
//
//    This class should be used for overriding validation in AutoCusRefTariffLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefTariffLanguageValidation : AutoCusRefTariffLanguageValidation
	{
		public CusRefTariffLanguageValidation(AutoCusRefTariffLanguage parent) : base(parent)
		{
		}
	}
}

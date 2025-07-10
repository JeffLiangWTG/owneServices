//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTradeGroupLanguageValidation
//
//    This class should be used for overriding validation in AutoRefCusTradeGroupLanguageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTradeGroupLanguageValidation : AutoRefCusTradeGroupLanguageValidation
	{
		public RefCusTradeGroupLanguageValidation(AutoRefCusTradeGroupLanguage parent) : base(parent)
		{
		}
	}
}

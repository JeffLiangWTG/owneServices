//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTradeGroupCountryViewValidation
//
//    This class should be used for overriding validation in AutoCusRefTradeGroupCountryViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupCountryViewValidation : AutoCusRefTradeGroupCountryViewValidation
	{
		public CusRefTradeGroupCountryViewValidation(AutoCusRefTradeGroupCountryView parent) : base(parent)
		{
		}
	}
}

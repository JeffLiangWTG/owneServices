//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTradeGroupCountryValidation
//
//    This class should be used for overriding validation in AutoRefCusTradeGroupCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusTradeGroupCountryValidation : AutoRefCusTradeGroupCountryValidation
	{
		public RefCusTradeGroupCountryValidation(AutoRefCusTradeGroupCountry parent) : base(parent)
		{
		}
	}
}

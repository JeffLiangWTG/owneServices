//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefExchangeRateZZValidation
//
//    This class should be used for overriding validation in AutoRefExchangeRateZZValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefExchangeRateZZValidation : AutoRefExchangeRateZZValidation
	{
		public RefExchangeRateZZValidation(AutoRefExchangeRateZZ parent) : base(parent)
		{
		}
	}
}

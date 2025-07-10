//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefExchangeRateValidation
//
//    This class should be used for overriding validation in AutoZZRefExchangeRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ZZRefExchangeRateValidation : AutoZZRefExchangeRateValidation
	{
		public ZZRefExchangeRateValidation(AutoZZRefExchangeRate parent) : base(parent)
		{
		}
	}
}

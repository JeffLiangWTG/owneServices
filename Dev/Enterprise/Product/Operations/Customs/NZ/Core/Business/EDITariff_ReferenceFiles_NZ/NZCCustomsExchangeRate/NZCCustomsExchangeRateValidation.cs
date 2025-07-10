//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCCustomsExchangeRateValidation
//
//    This class should be used for overriding validation in AutoNZCCustomsExchangeRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCustomsExchangeRateValidation : AutoNZCCustomsExchangeRateValidation
	{
		public NZCCustomsExchangeRateValidation(AutoNZCCustomsExchangeRate parent)
			: base(parent)
		{
		}
	}
}

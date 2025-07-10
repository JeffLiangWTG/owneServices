//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCACCaseTariffValidation
//
//    This class should be used for overriding validation in AutoUSCACCaseTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCACCaseTariffValidation : AutoUSCACCaseTariffValidation
	{
		public USCACCaseTariffValidation(AutoUSCACCaseTariff parent) : base(parent)
		{
		}
	}
}

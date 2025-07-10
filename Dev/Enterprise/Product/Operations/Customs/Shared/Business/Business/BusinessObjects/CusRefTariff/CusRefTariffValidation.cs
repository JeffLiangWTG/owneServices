//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTariffValidation
//
//    This class should be used for overriding validation in AutoCusRefTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefTariffValidation : AutoCusRefTariffValidation
	{
		public CusRefTariffValidation(AutoCusRefTariff parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTariffUomValidation
//
//    This class should be used for overriding validation in AutoCusRefTariffUomValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefTariffUomValidation : AutoCusRefTariffUomValidation
	{
		public CusRefTariffUomValidation(AutoCusRefTariffUom parent) : base(parent)
		{
		}
	}
}

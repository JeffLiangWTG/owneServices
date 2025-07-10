//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefRateUomValidation
//
//    This class should be used for overriding validation in AutoCusRefRateUomValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusRefRateUomValidation : AutoCusRefRateUomValidation
	{
		public CusRefRateUomValidation(AutoCusRefRateUom parent) : base(parent)
		{
		}
	}
}

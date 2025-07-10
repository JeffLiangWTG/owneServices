//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusLiquidationValidation
//
//    This class should be used for overriding validation in AutoCusLiquidationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class CusLiquidationValidation : AutoCusLiquidationValidation
	{
		public CusLiquidationValidation(AutoCusLiquidation parent) : base(parent)
		{
		}
	}
}

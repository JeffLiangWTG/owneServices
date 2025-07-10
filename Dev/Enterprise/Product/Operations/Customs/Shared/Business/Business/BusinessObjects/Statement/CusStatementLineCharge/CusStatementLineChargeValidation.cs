//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementLineChargeValidation
//
//    This class should be used for overriding validation in AutoCusStatementLineChargeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusStatementLineChargeValidation : AutoCusStatementLineChargeValidation
	{
		public CusStatementLineChargeValidation(AutoCusStatementLineCharge parent) : base(parent)
		{
		}
	}
}

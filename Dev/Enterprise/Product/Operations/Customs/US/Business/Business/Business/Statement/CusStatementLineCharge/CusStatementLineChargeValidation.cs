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

namespace Enterprise.Customs.US.Business
{
	public class CusStatementLineChargeValidation : Customs.Business.CusStatementLineChargeValidation
	{
		public CusStatementLineChargeValidation(CusStatementLineCharge parent)
			: base(parent)
		{
		}
	}
}

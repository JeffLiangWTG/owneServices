//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusWHSOperatorTransactionValidation
//
//    This class should be used for overriding validation in AutoCusWHSOperatorTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionValidation : AutoCusWHSOperatorTransactionValidation
	{
		public CusWHSOperatorTransactionValidation(AutoCusWHSOperatorTransaction parent) : base(parent)
		{
		}
	}
}

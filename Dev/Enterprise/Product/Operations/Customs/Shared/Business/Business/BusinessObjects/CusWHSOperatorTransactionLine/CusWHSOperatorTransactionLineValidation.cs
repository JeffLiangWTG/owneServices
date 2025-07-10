//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusWHSOperatorTransactionLineValidation
//
//    This class should be used for overriding validation in AutoCusWHSOperatorTransactionLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionLineValidation : AutoCusWHSOperatorTransactionLineValidation
	{
		public CusWHSOperatorTransactionLineValidation(AutoCusWHSOperatorTransactionLine parent) : base(parent)
		{
		}
	}
}

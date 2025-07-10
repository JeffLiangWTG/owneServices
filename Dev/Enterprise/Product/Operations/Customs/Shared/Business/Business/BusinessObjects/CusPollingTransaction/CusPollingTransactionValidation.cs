//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPollingTransactionValidation
//
//    This class should be used for overriding validation in AutoCusPollingTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPollingTransactionValidation : AutoCusPollingTransactionValidation
	{
		public CusPollingTransactionValidation(AutoCusPollingTransaction parent) : base(parent)
		{
		}
	}
}

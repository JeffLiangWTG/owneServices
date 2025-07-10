//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusWHSOperatorTransactionBatchValidation
//
//    This class should be used for overriding validation in AutoCusWHSOperatorTransactionBatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusWHSOperatorTransactionBatchValidation : AutoCusWHSOperatorTransactionBatchValidation
	{
		public CusWHSOperatorTransactionBatchValidation(AutoCusWHSOperatorTransactionBatch parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionLineSubAccountValidation
//
//    This class should be used for overriding validation in AutoAccTransactionLineSubAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLineSubAccountValidation : AutoAccTransactionLineSubAccountValidation
	{
		public AccTransactionLineSubAccountValidation(AutoAccTransactionLineSubAccount parent) : base(parent)
		{
		}
	}
}

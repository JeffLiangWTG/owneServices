//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderSubAccountValidation
//
//    This class should be used for overriding validation in AutoAccTransactionHeaderSubAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionHeaderSubAccountValidation : AutoAccTransactionHeaderSubAccountValidation
	{
		public AccTransactionHeaderSubAccountValidation(AutoAccTransactionHeaderSubAccount parent) : base(parent)
		{
		}
	}
}

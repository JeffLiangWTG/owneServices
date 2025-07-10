//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPaymentApprovalItemValidation
//
//    This class should be used for overriding validation in AutoAccPaymentApprovalItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApprovalItemValidation : AutoAccPaymentApprovalItemValidation
	{
		public AccPaymentApprovalItemValidation(AutoAccPaymentApprovalItem parent) : base(parent)
		{
		}
	}
}

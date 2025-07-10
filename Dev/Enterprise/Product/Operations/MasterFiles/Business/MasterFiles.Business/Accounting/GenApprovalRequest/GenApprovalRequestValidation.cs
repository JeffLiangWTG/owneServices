//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenApprovalRequestValidation
//
//    This class should be used for overriding validation in AutoGenApprovalRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class GenApprovalRequestValidation : AutoGenApprovalRequestValidation
	{
		public GenApprovalRequestValidation(AutoGenApprovalRequest parent)
			: base(parent)
		{
		}

		protected override void CheckXP_ApprovalRequestDataIsValidZBlobSize()
		{
		}
	}
}

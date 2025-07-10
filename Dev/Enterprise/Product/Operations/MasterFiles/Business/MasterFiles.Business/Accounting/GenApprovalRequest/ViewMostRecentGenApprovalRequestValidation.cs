//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewMostRecentGenApprovalRequestValidation
//
//    This class should be used for overriding validation in AutoViewMostRecentGenApprovalRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class ViewMostRecentGenApprovalRequestValidation : AutoViewMostRecentGenApprovalRequestValidation
	{
		public ViewMostRecentGenApprovalRequestValidation(AutoViewMostRecentGenApprovalRequest parent) : base(parent)
		{
		}
	}
}

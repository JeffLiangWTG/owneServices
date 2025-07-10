//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgMatchApprovalValidation
//
//    This class should be used for overriding validation in AutoOrgMatchApprovalValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgMatchApprovalValidation : AutoOrgMatchApprovalValidation
	{
		public OrgMatchApprovalValidation(AutoOrgMatchApproval parent) : base(parent)
		{
		}
	}
}

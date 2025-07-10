//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCommissionCalculationQueueValidation
//
//    This class should be used for overriding validation in AutoOrgCommissionCalculationQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionCalculationQueueValidation : AutoOrgCommissionCalculationQueueValidation
	{
		public OrgCommissionCalculationQueueValidation(AutoOrgCommissionCalculationQueue parent) : base(parent)
		{
		}
	}
}

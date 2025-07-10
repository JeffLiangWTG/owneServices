//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsReadyForPlanningJobsViewValidation
//
//    This class should be used for overriding validation in AutoWhsReadyForPlanningJobsViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReadyForPlanningJobsViewValidation : AutoWhsReadyForPlanningJobsViewValidation
	{
		public WhsReadyForPlanningJobsViewValidation(AutoWhsReadyForPlanningJobsView parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewRatingContractAllocationLineSummaryValidation
//
//    This class should be used for overriding validation in AutoViewRatingContractAllocationLineSummaryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractAllocationLineSummaryValidation : AutoViewRatingContractAllocationLineSummaryValidation
	{
		public ViewRatingContractAllocationLineSummaryValidation(AutoViewRatingContractAllocationLineSummary parent) : base(parent)
		{
		}
	}
}

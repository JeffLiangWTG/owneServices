//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewRatingContractSummaryValidation
//
//    This class should be used for overriding validation in AutoViewRatingContractSummaryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractSummaryValidation : AutoViewRatingContractSummaryValidation
	{
		public ViewRatingContractSummaryValidation(AutoViewRatingContractSummary parent) : base(parent)
		{
		}
	}
}

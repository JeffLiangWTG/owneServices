//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewRatingContractQuantityValidation
//
//    This class should be used for overriding validation in AutoViewRatingContractQuantityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ContractManagement.Business
{
	public class ViewRatingContractQuantityValidation : AutoViewRatingContractQuantityValidation
	{
		public ViewRatingContractQuantityValidation(AutoViewRatingContractQuantity parent) : base(parent)
		{
		}
	}
}

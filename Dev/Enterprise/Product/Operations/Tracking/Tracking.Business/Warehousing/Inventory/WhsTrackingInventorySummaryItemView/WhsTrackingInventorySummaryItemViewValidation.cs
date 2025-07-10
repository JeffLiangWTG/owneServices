//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsTrackingInventorySummaryItemViewValidation
//
//    This class should be used for overriding validation in AutoWhsTrackingInventorySummaryItemViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Tracking.Business
{
	public class WhsTrackingInventorySummaryItemViewValidation : AutoWhsTrackingInventorySummaryItemViewValidation
	{
		public WhsTrackingInventorySummaryItemViewValidation(AutoWhsTrackingInventorySummaryItemView parent) : base(parent)
		{
		}
	}
}

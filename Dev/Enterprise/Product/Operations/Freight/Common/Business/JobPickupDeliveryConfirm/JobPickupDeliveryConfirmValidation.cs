//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobPickupDeliveryConfirmValidation
//
//    This class should be used for overriding validation in AutoJobPickupDeliveryConfirmValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobPickupDeliveryConfirmValidation : AutoJobPickupDeliveryConfirmValidation
	{
		public JobPickupDeliveryConfirmValidation(AutoJobPickupDeliveryConfirm parent) : base(parent)
		{
		}
	}
}

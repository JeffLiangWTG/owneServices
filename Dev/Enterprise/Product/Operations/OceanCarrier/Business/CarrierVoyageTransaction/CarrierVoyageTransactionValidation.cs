//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCarrierVoyageTransactionValidation
//
//    This class should be used for overriding validation in AutoCarrierVoyageTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageTransactionValidation : AutoCarrierVoyageTransactionValidation
	{
		public CarrierVoyageTransactionValidation(AutoCarrierVoyageTransaction parent) : base(parent)
		{
		}
	}
}


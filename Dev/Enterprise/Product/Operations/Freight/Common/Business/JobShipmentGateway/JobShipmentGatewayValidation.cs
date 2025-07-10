//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobShipmentGatewayValidation
//
//    This class should be used for overriding validation in AutoJobShipmentGatewayValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobShipmentGatewayValidation : AutoJobShipmentGatewayValidation
	{
		public JobShipmentGatewayValidation(AutoJobShipmentGateway parent) : base(parent)
		{
		}
	}
}

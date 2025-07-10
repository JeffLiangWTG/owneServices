//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConShipLinkValidation
//
//    This class should be used for overriding validation in AutoJobConShipLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	public class JobConShipLinkValidation : AutoJobConShipLinkValidation
	{
		public JobConShipLinkValidation(AutoJobConShipLink parent) : base(parent)
		{
		}
	}
}

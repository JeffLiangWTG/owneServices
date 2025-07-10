//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyDestinationValidation
//
//    This class should be used for overriding validation in AutoJobVoyDestinationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobVoyDestinationValidation : AutoJobVoyDestinationValidation
	{
		public JobVoyDestinationValidation(AutoJobVoyDestination parent)
			: base(parent)
		{
		}
	}
}

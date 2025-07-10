//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobContainerValidation
//
//    This class should be used for overriding validation in AutoJobContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobContainerValidation : AutoJobContainerValidation
	{
		public JobContainerValidation(AutoJobContainer parent) : base(parent)
		{
		}
	}
}

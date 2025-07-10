//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetValidation
//
//    This class should be used for overriding validation in AutoJobCartageRunSheetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class JobContainerLegsValidation : AutoJobContainerLegsValidation
	{
		public JobContainerLegsValidation(AutoJobContainerLegs parent)
			: base(parent)
		{
		}
	}
}

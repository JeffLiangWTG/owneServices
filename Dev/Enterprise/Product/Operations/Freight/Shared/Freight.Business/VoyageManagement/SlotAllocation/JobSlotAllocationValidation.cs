//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSlotAllocationValidation
//
//    This class should be used for overriding validation in AutoJobSlotAllocationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	public class JobSlotAllocationValidation : AutoJobSlotAllocationValidation
	{
		public JobSlotAllocationValidation(AutoJobSlotAllocation parent) : base(parent)
		{
		}
	}
}

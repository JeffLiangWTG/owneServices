//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsItemCycleCountLocationVarianceValidation
//
//    This class should be used for overriding validation in AutoWhsItemCycleCountLocationVarianceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemCycleCountLocationVarianceValidation : AutoWhsItemCycleCountLocationVarianceValidation
	{
		public WhsItemCycleCountLocationVarianceValidation(AutoWhsItemCycleCountLocationVariance parent) : base(parent)
		{
		}
	}
}


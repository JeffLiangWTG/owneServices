//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsWorkOrderWithRelatedOrderViewValidation
//
//    This class should be used for overriding validation in AutoWhsWorkOrderWithRelatedOrderViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderWithRelatedOrderViewValidation : AutoWhsWorkOrderWithRelatedOrderViewValidation
	{
		public WhsWorkOrderWithRelatedOrderViewValidation(AutoWhsWorkOrderWithRelatedOrderView parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsOrderTrolleyViewValidation
//
//    This class should be used for overriding validation in AutoWhsOrderTrolleyViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderTrolleyViewValidation : AutoWhsOrderTrolleyViewValidation
	{
		public WhsOrderTrolleyViewValidation(AutoWhsOrderTrolleyView parent) : base(parent)
		{
		}
	}
}

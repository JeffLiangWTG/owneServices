//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsOrderTrolleyViewLookups
//
//    This class should be used for overriding collections in AutoWhsOrderTrolleyViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderTrolleyViewLookups : AutoWhsOrderTrolleyViewLookups
	{
		public WhsOrderTrolleyViewLookups(AutoWhsOrderTrolleyView parent) : base(parent)
		{
		}
	}
}

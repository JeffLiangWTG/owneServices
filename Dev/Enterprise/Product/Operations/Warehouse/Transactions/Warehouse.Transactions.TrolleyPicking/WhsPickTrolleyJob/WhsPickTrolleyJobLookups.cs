//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickTrolleyJobLookups
//
//    This class should be used for overriding collections in AutoWhsPickTrolleyJobLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickTrolleyJobLookups : AutoWhsPickTrolleyJobLookups
	{
		public WhsPickTrolleyJobLookups(AutoWhsPickTrolleyJob parent) : base(parent)
		{
		}
	}
}

// Add tests to TrolleyPicking.Testing project.

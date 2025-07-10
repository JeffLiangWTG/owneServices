//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPackageAuditLineFailureLookups
//
//    This class should be used for overriding collections in AutoWhsPackageAuditLineFailureLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageAuditLineFailureLookups : AutoWhsPackageAuditLineFailureLookups
	{
		public WhsPackageAuditLineFailureLookups(AutoWhsPackageAuditLineFailure parent) : base(parent)
		{
		}
	}
}

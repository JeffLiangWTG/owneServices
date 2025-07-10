//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickLineLookups
//
//    This class should be used for overriding collections in AutoWhsPickLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickLineLookups : AutoWhsPickLineLookups
	{
		public WhsPickLineLookups(AutoWhsPickLine parent) : base(parent)
		{
		}

		public override ProcessTaskCollection Tasks => Factory.GetCachedValue("WhsPickLineLookups|Tasks", () => new ProcessTaskCollection(Factory, ZQuery.NoResultQuery));
	}
}

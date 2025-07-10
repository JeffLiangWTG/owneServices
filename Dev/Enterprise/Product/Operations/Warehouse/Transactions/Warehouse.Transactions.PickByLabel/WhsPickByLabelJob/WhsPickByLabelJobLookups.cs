//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickByLabelJobLookups
//
//    This class should be used for overriding collections in AutoWhsPickByLabelJobLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.PickByLabel
{
	public class WhsPickByLabelJobLookups : AutoWhsPickByLabelJobLookups
	{
		public WhsPickByLabelJobLookups(AutoWhsPickByLabelJob parent) : base(parent)
		{
		}

		public override ProcessTaskCollection Tasks => Factory.GetCachedValue("WhsPickByLabelJobLookups|Tasks", () => new ProcessTaskCollection(Factory, ZQuery.NoResultQuery));
	}
}
// Add tests to PickByLabel.Testing project.

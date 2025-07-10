//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCarrierCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCarrierCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCarrierCombinedLookups : AutoZZRefCarrierCombinedLookups
	{
		public ZZRefCarrierCombinedLookups(AutoZZRefCarrierCombined parent)
			: base(parent)
		{ }

		public IBusinessObjectCollection CountryOrGroupingList => new RefDataGroupingCollection(Factory);
	}
}

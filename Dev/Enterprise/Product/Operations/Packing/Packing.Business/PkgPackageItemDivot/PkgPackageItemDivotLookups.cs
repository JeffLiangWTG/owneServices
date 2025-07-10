//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageItemDivotLookups
//
//    This class should be used for overriding collections in AutoPkgPackageItemDivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public class PkgPackageItemDivotLookups : AutoPkgPackageItemDivotLookups
	{
		public PkgPackageItemDivotLookups(AutoPkgPackageItemDivot parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList WeightUQs => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
	}
}

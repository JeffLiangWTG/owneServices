//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgBuyerSupplierLinkPackPivotLookups
//
//    This class should be used for overriding collections in AutoOrgBuyerSupplierLinkPackPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgBuyerSupplierLinkPackPivotLookups : AutoOrgBuyerSupplierLinkPackPivotLookups
	{
		public OrgBuyerSupplierLinkPackPivotLookups(AutoOrgBuyerSupplierLinkPackPivot parent) : base(parent)
		{
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList LengthUnitList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		public RefPackTypeCollection PackTypeList
		{
			get { return new RefPackTypeCollection(Factory); }
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSupplierBuyerLinkLookups
//
//    This class should be used for overriding collections in AutoOrgSupplierBuyerLinkLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkLookups : AutoOrgSupplierBuyerLinkLookups
	{
		public OrgSupplierBuyerLinkLookups(AutoOrgSupplierBuyerLink parent) : base(parent)
		{
		}

		public new OrgSupplierBuyerLink Parent
		{
			get { return (OrgSupplierBuyerLink)base.Parent; }
		}

		#region AuthorityToLeaveOptions

		public CodeDescriptionPairList AuthorityToLeaveOptions
		{
			get { return Factory.GetCachedValue<AuthorityToLeaveOptions>(); }
		}

		#endregion

		public OrgRelationTypeList RelationTypeList
		{
			get { return Factory.GetCachedValue<OrgRelationTypeList>(); }
		}

		#region Implementation

		protected BusinessObjectFactory ReadOnlyFactory
		{
			get { return Parent.Buyer != null ? Parent.Buyer.ReadOnlyFactory : new BusinessObjectFactory(); }
		}

		#endregion

	}
}

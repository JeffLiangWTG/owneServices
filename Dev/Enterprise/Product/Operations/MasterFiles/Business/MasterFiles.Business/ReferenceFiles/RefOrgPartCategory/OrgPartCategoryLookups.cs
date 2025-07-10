//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgPartCategoryLookups
//
//    This class should be used for overriding collections in AutoOrgPartCategoryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartCategoryLookups : AutoOrgPartCategoryLookups
	{
		public OrgPartCategoryLookups(AutoOrgPartCategory parent)
			: base(parent)
		{
		}

		#region Parents

		public override OrgPartCategoryCollection Parents
		{
			get { return new OrgPartCategoryParentCollection(Factory, (OrgPartCategory)Parent); }
		}

		#endregion
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSecondaryPartBOMPivotLookups
//
//    This class should be used for overriding collections in AutoOrgSecondaryPartBOMPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMPivotLookups : AutoOrgSecondaryPartBOMPivotLookups
	{
		public OrgSecondaryPartBOMPivotLookups(AutoOrgSecondaryPartBOMPivot parent)
			: base(parent)
		{
		}

		public override OrgPartBOMCollection Components
		{
			get
			{
				var secondaryPart = ((OrgSecondaryPartBOMPivot)Parent).SecondaryPart;
				return secondaryPart != null
					? Factory.GetCachedValue("OrgSecondaryPartBOMPivotLookups|Components|" + secondaryPart.OSB_OP_MainProduct, () => new OrgPartBOMCollection(secondaryPart.MainProduct))
					: Factory.GetCachedValue("OrgSecondaryPartBOMPivotLookups|Components", () => new OrgPartBOMCollection(Factory));
			}
		}
	}
}

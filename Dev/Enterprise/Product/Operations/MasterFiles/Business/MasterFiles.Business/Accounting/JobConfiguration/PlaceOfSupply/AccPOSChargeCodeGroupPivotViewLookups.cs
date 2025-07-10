//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPOSChargeCodeGroupPivotViewLookups
//
//    This class should be used for overriding collections in AutoAccPOSChargeCodeGroupPivotViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSChargeCodeGroupPivotViewLookups : AutoAccPOSChargeCodeGroupPivotViewLookups
	{
		public AccPOSChargeCodeGroupPivotViewLookups(AutoAccPOSChargeCodeGroupPivotView parent) : base(parent)
		{
		}

		protected new AccPOSChargeCodeGroupPivot Parent => base.Parent as AccPOSChargeCodeGroupPivot;

		public AccPOSChargeCodeGroupCollection Groups => new AccPOSChargeCodeGroupCollection(Parent.Company ?? Factory.Load<GlbCompany>(Env.CurrentCompanyPK));

		public AccChargeCodeCollection ChargeCodes => Parent.Company != null
			? new AccChargeCodeCollection(Factory, Parent.Company)
			: new AccChargeCodeCollection(Factory, ZQuery.NoResultQuery);
	}
}


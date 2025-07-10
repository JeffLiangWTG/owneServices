using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderFilterBusinessObject : OrderFilterBusinessObject
	{
		protected override WhsOrgSupplierPartCollection GetProducts()
		{
			return new WhsOrgSupplierPartCollection(Factory,
				OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP));
		}

		protected override bool ShouldProductFilterEnsureClientFilterIsEnteredFirst
		{
			get { return false; }
		}

		public OrgContact LoggedInUser { get; set; }

		protected override void WarehouseWebFilterValidation(ZPropertyInfo info)
		{
			base.WarehouseWebFilterValidation(info);
			WarehouseValidationHelper.CheckWarehouseEligibility(info, LoggedInUser);
		}

		protected override FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			return new FilterStripLayoutsHelperForWeb(this, LoggedInUser);
		}
	}
}

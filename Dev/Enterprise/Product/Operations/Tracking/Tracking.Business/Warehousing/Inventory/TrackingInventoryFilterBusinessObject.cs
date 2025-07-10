using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;

namespace Enterprise.Tracking.Business
{
	public class TrackingInventoryFilterBusinessObject : InventoryFilterBusinessObject
	{
		public TrackingInventoryFilterBusinessObject(OrgHeader loggedInWebUsersOrg)
			: base(loggedInWebUsersOrg)
		{
		}

		protected override WhsOrgSupplierPartCollection GetProductsCore()
		{
			return new WhsOrgSupplierPartCollection(Factory, GetProductQueryForTracking());
		}

		public TrackingInventoryFilterBusinessObject()
		{
		}

		static ZQuery GetProductQueryForTracking()
		{
			var query = OrgRestrictionFilterFactory.Instance.GetSubQuery(typeof(OrgSupplierPart), typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			query.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.True);
			return query;
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

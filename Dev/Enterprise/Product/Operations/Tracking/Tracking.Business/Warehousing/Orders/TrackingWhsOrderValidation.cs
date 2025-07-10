using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderValidation : WhsOrderValidation
	{
		public TrackingWhsOrderValidation(TrackingWhsOrder parent) : base(parent.WhsOrder)
		{
			this.parent = parent;
		}

		readonly TrackingWhsOrder parent;
		protected new TrackingWhsOrder Parent
		{
			get { return parent; }
		}

		protected override void CheckWD_WW_Whs()
		{
			base.CheckWD_WW_Whs();
			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				if (siteUser != null)
				{
					WarehouseValidationHelper.CheckWarehouseEligibility(Parent.WhsOrder.WD_WW_WhsInfo, siteUser.LoggedInUser);
				}
			}
		}
	}
}

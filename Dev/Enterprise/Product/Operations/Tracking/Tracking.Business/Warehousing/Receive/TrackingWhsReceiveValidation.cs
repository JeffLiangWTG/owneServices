using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReceiveValidation : WhsReceiveValidation
	{
		public TrackingWhsReceiveValidation(TrackingWhsReceive parent)
			: base(parent.WhsReceive)
		{
		}

		protected override void CheckWD_WW_Whs()
		{
			base.CheckWD_WW_Whs();
			if (WebEnv.AppInstance?.SiteUser is OrgContactWebUser siteUser)
			{
				WarehouseValidationHelper.CheckWarehouseEligibility(Parent.WD_WW_WhsInfo, siteUser.LoggedInUser);
			}
		}
	}
}

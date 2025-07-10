using CargoWise.EntityFramework;

using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class GlbCompanyCampaignWithoutFilterController : CRMGlbCompanyCampaignController
	{
		public GlbCompanyCampaignWithoutFilterController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbCompanyCampaignWithoutFilter; }
		}

		#region Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCompanyCampaignWithoutFilter; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbCompanyCampaignForm((GlbCompanyCampaign)businessEntity, false);
		}

		#endregion
	}
}

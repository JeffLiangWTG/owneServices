using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class GlbCompanyCampaignWithoutFilterModule : CRMGlbCompanyCampaignModule
	{
		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GlbCompanyCampaignWithoutFilter; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignWithoutFilter);
		}

		#endregion
	}
}

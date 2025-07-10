using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignContactModule : GlbCompanyCampaignContactModule
	{
		#region Overrides

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRGlbCompanyCampaignContact);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			var campaign = (HRGlbCompanyCampaign)Campaign;
			var moduleId = campaign?.DripMarketingFilterRuleModule ?? ModuleIDs.DripMarketingFilterRuleHR;
			return (FilterBusinessObject)RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleId, campaign);
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new HRGlbCompanyCampaignContactFilterControl(GridCollection, (HRGlbCompanyCampaignContactFilterBusinessObject)FilterBusinessObject, (HRGlbCompanyCampaign)Campaign);
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return Campaign != null ? new GlbCampaignContactCollection(Campaign) : new GlbCampaignContactCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRGlbCompanyCampaignContact; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.HumanResourcesCampaignManager; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRCampaignManagement; }
		}

		protected override ZString DripMarketingFilterRuleModuleName
		{
			get
			{
				return ModuleIDs.DripMarketingFilterRuleHR.Name;
			}
		}

		#endregion
	}
}

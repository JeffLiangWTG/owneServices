using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class SurveyCampaignPlugInController : VoteExamSurveyCampaignPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.SurveyCampaignPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override VoteExamSurveyPlugIn GetVoteExamSurveyPlugIn(GlbCompanyCampaign campaign)
		{
			return new SurveyCampaignPlugIn(campaign);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.MarketingManager.Module.Res.GetData("PlugInTabPage|SurveyCampaignPlugIn", "Survey"); } }
	}
}

using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	class VoteCampaignPlugInController : VoteExamSurveyCampaignPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.VoteCampaignPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override VoteExamSurveyPlugIn GetVoteExamSurveyPlugIn(GlbCompanyCampaign campaign)
		{
			return new VoteCampaignPlugIn(campaign);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.MarketingManager.Module.Res.GetData("PlugInTabPage|VoteCampaignPlugIn", "Voting"); } }
	}
}

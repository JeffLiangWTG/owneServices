using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.MarketingManager.Module;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class LearningCentreExamPlugInController : SurveyCampaignPlugInController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.LearningCentreExamPlugIn; }
		}

		protected override VoteExamSurveyPlugIn GetVoteExamSurveyPlugIn(GlbCompanyCampaign campaign)
		{
			return new LearningCentreExamPlugIn((LearningCentreCampaign)campaign);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|LearningCentreExamPlugIn", "Examination"); } }

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRJobSkillExamCampaignDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRJobSkillExamCampaignEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRJobSkillExamCampaignNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRJobSkillExamCampaignView; }
		}

		#endregion
	}
}

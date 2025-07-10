using System.Windows.Forms;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.GUI
{
	public class LearningCentreScaledTestPlugIn : LearningCentreCampaignPlugIn
	{
		public LearningCentreScaledTestPlugIn(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		public override string Name
		{
			get { return "LearningCentreScaledTest"; }
		}

		protected override string LearningCentreTestType
		{
			get { return LearningCentreTestTypes.Codes.Scaled; }
		}

		protected override QuestionsUserControl GetNewQuestionsControl()
		{
			return new ScaledTestQuestionsUserControl();
		}

		protected override QuestionDetailsUserControl GetNewQuestionDetailsControl()
		{
			return new ScaledTestQuestionDetailsUserControl();
		}

		protected override Control GetNewResultsControlOverride()
		{
			return new ScaledTestResultsUserControl();
		}
	}
}

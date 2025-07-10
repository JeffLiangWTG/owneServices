using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public class LearningCentreExamPlugIn : LearningCentreCampaignPlugIn
	{
		public LearningCentreExamPlugIn(LearningCentreCampaign campaign)
			: base(campaign)
		{
		}

		public override string Name
		{
			get { return "LearningCentreExam"; }
		}

		protected override string LearningCentreTestType
		{
			get { return LearningCentreTestTypes.Codes.Exam; }
		}

		protected override QuestionDetailsUserControl GetNewQuestionDetailsControl()
		{
			return new ExamQuestionDetailsUserControl();
		}

		protected override ResultsByQuestionUserControl GetNewResultsByQuestionControl()
		{
			return new ExamResultsByQuestionUserControl();
		}

		protected override QuestionsUserControl GetNewQuestionsControl()
		{
			return new ExamQuestionsUserControl();
		}

		protected override ZUserControl GetNewResultSummaryControl()
		{
			return new ExamSummaryUserControl();
		}
	}
}

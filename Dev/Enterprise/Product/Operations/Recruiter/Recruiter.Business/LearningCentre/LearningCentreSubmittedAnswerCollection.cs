using System.Collections.Generic;
using System.Linq;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreSubmittedAnswerCollection : VoteExamSurveySubmittedAnswerCollection
	{
		public LearningCentreSubmittedAnswerCollection(LearningCentreQuestion question)
			: base(question)
		{
		}

		public LearningCentreSubmittedAnswerCollection(LearningCentreCampaignItem campaignItem, string category = "")
			: base(campaignItem)
		{
			this.category = category;
		}

		public LearningCentreSubmittedAnswerCollection(VoteExamSurveyAnswerSet voteExamSurveyAnswerSet)
			: base(voteExamSurveyAnswerSet)
		{
		}

		protected override bool ShouldCreateSubmittedAnswer(GlbCompanyCampaignItem campaignItem, VoteExamSurveyQuestion question)
		{
			bool result = base.ShouldCreateSubmittedAnswer(campaignItem, question);
			if (result && !string.IsNullOrEmpty(category))
			{
				result = question.HY_QuestionCategory == category;
			}
			return result;
		}

		public new LearningCentreSubmittedAnswer this[int index]
		{
			get { return (LearningCentreSubmittedAnswer)base[index]; }
		}

		public new LearningCentreSubmittedAnswer AddNew()
		{
			return (LearningCentreSubmittedAnswer)base.AddNew();
		}

		public IEnumerable<ILearningCentreSubmittedAnswer> CorrectAnswers
		{
			get { return PopulatedAnswers.Cast<ILearningCentreSubmittedAnswer>().Where(a => a.IsAnsweredCorrectly); }
		}

		public IEnumerable<ILearningCentreSubmittedAnswer> IncorrectAnswers
		{
			get { return PopulatedAnswers.Cast<ILearningCentreSubmittedAnswer>().Where(a => a.IsAnsweredIncorrectly); }
		}

		public IEnumerable<ILearningCentreSubmittedAnswer> EmptyAnswers
		{
			get { return this.Cast<VoteExamSurveySubmittedAnswer>().Except(PopulatedAnswers).Cast<ILearningCentreSubmittedAnswer>(); }
		}

		protected override VoteExamSurveySubmittedAnswer GetNewSubmittedAnswer(GlbCompanyCampaignItem item, VoteExamSurveyQuestion question)
		{
			return new LearningCentreSubmittedAnswer((LearningCentreCampaignItem)item, (LearningCentreQuestion)question);
		}

		readonly string category;
	}
}

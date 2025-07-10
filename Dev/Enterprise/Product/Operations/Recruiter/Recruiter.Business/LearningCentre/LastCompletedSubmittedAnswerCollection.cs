using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class LastCompletedSubmittedAnswerCollection : LearningCentreSubmittedAnswerCollection
	{
		public LastCompletedSubmittedAnswerCollection(LearningCentreQuestion question)
			: base(question)
		{
		}

		public LastCompletedSubmittedAnswerCollection(LearningCentreCampaignItem campaignItem)
			: base(campaignItem)
		{
		}

		public new LastCompletedSubmittedAnswer this[int index]
		{
			get { return (LastCompletedSubmittedAnswer)base[index]; }
		}

		public new LastCompletedSubmittedAnswer AddNew()
		{
			return (LastCompletedSubmittedAnswer)base.AddNew();
		}

		protected override VoteExamSurveySubmittedAnswer GetNewSubmittedAnswer(GlbCompanyCampaignItem campaignItem, VoteExamSurveyQuestion question)
		{
			return new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, (LearningCentreQuestion)question);
		}

		public override void Load()
		{
			RemoveAll();

			foreach (GlbCompanyCampaignItem campaignItem in campaignItems)
			{
				campaignItem.Factory.AddFetchHint(typeof(ExamAttempt), new ZQuery(ExamAttemptSchema.EXA_G8, campaignItem.PK));
			}

			foreach (GlbCompanyCampaignItem campaignItem in campaignItems)
			{
				foreach (VoteExamSurveyQuestion question in questions)
				{
					if (ShouldCreateSubmittedAnswer(campaignItem, question))
					{
						Add(GetNewSubmittedAnswer(campaignItem, question));
					}
				}
			}

			if (SortInformation == null)
			{
				Sort(new SubmittedAnswerComparer(campaign));
			}
			IsLoaded = true;
		}
	}
}

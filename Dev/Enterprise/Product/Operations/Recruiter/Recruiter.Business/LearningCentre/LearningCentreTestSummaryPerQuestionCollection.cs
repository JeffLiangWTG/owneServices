using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreTestSummaryPerQuestionCollection : VoteExamSurveySummaryCollection
	{
		readonly GlbCompanyCampaign CompanyCampaign;

		readonly LearningCentreQuestion question;

		public bool AnalyzeProgressCancelled { get; set; }

		public LearningCentreQuestion Question
		{
			get
			{
				return question;
			}
		}

		public LearningCentreTestSummaryPerQuestionCollection(LearningCentreQuestion learningCentreQuestion)
			: base(null)
		{
			CompanyCampaign = learningCentreQuestion.Campaign;
			question = learningCentreQuestion;
			AnalyzeProgressCancelled = false;
		}

		public event EventHandler SummaryAnalyzeBegin;
		void OnSummaryAnalyzeBegin()
		{
			SummaryAnalyzeBegin?.Invoke(this, EventArgs.Empty);
		}

		[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
		public delegate void SummaryAnalyzedEventHandler(object sender, SummaryAnalyzedEventArgs e);
		public event SummaryAnalyzedEventHandler SummaryAnalyzedUpdate;
		void OnSummaryAnalyzedUpdate(int total, int current)
		{
			SummaryAnalyzedEventArgs args = new SummaryAnalyzedEventArgs(total, current);
			SummaryAnalyzedUpdate?.Invoke(this, args);
		}

		public event EventHandler SummaryAnalyzeEnd;
		void OnSummaryAnalyzeEnd()
		{
			SummaryAnalyzeEnd?.Invoke(this, EventArgs.Empty);
		}

		public void CancelAnalyzeProgress(object sender, EventArgs e)
		{
			AnalyzeProgressCancelled = true;
		}

		protected override void AnalyzeResults()
		{
			var totalAnswers = 0;
			var current = 0;
			OnSummaryAnalyzeBegin();
			AnalyzeProgressCancelled = false;
			RemoveAll();

			try
			{
				var campaignItems = CompanyCampaign.CampaignsItemsSent;
				var summary = new LearningCentreTestSummary(Question);
				foreach (GlbCompanyCampaignItem campaignItem in campaignItems)
				{
					if (AnalyzeProgressCancelled)
					{
						break;
					}
					if (totalAnswers == 0)
					{
						totalAnswers = Question.Campaign.CampaignsItemsSent.Count;
					}
					current++;
					var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, Question);
					summary.AddSubmittedAnswerIntoSummary(answer);
					OnSummaryAnalyzedUpdate(totalAnswers, current);
				}

				if (!AnalyzeProgressCancelled)
				{
					summary.AnalyzeCampaignResultLastStep();

					if (summary.AnswerSummaries.SortInformation == null)
					{
						summary.AnswerSummaries.Sort(new AnswerSummaryComparer());
					}
					Add(summary);
				}
				else
				{
					RemoveAll();
				}
			}
			finally
			{
				OnSummaryAnalyzeEnd();
			}
		}

		public class SummaryAnalyzedEventArgs : EventArgs
		{
			public SummaryAnalyzedEventArgs(int total, int current)
				: base()
			{
				this.current = current;
				this.total = total;
			}
			public int current;
			public int total;
		}
	}
}

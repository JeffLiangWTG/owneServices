using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreTestSummaryCollection : VoteExamSurveySummaryCollection
	{
		public LearningCentreTestSummaryCollection(VoteExamSurveyQuestionSet surveyQuestions)
			: base(surveyQuestions)
		{
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

		protected override void AnalyzeResults()
		{
			var totalAnswers = 0;
			var current = 0;
			OnSummaryAnalyzeBegin();

			try
			{
				var campaignItems = SurveyQuestions.campaign.CampaignsItemsSent;
				var summariesMap = new Dictionary<ZGuid, LearningCentreTestSummary>();

				foreach (LearningCentreQuestion question in SurveyQuestions)
				{
					var summary = new LearningCentreTestSummary(question);
					summariesMap[question.PK] = summary;
				}

				foreach (GlbCompanyCampaignItem campaignItem in campaignItems)
				{
					foreach (LearningCentreQuestion question in SurveyQuestions)
					{
						if (totalAnswers == 0)
						{
							totalAnswers = SurveyQuestions.Count * campaignItems.Count;
						}
						current++;
						var answer = new LastCompletedSubmittedAnswer((LearningCentreCampaignItem)campaignItem, question);
						summariesMap[question.PK].AddSubmittedAnswerIntoSummary(answer);
						OnSummaryAnalyzedUpdate(totalAnswers, current);
					}
				}

				foreach (LearningCentreQuestion question in SurveyQuestions)
				{
					summariesMap[question.PK].AnalyzeCampaignResultLastStep();
					if (summariesMap[question.PK].AnswerSummaries.SortInformation == null)
					{
						summariesMap[question.PK].AnswerSummaries.Sort(new AnswerSummaryComparer());
					}
					Add(summariesMap[question.PK]);
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

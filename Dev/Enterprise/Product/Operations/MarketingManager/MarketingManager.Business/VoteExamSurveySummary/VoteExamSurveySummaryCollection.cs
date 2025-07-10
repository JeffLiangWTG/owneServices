using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySummaryCollection : NonPersistentBusinessObjectCollection<VoteExamSurveySummary>
	{
		public VoteExamSurveySummaryCollection(VoteExamSurveyQuestionSet surveyQuestions)
		{
			this.surveyQuestions = surveyQuestions;
		}

		readonly VoteExamSurveyQuestionSet surveyQuestions;

		public VoteExamSurveyQuestionSet SurveyQuestions
		{
			get { return surveyQuestions; }
		}

		public override void Load()
		{
			AnalyzeResults();
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Should not create new VoteExamSurveySummary, every VoteExamSurveySummary should be associated with a Question");
		}

		#endregion

		protected virtual void AnalyzeResults()
		{
			foreach (VoteExamSurveyQuestion question in SurveyQuestions)
			{
				VoteExamSurveySummary summary = new VoteExamSurveySummary(question);
				this.Add(summary);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

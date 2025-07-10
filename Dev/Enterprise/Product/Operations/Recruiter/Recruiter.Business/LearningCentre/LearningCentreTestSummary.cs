using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreTestSummary : VoteExamSurveySummary
	{
		public LearningCentreTestSummary(LearningCentreQuestion question)
			: base(question)
		{
			answerSummaries = new LearningCentreTestAnswerSummaryCollection();
			answerSummariesMap = new Dictionary<int, LearningCentreTestAnswerSummary>();
			answeredNumber = 0;
			ratingSum = 0;
			repliedAnswers = 0;
		}

		readonly LearningCentreTestAnswerSummaryCollection answerSummaries;
		readonly Dictionary<int, LearningCentreTestAnswerSummary> answerSummariesMap;

		public new LearningCentreQuestion Question
		{
			get { return (LearningCentreQuestion)base.Question; }
		}

		public LearningCentreTestAnswerSummaryCollection AnswerSummaries
		{
			get
			{
				return answerSummaries;
			}
		}

		protected int repliedAnswers;

		public void AddSubmittedAnswerIntoSummary(LastCompletedSubmittedAnswer answer)
		{
			if (answer.IsRepliedAnswer())
			{
				repliedAnswers++;
			}

			if (!answer.IsEmptyAnswer())
			{
				switch (Question.HY_AnswerType)
				{
					case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
						GetMultipleChoiceRating(answer);
						break;
					case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
					case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
					case VoteExamSurveyAnswerTypeList.Codes.YesNo:
						GetTextScaleRating(answer);
						break;
					case VoteExamSurveyAnswerTypeList.Codes.NumericScale:
					case VoteExamSurveyAnswerTypeList.Codes.Percentage:
						GetNumericScaleRating(answer);
						break;
					case VoteExamSurveyAnswerTypeList.Codes.VotingItem:
						GetVotingItemRating(answer);
						break;
					case VoteExamSurveyAnswerTypeList.Codes.FreeText:
						answeredNumber++;
						break;
					default:
						break;
				}
			}
		}

		protected override void CountAnswer(int selectedIndex)
		{
			if (selectedIndex >= 1)
			{
				LearningCentreTestAnswerSummary answerSummary = null;
				if (!answerSummariesMap.TryGetValue(selectedIndex, out answerSummary))
				{
					answerSummary = new LearningCentreTestAnswerSummary();
					answerSummariesMap.Add(selectedIndex, answerSummary);
					answerSummaries.Add(answerSummary);
				}
				answerSummary.OptionNumber = selectedIndex;
				answerSummary.AnsweredCount += 1;
			}
		}

		protected override void SetAnswerText(int selectedIndex, string text)
		{
			if (selectedIndex >= 1 && !string.IsNullOrEmpty(text))
			{
				LearningCentreTestAnswerSummary answerSummary = null;
				if (!answerSummariesMap.TryGetValue(selectedIndex, out answerSummary))
				{
					answerSummary = new LearningCentreTestAnswerSummary();
					answerSummariesMap.Add(selectedIndex, answerSummary);
					answerSummaries.Add(answerSummary);
				}
				answerSummary.OptionNumber = selectedIndex;
				int maxLength = answerSummary.FindPropertyInfo(LearningCentreTestAnswerSummary.Schema.AnswerText).MaxLength;
				answerSummary.AnswerText = text.Length <= maxLength ? text : text.Substring(0, maxLength);
			}
		}

		protected void GetMultipleChoiceRating(LastCompletedSubmittedAnswer answer)
		{
			int? selectedIndex = null;
			foreach (var option in answer.SelectedMultipleChoiceOptionsList)
			{
				selectedIndex = option.HY_SubQuestionOrder;
				if (selectedIndex != null)
				{
					CountAnswer(selectedIndex.Value);
					ratingSum += selectedIndex.Value;
					answeredNumber++;
				}
			}
		}

		protected override void AnalyzeCampaignResult()
		{
		}

		public void AnalyzeCampaignResultLastStep()
		{
			this.QuestionText = Question.QuestionTextForWeb;
			this.Number = Question.ActualOrder;

			if (Question.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.Header)
			{
				this.IsHeader = false;
				this.IsDisplayZeroForEmpty = false;

				GetAnswerText();

				this.NumberOfRecipient = Question.Campaign.CampaignsItemsSent.Count;
				this.NumberOfRecipientReplied = repliedAnswers;

				if (Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.VotingItem &&
					Question.Campaign.VoteHeader.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote)
				{
					this.RecipientAnswered = this.NumberOfRecipientReplied;
					this.RecipientSkipped = 0;
					this.Answer2Count = this.RecipientAnswered - this.Answer1Count;

					if (this.RecipientAnswered != 0)
					{
						decimal average = (this.Answer1Count + this.Answer2Count * 2) / (decimal)this.RecipientAnswered;
						this.Average = new ZDecimal(average);
					}
				}
				else
				{
					if (Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.FreeText)
					{
						this.IsText = true;
					}
					else
					{
						this.IsText = false;
					}
					this.RecipientAnswered = answeredNumber;
					this.RecipientSkipped = new ZInt(this.NumberOfRecipientReplied - answeredNumber);

					if (this.RecipientAnswered != 0)
					{
						decimal average = ratingSum / (decimal)this.RecipientAnswered;
						this.Average = new ZDecimal(average);
					}
				}

				CalculateAverageAsPercentage();
			}
			else
			{
				this.IsHeader = true;
			}
		}
	}
}

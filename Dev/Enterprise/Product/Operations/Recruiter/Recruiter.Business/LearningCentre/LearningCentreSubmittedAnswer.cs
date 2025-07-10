using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	public class LearningCentreSubmittedAnswer : VoteExamSurveySubmittedAnswer, ILearningCentreSubmittedAnswer
	{
		public LearningCentreSubmittedAnswer(LearningCentreCampaignItem campaignItem, LearningCentreQuestion question)
			: base(campaignItem, question)
		{
		}

		#region Score

		public ZByte Score
		{
			get
			{
				if (score == null)
				{
					score = 0;
					if (Question != null)
					{
						switch (Question.HY_AnswerType)
						{
							case VoteExamSurveyAnswerTypeList.Codes.LikertScale:
								score = (ZByte)(ZByte.ParseSafe(((IVoteExamSurveySubmittedAnswer)this).PersistedAnswer, 1) - 1);
								break;

							case VoteExamSurveyAnswerTypeList.Codes.YesNo:
							case VoteExamSurveyAnswerTypeList.Codes.TrueFalse:
								score = (ZByte)(2 - ZByte.ParseSafe(((IVoteExamSurveySubmittedAnswer)this).PersistedAnswer, 2));
								break;

							case VoteExamSurveyAnswerTypeList.Codes.MultipleChoice:
								score = (ZByte)SelectedMultipleChoiceOptions.Sum(q => q.HY_AnswerWeighting);
								break;
						}
					}
				}
				return score.Value;
			}
		}

		public ZPropertyInfo ScoreInfo
		{
			get { return GetZPropertyInfo(nameof(Score)); }
		}

		ZByte? score;

		#endregion

		#region IsAnsweredCorrectly

		public ZBool IsAnsweredCorrectly
		{
			get { return IsAnswered(true); }
		}

		public ZPropertyInfo IsAnsweredCorrectlyInfo
		{
			get { return GetZPropertyInfo(nameof(IsAnsweredCorrectly)); }
		}

		public ZBool IsAnsweredIncorrectly
		{
			get { return IsAnswered(false); }
		}

		public ZPropertyInfo IsAnsweredIncorrectlyInfo
		{
			get { return GetZPropertyInfo(nameof(IsAnsweredIncorrectly)); }
		}

		protected virtual bool IsAnswered(bool answeredCorrectly)
		{
			bool result = IsPopulated;

			if (result)
			{
				if (Question.IsMultipleChoiceQuestion)
				{
					result = (answeredCorrectly == IsMultipleChoiceAnsweredCorrectly());
				}
				else
				{
					VoteExamSurveyAnswer completedAnswer = GetPersistedAnswerFromQuestion();
					result = completedAnswer != null && ((completedAnswer.HZ_Answer == Question.HY_ExamCorrectAnswer) == answeredCorrectly);
				}
			}

			return result;
		}

		bool IsMultipleChoiceAnsweredCorrectly()
		{
			int selectedOptionCount = 0;
			foreach (LearningCentreQuestion selectedOption in SelectedMultipleChoiceOptions)
			{
				if (!selectedOption.CorrectAnswerAsBool)
				{
					return false;
				}

				selectedOptionCount++;
			}

			return (selectedOptionCount >= Question.HY_Min);
		}

		#endregion

		#region Result As Text

		public ZString ResultAsText
		{
			get { return this.GetResultAsText(); }
		}

		#endregion

		public new LearningCentreQuestion Question
		{
			get { return (LearningCentreQuestion)base.Question; }
		}

		public ZInt QuestionNumber
		{
			get { return Wrapper != null ? Wrapper.QuestionOrder : Question.ActualOrder; }
		}

		public IEnumerable<ZGuid> OrderedMultipleChoiceOptionPKs
		{
			get
			{
				ZGuid[] result = null;
				if (Wrapper != null && Wrapper.OrderedSubQuestionPKs != null)
				{
					result = Wrapper.OrderedSubQuestionPKs.ToArray();
				}
				else
				{
					result = Question.SubQuestions.Select(x => x.PK).ToArray();
				}
				return result;
			}
		}

		VoteExamSurveyAnswerWrapper Wrapper
		{
			get
			{
				VoteExamSurveyAnswerWrapper result = null;
				if (CampaignItem.AnswerWrappers != null)
				{
					result = CampaignItem.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapper>(Question, CampaignItem);
				}
				return result;
			}
		}

		public new LearningCentreCampaignItem CampaignItem
		{
			get { return (LearningCentreCampaignItem)base.CampaignItem; }
		}
	}
}

using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(AutoVoteExamSurveyQuestion.Schema.HY_Question), DescriptionProperty(AutoVoteExamSurveyQuestion.Schema.HY_Question)]
	public class LearningCentreQuestion : VoteExamSurveyQuestion
	{
		public LearningCentreQuestion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			NeedToReloadVoteExamSurveySummaries = true;
		}

		#region Correct Answer

		[List("Lookups.ExamCorrectAnswers")]
		public override ZString HY_ExamCorrectAnswer
		{
			get { return base.HY_ExamCorrectAnswer; }
			set { base.HY_ExamCorrectAnswer = value; }
		}

		public ZByte CorrectAnswerAsByte
		{
			get { return ZByte.ParseSafe(HY_ExamCorrectAnswer, ZByte.Zero); }
			set { HY_ExamCorrectAnswer = value.ToString(); }
		}

		public ZPropertyInfo CorrectAnswerAsByteInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CorrectAnswerAsByte), x => HY_ExamCorrectAnswerInfo); }
		}

		public ZBool CorrectAnswerAsBool
		{
			get
			{
				ZBool result = false;

				try
				{
					result = (!HY_ExamCorrectAnswer.IsEmpty) ? new ZBool(HY_ExamCorrectAnswer) : ZBool.False;
				}
				catch (ZTypeValueException)
				{
					result = false;
				}

				return result;
			}
			set { HY_ExamCorrectAnswer = (value) ? value.ToString() : ""; }
		}

		public ZPropertyInfo CorrectAnswerAsBoolInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CorrectAnswerAsBool), x => HY_ExamCorrectAnswerInfo); }
		}

		#endregion

		public new LearningCentreSubQuestionCollection SubQuestions
		{
			get { return (LearningCentreSubQuestionCollection)base.SubQuestions; }
		}

		protected override VoteExamSurveySubQuestionCollection GetNewSubQuestions(VoteExamSurveyQuestion question, bool isActive)
		{
			return new LearningCentreSubQuestionCollection((LearningCentreQuestion)question, isActive);
		}

		public new LearningCentreSubmittedAnswerCollection SubmittedAnswers
		{
			get { return (LearningCentreSubmittedAnswerCollection)base.SubmittedAnswers; }
		}

		protected override VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswerCollection()
		{
			return new LearningCentreSubmittedAnswerCollection(this);
		}

		public LastCompletedSubmittedAnswerCollection LastCompletedSubmittedAnswers
		{
			get
			{
				if (lastCompletedSubmittedAnswers == null)
				{
					lastCompletedSubmittedAnswers = new LastCompletedSubmittedAnswerCollection(this);
					lastCompletedSubmittedAnswers.Load();
				}
				return lastCompletedSubmittedAnswers;
			}
		}
		LastCompletedSubmittedAnswerCollection lastCompletedSubmittedAnswers;

		public bool NeedToReloadVoteExamSurveySummaries { get; set; }
		public LearningCentreTestSummaryPerQuestionCollection VoteExamSurveySummaries
		{
			get
			{
				if (surveySummaries == null)
				{
					surveySummaries = new LearningCentreTestSummaryPerQuestionCollection(this);
				}
				if (NeedToReloadVoteExamSurveySummaries)
				{
					foreach (LearningCentreQuestion otherQuestion in Campaign.ActualQuestionsForBinding)
					{
						if (otherQuestion.PK != PK)
						{
							otherQuestion.NeedToReloadVoteExamSurveySummaries = true;
						}
					}
					Campaign.OnLoadVoteExamSurveySummaries(surveySummaries, EventArgs.Empty);
					surveySummaries.Load();
					NeedToReloadVoteExamSurveySummaries = false;
				}
				return surveySummaries;
			}
		}
		LearningCentreTestSummaryPerQuestionCollection surveySummaries;

		public LearningCentreTestAnswerSummaryCollection VoteSurveyAnswerSummaries
		{
			get
			{
				return VoteExamSurveySummaries.Count > 0 ? (VoteExamSurveySummaries[0] as LearningCentreTestSummary).AnswerSummaries : new LearningCentreTestAnswerSummaryCollection();
			}
		}

		public new LearningCentreQuestionLookups Lookups
		{
			get { return (LearningCentreQuestionLookups)base.Lookups; }
		}

		protected override VoteExamSurveyQuestionLookups GetNewLookups()
		{
			return new LearningCentreQuestionLookups(this);
		}

		public new LearningCentreCampaign Campaign
		{
			get { return (LearningCentreCampaign)base.Campaign; }
		}

		protected override VoteExamSurveyQuestionValidation GetNewValidation()
		{
			return new LearningCentreQuestionValidation(this);
		}

		public override ZGuid HY_G0
		{
			get { return base.HY_G0; }
			set
			{
				if (base.HY_G0 != value)
				{
					base.HY_G0 = value;
					if (Campaign != null && Campaign.QuestionCategories.Count == 1)
					{
						HY_QuestionCategory = Campaign.QuestionCategories[0].Code;
					}
				}
			}
		}

		[List("Campaign.QuestionCategories")]
		public override ZString HY_QuestionCategory
		{
			get { return base.HY_QuestionCategory; }
			set { base.HY_QuestionCategory = value; }
		}
	}
}

using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public abstract class VoteExamSurveyAnswerWrapperBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		public VoteExamSurveyAnswerWrapperBase(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage)
			: base(voteExamSurveyAnswerSet.Factory)
		{
			this.fQuestion = question;
			this.questionOrder = question.HY_QuestionOrder;
			this.QuestionsPerPage = questionsPerPage;
			this.VoteExamSurveyAnswerSet = voteExamSurveyAnswerSet;
			this.CampaignItem = campaignItem;
		}

		public VoteExamSurveyAnswerWrapperBase(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage, ZShort questionOrder)
			: this(question, voteExamSurveyAnswerSet, campaignItem, questionsPerPage)
		{
			this.questionOrder = questionOrder;
		}

		public VoteExamSurveyAnswerWrapperBase(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, GlbCompanyCampaignItem campaignItem, int questionsPerPage, VoteExamSurveyAnswer answer)
			: this(question, voteExamSurveyAnswerSet, campaignItem, questionsPerPage)
		{
			this.fAnswer = answer;
			this.questionOrder = answer.HZ_QuestionOrder;
		}

		#region Question Order

		public ZShort QuestionOrder
		{
			get { return questionOrder; }
		}
		readonly ZShort questionOrder;

		#endregion

		#region Answer

		[ChildEditable]
		public VoteExamSurveyAnswer Answer
		{
			get
			{
				if (fAnswer == null || fAnswer.IsDeleted)
				{
					fAnswer = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Question, CampaignItem);

					if (!fAnswer.IsInDatabase)
					{
						fAnswer.HZ_QuestionOrder = questionOrder;
					}

					RegisterEditableChildObject(fAnswer);
				}
				return fAnswer;
			}
		}
		VoteExamSurveyAnswer fAnswer;

		#endregion

		#region Paging Support

		public ZString QuestionTextForWeb
		{
			get
			{
				ZString result = Question.QuestionTextForWeb;
				if (VoteExamSurveyAnswerSet.PageCount > 1 && IsContinuedHeaderForPaging)
				{
					result += " " + Res.GetString("dfebc613-25a3-4979-bd76-f0ad4274b6f1", "(...continued)");
				}
				return result;
			}
		}

		public ZPropertyInfo QuestionTextForWebInfo
		{
			get { return GetZPropertyInfo(nameof(QuestionTextForWeb)); }
		}

		public int IndexForPaging
		{
			get
			{
				int result = 0;

				foreach (VoteExamSurveyAnswerWrapperBase wrapper in VoteExamSurveyAnswerSet.AnswerWrappers)
				{
					if (wrapper == this)
					{
						break;
					}

					if (!wrapper.Question.IsHeader)
					{
						result++;
					}
				}

				return result;
			}
		}

		public bool IsContinuedHeaderForPaging
		{
			get
			{
				bool result;
				int currentPageStartingIndex = (VoteExamSurveyAnswerSet.CurrentPage - 1) * QuestionsPerPage;

				result = Question.IsHeader && VoteExamSurveyAnswerSet.PageCount > 1 && IndexForPaging < currentPageStartingIndex;
				if (result)
				{
					int answerWrapperIndex = ((IList)VoteExamSurveyAnswerSet.AnswerWrappers).IndexOf(this);
					for (int i = answerWrapperIndex + 1; i < VoteExamSurveyAnswerSet.AnswerWrappers.Count; i++)
					{
						if (VoteExamSurveyAnswerSet.AnswerWrappers[i].IndexForPaging > currentPageStartingIndex)
						{
							break;
						}

						if (VoteExamSurveyAnswerSet.AnswerWrappers[i].Question.IsHeader)
						{
							result = false;
							break;
						}
					}
				}

				return result;
			}
		}

		#endregion

		public VoteExamSurveyQuestion Question
		{
			get { return fQuestion; }
		}

		readonly VoteExamSurveyQuestion fQuestion;
		protected readonly int QuestionsPerPage;
		protected readonly IVoteExamSurveyAnswerSet VoteExamSurveyAnswerSet;
		protected readonly GlbCompanyCampaignItem CampaignItem;
	}
}

using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySubmittedAnswer : NonPersistentBusinessObject, IObsoleteValidation, IVoteExamSurveySubmittedAnswer
	{
		public static class Schema
		{
			public const string Answer = "Answer";
		}

		public VoteExamSurveySubmittedAnswer(GlbCompanyCampaignItem campaignItem, VoteExamSurveyQuestion question)
			: base(campaignItem.Factory)
		{
			Argument.NotNull(campaignItem, "campaignItem");
			Argument.NotNull(question, "question");

			this.campaignItem = campaignItem;
			this.question = question;
		}

		#region Answer

		public ZString Answer
		{
			get { return this.GetAnswer(); }
		}

		public ZPropertyInfo AnswerInfo
		{
			get { return GetZPropertyInfo(nameof(Answer)); }
		}

		#endregion

		#region AnswerAsInt

		public ZInt AnswerAsInt
		{
			get { return ZInt.ParseSafe(Answer, 0); }
		}

		public ZPropertyInfo AnswerAsIntInfo
		{
			get { return GetZPropertyInfo(nameof(AnswerAsInt)); }
		}

		#endregion

		#region AnswerFieldType

		public ZString AnswerFieldType
		{
			get { return this.GetAnswerFieldType(); }
		}

		public ZPropertyInfo AnswerFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AnswerFieldType)); }
		}

		#endregion

		#region Populated

		public ZBool IsPopulated
		{
			get { return this.IsPopulated(); }
		}

		public ZPropertyInfo IsPopulatedInfo
		{
			get { return GetZPropertyInfo(nameof(IsPopulated)); }
		}

		#endregion

		public virtual ZString PersistedAnswer
		{
			get
			{
				string result = "";
				VoteExamSurveyAnswer answer = GetPersistedAnswerFromQuestion();
				if (answer != null)
				{
					result = (Question.HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.FreeText) ? answer.HZ_Answer : answer.HZ_AnswerComment;
				}
				return result;
			}
		}

		public virtual IEnumerable<VoteExamSurveyQuestion> SelectedMultipleChoiceOptions
		{
			get
			{
				foreach (VoteExamSurveyQuestion multipleChoiceOption in Question.SubQuestions)
				{
					VoteExamSurveyAnswer optionAnswer = campaignItem.PersistedAnswers.FindByQuestion(multipleChoiceOption);
					if (optionAnswer != null && optionAnswer.AnswerAsBool)
					{
						yield return multipleChoiceOption;
					}
				}
			}
		}

		public VoteExamSurveyQuestion Question
		{
			get { return question; }
		}

		public GlbCompanyCampaignItem CampaignItem
		{
			get { return campaignItem; }
		}

		protected VoteExamSurveyAnswer GetPersistedAnswerFromQuestion()
		{
			if (persistedVoteExamSurveyAnswer == null)
			{
				persistedVoteExamSurveyAnswer = campaignItem.PersistedAnswers.FindByQuestion(Question);
			}
			return persistedVoteExamSurveyAnswer;
		}
		VoteExamSurveyAnswer persistedVoteExamSurveyAnswer;

		readonly GlbCompanyCampaignItem campaignItem;
		readonly VoteExamSurveyQuestion question;
	}
}

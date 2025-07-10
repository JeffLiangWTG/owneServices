using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveyQuestionValidation : AutoVoteExamSurveyQuestionValidation
	{
		public VoteExamSurveyQuestionValidation(AutoVoteExamSurveyQuestion parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			Parent.ClearRowNotifications();
			ValidateNumberOfVotingItems();
			ValidateNumberOfMultipleChoiceOptions();
		}

		protected new VoteExamSurveyQuestion Parent
		{
			get { return (VoteExamSurveyQuestion)base.Parent; }
		}

		protected override void CheckHY_AnswerType()
		{
			base.CheckHY_AnswerType();
			MandatoryValidation.CheckEntered(Parent.HY_AnswerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HY_AnswerTypeInfo, Parent.Lookups.AnswerTypes);
		}

		protected override void CheckHY_Question()
		{
			base.CheckHY_Question();
			MandatoryValidation.CheckEntered(Parent.HY_QuestionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.HY_QuestionInfo);
		}

		protected override void CheckHY_QuestionOrder()
		{
			base.CheckHY_QuestionOrder();

			if (ShouldMandatoryValidateQuestionOrders)
			{
				MandatoryValidation.CheckEntered(Parent.HY_QuestionOrderInfo);

				if (Parent.IsSubQuestion || Parent.IsHeader)
				{
					return;
				}

				var campaign = Parent.Campaign;
				if (campaign != null && campaign.Questions.Any(q => q.PK != Parent.PK && !q.IsSubQuestion && !q.IsHeader && q.HY_QuestionOrder == Parent.HY_QuestionOrder))
				{
					string errorMessage = Res.GetString("260E73F9-24A8-4EBF-B359-D59984911D9A", "All questions must have a unique order");
					Parent.HY_QuestionOrderInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckHY_SubQuestionOrder()
		{
			base.CheckHY_SubQuestionOrder();

			if (ShouldMandatoryValidateQuestionOrders && Parent.IsSubQuestion)
			{
				MandatoryValidation.CheckEntered(Parent.HY_SubQuestionOrderInfo);
			}
		}

		protected virtual bool ShouldMandatoryValidateQuestionOrders { get { return true; } }

		protected override void CheckHY_Min()
		{
			base.CheckHY_Min();

			if (Parent.IsVotingHeader)
			{
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.HY_MinInfo, Parent.HY_MaxInfo);
				CompareValidation.CheckWithinRange(Parent.HY_MinInfo, MinVotesAllowed, MaxVotesAllowed);
			}
			else if (Parent.IsMultipleChoiceQuestion)
			{
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(Parent.HY_MinInfo, Parent.HY_MaxInfo);
				CompareValidation.CheckWithinRange(Parent.HY_MinInfo, MinOptionSelectionAllowed, MaxOptionSelectionAllowed);
			}
			else if (Parent.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.NumericScale)
			{
				CompareValidation.CheckNumberLessThanOtherNumber(Parent.HY_MinInfo, Parent.HY_MaxInfo);
			}
		}

		protected override void CheckHY_Max()
		{
			base.CheckHY_Max();

			if (Parent.IsVotingHeader)
			{
				CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.HY_MaxInfo, Parent.HY_MinInfo);
				CompareValidation.CheckWithinRange(Parent.HY_MaxInfo, MinVotesAllowed, MaxVotesAllowed);
			}
			else if (Parent.IsMultipleChoiceQuestion)
			{
				CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(Parent.HY_MaxInfo, Parent.HY_MinInfo);
				CompareValidation.CheckWithinRange(Parent.HY_MaxInfo, MinOptionSelectionAllowed, MaxOptionSelectionAllowed);
			}
			else if (Parent.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.NumericScale)
			{
				CompareValidation.CheckNumberGreaterThanOtherNumber(Parent.HY_MaxInfo, Parent.HY_MinInfo);
				CompareValidation.CheckGreaterThanOrEqualTo(Parent.HY_MaxInfo, 1);
			}
		}

		void ValidateNumberOfVotingItems()
		{
			if (Parent.IsVotingHeader)
			{
				ZQuery query = new ZQuery(VoteExamSurveyQuestionSchema.HY_AnswerType, VoteExamSurveyAnswerTypeList.Codes.VotingItem);
				IList<VoteExamSurveyQuestion> voteItems = new List<VoteExamSurveyQuestion>(Parent.SubQuestions.Find(query));
				if (voteItems.Count < Parent.HY_Min)
				{
					string errorMessage = Res.GetString("616e81cd-6ee4-4b11-97bc-f21a13d9a97c", "There has to be at least {0} vote item(s). You can either add more vote items or adjust the allowable number of votes.", Parent.HY_Min);
					Parent.AddRowError(errorMessage);
				}
			}
		}

		void ValidateNumberOfMultipleChoiceOptions()
		{
			if (Parent.IsMultipleChoiceQuestion && Parent.SubQuestions != null && Parent.SubQuestions.Count < 2)
			{
				Parent.AddRowError(Res.GetString("c9da922f-242d-4dd1-8468-a87d779f8de2", "There has to be at least 2 multiple choice options"));
			}
		}

		const byte MinOptionSelectionAllowed = 1;
		byte MaxOptionSelectionAllowed
		{
			get { return (Parent.SubQuestions == null || Parent.SubQuestions.Count == 0) ? byte.MaxValue : (byte)Parent.SubQuestions.Count; }
		}

		const byte MinVotesAllowed = 1;
		const byte MaxVotesAllowed = 10;
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class VoteExamSurveySubQuestionCollection : VoteExamSurveyQuestionSet
	{
		public VoteExamSurveySubQuestionCollection(VoteExamSurveyQuestion master)
			: this(master, true)
		{
		}

		public VoteExamSurveySubQuestionCollection(VoteExamSurveyQuestion master, bool? isActiveFilterValue)
			: base(master.Campaign, isActiveFilterValue, GetSubQuestionsFilter(master))
		{
			this.Master = master;
			Master.HY_QuestionOrderInfo.ValueChanged += HY_QuestionOrderInfo_ValueChanged;
		}

		protected override VoteExamSurveyQuestionSet GetNewInstance(bool? isActiveFilterValue)
		{
			return new VoteExamSurveySubQuestionCollection(Master, isActiveFilterValue);
		}

		static ZQuery GetSubQuestionsFilter(VoteExamSurveyQuestion parentQuestion)
		{
			ZQuery result = new ZQuery(VoteExamSurveyQuestionSchema.PK, SQLComparisonOperator.NotEqual, parentQuestion.PK);
			result.AddToFilter(VoteExamSurveyQuestionSchema.HY_SubQuestionOrder, SQLComparisonOperator.NotEqual, ZShort.Zero);
			return result;
		}

		void HY_QuestionOrderInfo_ValueChanged(object sender, EventArgs e)
		{
			if (((INeedRow)sender).Row.RowState != System.Data.DataRowState.Detached)
			{
				((IActiveBusinessObjectCollection)this).Refresh();
			}
		}

		protected override bool MatchesFilterCore(VoteExamSurveyQuestion element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache)
				&& !Master.IsDeleted && !element.IsDeleted
				&& !Master.IsHeader
				&& element.HY_QuestionOrder == Master.HY_QuestionOrder;
		}

		protected override object[] GetCollectionState()
		{
			return new object[] { Master };
		}

		protected override void SetDefaultsForNewElementCore(VoteExamSurveyQuestion newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.HY_G0 = Master.HY_G0;
			newElement.HY_SubQuestionOrder = GetNextQuestionOrder();
			newElement.HY_QuestionOrder = Master.HY_QuestionOrder;
			if (newElement.IsVotingItem)
			{
				newElement.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			}
			else if (Master.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.MultipleChoice)
			{
				newElement.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			}
		}

		public override SchemaShortColumn SchemaOrderColumn
		{
			get { return VoteExamSurveyQuestionSchema.HY_SubQuestionOrder; }
		}

		public readonly VoteExamSurveyQuestion Master;

		protected override bool AllowNew => base.AllowNew && Master != null && !Master.IsDeleted && !Master.HY_Question.IsEmpty;
	}
}

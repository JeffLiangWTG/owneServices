using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class ExamSurveySubAnswerCollection : BusinessObjectCollection<VoteExamSurveyAnswer>, IEnumerable<IBindableBooleanItem>
	{
		public ExamSurveySubAnswerCollection(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet voteExamSurveyAnswerSet, ZShort questionOrder)
		  : base(question.Factory)
		{
			this.Question = question;
			this.VoteExamSurveyAnswerSet = voteExamSurveyAnswerSet;
			this.QuestionOrder = questionOrder;
		}

		public override void Load()
		{
			RemoveAll();

			if (!Question.IsSubQuestion)
			{
				var answers = VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Cast<VoteExamSurveyAnswer>().Where(x => x.HZ_QuestionOrder == QuestionOrder && x.HZ_SubQuestionOrder > 0).OrderBy(x => x.HZ_SubQuestionOrder).ToArray();
				AddRange(answers);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Should not be calling AddNew on this collection");
		}

		readonly VoteExamSurveyQuestion Question;
		readonly IVoteExamSurveyAnswerSet VoteExamSurveyAnswerSet;
		readonly ZShort QuestionOrder;

		#region IEnumerable<IBindableBooleanItem> Members

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			foreach (IBindableBooleanItem element in (IEnumerable)this)
			{
				yield return element;
			}
		}

		#endregion
	}
}

using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class ExamSurveyMultipleChoiceAnswerCapturerBizO : NonPersistentBusinessObject, IEnumerable<IBindableBooleanItem>, IObsoleteValidation
	{
		public ExamSurveyMultipleChoiceAnswerCapturerBizO(VoteExamSurveyAnswerWrapper answerWrapper)
		{
			this.AnswerWrapper = answerWrapper;
		}

		IEnumerable<IBindableBooleanItem> InnerEnumerable
		{
			get
			{
				if (fInnerEnumerable == null)
				{
					if (FixedMultipleChoiceAnswerTypes.Contains(AnswerWrapper.Question.HY_AnswerType))
					{
						fInnerEnumerable = new FixedMultipleChoiceSingleAnswerCapturer(AnswerWrapper.SingleAnswer);
					}
					else if (AnswerWrapper.Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.MultipleChoice)
					{
						fInnerEnumerable = AnswerWrapper.GetMultipleAnswers();
					}
				}
				return fInnerEnumerable;
			}
		}

		readonly IList<string> FixedMultipleChoiceAnswerTypes = new string[]
		{
			VoteExamSurveyAnswerTypeList.Codes.LikertScale,
			VoteExamSurveyAnswerTypeList.Codes.YesNo,
			VoteExamSurveyAnswerTypeList.Codes.TrueFalse
		};

		public readonly VoteExamSurveyAnswerWrapper AnswerWrapper;
		IEnumerable<IBindableBooleanItem> fInnerEnumerable;

		#region IEnumerable<IBindableBooleanItem> Members

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			return InnerEnumerable.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return InnerEnumerable.GetEnumerator();
		}

		#endregion
	}
}

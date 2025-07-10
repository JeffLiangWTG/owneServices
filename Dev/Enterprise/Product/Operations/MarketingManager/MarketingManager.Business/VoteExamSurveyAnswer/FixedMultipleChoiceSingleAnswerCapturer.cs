using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class FixedMultipleChoiceSingleAnswerCapturer : IEnumerable<IBindableBooleanItem>
	{
		public FixedMultipleChoiceSingleAnswerCapturer(VoteExamSurveyAnswer answer)
		{
			this.Answer = answer;
		}

		ReadOnlyCollection<MultipleChoiceOptionBizO> Elements
		{
			get
			{
				if (fElements == null)
				{
					List<MultipleChoiceOptionBizO> list = new List<MultipleChoiceOptionBizO>();
					CodeDescriptionPairList codeList = Answer.Lookups.AnswerOptionList as CodeDescriptionPairList;
					if (codeList != null)
					{
						foreach (CodeDescriptionPair pair in codeList)
						{
							string text = (string.IsNullOrEmpty(pair.Description)) ? pair.Code : pair.Description;
							list.Add(new MultipleChoiceOptionBizO(this, text));
						}
					}
					fElements = list.AsReadOnly();
					ZInt selectedElementIndex = Answer.AnswerAsInt - 1;
					if (selectedElementIndex > -1 && selectedElementIndex < fElements.Count)
					{
						fElements[selectedElementIndex].BoolValue = true;
					}
				}
				return fElements;
			}
		}

		void SetAnswer(MultipleChoiceOptionBizO selectedOption)
		{
			int indexOneBased = Elements.IndexOf(selectedOption) + 1;
			Answer.AnswerAsInt = indexOneBased;
		}

		ReadOnlyCollection<MultipleChoiceOptionBizO> fElements;
		public readonly VoteExamSurveyAnswer Answer;

		#region MultipleChoiceOptionBizO

#if DEBUG
		internal
#endif
		class MultipleChoiceOptionBizO : NonPersistentBusinessObject, IBindableBooleanItem
		{
			public MultipleChoiceOptionBizO(FixedMultipleChoiceSingleAnswerCapturer answerCapturer, ZString text)
			{
				this.AnswerCapturer = answerCapturer;
				this.fText = text;
			}

			#region IBindableBooleanItem Members

			public ZBool BoolValue
			{
				get { return fBoolValue; }
				set
				{
					fBoolValue = value;
					if (value)
					{
						AnswerCapturer.SetAnswer(this);
					}

					BoolValueInfo.RefreshBinding();
				}
			}

			public ZPropertyInfo BoolValueInfo
			{
				get { return GetZPropertyInfo(nameof(BoolValue)); }
			}

			public ZString Text
			{
				get { return fText; }
			}

			ZBool fBoolValue;

			#endregion

			readonly FixedMultipleChoiceSingleAnswerCapturer AnswerCapturer;
			readonly ZString fText;
		}

		#endregion

		#region IEnumerable<IBindableBooleanItem> Members

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			foreach (IBindableBooleanItem element in Elements)
			{
				yield return element;
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IBindableBooleanItem>)this).GetEnumerator();
		}

		#endregion
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	[FreezeSortOnGridCollectionElementModify(false)]
	public abstract class VoteExamSurveyQuestionSet : ActiveBusinessObjectCollection<VoteExamSurveyQuestion>, IBindingList, ICancelAddNew
	{
		static ZQuery GetQuestionsFilter(bool? isActiveFilterValue, ZQuery additionalFilter)
		{
			ZQuery result = new ZQuery();
			if (isActiveFilterValue != null)
			{
				result.AddToFilter(VoteExamSurveyQuestionSchema.HY_IsActive, isActiveFilterValue.Value);
			}

			if (additionalFilter != null && !additionalFilter.IsEmpty)
			{
				result.AddToFilter(additionalFilter);
			}
			return result;
		}

		public VoteExamSurveyQuestionSet(GlbCompanyCampaign campaign, bool? isActiveFilterValue)
			: this(campaign, isActiveFilterValue, null)
		{
		}

		public VoteExamSurveyQuestionSet(GlbCompanyCampaign campaign, bool? isActiveFilterValue, ZQuery filter)
			: base(campaign, GetQuestionsFilter(isActiveFilterValue, filter))
		{
			this.campaign = campaign;
			this.isActiveFilterValue = isActiveFilterValue;

			ApplySort((IComparer)new VoteExamSurveyQuestionComparer(SchemaOrderColumn));
		}

		#region Filtered Questions

		public VoteExamSurveyQuestionSet AllQuestions
		{
			get
			{
				return (!isActiveFilterValue.HasValue)
					? this
					: GetNewInstance(null);
			}
		}

		public VoteExamSurveyQuestionSet ActiveQuestions
		{
			get
			{
				return (isActiveFilterValue.HasValue && isActiveFilterValue.Value)
					? this
					: GetNewInstance(true);
			}
		}

		public VoteExamSurveyQuestionSet InactiveQuestions
		{
			get
			{
				return (isActiveFilterValue.HasValue && !isActiveFilterValue.Value)
					? this
					: GetNewInstance(false);
			}
		}

		protected abstract VoteExamSurveyQuestionSet GetNewInstance(bool? isActiveFilterValue);

		#endregion

		protected override bool AllowNew
		{
			get { return (!isActiveFilterValue.HasValue || isActiveFilterValue.Value) && base.AllowNew; }
		}

		bool IBindingList.SupportsSorting
		{
			get { return false; }
		}

		protected override void EndNew(int index)
		{
			VoteExamSurveyQuestion addedQuestion = null;

			if (index >= 0)
			{
				addedQuestion = this[index];
			}

			base.EndNew(index);
			addedQuestion?.RunDelayedHandleOrderChanging();
		}

		#region Questions Re-ordering

		public void HandleAnswerTypeChanging(VoteExamSurveyQuestion editedQuestion, ZString newAnswerType)
		{
			Argument.NotNull(editedQuestion, "editedQuestion");

			if (!isShiftingQuestions)
			{
				using (IsShiftingQuestionsFlagChanger())
				{
					ShiftAffectedQuestions(editedQuestion, newAnswerType);
				}
			}
		}

		public void HandleOrderChanging(VoteExamSurveyQuestion editedQuestion, ZShort newOrder, ZShort previousOrder)
		{
			Argument.NotNull(editedQuestion, "editedQuestion");

			if (!isShiftingQuestions)
			{
				using (IsShiftingQuestionsFlagChanger())
				{
					if (!editedQuestion.IsHeader)
					{
						ShiftAffectedQuestions(editedQuestion, newOrder, previousOrder);
					}
				}
			}
		}

		IDisposable IsShiftingQuestionsFlagChanger()
		{
			isShiftingQuestions = true;
			return new DisposableAction(delegate
			{ isShiftingQuestions = false; });
		}

		void ShiftAffectedQuestions(VoteExamSurveyQuestion editedQuestion, ZShort newOrder, ZShort previousOrder)
		{
			PredicateAndOrderOffset predicateAndOrderOffset = GetSearchPredicateAndOrderOffset(editedQuestion, newOrder, previousOrder);
			VoteExamSurveyQuestion[] affectedQuestions = GetAffectedQuestionsInCorrectSortDirection(predicateAndOrderOffset);
			ShiftAffectedQuestions(affectedQuestions, predicateAndOrderOffset.orderOffset);
		}

		void ShiftAffectedQuestions(VoteExamSurveyQuestion editedQuestion, ZString newAnswerType)
		{
			ZShort orderOffset = newAnswerType == VoteExamSurveyAnswerTypeList.Codes.Header ? new ZShort(-1) : new ZShort(1);
			VoteExamSurveyQuestion[] affectedQuestions = GetAffectedQuestionsInCorrectSortDirection(editedQuestion, orderOffset);
			ShiftAffectedQuestions(affectedQuestions, orderOffset);
		}

		void ShiftAffectedQuestions(VoteExamSurveyQuestion[] affectedQuestions, short orderOffset)
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				foreach (VoteExamSurveyQuestion questionToReOrder in affectedQuestions)
				{
					questionToReOrder[SchemaOrderColumn] = (ZShort)questionToReOrder[SchemaOrderColumn] + orderOffset;
				}
			}
		}

		VoteExamSurveyQuestion[] GetAffectedQuestionsInCorrectSortDirection(PredicateAndOrderOffset predicateAndOrderOffset)
		{
			VoteExamSurveyQuestion[] questions = this.ToArray<VoteExamSurveyQuestion>();

			VoteExamSurveyQuestion[] questionsToReOrder = Array.FindAll(questions, predicateAndOrderOffset.predicate);
			Array.Sort(questionsToReOrder, GetSortDelegate(predicateAndOrderOffset.orderOffset < 0));

			return questionsToReOrder;
		}

		VoteExamSurveyQuestion[] GetAffectedQuestionsInCorrectSortDirection(VoteExamSurveyQuestion editedQuestion, int orderOffset)
		{
			VoteExamSurveyQuestion[] result;

			VoteExamSurveyQuestion[] questions = ToArray();
			int editedQuestionIndex = Array.IndexOf(questions, editedQuestion);
			if (editedQuestionIndex > -1 && editedQuestionIndex < questions.Length - 1)
			{
				result = new VoteExamSurveyQuestion[questions.Length - editedQuestionIndex - 1];
				Array.Copy(questions, editedQuestionIndex + 1, result, 0, result.Length);
				Array.Sort(result, GetSortDelegate(orderOffset < 0));
			}
			else
			{
				result = Array.Empty<VoteExamSurveyQuestion>();
			}

			return result;
		}

		Comparison<VoteExamSurveyQuestion> GetSortDelegate(bool shiftQuestionsUp)
		{
			return (shiftQuestionsUp)
				? delegate(VoteExamSurveyQuestion a, VoteExamSurveyQuestion b)
				{ return ((ZShort)a[SchemaOrderColumn]).CompareTo(b[SchemaOrderColumn]); }
			: delegate(VoteExamSurveyQuestion a, VoteExamSurveyQuestion b)
			{ return ((ZShort)b[SchemaOrderColumn]).CompareTo(a[SchemaOrderColumn]); };
		}

		PredicateAndOrderOffset GetSearchPredicateAndOrderOffset(VoteExamSurveyQuestion editedQuestion, ZShort newOrder, ZShort previousOrder)
		{
			Predicate<VoteExamSurveyQuestion> predicate;
			ZShort orderOffset;

			if (previousOrder > newOrder)
			{
				if (newOrder < 0)
				{
					newOrder = 0;
				}

				predicate = (VoteExamSurveyQuestion questionToMatch) =>
				{
					ZShort order = (ZShort)questionToMatch[SchemaOrderColumn];
					return (order >= newOrder && order <= previousOrder && editedQuestion != questionToMatch/* && !questionToMatch.IsHeader*/);
				};
				orderOffset = 1;
			}
			else
			{
				ZShort maxOrder = GetMaxQuestionOrder();
				if (newOrder > maxOrder)
				{
					newOrder = maxOrder;
				}

				predicate = (VoteExamSurveyQuestion questionToMatch) =>
				{
					ZShort order = (ZShort)questionToMatch[SchemaOrderColumn];
					return (order > previousOrder && order <= newOrder);
				};
				orderOffset = -1;
			}

			return new PredicateAndOrderOffset(predicate, orderOffset);
		}

		class PredicateAndOrderOffset
		{
			public PredicateAndOrderOffset(Predicate<VoteExamSurveyQuestion> predicate, ZShort orderOffset)
			{
				this.predicate = predicate;
				this.orderOffset = orderOffset;
			}

			public readonly Predicate<VoteExamSurveyQuestion> predicate;
			public readonly ZShort orderOffset;
		}

		bool isShiftingQuestions;

#if DEBUG

		public IDisposable DisableQuestionsShiftingForTest()
		{
			return IsShiftingQuestionsFlagChanger();
		}

#endif

		#endregion

		#region Helper Methods

		public ZShort GetValidQuestionOrder(ZShort newOrder)
		{
			ZShort result = newOrder;

			if (result < 1)
			{
				result = 1;
			}
			else
			{
				ZShort nextOrder = GetNextQuestionOrder();
				if (result > nextOrder)
				{
					result = nextOrder;
				}
			}

			return result;
		}

		public ZShort GetNextQuestionOrder()
		{
			ZShort maxOrder = GetMaxQuestionOrder();
			bool lastQuestionIsHeader = true;
			foreach (VoteExamSurveyQuestion questionFromOrder in GetQuestionsFromOrder(maxOrder))
			{
				lastQuestionIsHeader = questionFromOrder.IsHeader;
				if (!lastQuestionIsHeader)
				{
					break;
				}
			}

			return (lastQuestionIsHeader && maxOrder > 0) ? maxOrder : maxOrder + 1;
		}

		public ZShort GetMaxQuestionOrder()
		{
			ZShort max = 0;

			foreach (VoteExamSurveyQuestion question in this)
			{
				ZShort order = (ZShort)question[SchemaOrderColumn];
				if (order > max)
				{
					max = order;
				}
			}

			return max;
		}

		IEnumerable<VoteExamSurveyQuestion> GetQuestionsFromOrder(ZShort order)
		{
			foreach (VoteExamSurveyQuestion question in this)
			{
				if ((ZShort)question[SchemaOrderColumn] == order)
				{
					yield return question;
				}
			}
		}

		#endregion

		#region VoteExamSurveyQuestionComparer

		class VoteExamSurveyQuestionComparer : PropertyComparer, IComparer<VoteExamSurveyQuestion>
		{
			public VoteExamSurveyQuestionComparer(SchemaShortColumn schemaOrderColumn)
				: base(typeof(VoteExamSurveyQuestion), schemaOrderColumn.Name, ListSortDirection.Ascending)
			{
			}

			[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
			public override int Compare(BusinessObject x, BusinessObject y)
			{
				int result = base.Compare(x, y);

				VoteExamSurveyQuestion questionX = (VoteExamSurveyQuestion)x;
				VoteExamSurveyQuestion questionY = (VoteExamSurveyQuestion)y;

				if (result == 0)
				{
					result = questionY.IsHeader.CompareTo(questionX.IsHeader);
				}

				if (result == 0)
				{
					result = questionX.HY_Question.CompareTo(questionY.HY_Question);
				}

				return result;
			}

			#region IComparer<VoteExamSurveyQuestion> Members

			int IComparer<VoteExamSurveyQuestion>.Compare(VoteExamSurveyQuestion x, VoteExamSurveyQuestion y)
			{
				return Compare(x, y);
			}

			#endregion
		}

		#endregion

		public abstract SchemaShortColumn SchemaOrderColumn { get; }
		public readonly GlbCompanyCampaign campaign;
		readonly bool? isActiveFilterValue;
	}
}

using System;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class PagedVoteExamSurveyAnswerWrapperCollection : BusinessObjectCollectionView<VoteExamSurveyAnswerWrapperBase>
	{
		public PagedVoteExamSurveyAnswerWrapperCollection(IVoteExamSurveyAnswerSet answerSet)
			: base(answerSet.AnswerWrappers)
		{
			this.answerSet = answerSet;
			LazyLoadAnswerCapturers();
		}

		protected override void RebuildOnConstruction()
		{
			// don't rebuild yet as we have not set the answerSet
		}

		protected override void RebuildCore()
		{
			RemoveAllButLeaveRelationshipsIntact();
			base.RebuildCore();
			OnRebuilt();
		}

		public event EventHandler Rebuilt;

		void OnRebuilt()
		{
			if (Rebuilt != null)
			{
				Rebuilt(this, EventArgs.Empty);
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = true;

			if (answerSet.CompanyCampaign != null && answerSet.PageCount > 1)
			{
				VoteExamSurveyAnswerWrapperBase answerWrapper = (VoteExamSurveyAnswerWrapperBase)element;
				VoteExamSurveyAnswerWrapperCollection answerWrappers = (VoteExamSurveyAnswerWrapperCollection)CollectionToFilter;

				int index = answerWrapper.IndexForPaging;
				int minIndex = (answerSet.CurrentPage - 1) * answerSet.CompanyCampaign.G0_QuestionsPerWebPage;
				int maxIndex = minIndex + answerSet.CompanyCampaign.G0_QuestionsPerWebPage;

				result = (index >= minIndex && index < maxIndex) || answerWrapper.IsContinuedHeaderForPaging;
			}

			return result;
		}

		void LazyLoadAnswerCapturers()
		{
			foreach (VoteExamSurveyAnswerWrapperBase answerWrapper in answerSet.AnswerWrappers)
			{
				object lazyLoadCapturer = answerWrapper.Answer;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		readonly IVoteExamSurveyAnswerSet answerSet;
	}
}

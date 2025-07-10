using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class ScaleRangeCollection : ActiveBusinessObjectCollection<LearningCentreQuestion>
	{
		public ScaleRangeCollection(QuestionCategory category)
			: base(category.campaign)
		{
			this.category = category;
		}

		protected override object[] GetCollectionState()
		{
			return new[] { category };
		}

		protected override void SetDefaultsForNewElementCore(LearningCentreQuestion newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.HY_AnswerType = LearningCentreAnswerTypeList.Codes.ScaleRange;
			newElement.HY_QuestionCategory = category.Code;
		}

		protected override bool MatchesFilterCore(LearningCentreQuestion element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && element.HY_QuestionCategory == category.Code;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(VoteExamSurveyQuestionSchema.HY_AnswerType, LearningCentreAnswerTypeList.Codes.ScaleRange);
			return query;
		}

		readonly QuestionCategory category;
	}
}

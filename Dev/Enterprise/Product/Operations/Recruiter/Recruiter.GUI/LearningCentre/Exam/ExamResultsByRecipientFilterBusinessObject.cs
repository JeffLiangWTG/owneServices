using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI
{
	public class ExamResultsByRecipientFilterBusinessObject : LearningCentreCampaignItemFilterBusinessObject
	{
		public ExamResultsByRecipientFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "ExamResultsByRecipientFilter";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddFilters(filters);
			return filters;
		}

		#region Filters

		void AddFilters(ModuleFilterCollection filters)
		{
			var examAttemptSubGroup = new ExamAttemptSubGroup();
			var commencedFilter = filters.AddDateFilter("Commenced", ExamAttemptSchema.EXA_TestCommencedUtc);
			commencedFilter.SubGroup = examAttemptSubGroup;
			commencedFilter.MultilingualDescription = ResString.GetMultilingualString("777c64c8-ca49-44b3-8756-65dd066249b6", "Commenced");

			var completedFilter = filters.AddDateFilter("Completed", ExamAttemptSchema.EXA_TestCompletedUtc);
			completedFilter.SubGroup = examAttemptSubGroup;
			completedFilter.MultilingualDescription = ResString.GetMultilingualString("f556b204-ee7c-4ed1-b63b-0c4a69240039", "Completed");
		}

		class ExamAttemptSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				var contactQuery = new ZDBOnlySubQuery(typeof(ExamAttempt), ExamAttemptSchema.EXA_G8);
				contactQuery.AddToFilter(filter);

				result.AddSubQuery(GlbCompanyCampaignItemSchema.PK, contactQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion
	}
}

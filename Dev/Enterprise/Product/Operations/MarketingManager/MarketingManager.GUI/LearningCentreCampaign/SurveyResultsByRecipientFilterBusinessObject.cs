using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class SurveyResultsByRecipientFilterBusinessObject : LearningCentreCampaignItemFilterBusinessObject
	{
		public SurveyResultsByRecipientFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "SurveyResultsByRecipientFilter";
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
			var closedDateFilter = filters.AddDateFilter("Closed Date", GlbCompanyCampaignItemSchema.G8_ClosedDateUtc);
			closedDateFilter.MultilingualDescription = ResString.GetMultilingualString("f97996fd-5987-482d-a8c2-6efd21110aa5", "Closed Date");
			var senderStaffNameFilter = filters.AddTextFilter("Sender Staff Name", GetOrganisationTypes);
			senderStaffNameFilter.MultilingualDescription = ResString.GetMultilingualString("c3594762-7424-42f9-923d-173a5227590f", "Sender Staff Name");
		}

		ZQuery GetOrganisationTypes(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			var contactQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			contactQuery.AddToFilter(GlbStaffSchema.GS_FullName, comparisonOperator, value);

			result.AddSubQuery(GlbCompanyCampaignItemSchema.G8_SystemCreateUser, contactQuery, JoinCondition.And);

			return result;
		}

		#endregion
	}
}

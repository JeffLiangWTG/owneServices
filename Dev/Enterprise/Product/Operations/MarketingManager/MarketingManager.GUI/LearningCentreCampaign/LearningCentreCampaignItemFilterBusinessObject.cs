using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class LearningCentreCampaignItemFilterBusinessObject : FilterStripBusinessObject
	{
		public LearningCentreCampaignItemFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "ResultsByRecipientFilter";
		}

		protected GlbCompanyCampaign Campaign;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddFilters(filters);
			return filters;
		}

		#region Filters

		void AddFilters(ModuleFilterCollection filters)
		{
			var contactSubGroup = new ContactSubGroup();
			var contactNameFilter = filters.AddTextFilter("Contact Name", ViewCampaignContactSchema.VCC_ContactName);
			contactNameFilter.SubGroup = contactSubGroup;
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("889633ef-49fa-4226-923d-acaa103b9b1f", "Contact Name");

			var contactEmailFilter = filters.AddTextFilter("Contact Email", ViewCampaignContactSchema.VCC_Email);
			contactEmailFilter.SubGroup = contactSubGroup;
			contactEmailFilter.MultilingualDescription = ResString.GetMultilingualString("afe1c704-246c-48fe-b4da-653bc4c9cde8", "Contact Email");
		}

		class ContactSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				var contactQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK);
				contactQuery.AddToFilter(filter);

				result.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contactQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion
	}
}

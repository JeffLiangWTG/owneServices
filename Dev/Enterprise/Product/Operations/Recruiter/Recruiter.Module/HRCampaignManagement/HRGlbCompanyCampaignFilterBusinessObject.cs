using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRGlbCompanyCampaignFilterBusinessObject : GlbCompanyCampaignFilterBusinessObject
	{
		#region Filter Overrides

		protected override ZQuery GetIsHumanResourcesCampaignQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(GlbCompanyCampaignSchema.G0_IsSalesAndMarketing, ZBool.False);
			query.AddToFilter(JoinCondition.And, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, SQLComparisonOperator.NotEqual, Core.Constants.Recruiter.LearningCentreCampaignType);
			return query;
		}

		protected override void AddCategoryTypeFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Category 1", GlbCompanyCampaignSchema.G0_Category, OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value.GetActiveCodeDescriptionPairList()).MultilingualDescription = OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.Value;
			filters.AddTextFilter("Category 2", GlbCompanyCampaignSchema.G0_Type, OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetActiveCodeDescriptionPairList()).MultilingualDescription = OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.Value;
		}

		protected override void AddWorkflowCustomFieldsFilters(ModuleFilterCollection filters)
		{
			filters.AddWorkflowCustomFieldsFilters(Factory, HRCampaignWorkflowDescriptor.WorkflowTypeCode, typeof(HRGlbCompanyCampaign));
		}

		protected override void AddCRMSecurityFilters(ModuleFilterCollection filters)
		{
		}

		#endregion

		#region Lookup Overrides

		protected override GlbCompanyCampaignLookups GetNewLookups()
		{
			return new HRGlbCompanyCampaignLookups(Factory);
		}

		#endregion
	}
}

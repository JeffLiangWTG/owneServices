using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MarketingManager.Business.SalesDashboardActivityTypeCodeList;

namespace Enterprise.MarketingManager.Module
{
	class SalesDashboardCRMSecurityProvider
	{
		/// <summary>
		/// Enquiry, Opportunity, Communication, Campaign, OneOffQuote, Quotation and Project have different CRM security filters individually.
		/// The dashboard filters combine the multiple filters by the type
		/// </summary>
		public void AddCRMSecurityFilterStrips(BusinessObjectFactory factory, ModuleFilterCollection mainFilters)
		{
			var securityList = new[]
			{
				new
				{
					code = Codes.Opportunity, description = Descriptions.Opportunity, type = typeof(OrgOpportunity), pk = OrgOpportunitySchema.PK,
					filters = GetFilters(factory, new OrgOpportunityCRMSecurityProvider())
				},
				new
				{
					code = Codes.Inquiry, description = Descriptions.Inquiry, type = typeof(SalesEnquiry), pk = OrgColdCallRegisterSchema.PK,
					filters = GetFilters(factory, new SalesEnquiryCRMSecurityProvider())
				},
				new
				{
					code = Codes.Communication, description = Descriptions.Communication, type = typeof(OrgSalesCall), pk = OrgSalesCallSchema.PK,
					filters = GetFilters(factory, new CommunicationCRMSecurityProvider())
				},
				new
				{
					code = Codes.Campaign, description = Descriptions.Campaign, type = typeof(GlbCompanyCampaignItem), pk = GlbCompanyCampaignItemSchema.PK,
					filters = GetFilters(factory, new GlbCompanyCampaignItemCRMSecurityProvider())
				},
				new
				{
					code = Codes.OneOffQuote, description = Descriptions.OneOffQuote, type = typeof(RateOneOffShipment), pk = RateOneOffShipmentSchema.PK,
					filters = GetFilters(factory, new SalesDashboardOneOffQuoteCRMSecurityProvider())
				},
				new
				{
					code = Codes.Quotation, description = Descriptions.Quotation, type = typeof(Quote), pk = RatingHeaderSchema.PK,
					filters = GetFilters(factory, new QuotationsCRMSecurityProvider())
				},
				new
				{
					code = Codes.Project, description = Descriptions.Project, type = typeof(Project), pk = WorkProjectSchema.PK,
					filters = GetFilters(factory, new ProjectCRMSecurityProvider())
				}
			};

			var enforcedSecurity = securityList.Where(t => t.filters.Any());
			if (enforcedSecurity.Any())
			{
				var combinedFilter = mainFilters.AddFlagsFilter("SalesDashboardCRMSecurityFilter",
					securityList.Select(t => (string)t.description).ToArray(),
					securityList.Select(t => new GetFlagsQuery((x) => GetEnforcedSecurityQuery(t.filters, t.code, t.type, t.pk))).ToArray(),
					JoinCondition.Or);
				combinedFilter.Category = FilterCategories.CRMSecurity;
				combinedFilter.MultilingualDescription = ResString.GetMultilingualString("SalesDashboardCRMSecurityFiler|EnforcedCRMSecurity", "Enforced CRM Security");
				combinedFilter.IsPublishedOnWeb = false;
				combinedFilter.GroupOrCategory = FilterOrCategory.None;
				combinedFilter.IsGroupOrCategoryReadOnly = true;
				combinedFilter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
				combinedFilter.ReadOnly = true;
				combinedFilter.OrCategory = FilterOrCategory.MandatoryFilterOrCategory;
				combinedFilter.IsOrCategoryReadOnly = true;
				combinedFilter.IsMandatorySecurityFilter = true;

				enforcedSecurity.ToList().ForEach((t) => combinedFilter.DefaultProperties[t.description] = true);
			}
		}

		ModuleFilterCollection GetFilters<T>(BusinessObjectFactory factory, CRMSecurityProvider<T> provider) where T : BusinessObject
		{
			var result = new ModuleFilterCollection();
			provider.AddCRMSecurityFilterStrips(factory, result);
			return result;
		}

		ZQuery GetEnforcedSecurityQuery(IEnumerable<ModuleFilter> filtersToBeCombined, ZString activityType, Type typeOfBizObj, SchemaPKColumn pKColumn)
		{
			var filterQuery = new ZDBOnlyQuery(typeof(SalesDashboardActivity));
			filterQuery.AddToFilter(JoinCondition.And, ViewSalesDashboardActivitySchema.VSA_ActivityType, activityType);

			if (filtersToBeCombined.Any())
			{
				var dashboardQuery = new ZDBOnlySubQuery(typeof(SalesDashboardActivity), ViewSalesDashboardActivitySchema.PK);
				dashboardQuery.AddToFilter(ViewSalesDashboardActivitySchema.PK, ZGuid.Empty);
				foreach (var filter in filtersToBeCombined)
				{
					var subFilterQuery = new ZDBOnlySubQuery(typeOfBizObj, pKColumn);
					subFilterQuery.AddToFilter(filter.Query);
					dashboardQuery.AddAsUnionQuery(subFilterQuery, true);
				}
				filterQuery.AddSubQuery(dashboardQuery, JoinCondition.And);
			}

			return filterQuery;
		}
	}
}

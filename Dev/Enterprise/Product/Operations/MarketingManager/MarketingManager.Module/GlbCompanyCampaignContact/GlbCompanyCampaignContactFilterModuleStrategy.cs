using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using CampaignContactFilterCategories = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories;

namespace Enterprise.MarketingManager.Module
{
	public class GlbCompanyCampaignContactFilterModuleStrategy : FilterModuleStrategy
	{
		public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
		{
			if (typeof(CampaignContact).IsAssignableFrom(gridCollection.TypeOfElements))
			{
				OrgRelatedPartiesFilterHelper.AddAllFilterControlBuilders(control);
				SalesRelationActivityFilterHelper.AddAllFilterControlColumnsAndBuilders(control);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A programmatic constant for filter description")]
		protected override IEnumerable<ModuleFilter> FiltersToAdd
		{
			get
			{
				if (typeof(CampaignContact).IsAssignableFrom(BizoType))
				{
					Type type = typeof(GlbCompanyCampaignContactFilterModuleStrategy);
					type = TypeDecider.GetTypeForBinding(type);
					var glbCompanyCampaignContactFilterModuleStrategy = (GlbCompanyCampaignContactFilterModuleStrategy)Activator.CreateInstance(type);
					yield return glbCompanyCampaignContactFilterModuleStrategy.GetNewOrgRelatedPartiesModuleFilter();

					foreach (var filter in SalesRelationActivityFilterHelper.GetAllModuleFilters(Factory, ViewCampaignContactSchema.Instance, ViewCampaignContactSchema.Constants.Prefix, null, typeof(CampaignContact)))
					{
						filter.Category = CampaignContactFilterCategories.Inquiries;
						yield return filter;
					}

					foreach (var filter in SalesRelationActivityFilterHelper.GetAllModuleFilters("Campaign Tracking", Factory, GlbCompanyCampaignItemSchema.Instance, GlbCompanyCampaignItemSchema.Constants.Prefix, ViewCampaignContactSchema.Instance, GlbCompanyCampaignItemSchema.G8_RecipientID, null, typeof(CampaignContact)))
					{
						filter.Category = CampaignContactFilterCategories.CampaignTracking;
						yield return filter;
					}

					yield return new OrganisationHasSalesRelationFilter(ZString.Format("Organization"), ZString.Empty, ViewCampaignContactSchema.VCC_OH);
					yield return new OrganisationRecentActivityDateFilter(ZString.Format("Organization"), ZString.Empty, ViewCampaignContactSchema.VCC_OH);
				}
			}
		}

		protected virtual OrgRelatedPartiesModuleFilter GetNewOrgRelatedPartiesModuleFilter()
		{
			var orgRelatedPartiesModulesFilter = new OrgRelatedPartiesModuleFilter("Related Parties", GetCustomerIntelligenceQuery);
			orgRelatedPartiesModulesFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			orgRelatedPartiesModulesFilter.MultilingualDescription = ResString.GetMultilingualString("0B612E0B-2306-49F0-B891-84D0B5816CB4", "Related Parties");
			return orgRelatedPartiesModulesFilter;
		}

		protected override MultilingualString GetUniqueMultilingualDescription(ModuleFilter filter, ModuleFilterCollection filters)
		{
			return filter.MultilingualDescription;
		}

		protected ZQuery GetCustomerIntelligenceQuery(ZQuery orgHeaderFilter)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			subQuery.AddToFilter(orgHeaderFilter);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subQuery, JoinCondition.And);
			return query;
		}
	}
}

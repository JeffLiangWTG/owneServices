using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public abstract class GlbCompanyCampaignContactFilterTestCaseWithSubGroupCheckExclusions : FilterStripBusinessObjectTestCase
	{
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "ActiveQuery"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "SubscriptionStatus"));
			result.Add(TableFilter(GlbCompanyCampaignSubscriptionSchema.Constants.TableName, "SubscriptionStatus"));
			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "SubscriptionStatus"));
			result.Add(TableFilter(ViewCampaignContactSchema.Constants.TableName, "SubscriptionStatus"));

			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(OrgAddressCapabilitySchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(OrgContactSchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Country / Port"));
			result.Add(TableFilter(RefUNLOCOSchema.Constants.TableName, "Country / Port"));

			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Achievable Business"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Client Relationship"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Client Desire to Remain with Company"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Client Desire to Remain with Company"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Difficulty which Client can be poached"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Amount of Client Electronic Integration"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Consulting Revenue"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Conversion Certainty"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Estimated Profit"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Percentage Won"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Related Staff"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Total Revenue"));
			result.Add(TableFilter(OrgMiscServSchema.Constants.TableName, "Sales - Warehouse Revenue"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "City"));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, "City"));
			result.Add(TableFilter(OrgAddressCapabilitySchema.Constants.TableName, "City"));

			result.Add(TableFilter(OrgContactSchema.Constants.TableName, "Contact Attribute"));
			result.Add(TableFilter(OrgContactAttributeSchema.Constants.TableName, "Contact Attribute"));

			result.Add(TableFilter(OrgDocumentSchema.Constants.TableName, "Document Group"));
			result.Add(TableFilter(OrgDocumentSchema.Constants.TableName, "Official Contact for Document Group"));
			result.Add(TableFilter(OrgContactSchema.Constants.TableName, "Official Contact for Document Group"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Official Contact for Document Group"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Organization Type"));

			result.Add(TableFilter(StmALogSchema.Constants.TableName, "Contact Details Verified"));

			result.Add(TableFilter(GlbBranchSchema.Constants.TableName, "Branch"));
			result.Add(TableFilter(OrgCompanyDataSchema.Constants.TableName, "Branch"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Branch"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Main Competitor"));
			result.Add(TableFilter(OrgRelatedPartySchema.Constants.TableName, "Main Competitor"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Main Broker"));
			result.Add(TableFilter(OrgRelatedPartySchema.Constants.TableName, "Main Broker"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Sales Trade Lane - Origin Port"));
			result.Add(TableFilter(OrgSalesSchema.Constants.TableName, "Sales Trade Lane - Origin Port"));
			result.Add(TableFilter(ViewLocationSchema.Constants.TableName, "Sales Trade Lane - Origin Port"));
			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Sales Trade Lane - Destination Port"));
			result.Add(TableFilter(OrgSalesSchema.Constants.TableName, "Sales Trade Lane - Destination Port"));
			result.Add(TableFilter(ViewLocationSchema.Constants.TableName, "Sales Trade Lane - Destination Port"));

			result.Add(TableFilter(RefCountrySchema.Constants.TableName, "State"));
			result.Add(TableFilter(RefCountryStatesSchema.Constants.TableName, "State"));
			result.Add(TableFilter(RefUNLOCOSchema.Constants.TableName, "State"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Last Edit User"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Last Edit User"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Last Edit User"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Sent By Person"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Sent By Person"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Sent By Person"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Tracking Status"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Tracking Status"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Tracking Status"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Created On Web/Internal"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Created On Web/Internal"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Created On Web/Internal"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Unique Day(s)Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Unique Day(s)Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Unique Day(s)Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Unique Day(s)Activity Count"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Has Destination URL Activity"));

			result.Add(TableFilter(GlbCompanySchema.Constants.TableName, "Has Context Activity"));
			result.Add(TableFilter(GlbCompanyCampaignSchema.Constants.TableName, "Has Context Activity"));
			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Has Context Activity"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Has Context Activity"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Staff Assignment Person And Role"));
			result.Add(TableFilter(OrgStaffAssignmentsSchema.Constants.TableName, "Staff Assignment Person And Role"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Main Competitor"));
			result.Add(TableFilter(OrgRelatedPartySchema.Constants.TableName, "Main Competitor"));

			result.Add(TableFilter(OrgHeaderSchema.Constants.TableName, "Main Broker"));
			result.Add(TableFilter(OrgRelatedPartySchema.Constants.TableName, "Main Broker"));

			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Has Destination URL Activity"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Has Destination URL Activity"));

			result.Add(TableFilter(GlbCompanyCampaignClickSchema.Constants.TableName, "Unique Day(s) Activity Count"));
			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Unique Day(s) Activity Count"));

			result.Add(TableFilter(GlbCompanyCampaignItemSchema.Constants.TableName, "Created On Web/Internal"));
			return result;
		}
	}
}

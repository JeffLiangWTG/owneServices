using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class SalesEnquiryFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleFountainFilter("Inquiry ID", OrgColdCallRegisterSchema.O1_LeadUniqueReference, "I");
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|InquiryID", "Inquiry ID");
			return filter;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddFountainFilter("Legacy Inquiry ID", OrgColdCallRegisterSchema.O1_LeadUniqueReference, "").MultilingualDescription = LegacyInquiryID;
			filters.AddTextFilter("Source", OrgColdCallRegisterSchema.O1_LeadSource, OpportunitySourceList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Source", "Source");
			filters.AddTextFilter("Source Details", OrgColdCallRegisterSchema.O1_OpportunitySourceDetails).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|SourceDetails", "Source Details");
			filters.AddTextFilter("Lead Interest", OrgColdCallRegisterSchema.O1_InterestLevel, LeadInterestList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|LeadInterest", "Lead Interest");

			filters.AddTextFilter("Address 1", GetAddress1Query).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|AddressOne", "Address 1");
			filters.AddTextFilter("Address 2", GetAddress2Query).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|AddressTwo", "Address 2");
			filters.AddTextFilter("City", GetCityQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|City", "City");
			filters.AddTextFilter("State", GetStateQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|State", "State");
			filters.AddTextFilter("Post Code", GetPostCodeQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|PostCode", "Post Code");
			filters.AddTextFilter("Business Registration Number", GetBusinessRegNoQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|BusinessRegNo", "Business Registration Number");

			filters.AddTextFilter("Contact Name", GetContactNameQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ContactName", "Contact Name");
			filters.AddTextFilter("Work Phone", GetContactPhoneQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|WorkPhone", "Work Phone");
			filters.AddTextFilter("Fax", GetContactFaxQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Fax", "Fax");
			filters.AddTextFilter("Email", GetContactEmailQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Email", "Email");
			filters.AddTextFilter("Job Category", GetContactJobCategoryQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ContactRole", "Job Category");

			filters.AddDateFilter("Original Call Date", OrgColdCallRegisterSchema.O1_LeadCalledDate, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|OriginalCallDate", "Original Call Date");

			var statusFilter = filters.AddTextFilter("Status", GetStatusQuery, O1_LeadStatus_List);
			statusFilter.MaxLength = OrgColdCallRegisterSchema.O1_LeadStatus.MaxLength;
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Status", "Status");
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var typeFilter = filters.AddTextFilter("Type", GetTypeQuery, AllEnquiryTypes);
			typeFilter.MaxLength = OrgColdCallRegisterSchema.O1_EnquiryType.MaxLength;
			typeFilter.Category = FilterCategories.StatusAndFlags;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Type", "Type");
			typeFilter.PropertyValidation = TypeValidation;
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var closeReasonFilter = filters.AddTextFilter("Close Reason", OrgColdCallRegisterSchema.O1_CloseReason, AllEnquiryCloseReasons);
			closeReasonFilter.Category = FilterCategories.StatusAndFlags;
			closeReasonFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|CloseReason", "Close Reason");

			var orgFilter = filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, Organisations_List);
			orgFilter.Category = FilterCategories.Organisations;
			orgFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Organisation", "Organization");

			var orgNameFilter = filters.AddTextFilter("Organization Name", GetOrgNameQuery);
			orgNameFilter.Category = FilterCategories.Organisations;
			orgNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|OrganizationName", "Organization Name");

			var assignedStaffFilter = filters.AddNkFilter("Assigned Staff", OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, ModuleIDs.GlbStaff, Staff);
			assignedStaffFilter.Category = FilterCategories.Organisations;
			assignedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|AssignedStaff", "Assigned Staff");

			var salesTeamFilter = filters.AddNkFilter("Sales Team", GetSalesTeamsQueryWithOperator, ModuleIDs.SalesTeam, SalesTeams);
			salesTeamFilter.Category = FilterCategories.Organisations;
			salesTeamFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|SalesTeam", "Sales Team");

			var assignedStaffBranchFilter = filters.AddGuidFilter("Assigned Staff Branch", ModuleIDs.GlbBranch, GetAssignedStaffBranchQuery, BranchList);
			assignedStaffBranchFilter.Category = FilterCategories.Organisations;
			assignedStaffBranchFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|AssignedStaffBranch", "Assigned Staff Branch");

			filters.AddGuidFilter("Referring Organization", ModuleIDs.Organisation, OrgColdCallRegisterSchema.O1_OH_SourceOfLead, Organisations_List).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ReferringOrganization", "Referring Organization");
			var referringContactNameFilter = filters.AddTextFilter("Referring Contact Name", GetReferringContactNameQuery);
			referringContactNameFilter.Category = FilterCategories.Organisations;
			referringContactNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ReferringContactName", "Referring Contact Name");

			filters.AddGuidFilter("Refer To Organization", ModuleIDs.Organisation, OrgColdCallRegisterSchema.O1_OH_ReferTo, Organisations_List).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ReferToOrganization", "Refer To Organization");
			var referToContactNameFilter = filters.AddTextFilter("Refer To Contact Name", GetReferToContactNameQuery);
			referToContactNameFilter.Category = FilterCategories.Organisations;
			referToContactNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|ReferToContactName", "Refer To Contact Name");

			AddLocationFilter(filters);

			filters.AddWorkflowCustomFieldsFilters(Factory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, typeof(SalesEnquiry));

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		MultilingualString LegacyInquiryID => ResString.GetMultilingualString("MasterFiles|EnquiryFilter|LegacyInquiryID", "Legacy Inquiry ID");

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			if (QueryObjectType != null && !QueryObjectType.IsInterface)
			{
				var tableName = BusinessObjectFactory.GetTableNameFromType(QueryObjectType, false);

				if (!string.IsNullOrEmpty(tableName))
				{
					ModuleAuditFilterProvider.AddAuditFilters(filters, EnterpriseSchema.GetTableSchema(tableName), Factory, QueryObjectType, WebInquiryProductName);
				}
			}
		}

		public string WebInquiryProductName => (NoResString)"Web Inquiry";

		void TypeValidation(ZPropertyInfo info)
		{
			ListValidation.ErrorIfInvalidCode(info, AllEnquiryTypes);
		}

		#endregion

		#region Lookups

		#region Staff

		GlbStaffCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = new GlbStaffCollection(Factory);
				}
				return staff;
			}
		}

		GlbStaffCollection staff;

		GlbBranchCollection BranchList
		{
			get
			{
				if (branchList == null)
				{
					branchList = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
				}
				return branchList;
			}
		}
		GlbBranchCollection branchList;

		ZQuery GetAssignedStaffBranchQuery(ZGuid value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			subQuery.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, SQLComparisonOperator.Equal, value);
			var query = new ZDBOnlyQuery(typeof(SalesEnquiry));
			query.AddSubQuery(OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region OpportunitySourceList

		ReadOnlyCodeDescriptionPairList OpportunitySourceList
		{
			get { return SalesEnquiryLookups.GetSourceList(); }
		}

		#endregion

		#region O1_LeadStatus_List

		CodeDescriptionPairList O1_LeadStatus_List
		{
			get { return new SalesEnquiryStatusCodeList(); }
		}

		#endregion

		#region Lead Interests

		CodeDescriptionPairList LeadInterestList
		{
			get { return new SalesEnquiryLookups(null).LeadInterest_List; }
		}

		#endregion

		#region AllEnquiryTypes

		CodeDescriptionPairList AllEnquiryTypes
		{
			get { return SalesEnquiryLookups.GetAllEnquiryTypes(); }
		}

		#endregion

		#region AllEnquiryCloseReasons

		CodeDescriptionPairList AllEnquiryCloseReasons
		{
			get { return SalesEnquiryLookups.CreateEnquiryCloseReasonList(); }
		}

		#endregion

		#region Organisations

		OrganisationsFindBoxCollection Organisations_List
		{
			get
			{
				if (organisations_List == null)
				{
					organisations_List = new OrganisationsFindBoxCollection(Factory);
				}
				return organisations_List;
			}
		}

		OrganisationsFindBoxCollection organisations_List;

		#endregion

		#region Sales Teams

		SalesTeamCollection SalesTeams
		{
			get
			{
				if (salesTeams == null)
				{
					salesTeams = new SalesTeamCollection(Factory);
				}
				return salesTeams;
			}
		}

		SalesTeamCollection salesTeams;

		#endregion

		#endregion

		#region Type

		ZQuery GetTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(OrgColdCallRegisterSchema.O1_EnquiryType, comparisonOperator, value);
		}

		#endregion

		#region Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			// TODO:Below code will be redundant and can be replaced when the following transformation is applied:
			// Old statuses of Legacy Cold Calls are turned into the new statuses in the SalesEnquiryStatusCodeList
			// Replacement: return new ZQuery(OrgColdCallRegisterSchema.O1_LeadStatus, comparisonOperator, value);
			ZQuery result = new ZQuery();

			if (comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				// Include records that have blank status descriptions
				ZQuery subQuery = new ZQuery();
				foreach (CodeDescriptionPair pair in O1_LeadStatus_List)
				{
					subQuery.AddToFilter(OrgColdCallRegisterSchema.O1_LeadStatus, SQLComparisonOperator.NotEqual, pair.Code);
				}

				if (comparisonOperator != SpecialComparisonOperator.IsBlank)
				{
					result.AddToFilter(OrgColdCallRegisterSchema.O1_LeadStatus, comparisonOperator, value);
				}

				result.AddToFilter(subQuery, JoinCondition.Or);
			}
			else
			{
				// Include only records that do have status descriptions
				foreach (CodeDescriptionPair pair in O1_LeadStatus_List)
				{
					result.AddToFilter(new ZQuery(OrgColdCallRegisterSchema.O1_LeadStatus, SQLComparisonOperator.Equal, pair.Code), JoinCondition.Or);
				}

				if (comparisonOperator != SpecialComparisonOperator.IsNotBlank)
				{
					result.AddToFilter(OrgColdCallRegisterSchema.O1_LeadStatus, comparisonOperator, value);
				}
			}

			return result;
		}

		#endregion

		#region Location

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter filter = filters.AddNkFilter("Country/ Port", GetLocationQuery, ModuleIDs.Location, Locations);
			filter.Category = FilterCategories.Locations;

			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|EnquiryFilter|Country/ Port", "Country/Region/ Port");

			if (!Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed)
			{
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				filter.PropertyValidation = LocationCountryFilterValidation;
			}
		}

		void LocationCountryFilterValidation(ZPropertyInfo info)
		{
			if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				string errorMessage = Res.GetString("D3DA7B60-D8EF-446A-B074-0C0BBF104054", @"Your current security rights only allow you to view inquiries relating to organizations based in your current login country/region ({0}).
If you think this is incorrect, please contact your system administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				info.AddError(errorMessage);
			}
		}

		ZQuery GetLocationQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			var notLinkedQuery = new ZQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, null);
			{
				ZQuery locationQuery = LocationHelper.GetLocationFilter(Factory, value, OrgColdCallRegisterSchema.O1_PortOrCountry, typeof(SalesEnquiry));
				notLinkedQuery.AddToFilter(locationQuery);
			}

			var linkedQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			{
				linkedQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);
				ZQuery locationQuery = LocationHelper.GetLocationFilter(Factory, value, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader));
				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
				orgSubQuery.AddToFilter(locationQuery);
				linkedQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			}

			query.AddToFilter(notLinkedQuery);
			query.AddToFilter(linkedQuery, JoinCondition.Or);
			return query;
		}

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Org Queries

		ZQuery GetOrgNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			var nameQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			nameQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, null);
			nameQuery.AddToFilter(OrgColdCallRegisterSchema.O1_CompanyName, comparisonOperator, value.SubstringSafe(0, OrgColdCallRegisterSchema.O1_CompanyName.MaxLength));

			var linkedNameQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			linkedNameQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);
			var subQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			subQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength));
			linkedNameQuery.AddSubQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, subQuery, JoinCondition.And);

			query.AddToFilter(nameQuery);
			query.AddToFilter(linkedNameQuery, JoinCondition.Or);
			return query;
		}

		protected ZQuery GetCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgColdCallRegisterSchema.O1_City, OrgAddressSchema.OA_City, comparisonOperator, value);
		}

		protected ZQuery GetStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgColdCallRegisterSchema.O1_State, OrgAddressSchema.OA_State, comparisonOperator, value);
		}

		protected ZQuery GetPostCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgColdCallRegisterSchema.O1_PostCode, OrgAddressSchema.OA_PostCode, comparisonOperator, value);
		}

		ZQuery GetAddress1Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgColdCallRegisterSchema.O1_Address1, OrgAddressSchema.OA_Address1, comparisonOperator, value);
		}

		ZQuery GetAddress2Query(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgColdCallRegisterSchema.O1_Address2, OrgAddressSchema.OA_Address2, comparisonOperator, value);
		}

		ZQuery GetAddressQuery(SchemaColumn enquiryColumn, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			var notLinkedQuery = new ZQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, null);
			notLinkedQuery.AddToFilter(enquiryColumn, comparisonOperator, value.SubstringSafe(0, enquiryColumn.MaxLength));

			var linkedQuery = GetLinkedAddressQuery(new ZQuery(orgAddressColumn, comparisonOperator, value.SubstringSafe(0, orgAddressColumn.MaxLength)));

			query.AddToFilter(notLinkedQuery);
			query.AddToFilter(linkedQuery, JoinCondition.Or);
			return query;
		}

		ZQuery GetLinkedAddressQuery(ZQuery orgAddressFilter)
		{
			ZDBOnlyQuery enquiryQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));

			ZDBOnlyQuery orgHeaderLinkedAddressQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			orgHeaderLinkedAddressQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);
			orgHeaderLinkedAddressQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OA_LinkedAddress, SQLComparisonOperator.Equal, null);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			ZDBOnlySubQuery orgAddressCapSubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			orgAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.Office.Code);
			orgAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, true);
			orgAddressSubQuery.AddToFilter(orgAddressFilter);
			orgAddressSubQuery.AddSubQuery(orgAddressCapSubQuery, JoinCondition.And);
			orgSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			orgHeaderLinkedAddressQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			enquiryQuery.AddToFilter(orgHeaderLinkedAddressQuery, JoinCondition.Or);

			ZDBOnlyQuery inquiryLinkedAddressQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			inquiryLinkedAddressQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);
			inquiryLinkedAddressQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OA_LinkedAddress, SQLComparisonOperator.NotEqual, null);
			ZDBOnlySubQuery inquiryOrgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgColdCallRegisterSchema.O1_OA_LinkedAddress);
			inquiryOrgAddressSubQuery.AddToFilter(orgAddressFilter);
			inquiryLinkedAddressQuery.AddSubQuery(inquiryOrgAddressSubQuery, JoinCondition.And);
			enquiryQuery.AddToFilter(inquiryLinkedAddressQuery, JoinCondition.Or);

			return enquiryQuery;
		}

		protected ZQuery GetBusinessRegNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			var notLinkedQuery = new ZQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, null);
			notLinkedQuery.AddToFilter(OrgColdCallRegisterSchema.O1_BusinessRegNo, comparisonOperator, value.SubstringSafe(0, OrgColdCallRegisterSchema.O1_BusinessRegNo.MaxLength));

			ZDBOnlyQuery linkedQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			linkedQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
			SQLComparisonOperator linkedOperator = comparisonOperator;
			bool notIn = false;
			if (comparisonOperator.IsNegativeSQLOperator())
			{
				notIn = true;
				linkedOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				notIn = true;
				linkedOperator = SpecialComparisonOperator.IsNotBlank;
			}
			ZDBOnlySubQuery regNoSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH, notIn);
			regNoSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, linkedOperator, value.SubstringSafe(0, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength));
			orgSubQuery.AddSubQuery(regNoSubQuery, JoinCondition.And);
			linkedQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			query.AddToFilter(notLinkedQuery);
			query.AddToFilter(linkedQuery, JoinCondition.Or);
			return query;
		}

		#endregion

		#region Contact Queries

		protected ZQuery GetContactNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetContactQuery(OrgColdCallRegisterSchema.O1_ContactName, OrgContactSchema.OC_ContactName, comparisonOperator, value);
		}

		protected ZQuery GetContactPhoneQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetContactQuery(OrgColdCallRegisterSchema.O1_Phone, OrgContactSchema.OC_Phone, comparisonOperator, value);
		}

		protected ZQuery GetContactEmailQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetContactQuery(OrgColdCallRegisterSchema.O1_Email, OrgContactSchema.OC_Email, comparisonOperator, value);
		}

		protected ZQuery GetContactFaxQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetContactQuery(OrgColdCallRegisterSchema.O1_Fax, OrgContactSchema.OC_Fax, comparisonOperator, value);
		}

		protected ZQuery GetContactJobCategoryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetContactQuery(OrgColdCallRegisterSchema.O1_JobCategory, OrgContactSchema.OC_JobCategory, comparisonOperator, value);
		}

		ZQuery GetReferringContactNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAssociatedContactNameQuery(OrgColdCallRegisterSchema.O1_OH_SourceOfLead, OrgColdCallRegisterSchema.O1_OC_ReferringContact, comparisonOperator, value);
		}

		ZQuery GetReferToContactNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAssociatedContactNameQuery(OrgColdCallRegisterSchema.O1_OH_ReferTo, OrgColdCallRegisterSchema.O1_OC_ReferToContact, comparisonOperator, value);
		}

		ZQuery GetContactQuery(SchemaColumn enquiryColumn, SchemaColumn orgContactColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			var notLinkedQuery = new ZQuery(OrgColdCallRegisterSchema.O1_OC_LinkedContact, null);
			notLinkedQuery.AddToFilter(enquiryColumn, comparisonOperator, value.SubstringSafe(0, enquiryColumn.MaxLength));

			var linkedQuery = GetLinkedContactQuery(new ZQuery(orgContactColumn, comparisonOperator, value.SubstringSafe(0, orgContactColumn.MaxLength)));
			query.AddToFilter(notLinkedQuery);
			query.AddToFilter(linkedQuery, JoinCondition.Or);
			return query;
		}

		ZQuery GetLinkedContactQuery(ZQuery orgContactFilter)
		{
			var enquiryQuery = new ZDBOnlyQuery(typeof(SalesEnquiry));
			enquiryQuery.AddToFilter(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, SQLComparisonOperator.NotEqual, null);

			var enquirySubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
			enquirySubQuery.AddToFilter(OrgContactSchema.OC_OH, SQLComparisonOperator.Equal, OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead);
			enquirySubQuery.AddToFilter(orgContactFilter, JoinCondition.And);
			enquiryQuery.AddSubQuery(OrgColdCallRegisterSchema.O1_OC_LinkedContact, enquirySubQuery, JoinCondition.And);

			return enquiryQuery;
		}

		ZQuery GetAssociatedContactNameQuery(SchemaColumn orgColumn, SchemaColumn contactColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var linkedOperator = comparisonOperator;
			var notIn = false;
			if (comparisonOperator.IsNegativeSQLOperator())
			{
				notIn = true;
				linkedOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				notIn = true;
				linkedOperator = SpecialComparisonOperator.IsNotBlank;
			}
			var nameQuery = new ZQuery(OrgContactSchema.OC_ContactName, linkedOperator, value.SubstringSafe(0, OrgContactSchema.OC_ContactName.MaxLength));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
			subQuery.AddToFilter(OrgContactSchema.OC_OH, SQLComparisonOperator.Equal, orgColumn);
			subQuery.AddToFilter(nameQuery, JoinCondition.And);

			var enquirySubQuery = new ZDBOnlySubQuery(typeof(SalesEnquiry), OrgColdCallRegisterSchema.PK, notIn);
			enquirySubQuery.AddSubQuery(contactColumn, subQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(SalesEnquiry));
			result.AddSubQuery(OrgColdCallRegisterSchema.PK, enquirySubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Sales Team

		ZQuery GetSalesTeamsQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString salesTeam)
		{
			var zQuery = new ZQuery();

			var salesEnquiryQuery = new ZDBOnlyQuery(typeof(OrgColdCallRegister));
			var subQueryGlbStaff = new ZDBOnlySubQuery(typeof(GlbStaff), OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, comparisonOperator == SQLComparisonOperator.NotEqual);
			var subQueryGroupLink = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS, comparisonOperator == SpecialComparisonOperator.IsBlank);
			var subQueryGroup = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupLinkSchema.GK_GG);
			subQueryGroup.AddToFilter(GlbGroupSchema.GG_IsSales, true);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				subQueryGroupLink.AddSubQuery(subQueryGroup, JoinCondition.And);
				subQueryGlbStaff.AddSubQuery(subQueryGroupLink, JoinCondition.And);
				salesEnquiryQuery.AddSubQuery(OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, GlbStaffSchema.GS_Code, subQueryGlbStaff, JoinCondition.And);

				salesEnquiryQuery.AddToFilter(JoinCondition.Or, OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, ZString.Empty);
			}
			else
			{
				var groupComparisonOperator = comparisonOperator;
				if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					groupComparisonOperator = SQLComparisonOperator.Equal;
				}

				subQueryGroup.AddToFilter(GlbGroupSchema.GG_Code, groupComparisonOperator, salesTeam.SubstringSafe(0, GlbGroupSchema.GG_Code.MaxLength));
				subQueryGroupLink.AddSubQuery(subQueryGroup, JoinCondition.And);
				subQueryGlbStaff.AddSubQuery(subQueryGroupLink, JoinCondition.And);
				salesEnquiryQuery.AddSubQuery(OrgColdCallRegisterSchema.O1_GS_NKRepAssigned, GlbStaffSchema.GS_Code, subQueryGlbStaff, JoinCondition.And);
			}

			zQuery.AddToFilter(salesEnquiryQuery);

			return zQuery;
		}

		#endregion

		readonly SalesEnquiryCRMSecurityProvider SecurityProvider = new SalesEnquiryCRMSecurityProvider();

		#region Index Search

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.Status:
					var statusFilter = new IndexSearchModuleTextFilter(searchField, new GetList(searchField.GetListDelegate), FilterCategories.StatusAndFlags);
					statusFilter.MaxLength = OrgColdCallRegisterSchema.O1_LeadStatus.MaxLength;
					RemoveComparison(statusFilter);
					return new SearchFieldOverride(searchField, statusFilter);
				case SearchFieldConstants.EnquiryType:
					var enquiryTypeFilter = new IndexSearchModuleTextFilter(searchField, new GetList(searchField.GetListDelegate), FilterCategories.StatusAndFlags);
					enquiryTypeFilter.MaxLength = OrgColdCallRegisterSchema.O1_EnquiryType.MaxLength;
					RemoveComparison(enquiryTypeFilter);
					return new SearchFieldOverride(searchField, enquiryTypeFilter);
				case SearchFieldConstants.CloseReason:
					return new SearchFieldOverride(searchField, FilterCategories.StatusAndFlags);
				case SearchFieldConstants.Organization:
				case SearchFieldConstants.ReferToOrganization:
				case SearchFieldConstants.ReferringOrganization:
					var organizationFilter = new IndexSearchModuleGuidFilter(searchField, ModuleIDs.Organisation, Organisations_List, FilterCategories.Organisations);
					return new SearchFieldOverride(searchField, organizationFilter);
				case SearchFieldConstants.SalesTeam:
					var salesTeamFilter = new IndexSearchModuleNKFilter(searchField, ModuleIDs.SalesTeam, SalesTeams, FilterCategories.Organisations);
					return new SearchFieldOverride(searchField, salesTeamFilter);
				case SearchFieldConstants.OrganizationName:
					return searchField.UIHidden ? null : new SearchFieldOverride(searchField, FilterCategories.Organisations);
				case SearchFieldConstants.AssignedStaffBranch:
				case SearchFieldConstants.AssignedStaff:
				case SearchFieldConstants.ReferToContactName:
				case SearchFieldConstants.ReferringContactName:
					return new SearchFieldOverride(searchField, FilterCategories.Organisations);
				case SearchFieldConstants.InquiryID:
					var inquiryIDFilter = new IndexSearchModuleFountainFilter(searchField, "I");
					var legacyInquiryIDFilter = new IndexSearchModuleFountainFilter(searchField, description: "Legacy Inquiry ID");
					legacyInquiryIDFilter.MultilingualDescription = LegacyInquiryID;
					return new SearchFieldOverride(searchField, new List<ModuleFilter>() { inquiryIDFilter, legacyInquiryIDFilter });
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		void RemoveComparison(IndexSearchModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.NotBlank);
			filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyStartsWith);
			filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.NoStartWith);
			filter.ComparisonOperator_List.RemoveCode(IndexSearchModuleTextFilter.IndexSearchTextFilterConstants.AnyExact);
		}

		static class SearchFieldConstants
		{
			public const string Status = "LEADSTATUS";

			public const string EnquiryType = "ENQUIRYTYPE";

			public const string CloseReason = "CLOSEREASON";

			public const string Organization = "ORGANIZATIONGUID";

			public const string OrganizationName = "ORGANIZATIONNAME";

			public const string AssignedStaff = "ASSIGNEDSTAFF";

			public const string SalesTeam = "SALESTEAM";

			public const string AssignedStaffBranch = "ASSIGNEDSTAFFBRANCH";

			public const string ReferToOrganization = "REFERTOORGANIZATION";

			public const string ReferringOrganization = "REFERRINGORGANIZATION";

			public const string ReferToContactName = "REFERTOCONTACTNAME";

			public const string ReferringContactName = "REFERRINGCONTACTNAME";

			public const string InquiryID = "INQUIRYID";
		}

		#endregion
	}
}

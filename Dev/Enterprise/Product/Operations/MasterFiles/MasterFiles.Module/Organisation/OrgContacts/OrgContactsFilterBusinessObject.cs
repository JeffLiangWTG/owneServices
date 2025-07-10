using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactsFilterBusinessObject : FilterStripBusinessObject, IOrgContactsFilterBusinessObject
	{
		public OrgContactsFilterBusinessObject() : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "OrgContacts";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddEmailFilters(filters);
			AddRelatedItemFilters(filters);
			AddHiddenFilters(filters);
			AddRelationshipOrgAndStaffFilters(filters);
			AddDateFilters(filters);
			AddFlagsFilters(filters);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", OrgContactSchema.OC_ContactName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Name", "Name");
			filters.AddTextFilter("Title", OrgContactSchema.OC_Title).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Title", "Title");
			filters.AddTextFilter("JobCategory", OrgContactSchema.OC_JobCategory, JobCategoryList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|JobCategory", "Job Category");
			filters.AddTextFilter("Organization Name", GetOrganisationWorkplaceQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|OrganizationName", "Organization Name");
			var filter = filters.AddTextFilter("Notification Role", GetNotificationRoleQuery, OrgCodeLists.ContactType_List);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrganisationFilter|NotificationRole", "Notification Role");
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
		}

		#region GetNotificationRoleQuery

		ZQuery GetNotificationRoleQuery(SQLComparisonOperator op, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgContact));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgDocument), OrgDocumentSchema.OD_OC, op == SQLComparisonOperator.NotContains);
			subQuery.AddToFilter(OrgDocumentSchema.OD_DocumentGroup, value);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region GetOrganisationWorkplaceQuery

		ZQuery GetOrganisationWorkplaceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeOperator = comparisonOperator.IsNegativeSQLOperator();
			var joinCondition = isNegativeOperator ? JoinCondition.And : JoinCondition.Or;

			ZDBOnlySubQuery GetMainAddressSubQuery(bool notIn = false)
			{
				var mainAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH, notIn || isNegativeOperator);
				mainAddressSubQuery.AddSubQuery(OrgAddressSchema.PK, GetOrgAddressCapabilitySubQuery(), JoinCondition.And);
				mainAddressSubQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgAddressSchema.OA_CompanyNameOverride.MaxLength));

				return mainAddressSubQuery;
			}

			var query = new ZDBOnlyQuery(typeof(OrgContact));

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK, isNegativeOperator);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_CompanyNameOverride, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgAddressSchema.OA_CompanyNameOverride.MaxLength));

			var orgMainAddressSubQuery = GetMainAddressSubQuery();
			orgMainAddressSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress, SQLComparisonOperator.Equal, DBNull.Value);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, isNegativeOperator);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength));
			orgHeaderSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress,
				SQLComparisonOperator.Equal, DBNull.Value);
			orgHeaderSubQuery.AddSubQuery(GetMainAddressSubQuery(true), JoinCondition.And);

			query.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery, joinCondition);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgMainAddressSubQuery, joinCondition);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgHeaderSubQuery, joinCondition);

			return query;
		}

		#endregion

		#endregion

		#region Email

		void AddEmailFilters(ModuleFilterCollection filters)
		{
			var glbEmailFilterSubGroup = new GlbEmailFilterSubGroup();
			var emailFilter = filters.AddTextFilter("Email", GetContactEmailQuery);
			emailFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Email", "Email");
			emailFilter.Category = FilterCategories.EmailAddress;

			var deliveryStatusFilter = filters.AddTextFilter("Delivery Status", GlbEmailAddressSchema.GI_DeliveryStatus, new EmailDeliveryReportStatus());
			deliveryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("5c3192a3-5fe7-459b-bdb2-b7d2d24a27db", "Delivery Status");
			deliveryStatusFilter.Category = FilterCategories.EmailAddress;
			deliveryStatusFilter.SubGroup = glbEmailFilterSubGroup;
			deliveryStatusFilter.ComparisonOperator_List.Clear();
			deliveryStatusFilter.ComparisonOperator_List.Add(ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.Exact));
			deliveryStatusFilter.ComparisonOperator_List.Add(ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.NotEqual));

			var statusReportFilter = filters.AddDateFilter("Status Report Date", GlbEmailAddressSchema.GI_DeliveryReportTimeUtc, convertFromLocalToUTC: true);
			statusReportFilter.MultilingualDescription = ResString.GetMultilingualString("4d017970-05cc-4cfc-b4ff-be497ba9469a", "Status Report Date");
			statusReportFilter.Category = FilterCategories.EmailAddress;
			statusReportFilter.SubGroup = glbEmailFilterSubGroup;
		}

		#region GetContactEmailQuery

		ZQuery GetContactEmailQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var isNegativeOperator = comparisonOperator.IsNegativeSQLOperator();

			var query = new ZDBOnlyQuery(typeof(OrgContact));
			var primaryEmailQuery = new ZQuery(OrgContactSchema.OC_Email, comparisonOperator, value.SubstringSafe(0, OrgContactSchema.OC_Email.MaxLength));
			var contactItemSubQuery = new ZDBOnlySubQuery(typeof(OrgContactItem), OrgContactItemSchema.OI_OC, isNegativeOperator);
			contactItemSubQuery.AddToFilter(OrgContactItemSchema.OI_ContactItemType, OrgContactItemTypes.Codes.Email);
			contactItemSubQuery.AddToFilter(OrgContactItemSchema.OI_Address, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), value.SubstringSafe(0, OrgContactItemSchema.OI_Address.MaxLength));

			var joinCondition = isNegativeOperator ? JoinCondition.And : JoinCondition.Or;
			query.AddToFilter(primaryEmailQuery, joinCondition);
			query.AddSubQuery(contactItemSubQuery, joinCondition);
			return query;
		}

		#endregion

		#endregion

		#region Related

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgContactSchema.OC_OH, OrgHeaderList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Organisation", "Organization");
			filters.AddGuidFilter("Organisation Override", ModuleIDs.Organisation, OrgContactSchema.OC_OH_AddressOverride, OrgHeaderList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|OrganisationOverride", "Organization Override");
			filters.AddGuidFilter("Working Location", ModuleIDs.RefUNLOCO, GetWorkingLocationQuery, new RefUNLOCOCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|WorkingLocation", "Working Location");

			if (SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value)
			{
				var personFilter = new ModuleGuidForeignCollectionFilter("Person", ModuleIDs.GlbPerson, GlbPersonSchema.PK, OrgContactSchema.OC_PER, new GlbPersonCollection(Factory), typeof(OrgContact));
				personFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Person", "Person");
				filters.AddFilter(personFilter);
			}

			var logsFilter = new ModuleGuidForeignCollectionFilter("Contact Logs", ModuleIDs.OrgContactsStmALog, OrgContactSchema.PK, StmALogSchema.SL_Parent, new StmALogCollection(Factory), typeof(OrgContact));
			logsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|StmALog", "Contact Logs");
			logsFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.AllMatch);
			filters.AddFilter(logsFilter);

			var collection = ObjectFactory.Get<ISalesDashboardActivityCollection>("ISalesDashboardActivityCollection", Factory);
			var dashboardFilter = new ModuleGuidForeignCollectionFilter("Sales Activity", ModuleIDs.SalesDashboard, OrgContactSchema.PK, ViewSalesDashboardActivitySchema.VSA_ActivityContact, collection, typeof(OrgContact));
			dashboardFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|SalesDashboardActivity", "Sales Activity");
			dashboardFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.AllMatch);
			filters.AddFilter(dashboardFilter);
		}

		#region GetWorkingLocationQuery

		ZQuery GetWorkingLocationQuery(ZGuid value)
		{
			ZDBOnlySubQuery GetLocationSubQuery(bool notIn = false) =>
				new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code)
					.AddToFilter(RefUNLOCOSchema.PK, notIn ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal, value) as ZDBOnlySubQuery;

			ZDBOnlySubQuery GetMainAddressSubQuery(bool notIn = false)
			{
				var mainAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH, notIn);
				mainAddressSubQuery.AddSubQuery(OrgAddressSchema.PK, GetOrgAddressCapabilitySubQuery(), JoinCondition.And);
				mainAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_RL_NKRelatedPortCode, GetLocationSubQuery(notIn), JoinCondition.And);
				return mainAddressSubQuery;
			}

			var query = new ZDBOnlyQuery(typeof(OrgContact));

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_RL_NKRelatedPortCode, GetLocationSubQuery(), JoinCondition.And);

			var orgMainAddressSubQuery = GetMainAddressSubQuery();
			orgMainAddressSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress, SQLComparisonOperator.Equal, DBNull.Value);

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddSubQuery(OrgHeaderSchema.OH_RL_NKClosestPort, GetLocationSubQuery(), JoinCondition.And);
			orgHeaderSubQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_OA_OrgAddress,
				SQLComparisonOperator.Equal, DBNull.Value);
			orgHeaderSubQuery.AddSubQuery(GetMainAddressSubQuery(true), JoinCondition.And);

			query.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, orgAddressSubQuery, JoinCondition.Or);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgMainAddressSubQuery, JoinCondition.Or);
			query.AddSubQuery(OrgContactSchema.OC_OH, orgHeaderSubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#endregion

		#region Hidden

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			ModuleFlagsFilter isOrgActiveFilter = filters.AddFlagsFilter("Is Organization Active",
				new[] { Res.GetString("EF745583-57C4-4242-9E0C-A24572903564", "Is Organization Active") },
				new GetFlagsQuery[] { IsOrgActiveQuery });
			isOrgActiveFilter.Property0 = true;
			isOrgActiveFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgContactsFilter|Is Organization Active", "Is Organization Active");
			isOrgActiveFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		#region IsOrgActiveQuery

		ZQuery IsOrgActiveQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgContact));
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, value);
			query.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#endregion

		#region RelationshipOrgAndStaffFilters

		void AddRelationshipOrgAndStaffFilters(ModuleFilterCollection filters)
		{
			var isPrimaryWorkplaceContactFilter = filters.AddTextFilter("Is Primary Workplace Contact", GetContactIsPrimaryWorkplaceQuery, ContactIsPrimaryWorkplaceList);
			isPrimaryWorkplaceContactFilter.MultilingualDescription = ResString.GetMultilingualString("930c1da8-5287-406a-8d19-ded36d0c8a79", "Is Primary Workplace Contact");
			isPrimaryWorkplaceContactFilter.Category = FilterCategories.RelationshipOrgAndStaff;
		}

		#region IsPrimaryWorkplaceQuery

		ZQuery GetContactIsPrimaryWorkplaceQuery(ZString contactSelectValue)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgContact));

			contactSelectValue = contactSelectValue.Trim().ToUpper();
			if (contactSelectValue == OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly)
			{
				var relationshipQuery = new ZDBOnlySubQuery(typeof(GlbPersonPrimaryRelationship), GlbPersonPrimaryRelationshipSchema.PPR_PrimaryId);
				query.AddSubQuery(OrgContactSchema.PK, relationshipQuery, JoinCondition.And);
			}
			else if (contactSelectValue == OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts)
			{
				var nonRelationshipQuery = new ZDBOnlySubQuery(typeof(GlbPersonPrimaryRelationship), GlbPersonPrimaryRelationshipSchema.PPR_PrimaryId, true);
				query.AddSubQuery(OrgContactSchema.PK, nonRelationshipQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#endregion

		#region DateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var verifiedByFilter = filters.AddDateFilter("Verified", OrgContactSchema.OC_DetailsVerified);
			verifiedByFilter.MultilingualDescription = ResString.GetMultilingualString("02907da1-79fe-4d9f-b422-e5afde78942a", "Verified");
			verifiedByFilter.Category = FilterCategories.Dates;

			var lastSentTimeByFilter = filters.AddDateFilter("Send Password Instructions - Last Sent Time", GetLastSentTimeQuery);
			lastSentTimeByFilter.MultilingualDescription = ResString.GetMultilingualString("b849380f-bf72-440f-8f08-6e44de1cfdcf", "Send Password Instructions - Last Sent Time");
			lastSentTimeByFilter.Category = FilterCategories.Dates;
		}

		ZQuery GetLastSentTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var mainQuery = new ZDBOnlyQuery(typeof(OrgContact));
			var lastSentTimeDefaultQuery = @"
(
OC_PK IN
    (
        SELECT SL_Parent
        FROM
        (
            SELECT SL_Parent
            FROM dbo.StmALog with(nolock)
            WHERE SL_SE_NKEvent = 'WPE'
            GROUP BY SL_Parent
            HAVING MAX(SL_PostedTimeUtc) {0}
        ) T
    )
    OR OC_PER IN
    (
        SELECT SL_Parent
        FROM
        (
            SELECT SL_Parent
            FROM dbo.StmALog with(nolock)
            WHERE SL_SE_NKEvent = 'WPE'
            GROUP BY SL_Parent
            HAVING MAX(SL_PostedTimeUtc) {0}
        ) T
    )
)";
			var simpleQuery = @"
(
    SELECT SL_Parent
    FROM dbo.StmALog with(nolock)
    WHERE SL_SE_NKEvent = 'WPE'
)";
			var lastSentTimeHasDateQuery = "( OC_PK IN " + simpleQuery + " OR OC_PER IN " + simpleQuery + ")";
			var lastSentTimeHasNoDateQuery = "( OC_PK NOT IN " + simpleQuery + " AND OC_PER NOT IN " + simpleQuery + ")";

			var dateFromUtc = dateFrom.IsEmpty ? string.Empty : Env.Time.GetUtcFromLocalTime(dateFrom.ToDateTime()).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
			var dateToUtc = dateTo.IsEmpty ? string.Empty : Env.Time.GetUtcFromLocalTime(dateTo.ToDateTime()).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
			var parameters = new ZSqlParameterCollection();
			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasNoDateEntered:
					mainQuery.AddFilterAndZSQLParameterCollection(lastSentTimeHasNoDateQuery, null);
					return mainQuery;
				case DateComparisonOperator.HasDateEntered:
					mainQuery.AddFilterAndZSQLParameterCollection(lastSentTimeHasDateQuery, null);
					return mainQuery;
				default:
					if (TryGetWhereClauseFromDateComparison(comparisonOperator, dateFromUtc, dateToUtc, out var havingClause))
					{
						var lastSentTimeQuery = ZString.Format(lastSentTimeDefaultQuery, havingClause);
						mainQuery.AddFilterAndZSQLParameterCollection(lastSentTimeQuery, parameters);
						return mainQuery;
					}
					else
					{
						return new ZQuery();
					}
			}
		}

		bool TryGetWhereClauseFromDateComparison(DateComparisonOperator comparisonOperator, ZString dateFrom, ZString dateTo, out ZString havingClause)
		{
			if (dateFrom.IsEmpty && dateTo.IsEmpty)
			{
				havingClause = string.Empty;
				return false;
			}
			else if (!dateFrom.IsEmpty)
			{
				if (dateTo.IsEmpty)
				{
					havingClause = string.Format(">= '{0}'", dateFrom);
				}
				else
				{
					havingClause = string.Format((NoResString)"BETWEEN '{0}' and '{1}'", dateFrom, dateTo);
				}
			}
			else
			{
				havingClause = string.Format("<= '{0}'", dateTo);
			}
			return true;
		}

		#endregion

		#region FlagsFilters

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			var webAccessEnabledFilter = filters.AddFlagsFilter("Web Access", new[] { Res.GetString("bf566e37-5b4f-410f-af4f-24a6e99033f6", "Enabled") },
				new GetFlagsQuery[] { HasWebAccessQuery });
			webAccessEnabledFilter.MultilingualDescription = ResString.GetMultilingualString("4EDA9C80-28F2-490F-85A4-61ED68004A0B", "Web Access");
			webAccessEnabledFilter.Category = FilterCategories.StatusAndFlags;
		}

		#region HasWebAccessQuery

		ZQuery HasWebAccessQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgContact));
			query.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, value);
			return query;
		}

		#endregion

		#endregion

		#endregion

		#region Lookups

		#region OrgHeaderList

		public OrganisationsFindBoxCollection OrgHeaderList
		{
			get
			{
				if (fOrgHeaderList == null)
				{
					fOrgHeaderList = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrgHeaderList;
			}
		}

		OrganisationsFindBoxCollection fOrgHeaderList;

		#endregion

		#region JobCategoryList

		public ReadOnlyCodeDescriptionPairList JobCategoryList
		{
			get { return OrganisationsDataRegistry.Instance.ContactJobCategories.Value.GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#region ContactIsPrimaryWorkplaceList

		public CodeDescriptionPairList ContactIsPrimaryWorkplaceList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.AllContacts, Res.GetString("MasterFiles|OrgContactsFilter|ContactIsPrimaryWorkplace|AllContacts", "All contacts"));
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly, Res.GetString("MasterFiles|OrgContactsFilter|ContactIsPrimaryWorkplace|PrimaryWorkplaceContactsOnly", "Primary Workplace Contacts Only"));
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts, Res.GetString("MasterFiles|OrgContactsFilter|ContactIsPrimaryWorkplace|NonPrimaryWorkplaceContacts", "Non Primary Workplace Contacts"));

				return list;
			}
		}

		#endregion

		#endregion

		#region Subgroups

		#region Email

		class GlbEmailFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(OrgContact));
				var glbEmailSubQuery = new ZDBOnlySubQuery(typeof(GlbEmailAddress), GlbEmailAddressSchema.GI_EmailAddress);
				glbEmailSubQuery.AddToFilter(filter);
				query.AddSubQuery(OrgContactSchema.OC_Email, glbEmailSubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#endregion

		#region RemoveUserDefinedFilters

		public void RemoveUserDefinedFilters()
		{
			removeUserDefinedFilters = true;
		}

		protected override bool ShouldAddUserDefinedFiltersCore
		{
			get => base.ShouldAddUserDefinedFiltersCore && !removeUserDefinedFilters;
		}

		bool removeUserDefinedFilters;

		#endregion

		#region QueryHelpers

		ZDBOnlySubQuery GetOrgAddressCapabilitySubQuery() =>
			new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA)
				.AddToFilter(JoinCondition.And, OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.Office.Code)
				.AddToFilter(JoinCondition.And, OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True) as ZDBOnlySubQuery;

		#endregion

		readonly OrgContactsCRMSecurityProvider SecurityProvider = new OrgContactsCRMSecurityProvider();
	}
}

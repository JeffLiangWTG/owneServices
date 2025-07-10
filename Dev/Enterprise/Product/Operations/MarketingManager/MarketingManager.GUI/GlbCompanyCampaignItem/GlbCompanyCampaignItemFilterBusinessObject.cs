using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignItemFilterBusinessObject : FilterStripBusinessObject
	{
		public GlbCompanyCampaignItemFilterBusinessObject(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
		}

		/// <summary>
		/// Parameterless constructor for color scheme support.
		/// </summary>
		public GlbCompanyCampaignItemFilterBusinessObject()
		{
		}

		internal GlbCompanyCampaign Campaign;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddCampaignItemFilter(filters);
			AddCampaignFilter(filters);
			AddRelationshipFilters(filters);
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddFlagFilters(filters);
			AddTouchReceivedModuleFilter(filters);

			if (Campaign != null && !Campaign.IsTargetList)
			{
				AddLinkTrackingFilters(filters);
			}

			return filters;
		}

		#region Touch Received Filter

		void AddTouchReceivedModuleFilter(ModuleFilterCollection filters)
		{
			if (Campaign != null && Campaign.IsMasterCampaign)
			{
				var filter = new TouchReceivedModuleFilter("Touch Points", Campaign);
				filter.Category = FilterCategories.RelationshipOrgAndStaff;
				filter.MultilingualDescription = ResString.GetMultilingualString("A89191AE-8A8A-4E0B-AC8D-2059100FE56D", "Touch Points");
				filters.AddCustomFilter(filter);
			}
		}

		#endregion

		#region Flag Filters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			if (Campaign != null && (Campaign.IsTouchCampaign || Campaign.IsMasterCampaign))
			{
				var filter = filters.AddFlagsFilter("Transition Status", new[] { Res.GetString("6ECBE363-CB0A-4CF1-8BF2-80443E277B73", "Transitioned") }, new GetFlagsQuery[] { GetTransitionStatus });
				filter.MultilingualDescription = ResString.GetMultilingualString("3F347680-53D7-41DD-B262-2ED6AB8EC3E7", "Transition Status");
			}
		}

		ZQuery GetTransitionStatus(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			query.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, Campaign.PK);

			var master = Campaign.IsMasterCampaign ? Campaign : Campaign.MasterCampaign;
			var nextTouches = master.AllTouches.Where(t => t.G0_HorizontalId > Campaign.G0_HorizontalId).Select(t => t.PK).ToArray();

			if (nextTouches.Length > 0)
			{
				var subquery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, !value);
				subquery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, nextTouches);
				query.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, subquery, JoinCondition.And);
			}
			else
			{
				return value
					? ZQuery.NoResultQuery
					: query;
			}

			return query;
		}

		#endregion

		#region Campaign Item Filter

		void AddCampaignItemFilter(ModuleFilterCollection filters)
		{
			var campaignItemFilter = filters.AddGuidFilter("CampaignItem", ModuleIDs.GlbCompanyCampaignItem, GetCampaignItemQuery, new GlbCompanyCampaignItemCampaignDependentCollection(Factory));
			campaignItemFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		internal IGlbCompanyCampaignItem CampaignItem;

		ZQuery GetCampaignItemQuery(ZGuid value)
		{
			return CampaignItem != null ? new ZQuery(GlbCompanyCampaignItemSchema.PK, CampaignItem.PK) : new ZQuery();
		}

		#endregion

		#region Link Tracking Filters

		void AddLinkTrackingFilters(ModuleFilterCollection filters)
		{
			if (Campaign == null || !Campaign.IsMasterCampaign)
			{
				var linkActivityContextFilter = new ContextLinkActivityModuleFilter("Has Context Activity", this, Campaign);
				linkActivityContextFilter.Category = LinkActivityCategories.LinkActivity;
				linkActivityContextFilter.MultilingualDescription = ResString.GetMultilingualString("c9f54b31-b576-4c55-80ae-75837577a93a", "Has Context Activity");
				filters.AddCustomFilter(linkActivityContextFilter);

				var linkActivityURLFilter = new DestinationURLLinkActivityModuleFilter("Has Destination URL Activity", this, Campaign);
				linkActivityURLFilter.Category = LinkActivityCategories.LinkActivity;
				linkActivityURLFilter.MultilingualDescription = ResString.GetMultilingualString("8d4c60e2-61d1-4759-b130-af0ec4c723c3", "Has Destination URL Activity");
				filters.AddCustomFilter(linkActivityURLFilter);

				var distinctDaysCountFilter = new UniqueDaysActivityCountFilter("Unique Day(s) Activity Count", GetDistinctDaysCountQuery);
				distinctDaysCountFilter.MultilingualDescription = ResString.GetMultilingualString("cfbb4fc1-9d03-495d-bcf0-50dd0778a30d", "Unique Day(s) Activity Count");
				distinctDaysCountFilter.DefaultComparisonOperator = Enterprise.MarketingManager.GUI.CampaignContactNumberFilter.ComparisonConstants.GreaterThanOrEqualTo;
				distinctDaysCountFilter.Category = LinkActivityCategories.LinkActivity;
				filters.AddCustomFilter(distinctDaysCountFilter);

				var isUnsubscribedFilter = filters.AddFlagsFilter("Unsubscribed state",
					new[] { Res.GetString("388c9281-fb4f-4da3-be73-13a71fa8098b", "Unsubscribed from this campaign") },
					new GetFlagsQuery[] { GetIsUnsubscribedQuery });
				isUnsubscribedFilter.Category = LinkActivityCategories.LinkActivity;
				isUnsubscribedFilter.MultilingualDescription = ResString.GetMultilingualString("9581a394-82a6-4dc2-9cce-62bb526d5c41", "Unsubscribed state");
			}
		}

		ZQuery GetDistinctDaysCountQuery(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));

			string sQL = GetDistinctDaysCountQueryHelper.GetDistinctDaysActivitySQL(comparisonOperator, value, new[] { Campaign.PK });

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			query.AddFilterAndZSQLParameterCollection(sQL, @params);

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a manually formatted query.")]
		ZQuery GetIsUnsubscribedQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			var notIn = "NOT IN";
			if (value)
			{
				notIn = "IN";
			}

			const string sqlUnsubscriptionStatus =
@"{0} IN
(
SELECT VCC_PK FROM dbo.ViewCampaignContact WHERE VCC_Email {1} 
(
	SELECT GCS_Email FROM  
	(
		SELECT GCS_Email, GCS_IsSubscribed, RANK() OVER (PARTITION BY t.GCS_Email ORDER BY t.GCS_MATHCINGLEVEL) AS rank
		  FROM ( 
			  SELECT GCS_PK, GCS_Email, GCS_IsSubscribed, GCS_MATHCINGLEVEL =
				CASE 
				 WHEN GCS_MediaCategory = '{2}' AND GCS_MediaType = '{3}' THEN 1
				 WHEN GCS_MediaCategory = '' AND GCS_MediaType = '{3}' THEN 2
				 WHEN GCS_MediaCategory = '{2}' AND GCS_MediaType = '' THEN 3
				 WHEN GCS_MediaCategory = '' AND GCS_MediaType = '' THEN 4
				 ELSE 5
				END
				FROM dbo.GlbCompanyCampaignSubscription WHERE
				GCS_G0 = '{4}' 
			  ) t
		  WHERE t.GCS_MATHCINGLEVEL < 5
	) s WHERE s.rank = 1 AND s.GCS_IsSubscribed = 0)
)";
			var sQL = string.Format(CultureInfo.InvariantCulture, sqlUnsubscriptionStatus,
				GlbCompanyCampaignItem.Schema.G8_RecipientID,
				notIn,
				Campaign.G0_Category,
				Campaign.G0_Type,
				Campaign.PK);
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			query.AddFilterAndZSQLParameterCollection(sQL, @params);

			return query;
		}

		#endregion

		#region Campaign Filter

		void AddCampaignFilter(ModuleFilterCollection filters)
		{
			if (Campaign != null)
			{
				var campaignFilter = filters.AddGuidFilter("Campaign", ModuleIDs.GlbCompanyCampaign, GetCampaignQuery, new GlbCompanyCampaignCollection(Factory));
				campaignFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		ZQuery GetCampaignQuery(ZGuid value)
		{
			return new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, Campaign.PK);
		}

		#endregion

		#region Relationship Filters

		void AddRelationshipFilters(ModuleFilterCollection filters)
		{
			if (Campaign != null && !Campaign.IsHRCampaign)
			{
				var organisationFilter = filters.AddNkFilter("Organization", ViewCampaignContactSchema.VCC_OrgCode, ModuleIDs.Organisation, OrgCollection);
				organisationFilter.MultilingualDescription = ResString.GetMultilingualString("54095094-c73e-4f05-87d9-28d54dbbc7e4", "Organization");
				organisationFilter.Category = FilterCategories.RelationshipOrgAndStaff;
				organisationFilter.SubGroup = GlbCompanyCampaignItemSubGroupInstance;

				var countryOrPortFilter = filters.AddNkFilter("Country / Port", GetCountryOrPort, ModuleIDs.Location, Locations);
				countryOrPortFilter.MultilingualDescription = ResString.GetMultilingualString("c823c95d-30f1-4d83-9c38-23c00be02fbf", "Country(Region) / Port");
				countryOrPortFilter.Category = FilterCategories.RelationshipOrgAndStaff;
				countryOrPortFilter.MaxLength =
					ViewCampaignContactSchema.VCC_RelatedPortCode.MaxLength > RefUNLOCOSchema.RL_RN_NKCountryCode.MaxLength
						? RefUNLOCOSchema.RL_RN_NKCountryCode.MaxLength
						: ViewCampaignContactSchema.VCC_RelatedPortCode.MaxLength;
			}
			else
			{
				var countryFilter = filters.AddNkFilter("Country", ViewCampaignContactSchema.VCC_RelatedPortCode, ModuleIDs.RefCountry, Countries);
				countryFilter.MultilingualDescription = ResString.GetMultilingualString("4AD7AEC5-1CA1-4774-B44B-901BD64B5846", "Country/Region");
				countryFilter.Category = FilterCategories.Locations;
				countryFilter.SubGroup = GlbCompanyCampaignItemSubGroupInstance;
				countryFilter.MaxLength = RefCountrySchema.RN_Code.MaxLength;
			}
		}

		readonly GlbCompanyCampaignItemSubGroup GlbCompanyCampaignItemSubGroupInstance = new GlbCompanyCampaignItemSubGroup();

		class GlbCompanyCampaignItemSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				var contactQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK);
				contactQuery.AddToFilter(filter);
				query.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contactQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetCountryOrPort(ZString nk)
		{
			var nkCountryCode = nk.Length > 2 ? nk.Substring(0, RefUNLOCOSchema.RL_RN_NKCountryCode.MaxLength) : ZString.Empty;
			var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			var contactQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK);

			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			var unlocoSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), OrgHeaderSchema.OH_RL_NKClosestPort);
			unlocoSubQuery.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.StartsWith, nkCountryCode.IsEmpty ? nk : nkCountryCode);

			orgSubQuery.AddSubQuery(OrgHeaderSchema.OH_RL_NKClosestPort, RefUNLOCOSchema.RL_Code, unlocoSubQuery, JoinCondition.And);
			contactQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
			contactQuery.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_RelatedPortCode, SQLComparisonOperator.StartsWith, nk);

			query.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contactQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var contactInquiryRelatedFilterSubGroup = new ContactInquiryRelatedFilterSubGroup();

			if ((Campaign != null && !Campaign.IsTargetList && !Campaign.IsMasterCampaign) || Campaign == null)
			{
				var trackingStatusFilter = filters.AddTextFilter("Tracking Status", GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusList);
				trackingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ba1ef480-4489-45c4-8763-c992de4f7fbf", "Delivery Status");
			}

			var emailAddressFilter = filters.AddTextFilter("Email Address", ViewCampaignContactSchema.VCC_Email);
			emailAddressFilter.SubGroup = contactInquiryRelatedFilterSubGroup;
			emailAddressFilter.MultilingualDescription = ResString.GetMultilingualString("cfe9c33b-870f-422e-bdd2-8db9d055f44c", "Email Address");
			emailAddressFilter.SubGroup = GlbCompanyCampaignItemSubGroupInstance;

			var contactNameFilter = filters.AddTextFilter("Contact Name", ViewCampaignContactSchema.VCC_ContactName);
			contactNameFilter.SubGroup = contactInquiryRelatedFilterSubGroup;
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("04f21d36-3f80-4647-835c-92154f70dda3", "Contact Name");
			contactNameFilter.SubGroup = GlbCompanyCampaignItemSubGroupInstance;

			if (Campaign != null && !Campaign.IsHRCampaign)
			{
				var organizationNameFilter = filters.AddTextFilter("Organization Name", ViewCampaignContactSchema.VCC_OrgFullName);
				organizationNameFilter.MultilingualDescription = ResString.GetMultilingualString("9e863850-eef5-4629-8845-a4db296fda4a", "Organization Name");
				organizationNameFilter.SubGroup = GlbCompanyCampaignItemSubGroupInstance;
			}
		}

		class ContactInquiryRelatedFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlySubQuery viewQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK);
				viewQuery.AddToFilter(filter);
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				query.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, viewQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			if (Campaign == null || !Campaign.IsMasterCampaign)
			{
				var lastSentTimeFilter = filters.AddDateFilter("Last Sent Time", GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc, true);
				lastSentTimeFilter.MultilingualDescription = ResString.GetMultilingualString("c70bfa29-e959-4212-971f-0ba449b66fec", "Last Sent Time");

				var scheduledTimeFilter = filters.AddDateFilter("Scheduled Time", GlbCompanyCampaignItemSchema.G8_ScheduleTimeUtc, true);
				scheduledTimeFilter.MultilingualDescription = ResString.GetMultilingualString("886597b5-95e5-46d6-b128-049fd082c118", "Schedule Time");
			}
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.ReLoadExistingRows = true;
				return query;
			}
		}

		#region Lookups

		OrgHeaderCollection orgCollection;
		OrgHeaderCollection OrgCollection
		{
			get { return orgCollection ?? (orgCollection = Factory.GetCachedValue("GlbCompanyCampaignItemLookups.Organisations", delegate { return new OrgHeaderCollection(Factory); })); }
		}

		RefCountryCollection countries;
		RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(Factory)); }
		}

		CodeDescriptionPairList trackingStatusList;
		CodeDescriptionPairList TrackingStatusList
		{
			get
			{
				if (trackingStatusList == null)
				{
					trackingStatusList = new TrackingStatusCodes();
					trackingStatusList.RemoveCode(TrackingStatusCodes.Codes.SCH);
					trackingStatusList.RemoveCode(TrackingStatusCodes.Codes.UNS);
				}
				return trackingStatusList;
			}
		}

		GlbStaffCollection staffCollection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		GlbStaffCollection StaffCollection
		{
			get { return staffCollection ?? (staffCollection = new GlbStaffCollection(Factory)); }
		}

		public LocationCollection Locations
		{
			get
			{
				if (locations == null)
				{
					locations = new LocationCollection(Factory);
				}
				return locations;
			}
		}
		LocationCollection locations;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		TrackingLinkList TrackingContextList
		{
			get
			{
				if (trackingContextList == null)
				{
					var list = new TrackingLinkList(Campaign);
					trackingContextList = list.ListByContext();
				}
				return trackingContextList;
			}
		}
		TrackingLinkList trackingContextList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		TrackingLinkList DestinationURLList
		{
			get
			{
				if (destinationURLList == null)
				{
					var list = new TrackingLinkList(Campaign);
					destinationURLList = list.ListByURL();
				}
				return destinationURLList;
			}
		}
		TrackingLinkList destinationURLList;

		class TrackingLinkList : CodeDescriptionPairList
		{
			public TrackingLinkList(GlbCompanyCampaign campaign)
			{
				Campaign = campaign;
			}

			readonly GlbCompanyCampaign Campaign;

			public TrackingLinkList ListByContext()
			{
				foreach (var link in Campaign.TrackedLinks)
				{
					AddPair(link.GCL_Context, link.GCL_Context);
				}

				return this;
			}

			public TrackingLinkList ListByURL()
			{
				foreach (var link in Campaign.TrackedLinks)
				{
					AddPair(link.GCL_URL, link.GCL_URL);
				}

				return this;
			}
		}

		#endregion

		#region FilterCategory

		public static class LinkActivityCategories
		{
			public static FilterCategory LinkActivity
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.LinkActivity", "Link Activity")); }
			}
		}

		#endregion
	}
}

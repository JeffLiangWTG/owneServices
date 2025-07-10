using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignClickFilterBusinessObject : FilterStripBusinessObject
	{
		/// <summary>
		/// Parameterless constructor for color scheme support.
		/// </summary>
		public GlbCompanyCampaignClickFilterBusinessObject()
		{
		}

		public GlbCompanyCampaignClickFilterBusinessObject(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
		}

		protected GlbCompanyCampaign Campaign;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddFilters(filters);

			return filters;
		}

		#region Filters

		void AddFilters(ModuleFilterCollection filters)
		{
			var contactSubGroup = new ContactSubGroup();
			var contactNameFilter = filters.AddTextFilter("Contact Name", ViewCampaignContactSchema.VCC_ContactName);
			contactNameFilter.SubGroup = contactSubGroup;
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("946f44d9-c408-41c4-81b9-1c54e192215f", "Contact Name");

			var contactEmailFilter = filters.AddTextFilter("Email Address", ViewCampaignContactSchema.VCC_Email);
			contactEmailFilter.SubGroup = contactSubGroup;
			contactEmailFilter.MultilingualDescription = ResString.GetMultilingualString("96847065-a3dd-4dad-bc7a-ee6df938c4d8", "Email Address");

			var organisationFilter = filters.AddNkFilter("Organization", ViewCampaignContactSchema.VCC_OrgCode, ModuleIDs.Organisation, OrgCollection);
			organisationFilter.SubGroup = contactSubGroup;
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("dda262cb-f5cd-4a93-b300-be013546cada", "Organization");
			organisationFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var linkSubGroup = new LinkSubGroup();
			var trackingContextFilter = filters.AddTextFilter("Tracking Context", GlbCompanyCampaignLinkSchema.GCL_Context, TrackingContextList);
			trackingContextFilter.SubGroup = linkSubGroup;
			trackingContextFilter.MultilingualDescription = ResString.GetMultilingualString("e5b19c75-ba95-432b-a18f-b974344ac1f4", "Tracking Context");
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			trackingContextFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var destinationURLFilter = filters.AddTextFilter("Destination URL", GlbCompanyCampaignLinkSchema.GCL_URL, DestinationURLList);
			destinationURLFilter.SubGroup = linkSubGroup;
			destinationURLFilter.MultilingualDescription = ResString.GetMultilingualString("2d8f11c0-1ece-44c1-9fae-5c9819724793", "Destination URL");
			destinationURLFilter.MultilingualDescription = ResString.GetMultilingualString("86e82183-9f9b-4149-8b68-7a73017e44e8", "Has Destination URL Activity");
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			destinationURLFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var isImageFilter = filters.AddFlagsFilter("Image", new[] { Res.GetString("ac35c04f-145e-4eb4-9e9b-2544b376f23c", "Image") }, new[] { GlbCompanyCampaignLinkSchema.GCL_IsImage });
			isImageFilter.MultilingualDescription = ResString.GetMultilingualString("ac35c04f-145e-4eb4-9e9b-2544b376f23c", "Image");
			isImageFilter.SubGroup = linkSubGroup;

			var activityDateFilter = filters.AddDateFilter("Activity Date", GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc, true);
			activityDateFilter.SubGroup = linkSubGroup;
			activityDateFilter.MultilingualDescription = ResString.GetMultilingualString("aad4f8cb-67b2-4064-945d-378cce940f32", "Activity Date");
		}

		class ContactSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(GlbCompanyCampaignClick));
				var contactQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK);
				contactQuery.AddToFilter(filter);

				var campaignItemQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.PK);
				campaignItemQuery.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contactQuery, JoinCondition.And);

				result.AddSubQuery(GlbCompanyCampaignClickSchema.GCC_G8_Recipient, campaignItemQuery, JoinCondition.And);
				return result;
			}
		}

		class LinkSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignClick));
				var linkQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignLink), GlbCompanyCampaignLinkSchema.PK);
				linkQuery.AddToFilter(filter);
				query.AddSubQuery(GlbCompanyCampaignClickSchema.GCC_GCL, linkQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region Lookups

		OrgHeaderCollection orgCollection;
		OrgHeaderCollection OrgCollection
		{
			get { return orgCollection ?? (orgCollection = Factory.GetCachedValue("GlbCompanyCampaignClickLookups.Organisations", delegate { return new OrgHeaderCollection(Factory); })); }
		}

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
				if (Campaign != null)
				{
					foreach (var link in Campaign.TrackedLinks)
					{
						AddPair(link.GCL_Context, link.GCL_Context);
					}
				}

				return this;
			}

			public TrackingLinkList ListByURL()
			{
				if (Campaign != null)
				{
					foreach (var link in Campaign.TrackedLinks)
					{
						AddPair(link.GCL_URL, link.GCL_URL);
					}
				}

				return this;
			}
		}

		#endregion

	}
}

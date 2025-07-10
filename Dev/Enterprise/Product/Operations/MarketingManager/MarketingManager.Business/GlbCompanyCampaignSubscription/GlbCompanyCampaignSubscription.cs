using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignSubscription : AutoGlbCompanyCampaignSubscription, IGlbCompanyCampaignSubscription
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public GlbCompanyCampaignSubscription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ShouldHaveAllCampaignCategoryAndType = IsAllowedOrganizationControlSubscriptionPreferencesToAll || !CanEditOrDelete;
		}

		public ZString MediaCategoryDescription => Lookups.MediaCategoryList.GetDescriptionFromCode(GCS_MediaCategory);

		public ZString MediaTypeDescription => Lookups.MediaTypeList.GetDescriptionFromCode(GCS_MediaType);

		protected virtual bool GCS_G0_ReadOnly => true;
		protected virtual bool GCS_MediaCategory_ReadOnly => false;
		protected virtual bool GCS_MediaCategoryWithAll_ReadOnly => GCS_MediaCategory_ReadOnly;
		protected virtual bool GCS_MediaType_ReadOnly => false;
		protected virtual bool GCS_MediaTypeWithAll_ReadOnly => GCS_MediaType_ReadOnly;
		protected virtual bool GCS_IsSubscribed_ReadOnly => false;

		public override bool CanDelete => base.CanDelete && CanEditOrDelete;
		public override MultilingualString ReasonForNotAbleToDelete => OrganizationControlSubscriptionPreferencesToAllSecurityCheckpoint != null && !CanEditOrDelete ? OrganizationControlSubscriptionPreferencesToAllSecurityCheckpoint.ErrorMessageForNotAllowed : base.ReasonForNotAbleToDelete;

		bool CanEditOrDelete => !(IsInDatabase
			&& (((GCS_MediaCategoryWithAll == GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode && !GCS_MediaCategoryWithAllInfo.HasChanges)
				|| (GCS_MediaTypeWithAll == GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode && !GCS_MediaTypeWithAllInfo.HasChanges))
				&& !IsAllowedOrganizationControlSubscriptionPreferencesToAll));

		public override bool ReadOnly
		{
			get { return base.ReadOnly || !CanEditOrDelete; }
			set { base.ReadOnly = value; }
		}

		SecurityCheckpoint OrganizationControlSubscriptionPreferencesToAllSecurityCheckpoint => Env.Security?.OrganisationControlSubscriptionPreferencesToAll;

		bool IsAllowedOrganizationControlSubscriptionPreferencesToAll => OrganizationControlSubscriptionPreferencesToAllSecurityCheckpoint?.IsAllowed ?? false;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!IsAllowedOrganizationControlSubscriptionPreferencesToAll)
			{
				var mediaCategoryList = Lookups.MediaCategoryWithAllList;
				if (mediaCategoryList.Count > 0)
				{
					GCS_MediaCategoryWithAll = mediaCategoryList[0].Code;
				}

				var mediaTypeList = Lookups.MediaTypeWithAllList;
				if (mediaTypeList.Count > 0)
				{
					GCS_MediaTypeWithAll = mediaTypeList[0].Code;
				}
			}
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (!GCS_OH.IsEmpty && propertyPath.Select(descriptor => descriptor.ComponentType).Any(type => typeof(IOrgHeader).IsAssignableFrom(type)))
			{
				GCS_Email = "";
			}
		}
#endif

		[EmailAddress]
		public override ZString GCS_Email
		{
			get
			{
				return base.GCS_Email;
			}

			set
			{
				base.GCS_Email = value;
			}
		}

		[List("Lookups.Campaigns")]
		public override ZGuid GCS_G0
		{
			get { return base.GCS_G0; }
			set { base.GCS_G0 = value; }
		}

		[List("Lookups.MediaCategoryList")]
		public override ZString GCS_MediaCategory
		{
			get { return base.GCS_MediaCategory; }
			set
			{
				base.GCS_MediaCategory = value;
			}
		}

		[List("Lookups.MediaCategoryWithAllList")]
		public ZString GCS_MediaCategoryWithAll
		{
			get { return GCS_MediaCategory.IsEmpty ? GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode : GCS_MediaCategory; }
			set
			{
				if (value == GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode && (IsAllowedOrganizationControlSubscriptionPreferencesToAll || IsInDatabase))
				{
					GCS_MediaCategory = ZString.Empty;
				}
				else
				{
					GCS_MediaCategory = value;
				}
			}
		}

		public ZWrappedPropertyInfo GCS_MediaCategoryWithAllInfo => GetWrappedZPropertyInfo(nameof(GCS_MediaCategoryWithAll), x => GCS_MediaCategoryInfo);

		[List("Lookups.MediaTypeList")]
		public override ZString GCS_MediaType
		{
			get { return base.GCS_MediaType; }
			set
			{
				base.GCS_MediaType = value;
			}
		}

		[List("Lookups.MediaTypeWithAllList")]
		public ZString GCS_MediaTypeWithAll
		{
			get { return GCS_MediaType.IsEmpty ? GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode : GCS_MediaType; }
			set
			{
				if (value == GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode && (IsAllowedOrganizationControlSubscriptionPreferencesToAll || IsInDatabase))
				{
					GCS_MediaType = ZString.Empty;
				}
				else
				{
					GCS_MediaType = value;
				}
			}
		}

		public ZWrappedPropertyInfo GCS_MediaTypeWithAllInfo => GetWrappedZPropertyInfo(nameof(GCS_MediaTypeWithAll), x => GCS_MediaTypeInfo);

		public ZBool GCS_IsOrgLevel => !GCS_OH.IsEmpty;

		void SetContactLevelSubscriptions()
		{
			if (GCS_IsOrgLevel && !GCS_IsSubscribed)
			{
				var allContactEmailsFromOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_Email);
				allContactEmailsFromOrgSubQuery.AddToFilter(OrgContactSchema.OC_Email, SQLComparisonOperator.NotEqual, "");
				allContactEmailsFromOrgSubQuery.AddToFilter(OrgContactSchema.OC_OH, SQLComparisonOperator.Equal, GCS_OH);

				var allContactsSubscriptions = new ZDBOnlyQuery(typeof(GlbCompanyCampaignSubscription));
				allContactsSubscriptions.AddSubQuery(GlbCompanyCampaignSubscriptionSchema.GCS_Email, allContactEmailsFromOrgSubQuery, JoinCondition.And);
				allContactsSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, null);

				if (GCS_MediaCategoryWithAll != GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
				{
					allContactsSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, GCS_MediaCategory);
				}
				if (GCS_MediaTypeWithAll != GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
				{
					allContactsSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, GCS_MediaType);
				}

				Factory.Load<GlbCompanyCampaignSubscription>(allContactsSubscriptions).ForEach(sub =>
				{
					sub.GCS_IsSubscribed = false;
					sub.GCS_G0 = GCS_G0;
				});
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			SetContactLevelSubscriptions();
		}

		public ZBool GCS_IsHRCampaignSubscription
		{
			get
			{
				if (GCS_G0.IsEmpty)
				{
					return false;
				}

				var campaign = Factory.Load<GlbCompanyCampaign>(GCS_G0);
				return !campaign.G0_IsSalesAndMarketing;
			}
		}

		public ZWrappedPropertyInfo GCS_IsOrgLevelInfo => GetWrappedZPropertyInfo(nameof(GCS_IsOrgLevel), x => GCS_OHInfo);

		public bool ShouldHaveAllCampaignCategoryAndType { get; set; }
	}
}

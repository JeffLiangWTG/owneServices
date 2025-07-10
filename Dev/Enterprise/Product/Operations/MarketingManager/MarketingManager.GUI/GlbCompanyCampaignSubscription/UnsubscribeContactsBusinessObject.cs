using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MarketingManager.GUI
{
	class UnsubscribeContactsBusinessObject : NonPersistentBusinessObject
	{
		protected UnsubscribeContactsBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			contactsCollection = new Lazy<UnsubscribeContactsItemCollection>(() => new UnsubscribeContactsItemCollection(Factory));
		}

		public UnsubscribeContactsBusinessObject(GlbCompanyCampaign campaign)
			: this(campaign.Factory)
		{
			CampaignCategory = campaign.G0_Category;
			CampaignType = campaign.G0_Type;
			this.campaign = campaign;
		}

		public ZString CampaignType { get; }

		public ZString CampaignCategory { get; }

		public UnsubscribeContactsItemCollection ContactsCollection => contactsCollection.Value;

		public bool HasUnsubscribeType => GetUnsubscribeType().HasValue;

		public bool IsHRCampaign => campaign.IsHRCampaign;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UnsubscribeContact = true;
		}

		UnsubscribeType? GetUnsubscribeType()
		{
			if (UnsubscribeFromCategoryAndType)
			{
				return UnsubscribeType.MediaCategoryAndType;
			}
			if (UnsubscribeFromCategory)
			{
				return UnsubscribeType.MediaCategory;
			}
			if (UnsubscribeFromType)
			{
				return UnsubscribeType.MediaType;
			}
			if (UnsubscribeFromAll)
			{
				return UnsubscribeType.Sender;
			}
			return null;
		}

		public void UnsubscribeContacts()
		{
			UnsubscribeType? unsubscribeType = UnsubscribeType.MediaCategoryAndType;

			foreach (var campaignContact in ContactsCollection)
			{
				foreach (SubscriptionProperties subscription in SubscriptionsList)
				{
					var unsubscr = new GlbCompanyCampaignUnsubscribe(Factory, campaignContact.GlbCompanyCampaignPk, unsubscribeType.Value);
					unsubscr.Process(subscription, true);
					campaignContact.UnsubscribeResult = unsubscr.ResultMessage;
				}
			}
		}

		readonly Lazy<UnsubscribeContactsItemCollection> contactsCollection;
		ZBool unsubscribeContact;
		ZBool unsubscribeFromAll;
		ZBool unsubscribeFromCategory;
		ZBool unsubscribeFromCategoryAndType;
		ZBool unsubscribeFromType;
		ZBool unsubscribeOrganization;

		#region UnsubscribeFromCategoryAndType

		public ZBool UnsubscribeFromCategoryAndType
		{
			get { return unsubscribeFromCategoryAndType; }
			set { SetNonPersistentPropertyValue(UnsubscribeFromCategoryAndTypeInfo, ref unsubscribeFromCategoryAndType, value); }
		}

		public ZPropertyInfo UnsubscribeFromCategoryAndTypeInfo => GetZPropertyInfo(nameof(UnsubscribeFromCategoryAndType));

		#endregion

		#region UnsubscribeFromCategory

		public ZBool UnsubscribeFromCategory
		{
			get { return unsubscribeFromCategory; }
			set { SetNonPersistentPropertyValue(UnsubscribeFromCategoryInfo, ref unsubscribeFromCategory, value); }
		}

		public ZPropertyInfo UnsubscribeFromCategoryInfo => GetZPropertyInfo(nameof(UnsubscribeFromCategory));

		#endregion

		#region UnsubscribeFromType

		public ZBool UnsubscribeFromType
		{
			get { return unsubscribeFromType; }
			set { SetNonPersistentPropertyValue(UnsubscribeFromTypeInfo, ref unsubscribeFromType, value); }
		}

		public ZPropertyInfo UnsubscribeFromTypeInfo => GetZPropertyInfo(nameof(UnsubscribeFromType));

		#endregion

		#region UnsubscribeFromAll

		public ZBool UnsubscribeFromAll
		{
			get { return unsubscribeFromAll; }
			set { SetNonPersistentPropertyValue(UnsubscribeFromAllInfo, ref unsubscribeFromAll, value); }
		}

		public ZPropertyInfo UnsubscribeFromAllInfo => GetZPropertyInfo(nameof(UnsubscribeFromAll));

		#endregion

		#region UnsubscribeContact

		public ZBool UnsubscribeContact
		{
			get { return unsubscribeContact; }
			set { SetNonPersistentPropertyValue(UnsubscribeContactInfo, ref unsubscribeContact, value); }
		}

		public ZPropertyInfo UnsubscribeContactInfo => GetZPropertyInfo(nameof(UnsubscribeContact));

		#endregion

		#region UnsubscribeOrgs

		public ZBool UnsubscribeOrganization
		{
			get { return unsubscribeOrganization; }
			set { SetNonPersistentPropertyValue(UnsubscribeOrganizationInfo, ref unsubscribeOrganization, value); }
		}

		public ZPropertyInfo UnsubscribeOrganizationInfo => GetZPropertyInfo(nameof(UnsubscribeOrganization));

		public
#if DEBUG
		virtual
#endif
		SubscriptionListNodeCollection SubscriptionsList
		{
			get
			{
				if (subscriptionsList == null)
				{
					subscriptionsList = new SubscriptionListNodeCollection(false);
					subscriptionsList.OnNewBusinessObjectAdded += new EventHandler(OnNewSubscriptionAdded);
					SubscriptionProperties sub = subscriptionsList.AddNew();
					sub.MediaCategoryWithAll = this.CampaignCategory;
					sub.MediaTypeWithAll = this.CampaignType;
					sub.IsSubscribed = false;
					RegisterEditableChildObject(subscriptionsList);
				}
				return subscriptionsList;
			}
		}

#if DEBUG
		protected
#endif
		SubscriptionListNodeCollection subscriptionsList;

		readonly GlbCompanyCampaign campaign;

#if DEBUG
		protected
#endif
		void OnNewSubscriptionAdded(object sender, EventArgs e)
		{
			var sub = (SubscriptionProperties)sender;
			sub.CampaignPK = campaign != null ? campaign.PK : ZGuid.Empty;
			sub.IsHRCampaign = campaign != null ? campaign.IsHRCampaign : ZBool.False;
			sub.OnIsSubscribedChanged += new EventHandler(OnIsSubscribedChanged);

			if (!Env.Security.OrganisationControlSubscriptionPreferencesToAll.IsAllowed)
			{
				sub.ShouldHaveAllCampaignCategoryAndType = false;
				var mediaCategoryList = sub.Lookups.MediaCategoryWithAllList;
				if (mediaCategoryList.Count > 0)
				{
					sub.MediaCategoryWithAll = mediaCategoryList[0].Code;
				}
				var mediaTypeList = sub.Lookups.MediaTypeWithAllList;
				if (mediaTypeList.Count > 0)
				{
					sub.MediaTypeWithAll = mediaTypeList[0].Code;
				}
			}
		}

		void OnIsSubscribedChanged(object sender, EventArgs e)
		{
			var sub = (SubscriptionProperties)sender;
			if (sub.IsSubscribed)
			{
				sub.CampaignPK = campaign.PK;
			}
			else
			{
				sub.CampaignPK = ZGuid.Empty;
			}
		}
		#endregion
	}
}

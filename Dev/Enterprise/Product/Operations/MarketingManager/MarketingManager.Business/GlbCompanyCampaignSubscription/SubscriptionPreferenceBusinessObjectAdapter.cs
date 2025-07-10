using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SubscriptionPreferenceBusinessObjectAdapter : GlbCompanyCampaignUnsubscribe
	{
		readonly ZString PublishedListCode = ZString.Empty;
		readonly OrgContact contact;

		static MultilingualString ErrorLoadingSubscriptionPreferenceMessage { get; } = ResString.GetMultilingualString("c27fa2b6-2983-400f-bed8-ea515845c34a", "A subscription preference list could not be found.");

		public SubscriptionPreferenceBusinessObjectAdapter(BusinessObjectFactory factory, ZGuid campaignItemPk, string unsubscribeTypeString, string resubscribe)
			: base(factory, campaignItemPk, unsubscribeTypeString, resubscribe)
		{
		}

		public SubscriptionPreferenceBusinessObjectAdapter(BusinessObjectFactory factory, ZGuid contactPK, ZString code)
			: base(factory, ZGuid.Empty, "", "")
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			PublishedListCode = code;
			contact = Factory.Load<OrgContact>(contactPK);
		}

		protected GlbCompanyCampaignSubscriptionForOrganisationContactCollection ContactSubscriptions
		{
			get
			{
				if (contactSubscriptions == null)
				{
					var parentContact = CampaignItemPk.IsEmpty ? contact : ContactCampaignItem?.RecipientAsOrgContact;

					if (parentContact != null)
					{
						contactSubscriptions = new GlbCompanyCampaignSubscriptionForOrganisationContactCollection(parentContact);
					}
				}
				return contactSubscriptions;
			}
		}
		GlbCompanyCampaignSubscriptionForOrganisationContactCollection contactSubscriptions;

		public SubscriptionListNodeCollection SubscriptionList
		{
			get
			{
				if (subscriptionList == null)
				{
					var publishedListCode = CampaignItemPk.IsEmpty ? PublishedListCode : ContactCampaignItem?.CompanyCampaign?.G0_PublishedListCode;

					if (publishedListCode.HasValue)
					{
						subscriptionList = GetSubscriptionListFromCode(publishedListCode);
					}
				}

				if (subscriptionList == null || subscriptionList.Count == 0)
				{
					SetErrorResult(ErrorLoadingSubscriptionPreferenceMessage);
				}
				else
				{
					if (ContactSubscriptions != null)
					{
						MatchingSubscriptionListStatus(subscriptionList, contactSubscriptions);
					}
				}

				return subscriptionList;
			}
		}

		SubscriptionListNodeCollection subscriptionList;

		SubscriptionListNodeCollection GetSubscriptionListFromCode(string publishedListCode)
		{
			var list = new SubscriptionListNodeCollection(false);

			var campaignCompany = ContactCampaignItem?.CompanyCampaign?.Company;
			SubscriptionRuleCollection rules = campaignCompany != null
				? OrganisationsDataRegistry.Instance.SubscriptionRules.GetFallBackValueAtAllLevels(campaignCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
				: OrganisationsDataRegistry.Instance.SubscriptionRules.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);

			foreach (SubscriptionRule rule in rules)
			{
				if (rule.Code.Equals(publishedListCode))
				{
					foreach (SubscriptionProperties sub in rule.Nodes)
					{
						SubscriptionProperties mySub = list.AddNew();
						mySub.PublishedDescription = sub.PublishedDescription;
						mySub.PublishedSummary = sub.PublishedSummary;
						mySub.MediaCategoryWithAll = sub.MediaCategoryWithAll;
						mySub.MediaTypeWithAll = sub.MediaTypeWithAll;
					}
					break;
				}
			}
			return list;
		}

		static void MatchingSubscriptionListStatus(SubscriptionListNodeCollection subList, GlbCompanyCampaignSubscriptionForOrganisationContactCollection contactSubscrptions)
		{
			int matchingLevel = 0;
			foreach (SubscriptionProperties sub in subList)
			{
				matchingLevel = 0;
				foreach (GlbCompanyCampaignSubscription contactSub in contactSubscrptions)
				{
					if (contactSub.GCS_MediaCategory.Equals(sub.MediaCategory)
					&& contactSub.GCS_MediaType.Equals(sub.MediaType))
					{
						matchingLevel = 4;
						sub.IsSubscribed = contactSub.GCS_IsSubscribed;
						break;
					}
					else if (contactSub.GCS_MediaCategoryWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
						&& contactSub.GCS_MediaType.Equals(sub.MediaType))
					{
						matchingLevel = 3;
						sub.IsSubscribed = contactSub.GCS_IsSubscribed;
					}
					else if (contactSub.GCS_MediaCategory.Equals(sub.MediaCategory)
						&& contactSub.GCS_MediaTypeWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
					{
						if (matchingLevel < 2)
						{
							matchingLevel = 2;
							sub.IsSubscribed = contactSub.GCS_IsSubscribed;
						}
					}
					else if (contactSub.GCS_MediaCategoryWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode)
						&& contactSub.GCS_MediaTypeWithAll.Equals(GlbCompanyCampaignSubscriptionLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
					{
						if (matchingLevel < 1)
						{
							matchingLevel = 1;
							sub.IsSubscribed = contactSub.GCS_IsSubscribed;
						}
					}
				}
			}
		}

		public override void SetContactVerified()
		{
			if (CampaignItemPk.IsEmpty && Contact != null)
			{
				if (!string.IsNullOrEmpty(Contact.Email))
				{
					var emailAddress = GlbEmailAddress.LoadOrNew(Factory, Contact.Email);
					emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.Unverified;
					Factory.Save();
				}
			}
			else
			{
				base.SetContactVerified();
			}
		}

		public void Process(SubscriptionProperties subscription)
		{
			if (!CampaignItemPk.IsEmpty)
			{
				base.Process(subscription, false);
			}
			else
			{
				try
				{
					var glbCompanyCampaignSubscription = Factory.New<GlbCompanyCampaignSubscription>();
					var queryToCheckExistance = new ZQuery(GlbCompanyCampaignSubscriptionSchema.PK, SQLComparisonOperator.NotEqual,
						glbCompanyCampaignSubscription.PK);

					glbCompanyCampaignSubscription.GCS_MediaCategory = subscription.MediaCategory;
					glbCompanyCampaignSubscription.GCS_MediaType = subscription.MediaType;
					queryToCheckExistance.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, subscription.MediaCategory);
					queryToCheckExistance.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, subscription.MediaType);

					glbCompanyCampaignSubscription.GCS_Email = Contact.OC_Email;
					queryToCheckExistance.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, Contact.OC_Email);
					queryToCheckExistance.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, null);

					glbCompanyCampaignSubscription.GCS_IsSubscribed = subscription.IsSubscribed;

					var existingGblCompanyCampaignSubscriptions = Factory.Load<GlbCompanyCampaignSubscription>(queryToCheckExistance);

					if (existingGblCompanyCampaignSubscriptions != null)
					{
						foreach (var existingGblCompanyCampaignSubscription in existingGblCompanyCampaignSubscriptions)
						{
							existingGblCompanyCampaignSubscription.Delete();
						}
					}

					try
					{
						SetSuccessResult(SubscriptionPreferenceMessageUser);
						Factory.Save();
					}
					catch (ZSaveException e) when (DuplicateRecordExceptionFilter(e))
					{
						glbCompanyCampaignSubscription.Delete();
						SetSuccessResult(AlreadyUnsubscribedMessage);
						Factory.Save();
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					SetErrorResult(e.Message);
					throw;
				}
			}
		}

		public OrgContact Contact
		{
			get { return contact; }
		}
	}
}

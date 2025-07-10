using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignUnsubscribe : NonPersistentBusinessObject
	{
		public GlbCompanyCampaignUnsubscribe(BusinessObjectFactory factory, ZGuid campaignItemPk, string unsubscribeTypeString, string resubscribe)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}
			if (unsubscribeTypeString == null)
			{
				throw new ArgumentNullException(nameof(unsubscribeTypeString));
			}

			CampaignItemPk = campaignItemPk;
			UnsubscribeType = unsubscribeTypeString;
			Resubscribe = resubscribe;

			IsErrorResult = true;
			ResubscribeAvailable = false;
			ResubscribeHref = null;
		}

		public GlbCompanyCampaignUnsubscribe(BusinessObjectFactory factory, ZGuid campaignItemPk, UnsubscribeType unsubscribeType, bool? resubscribe = null)
			: this(factory, campaignItemPk, unsubscribeType.ToString("G"), resubscribe?.ToString())
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}
		}

		static MultilingualString WrongUrlDataMessage { get; } = ResString.GetMultilingualString("2c97b554-4eb8-4f03-9846-7c1bde8cd49a", "Wrong data in URL.");
		static MultilingualString ErrorLoadingCampaignMessage { get; } = ResString.GetMultilingualString("02b24112-c141-4fe8-98d9-f36d3fa368e3", "Error loading campaign.");
		static MultilingualString EmptyEmailMessage { get; } = ResString.GetMultilingualString("2948412d-1748-4360-9f73-71414811f005", "Empty E-Mail address.");
		static MultilingualString SubscriptionPreferenceMessageAdmin => OrganisationsDataRegistry.Instance.SubscripitionPreferenceSuccessfullyAdmin.Value;

		public MultilingualString AlreadyUnsubscribedMessage => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.AlreadyUnsubscribed);
		public MultilingualString ResubscribeLabelMessage => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.ResubscribeLabel);
		public MultilingualString ResubscribeSuccessMessage => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.ResubscribedSuccessfully);
		public MultilingualString SuccessMessage => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.UnsubscribedSuccessfully);
		public MultilingualString SubscriptionPreferenceMessageUser => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferenceSuccessfullyUser);
		public MultilingualString SubscripitionPreferencePageTitle => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferencePageTitle);
		public MultilingualString SubscripitionPreferencePageMessage => GetDescriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferencePageMessage);

		MultilingualString GetDescriptionRegistryValue(MultilingualStringRegistryItem registryItem)
		{
			var campaignCompany = ContactCampaignItem?.CompanyCampaign?.Company;
			if (campaignCompany != null)
			{
				return registryItem.GetFallBackValueAtAllLevels(campaignCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}
			else
			{
				return registryItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		static string ErrorHref { get; } = "#";

		public void Process(bool unsubscribeOrganization = false)
		{
			UnsubscribeItemPk = ZGuid.Empty;
			var campaign = ContactCampaignItem?.CompanyCampaign;

			try
			{
				if (campaign == null)
				{
					SetErrorResult(ErrorLoadingCampaignMessage);
					return;
				}

				if (ContactCampaignItem.EmailAddress.IsEmpty)
				{
					SetErrorResult(EmptyEmailMessage);
					return;
				}

				UnsubscribeType type;
				if (!Enum.TryParse(UnsubscribeType, out type))
				{
					SetErrorResult(WrongUrlDataMessage);
					return;
				}

				var glbCompanyCampaignSubscription = Factory.New<GlbCompanyCampaignSubscription>();
				var queryToCheckExistence = new ZQuery(GlbCompanyCampaignSubscriptionSchema.PK, SQLComparisonOperator.NotEqual,
					glbCompanyCampaignSubscription.PK);
				var queryForRelevantSubscriptions = new ZQuery();

				switch (type)
				{
					case Business.UnsubscribeType.Sender:
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, ZString.Empty);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, ZString.Empty);
						break;
					case Business.UnsubscribeType.MediaCategory:
						glbCompanyCampaignSubscription.GCS_MediaCategory = campaign.G0_Category;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, campaign.G0_Category);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, ZString.Empty);
						queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, campaign.G0_Category);
						break;
					case Business.UnsubscribeType.MediaType:
						glbCompanyCampaignSubscription.GCS_MediaType = campaign.G0_Type;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, ZString.Empty);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, campaign.G0_Type);
						queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, campaign.G0_Type);
						break;
					case Business.UnsubscribeType.MediaCategoryAndType:
						glbCompanyCampaignSubscription.GCS_MediaCategory = campaign.G0_Category;
						glbCompanyCampaignSubscription.GCS_MediaType = campaign.G0_Type;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, campaign.G0_Category);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, campaign.G0_Type);
						break;
					default:
						SetErrorResult(WrongUrlDataMessage);
						return;
				}

				if (unsubscribeOrganization)
				{
					glbCompanyCampaignSubscription.GCS_OH = ContactCampaignItem.OrgPK;
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ZString.Empty);
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, ContactCampaignItem.OrgPK);
					queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ZString.Empty);
					queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, ContactCampaignItem.OrgPK);
				}
				else
				{
					glbCompanyCampaignSubscription.GCS_Email = ContactCampaignItem.EmailAddress;
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ContactCampaignItem.EmailAddress);
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, null);
					queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ContactCampaignItem.EmailAddress);
					queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, null);
				}

				var existingGlbCompanyCampaignSubscription = Factory.Load<GlbCompanyCampaignSubscription>(queryToCheckExistence).SingleOrDefault();

				if (existingGlbCompanyCampaignSubscription != null)
				{
					glbCompanyCampaignSubscription.Delete();
					glbCompanyCampaignSubscription = existingGlbCompanyCampaignSubscription;
				}

				queryForRelevantSubscriptions.AddToFilter(GlbCompanyCampaignSubscriptionSchema.PK, SQLComparisonOperator.NotEqual,
					glbCompanyCampaignSubscription.PK);
				var relevantSubscriptions = type == Business.UnsubscribeType.MediaCategoryAndType ? Array.Empty<GlbCompanyCampaignSubscription>() : Factory.Load<GlbCompanyCampaignSubscription>(queryForRelevantSubscriptions);

				bool isResubscribeProcess;
				if (bool.TryParse(Resubscribe, out isResubscribeProcess) && isResubscribeProcess)
				{
					if (glbCompanyCampaignSubscription.GCS_IsSubscribed)
					{
						SetSuccessResult(ResubscribeSuccessMessage, false);
						return;
					}

					if (glbCompanyCampaignSubscription.IsInDatabase)
					{
						glbCompanyCampaignSubscription.GCS_IsSubscribed = true;
						UnsubscribeItemPk = glbCompanyCampaignSubscription.PK;
					}
					else
					{
						UnsubscribeItemPk = ZGuid.Empty;
						glbCompanyCampaignSubscription.Delete();
					}

					foreach (var subscription in relevantSubscriptions)
					{
						subscription.GCS_IsSubscribed = true;
					}

					SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, unsubscribeOrganization);
					SetSuccessResult(ResubscribeSuccessMessage, false);

					Factory.Save();
				}
				else
				{
					try
					{
						if (!glbCompanyCampaignSubscription.GCS_IsSubscribed && campaign.PK == glbCompanyCampaignSubscription.GCS_G0)
						{
							SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, unsubscribeOrganization);
							SetSuccessResult(AlreadyUnsubscribedMessage);
							Factory.Save();
							return;
						}

						glbCompanyCampaignSubscription.GCS_G0 = campaign.PK;
						glbCompanyCampaignSubscription.GCS_IsSubscribed = false;

						UnsubscribeItemPk = glbCompanyCampaignSubscription.PK;

						foreach (var subscription in relevantSubscriptions)
						{
							subscription.GCS_G0 = campaign.PK;
							subscription.GCS_IsSubscribed = false;
						}

						SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, unsubscribeOrganization);
						SetSuccessResult(SuccessMessage);
						Factory.Save();
					}
					catch (ZSaveException e) when (DuplicateRecordExceptionFilter(e))
					{
						glbCompanyCampaignSubscription.Delete();
						SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, unsubscribeOrganization);
						SetSuccessResult(AlreadyUnsubscribedMessage);
						Factory.Save();
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				SetErrorResult(e.Message);
				throw;
			}
			finally
			{
				if (!IsErrorResult)
				{
					if (campaign != null && campaign.IsTouchCampaign)
					{
						if (!unsubscribeOrganization)
						{
							campaign.TransitionAndSchedule(new[] { ContactCampaignItem.PK });
						}
					}
				}
			}
		}

		public void Process(SubscriptionProperties subscription, bool fromAdmin)
		{
			UnsubscribeItemPk = ZGuid.Empty;
			var campaign = ContactCampaignItem?.CompanyCampaign;

			try
			{
				UnsubscribeType type;
				if (subscription.MediaCategoryWithAll.Equals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode) && subscription.MediaTypeWithAll.Equals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
				{
					type = Business.UnsubscribeType.Sender;
				}
				else if (subscription.MediaCategoryWithAll.Equals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
				{
					type = Business.UnsubscribeType.MediaType;
				}
				else if (subscription.MediaTypeWithAll.Equals(SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode))
				{
					type = Business.UnsubscribeType.MediaCategory;
				}
				else
				{
					type = Business.UnsubscribeType.MediaCategoryAndType;
				}

				var glbCompanyCampaignSubscription = Factory.New<GlbCompanyCampaignSubscription>();
				var queryToCheckExistence = new ZQuery(GlbCompanyCampaignSubscriptionSchema.PK, SQLComparisonOperator.NotEqual,
					glbCompanyCampaignSubscription.PK);

				switch (type)
				{
					case Business.UnsubscribeType.Sender:
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, ZString.Empty);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, ZString.Empty);
						break;
					case Business.UnsubscribeType.MediaCategory:
						glbCompanyCampaignSubscription.GCS_MediaCategory = subscription.MediaCategoryWithAll;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, subscription.MediaCategoryWithAll);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, ZString.Empty);
						break;
					case Business.UnsubscribeType.MediaType:
						glbCompanyCampaignSubscription.GCS_MediaType = subscription.MediaTypeWithAll;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, ZString.Empty);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, subscription.MediaTypeWithAll);
						break;
					case Business.UnsubscribeType.MediaCategoryAndType:
						glbCompanyCampaignSubscription.GCS_MediaCategory = subscription.MediaCategory;
						glbCompanyCampaignSubscription.GCS_MediaType = subscription.MediaType;
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaCategory, subscription.MediaCategory);
						queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_MediaType, subscription.MediaType);
						break;
					default:
						SetErrorResult(WrongUrlDataMessage);
						return;
				}

				if (subscription.IsOrgLevel)
				{
					glbCompanyCampaignSubscription.GCS_OH = ContactCampaignItem.OrgPK;
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ZString.Empty);
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, ContactCampaignItem.OrgPK);
				}
				else
				{
					glbCompanyCampaignSubscription.GCS_Email = ContactCampaignItem.EmailAddress;
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_Email, ContactCampaignItem.EmailAddress);
					queryToCheckExistence.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_OH, null);
				}

				glbCompanyCampaignSubscription.GCS_IsSubscribed = subscription.IsSubscribed;

				glbCompanyCampaignSubscription.GCS_G0 = campaign.PK;

				var existingGlbCompanyCampaignSubscriptions = Factory.Load<GlbCompanyCampaignSubscription>(queryToCheckExistence);

				if (existingGlbCompanyCampaignSubscriptions != null)
				{
					foreach (var existingGlbCompanyCampaignSubscription in existingGlbCompanyCampaignSubscriptions)
					{
						existingGlbCompanyCampaignSubscription.Delete();
					}
				}

				try
				{
					UnsubscribeItemPk = glbCompanyCampaignSubscription.PK;

					SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, subscription.IsOrgLevel);
					if (fromAdmin)
					{
						SetSuccessResult(SubscriptionPreferenceMessageAdmin);
					}
					else
					{
						SetSuccessResult(SubscriptionPreferenceMessageUser);
					}
					Factory.Save();
				}
				catch (ZSaveException e) when (DuplicateRecordExceptionFilter(e))
				{
					SetCampaignItemsToCheckTransitions(campaign, ContactCampaignItem, subscription.IsOrgLevel);
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
			finally
			{
				if (!IsErrorResult)
				{
					if (campaign != null && campaign.IsTouchCampaign)
					{
						campaign.TransitionAndSchedule(new[] { ContactCampaignItem.PK });
					}
				}
			}
		}

		void SetCampaignItemsToCheckTransitions(GlbCompanyCampaign campaign, GlbCompanyCampaignItem campaignItem, bool unsubscribeOrganization)
		{
			if (!campaign.IsTouchCampaign)
			{
				return;
			}

			ZDBOnlyQuery campaignItemsQuery = null;

			if (!unsubscribeOrganization)
			{
				if (campaignItem.G8_TrackingStatus == TrackingStatusCodes.Codes.QUE)
				{
					campaignItem.G8_IsCheckTransitionRequired = true;
				}
				else
				{
					var campaignQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignSchema.PK, false);
					campaignQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, campaign.G0_G0_Master);

					campaignItemsQuery = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
					campaignItemsQuery.AddSubQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignQuery, JoinCondition.And);
					campaignItemsQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.QUE);
					campaignItemsQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, campaignItem.G8_RecipientID);
				}
			}
			else
			{
				var contactsQuery = new ZDBOnlySubQuery(typeof(CampaignContact), ViewCampaignContactSchema.PK, false);
				contactsQuery.AddToFilter(ViewCampaignContactSchema.VCC_OH, SQLComparisonOperator.Equal, campaignItem.OrgPK);

				var campaignQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), GlbCompanyCampaignSchema.PK, false);
				campaignQuery.AddToFilter(GlbCompanyCampaignSchema.G0_G0_Master, campaign.G0_G0_Master);

				campaignItemsQuery = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				campaignItemsQuery.AddSubQuery(GlbCompanyCampaignItemSchema.G8_G0, campaignQuery, JoinCondition.And);
				campaignItemsQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, TrackingStatusCodes.Codes.QUE);
				campaignItemsQuery.AddSubQuery(GlbCompanyCampaignItemSchema.G8_RecipientID, contactsQuery, JoinCondition.And);
			}

			if (campaignItemsQuery != null)
			{
				Factory.Load<GlbCompanyCampaignItem>(campaignItemsQuery).ForEach(item =>
				{
					item.G8_IsCheckTransitionRequired = true;
				});
			}
		}

		protected static bool DuplicateRecordExceptionFilter(ZSaveException e)
		{
			var sqlException = e?.InnerException?.InnerException as SqlException;
			return sqlException != null
				   && new DbErrorMatch(sqlException).ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey
				   && e.Message.Contains(GlbCompanyCampaignSubscriptionSchema.Constants.Indexes.NR_UX__GCS_MediaCategory_GCS_MediaType_GCS_OH_GCS_Email);
		}

		protected void SetSuccessResult(string message, bool resubscribeAvailable = true)
		{
			ResultMessage = message;
			IsErrorResult = false;
			ResubscribeHref = ErrorHref;
			ResubscribeAvailable = false;

			if (!resubscribeAvailable)
			{
				return;
			}

			Uri uri;
			try
			{
				uri = UnsubscribeUrlHelper.GetUnsubscribeUrl(ContactCampaignItem, UnsubscribeType, true);
			}
			catch (UriFormatException)
			{
				return;
			}

			ResubscribeAvailable = true;
			ResubscribeHref = uri.AbsoluteUri;
		}

		protected void SetErrorResult(string message)
		{
			ResultMessage = message;
			IsErrorResult = true;
			ResubscribeAvailable = false;
			ResubscribeHref = ErrorHref;
		}

		#region Public Properties

		public ZGuid CampaignItemPk { get; }
		public ZPropertyInfo CampaignItemPkInfo => GetZPropertyInfo(nameof(CampaignItemPk));

		public ZString UnsubscribeType { get; }
		public ZPropertyInfo UnsubscribeTypeInfo => GetZPropertyInfo(nameof(UnsubscribeType));

		public ZString Resubscribe { get; }
		public ZPropertyInfo ResubscribeInfo => GetZPropertyInfo(nameof(Resubscribe));

		public ZString ResultMessage { get; private set; }
		public ZPropertyInfo ResultMessageInfo => GetZPropertyInfo(nameof(ResultMessage));

		public ZGuid UnsubscribeItemPk { get; private set; }
		public ZPropertyInfo UnsubscribeItemPkInfo => GetZPropertyInfo(nameof(UnsubscribeItemPk));

		public ZBool IsErrorResult { get; private set; }
		public ZPropertyInfo IsErrorResultInfo => GetZPropertyInfo(nameof(IsErrorResult));

		public ZBool IsSuccessResult => !IsErrorResult;
		public ZPropertyInfo IsSuccessResultInfo => GetZPropertyInfo(nameof(IsSuccessResult));

		public ZBool ResubscribeAvailable { get; private set; }
		public ZPropertyInfo ResubscribeAvailableInfo => GetZPropertyInfo(nameof(ResubscribeAvailable));

		public ZString ResubscribeHref { get; private set; }
		public ZPropertyInfo ResubscribeHrefInfo => GetZPropertyInfo(nameof(ResubscribeHref));

		public GlbCompanyCampaignItem ContactCampaignItem
		{
			get
			{
				if (contactCampaignItem == null)
				{
					contactCampaignItem = Factory.Load<GlbCompanyCampaignItem>(CampaignItemPk);
				}
				return contactCampaignItem;
			}
		}
		GlbCompanyCampaignItem contactCampaignItem;

		public virtual void SetContactVerified()
		{
			ContactCampaignItem?.MarkAsVerified();
			Factory.Save();
		}

		#endregion
	}
}

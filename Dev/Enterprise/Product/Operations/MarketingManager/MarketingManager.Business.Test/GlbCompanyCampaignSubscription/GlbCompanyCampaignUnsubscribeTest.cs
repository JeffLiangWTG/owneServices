using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignUnsubscribe))]
	sealed class GlbCompanyCampaignUnsubscribeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProcessUnsubscribe_NoServer()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "");
			foreach (var type in Enum.GetValues(typeof(UnsubscribeType)).Cast<UnsubscribeType>())
			{
				var glbCompanyCampaignUnsubscribe = NewGlbCompanyCampaignUnsubscribeWithValidData(type);
				AssertNoExceptionThrown(delegate
				{ glbCompanyCampaignUnsubscribe.Process(); });

				AssertEquals("You have successfully unsubscribed.", glbCompanyCampaignUnsubscribe.ResultMessage);
				Assert(!glbCompanyCampaignUnsubscribe.CampaignItemPk.IsEmpty);
			}
		}

		public void TestConstructor()
		{
			var gccu = new GlbCompanyCampaignUnsubscribe(Factory, ZGuid.BrettsGuid, "asdfg", "qwer");
			AssertNotNull(gccu);
			AssertEquals(nameof(GlbCompanyCampaignUnsubscribe.Factory), Factory, gccu.Factory);
			AssertEquals(nameof(GlbCompanyCampaignUnsubscribe.CampaignItemPk), ZGuid.BrettsGuid, gccu.CampaignItemPk);
			AssertEquals(nameof(GlbCompanyCampaignUnsubscribe.UnsubscribeType), "asdfg", gccu.UnsubscribeType);
			AssertEquals(nameof(GlbCompanyCampaignUnsubscribe.Resubscribe), "qwer", gccu.Resubscribe);

			AssertExceptionThrown<ArgumentNullException>(() => new GlbCompanyCampaignUnsubscribe(null, ZGuid.BrettsGuid, "asd", null));
			AssertExceptionThrown<ArgumentNullException>(() => new GlbCompanyCampaignUnsubscribe(Factory, ZGuid.BrettsGuid, null, null));
		}

		public void TestProcessErrors()
		{
			var gccu = new GlbCompanyCampaignUnsubscribe(Factory, ZGuid.BrettsGuid, "asdfg", null);
			gccu.Process();
			AssertErrorResult(gccu, "Error loading campaign.");

			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			gccu = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, "asdfg", null);
			gccu.Process();
			AssertErrorResult(gccu, "Error loading campaign.");

			var campaign = Factory.New<GlbCompanyCampaign>();
			campaignItem = campaign.CampaignsItemsSent.AddNew();
			gccu = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, "asdfg", null);
			gccu.Process();
			AssertErrorResult(gccu, "Empty E-Mail address.");

			campaign = Factory.New<GlbCompanyCampaign>();
			campaignItem = campaign.CampaignsItemsSent.AddNew();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = $"{nameof(contact)}@ema.il";
			campaignItem.G8_RecipientID = contact.PK;
			gccu = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, "asdfg", null);
			gccu.Process();
			AssertErrorResult(gccu, "Wrong data in URL.");
		}

		static void AssertErrorResult(GlbCompanyCampaignUnsubscribe gccu, string resultMessage)
		{
			AssertEquals(resultMessage, gccu.ResultMessage);
			AssertEquals(ZGuid.Empty, gccu.UnsubscribeItemPk);
			Assert("Should be an error", gccu.IsErrorResult);
			Assert("Should be an error", !gccu.IsSuccessResult);
			Assert("Resubscribe isn't available", !gccu.ResubscribeAvailable);
			AssertEquals("Resubscribe URL", "#", gccu.ResubscribeHref);
		}

		public void TestProcessDuplicateRecord()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaCategoryAndType);
			gccu.Process();
			AssertEquals("You have successfully unsubscribed.", gccu.ResultMessage);
			Assert(!gccu.UnsubscribeItemPk.IsEmpty);

			gccu.Process();
			AssertAlreadyUnsubscribed(gccu);
		}

		static void AssertAlreadyUnsubscribed(GlbCompanyCampaignUnsubscribe gccu)
		{
			AssertEquals("You have already unsubscribed from this campaign.", gccu.ResultMessage);
			AssertEquals(ZGuid.Empty, gccu.UnsubscribeItemPk);
			Assert("Should be a success", !gccu.IsErrorResult);
			Assert("Should be a success", gccu.IsSuccessResult);
			Assert("Resubscribe is available", gccu.ResubscribeAvailable);
			AssertEquals("Resubscribe URL", UnsubscribeUrlHelper.GetUnsubscribeUrl(gccu.ContactCampaignItem, gccu.UnsubscribeType, true), new Uri(gccu.ResubscribeHref));
		}

		GlbCompanyCampaignUnsubscribe NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType unsubscribeType)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			return new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, unsubscribeType.ToString("G"), null);
		}

		IList<ZGuid> NewRelevantSubscriptionWithValidData(GlbCompanyCampaignUnsubscribe gccu, bool isSubscribed = true, bool isOrgUnsubscribed = false)
		{
			var campaignItem = Factory.Load<GlbCompanyCampaignItem>(gccu.CampaignItemPk);
			var orgContact = (OrgContact)campaignItem.Recipient;
			var subscriptions = new List<ZGuid>();
			var orgHeader = isOrgUnsubscribed ? orgContact.OC_OH : ZGuid.Empty;
			var email = isOrgUnsubscribed ? ZString.Empty : orgContact.OC_Email;
			var unsubscribeType = (UnsubscribeType)Enum.Parse(typeof(UnsubscribeType), gccu.UnsubscribeType);
			if (unsubscribeType == UnsubscribeType.Sender)
			{
				var subscription = NewGlbCompanyCampaignSubscriptionWithData(orgHeader, email, "CT", ZString.Empty, isSubscribed);
				subscriptions.Add(subscription.PK);
				subscription = NewGlbCompanyCampaignSubscriptionWithData(orgHeader, email, ZString.Empty, "TP", isSubscribed);
				subscriptions.Add(subscription.PK);
				subscription = NewGlbCompanyCampaignSubscriptionWithData(orgHeader, email, "CT", "TP", isSubscribed);
				subscriptions.Add(subscription.PK);
			}
			else if (unsubscribeType == UnsubscribeType.MediaCategory || unsubscribeType == UnsubscribeType.MediaType)
			{
				var subscription = NewGlbCompanyCampaignSubscriptionWithData(orgHeader, email, "CT", "TP", isSubscribed);
				subscriptions.Add(subscription.PK);
			}

			return subscriptions;
		}

		GlbCompanyCampaignSubscription NewGlbCompanyCampaignSubscriptionWithData(ZGuid orgHeader, ZString email, ZString category, ZString type, bool isSubscribed)
		{
			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			if (orgHeader != ZGuid.Empty)
			{
				subscription.GCS_OH = orgHeader;
			}
			if (email != ZString.Empty)
			{
				subscription.GCS_Email = email;
			}
			subscription.GCS_MediaCategory = category;
			subscription.GCS_MediaType = type;
			subscription.GCS_IsSubscribed = isSubscribed;
			return subscription;
		}

		public void TestProcessUnsubscribeSender()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.Sender);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription);
		}

		public void TestProcessUnsubscribeMediaCategory()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaCategory);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription);
		}

		public void TestProcessUnsubscribeMediaType()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription);
		}

		public void TestProcessUnsubscribeMediaCategoryAndType()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaCategoryAndType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription);
		}

		void AssertGlbCompanyCampaignUnsubscribeItem(GlbCompanyCampaignUnsubscribe gccu, IList<ZGuid> relevantSubscription, bool isOrgUnsubscribed = false)
		{
			AssertEquals("You have successfully unsubscribed.", gccu.ResultMessage);
			Assert(!gccu.UnsubscribeItemPk.IsEmpty);

			var campaignItem = Factory.Load<GlbCompanyCampaignItem>(gccu.CampaignItemPk);
			AssertNotNull(campaignItem);
			var campaign = campaignItem.CompanyCampaign;
			AssertNotNull(campaign);
			var unsubscribeItem = Factory.Load<GlbCompanyCampaignSubscription>(gccu.UnsubscribeItemPk);
			AssertNotNull(unsubscribeItem);
			var orgContact = (OrgContact)campaignItem.Recipient;

			if (isOrgUnsubscribed)
			{
				AssertEquals(campaignItem.OrgPK, unsubscribeItem.GCS_OH);
				AssertEquals("Empty Email", true, unsubscribeItem.GCS_Email.IsEmpty);
			}
			else
			{
				AssertEquals("Empty Org reference", true, unsubscribeItem.GCS_OH.IsEmpty);
				AssertEquals(campaignItem.EmailAddress, unsubscribeItem.GCS_Email);
			}

			AssertEquals(campaign.PK, unsubscribeItem.GCS_G0);
			AssertEquals(false, unsubscribeItem.GCS_IsSubscribed);

			switch ((UnsubscribeType)Enum.Parse(typeof(UnsubscribeType), gccu.UnsubscribeType))
			{
				case UnsubscribeType.Sender:
					AssertNullOrEmpty(unsubscribeItem.GCS_MediaCategory);
					AssertNullOrEmpty(unsubscribeItem.GCS_MediaType);
					break;
				case UnsubscribeType.MediaCategory:
					AssertEquals(campaign.G0_Category, unsubscribeItem.GCS_MediaCategory);
					AssertNullOrEmpty(unsubscribeItem.GCS_MediaType);
					break;
				case UnsubscribeType.MediaType:
					AssertNullOrEmpty(unsubscribeItem.GCS_MediaCategory);
					AssertEquals(campaign.G0_Type, unsubscribeItem.GCS_MediaType);
					break;
				case UnsubscribeType.MediaCategoryAndType:
					AssertEquals(campaign.G0_Category, unsubscribeItem.GCS_MediaCategory);
					AssertEquals(campaign.G0_Type, unsubscribeItem.GCS_MediaType);
					break;
				default:
					Fail("Wrong enum type");
					return;
			}

			Assert("Should be a success", !gccu.IsErrorResult);
			Assert("Should be a success", gccu.IsSuccessResult);
			Assert("Resubscribe is available", gccu.ResubscribeAvailable);
			AssertEquals("Resubscribe URL", UnsubscribeUrlHelper.GetUnsubscribeUrl(gccu.ContactCampaignItem, gccu.UnsubscribeType, true), new Uri(gccu.ResubscribeHref));

			foreach (var pk in relevantSubscription)
			{
				var subscription = Factory.Load<GlbCompanyCampaignSubscription>(pk);
				Assert("Relevant subscription should be unsubscribed.", !subscription.GCS_IsSubscribed);
			}
		}

		public void TestProcessUnsubscribe()
		{
			foreach (var type in Enum.GetValues(typeof(UnsubscribeType)).Cast<UnsubscribeType>())
			{
				var glbCompanyCampaignUnsubscribe = NewGlbCompanyCampaignUnsubscribeWithValidData(type);
				glbCompanyCampaignUnsubscribe.Process();
				AssertEquals("You have successfully unsubscribed.", glbCompanyCampaignUnsubscribe.ResultMessage);
				Assert(!glbCompanyCampaignUnsubscribe.CampaignItemPk.IsEmpty);
			}
		}

		public void TestUpdateCampaignReferenceForExistingSubscription()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription);
			AssertSourceCampaign(gccu);

			var unsubscribeItemPk = gccu.UnsubscribeItemPk;

			gccu.Process();
			AssertAlreadyUnsubscribed(gccu);

			var newGccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaType);
			newGccu.Process();
			AssertGlbCompanyCampaignUnsubscribeItem(newGccu, relevantSubscription);
			AssertSourceCampaign(newGccu);

			AssertEquals("PKs of Subscription records should be the same", unsubscribeItemPk, newGccu.UnsubscribeItemPk);

			newGccu.Process();
			AssertAlreadyUnsubscribed(newGccu);
		}

		void AssertSourceCampaign(GlbCompanyCampaignUnsubscribe gccu)
		{
			var campaignItem = Factory.Load<GlbCompanyCampaignItem>(gccu.CampaignItemPk);
			var campaign = campaignItem.CompanyCampaign;
			var campaignSubscription = Factory.Load<GlbCompanyCampaignSubscription>(gccu.UnsubscribeItemPk);
			AssertEquals("PK of the campaign", campaign.PK, campaignSubscription.GCS_G0);
		}

		public void TestUnsubscribeForExistingSubscription()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var existingSubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription.GCS_Email = campaignItem.EmailAddress;
			existingSubscription.GCS_MediaCategory = campaign.G0_Category;
			existingSubscription.GCS_MediaType = campaign.G0_Type;
			existingSubscription.GCS_IsSubscribed = true;

			Factory.Save();

			var newUnsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(newUnsubscribe);
			newUnsubscribe.Process();

			AssertGlbCompanyCampaignUnsubscribeItem(newUnsubscribe, relevantSubscription);
			AssertEquals(existingSubscription.PK, newUnsubscribe.UnsubscribeItemPk);
		}

		public void TestResubscribeWithCampaign()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingUnsubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingUnsubscription.GCS_Email = campaignItem.EmailAddress;
			existingUnsubscription.GCS_G0 = campaign.PK;
			existingUnsubscription.GCS_MediaCategory = campaign.G0_Category;
			existingUnsubscription.GCS_MediaType = campaign.G0_Type;
			existingUnsubscription.GCS_IsSubscribed = false;

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		public void TestProcessResubscribeSender()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingUnsubscription = NewUnsbuscriptionWithValidData(campaign, campaignItem, UnsubscribeType.Sender);

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.Sender, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		public void TestProcessResubscribeMediaCategory()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingUnsubscription = NewUnsbuscriptionWithValidData(campaign, campaignItem, UnsubscribeType.MediaCategory);

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategory, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		public void TestProcessResubscribeMediaType()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingUnsubscription = NewUnsbuscriptionWithValidData(campaign, campaignItem, UnsubscribeType.MediaType);

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		public void TestProcessResubscribeMediaCategoryAndType()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingUnsubscription = NewUnsbuscriptionWithValidData(campaign, campaignItem, UnsubscribeType.MediaCategoryAndType);

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		GlbCompanyCampaignSubscription NewUnsbuscriptionWithValidData(GlbCompanyCampaign campaign, GlbCompanyCampaignItem campaignItem, UnsubscribeType unsubscribeType)
		{
			var existingUnsubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingUnsubscription.GCS_Email = campaignItem.EmailAddress;
			existingUnsubscription.GCS_G0 = campaign.PK;
			switch (unsubscribeType)
			{
				case UnsubscribeType.Sender:
					existingUnsubscription.GCS_MediaCategory = ZString.Empty;
					existingUnsubscription.GCS_MediaType = ZString.Empty;
					break;
				case UnsubscribeType.MediaCategory:
					existingUnsubscription.GCS_MediaCategory = campaign.G0_Category;
					existingUnsubscription.GCS_MediaType = ZString.Empty;
					break;
				case UnsubscribeType.MediaType:
					existingUnsubscription.GCS_MediaCategory = ZString.Empty;
					existingUnsubscription.GCS_MediaType = campaign.G0_Type;
					break;
				case UnsubscribeType.MediaCategoryAndType:
					existingUnsubscription.GCS_MediaCategory = campaign.G0_Category;
					existingUnsubscription.GCS_MediaType = campaign.G0_Type;
					break;
				default:
					break;
			}
			existingUnsubscription.GCS_IsSubscribed = false;

			return existingUnsubscription;
		}

		public void TestResubscribeWithoutCampaign()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var existingUnsubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingUnsubscription.GCS_Email = contact.OC_Email;
			existingUnsubscription.GCS_MediaCategory = campaign.G0_Category;
			existingUnsubscription.GCS_MediaType = campaign.G0_Type;
			existingUnsubscription.GCS_IsSubscribed = false;

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, existingUnsubscription.PK);
		}

		public void TestResubscribeWhenNoUnsubscribe()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, ZGuid.Empty);
		}

		public void TestResubscribeWithExistingSubscriptionRecord()
		{
			GlbCompanyCampaign campaign;
			GlbCompanyCampaignItem campaignItem;
			PrepareTestResubscribeData(out campaign, out campaignItem);

			var existingSubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription.GCS_Email = campaignItem.EmailAddress;
			existingSubscription.GCS_G0 = campaign.PK;
			existingSubscription.GCS_MediaCategory = campaign.G0_Category;
			existingSubscription.GCS_MediaType = campaign.G0_Type;
			existingSubscription.GCS_IsSubscribed = true;

			Factory.Save();

			var resubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType, true);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(resubscribe, false);
			resubscribe.Process();

			AssertGlbCompanyCampaignResubscribeItem(resubscribe, relevantSubscription, ZGuid.Empty);

			var unsubscribeItemAfterResubscribe = Factory.Load<GlbCompanyCampaignSubscription>(existingSubscription.PK);
			AssertNotNull(unsubscribeItemAfterResubscribe);
		}

		void PrepareTestResubscribeData(out GlbCompanyCampaign campaign, out GlbCompanyCampaignItem campaignItem)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";

			campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
		}

		void AssertGlbCompanyCampaignResubscribeItem(GlbCompanyCampaignUnsubscribe gccu, IList<ZGuid> relevantSubscription, ZGuid unsubscriptionRecordGuid)
		{
			AssertEquals("You have successfully re-subscribed.", gccu.ResultMessage);
			AssertEquals(unsubscriptionRecordGuid, gccu.UnsubscribeItemPk);

			var campaignItem = Factory.Load<GlbCompanyCampaignItem>(gccu.CampaignItemPk);
			AssertNotNull(campaignItem);
			var campaign = campaignItem.CompanyCampaign;
			AssertNotNull(campaign);
			if (unsubscriptionRecordGuid != ZGuid.Empty)
			{
				var unsubscribeItem = Factory.Load<GlbCompanyCampaignSubscription>(gccu.UnsubscribeItemPk);
				AssertNotNull(unsubscribeItem);
			}

			Assert("Should be a success", !gccu.IsErrorResult);
			Assert("Should be a success", gccu.IsSuccessResult);
			Assert("Resubscribe isn't available", !gccu.ResubscribeAvailable);
			AssertEquals("Resubscribe URL", "#", gccu.ResubscribeHref);

			foreach (var pk in relevantSubscription)
			{
				var subscription = Factory.Load<GlbCompanyCampaignSubscription>(pk);
				Assert("Relevant subscription should be subscribed.", subscription.GCS_IsSubscribed);
			}
		}

		public void TestUnsubscribeOrgForContact()
		{
			var gccu = NewGlbCompanyCampaignUnsubscribeWithValidData(UnsubscribeType.MediaCategoryAndType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(gccu);
			gccu.Process(true);

			AssertGlbCompanyCampaignUnsubscribeItem(gccu, relevantSubscription, true);
		}

		public void TestUnsubscribeOrgForContactForExistingSubscription()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Edwards";
			contact.OC_Email = "john@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var existingSubscription = Factory.New<GlbCompanyCampaignSubscription>();
			existingSubscription.GCS_OH = orgHeader.PK;
			existingSubscription.GCS_MediaCategory = campaign.G0_Category;
			existingSubscription.GCS_MediaType = campaign.G0_Type;
			existingSubscription.GCS_IsSubscribed = true;

			Factory.Save();

			var newUnsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategoryAndType);
			var relevantSubscription = NewRelevantSubscriptionWithValidData(newUnsubscribe);
			newUnsubscribe.Process(true);

			AssertGlbCompanyCampaignUnsubscribeItem(newUnsubscribe, relevantSubscription, true);
			AssertEquals(existingSubscription.PK, newUnsubscribe.UnsubscribeItemPk);
		}

		public void TestUnsubscribeOrgWithExistingContactSubscriptions()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = $"{contact1.OC_ContactName}@abc.net";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = nameof(contact2);
			contact2.OC_Email = $"{contact2.OC_ContactName}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			var contactSubscriptionToUnsubscribe1 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe1.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe1.GCS_IsSubscribed = true;
			var contactSubscriptionToUnsubscribe2 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe2.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe2.GCS_MediaType = campaign.G0_Type;
			contactSubscriptionToUnsubscribe2.GCS_IsSubscribed = true;

			var contactSubscriptionToAddCampaignReference1 = contact2.Subscriptions.AddNew();
			contactSubscriptionToAddCampaignReference1.GCS_MediaCategory = campaign.G0_Category;
			var contactSubscriptionToAddCampaignReference2 = contact2.Subscriptions.AddNew();
			contactSubscriptionToAddCampaignReference2.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToAddCampaignReference2.GCS_MediaType = campaign.G0_Type;

			var contactSubacriptionToKeep = contact1.Subscriptions.AddNew();
			contactSubacriptionToKeep.GCS_MediaCategory = "CAT";

			Factory.Save();

			var unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategory);
			unsubscribe.Process(true);

			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe1, campaign.PK, campaign.G0_Category);
			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe2, campaign.PK, campaign.G0_Category, campaign.G0_Type);
			AssertUnsubscribedOrgContact(contactSubscriptionToAddCampaignReference1, campaign.PK, campaign.G0_Category);
			AssertUnsubscribedOrgContact(contactSubscriptionToAddCampaignReference2, campaign.PK, campaign.G0_Category, campaign.G0_Type);
			AssertUnsubscribedOrgContact(contactSubacriptionToKeep, ZGuid.Empty, "CAT");
		}

		static void AssertUnsubscribedOrgContact(IGlbCompanyCampaignSubscription subscription, ZGuid campaignPk, ZString category, string type = "")
		{
			AssertEquals(campaignPk, subscription.GCS_G0);
			AssertEquals(false, subscription.GCS_IsSubscribed);
			AssertEquals(category, subscription.GCS_MediaCategory);
			AssertEquals(type, subscription.GCS_MediaType);
		}

		public void TestUnsubscribeContact_CheckTransition()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";

			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Edward Johnes";
			contact2.OC_Email = "edward@abc.net";

			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "Joward Edon";
			contact3.OC_Email = "jow@abc.net";

			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "Ardjo Ednes";
			contact4.OC_Email = "ard@abc.net";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = "DRM";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			campaign.G0_G0_Master = master.PK;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "QUE";

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "QUE";

			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_TrackingStatus = "VER";

			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			campaignItem4.G8_TrackingStatus = "QUE";

			Factory.Save();

			var unsub = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem1.PK, UnsubscribeType.Sender);
			unsub.Process();

			AssertEquals(true, campaignItem1.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem2.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem3.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem4.G8_IsCheckTransitionRequired);
		}

		public void TestUnsubscribeOrg_CheckTransition()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";

			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Edward Johnes";
			contact2.OC_Email = "edward@abc.net";

			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "Joward Edon";
			contact3.OC_Email = "jow@abc.net";

			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "Ardjo Ednes";
			contact4.OC_Email = "ard@abc.net";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = "DRM";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			campaign.G0_G0_Master = master.PK;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "QUE";

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "QUE";

			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_TrackingStatus = "VER";

			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			campaignItem4.G8_TrackingStatus = "QUE";

			Factory.Save();

			var unsub = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem1.PK, UnsubscribeType.MediaCategoryAndType);
			unsub.Process(true);

			AssertEquals(true, campaignItem1.G8_IsCheckTransitionRequired);
			AssertEquals(true, campaignItem2.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem3.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem4.G8_IsCheckTransitionRequired);
		}

		public void TestResubscribeContact_CheckTransition()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";

			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Edward Johnes";
			contact2.OC_Email = "edward@abc.net";

			var contact3 = org1.Contacts.AddNew();
			contact3.OC_ContactName = "Joward Edon";
			contact3.OC_Email = "jow@abc.net";

			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "Ardjo Ednes";
			contact4.OC_Email = "ard@abc.net";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = "DRM";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			campaign.G0_G0_Master = master.PK;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "QUE";

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "QUE";

			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_TrackingStatus = "VER";

			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			campaignItem4.G8_TrackingStatus = "QUE";

			Factory.Save();

			var unsub = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem1.PK, UnsubscribeType.Sender, true);

			Factory.Save();
			unsub.Process();

			AssertEquals(true, campaignItem1.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem2.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem3.G8_IsCheckTransitionRequired);
			AssertEquals(false, campaignItem4.G8_IsCheckTransitionRequired);
		}

		public void TestSubscribeOrgWithExistingContactSubscriptionsWithSubscriptionProperties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = $"{contact1.OC_ContactName}@abc.net";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = nameof(contact2);
			contact2.OC_Email = $"{contact2.OC_ContactName}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			var contactSubscriptionToUnsubscribe1 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe1.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe1.GCS_MediaType = campaign.G0_Type;
			contactSubscriptionToUnsubscribe1.GCS_IsSubscribed = false;

			var contactSubscriptionToUnsubscribe2 = contact2.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe2.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe2.GCS_MediaType = campaign.G0_Type;
			contactSubscriptionToUnsubscribe2.GCS_IsSubscribed = false;

			Factory.Save();

			var unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategory);
			var subProp = new SubscriptionProperties();
			subProp.IsOrgLevel = true;
			subProp.IsSubscribed = true;
			subProp.MediaCategoryWithAll = "CT";
			subProp.MediaTypeWithAll = "TP";
			unsubscribe.Process(subProp, true);

			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe1, ZGuid.Empty, campaign.G0_Category, campaign.G0_Type);
			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe2, ZGuid.Empty, campaign.G0_Category, campaign.G0_Type);

			GlbCompanyCampaignSubscriptionForOrganisationCollection orgSubs = new GlbCompanyCampaignSubscriptionForOrganisationCollection(orgHeader);
			Assert("Organization should be subscribed.", orgSubs[0].GCS_IsSubscribed);
			Assert("Organization sub should be referenced.", orgSubs[0].GCS_G0 == campaign.PK);
		}

		public void TestUnsubscribeOrgWithExistingContactSubscriptionsWithSubscriptionProperties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = $"{contact1.OC_ContactName}@abc.net";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = nameof(contact2);
			contact2.OC_Email = $"{contact2.OC_ContactName}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			var contactSubscriptionToUnsubscribe1 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe1.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe1.GCS_IsSubscribed = true;

			var contactSubscriptionToUnsubscribe2 = contact2.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe2.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe2.GCS_IsSubscribed = false;

			var contactSubscriptionToUnsubscribe3 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe3.GCS_MediaCategory = "CT";
			contactSubscriptionToUnsubscribe3.GCS_MediaType = "TP";
			contactSubscriptionToUnsubscribe3.GCS_IsSubscribed = true;

			Factory.Save();

			var unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategory);
			var subProp = new SubscriptionProperties();
			subProp.IsOrgLevel = true;
			subProp.IsSubscribed = false;
			subProp.MediaCategoryWithAll = "CT";
			unsubscribe.Process(subProp, true);

			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe1, campaign.PK, campaign.G0_Category);
			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe2, campaign.PK, campaign.G0_Category);
			AssertUnsubscribedOrgContact(contactSubscriptionToUnsubscribe3, campaign.PK, "CT", "TP");

			GlbCompanyCampaignSubscriptionForOrganisationCollection orgSubs = new GlbCompanyCampaignSubscriptionForOrganisationCollection(orgHeader);
			Assert("Organization should be unsubscribed.", !orgSubs[0].GCS_IsSubscribed);
			Assert("Organization sub should be referenced.", orgSubs[0].GCS_G0 == campaign.PK);
		}

		public void TestUnsubscribeContactsWithSubscriptionProperties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = nameof(contact1);
			contact1.OC_Email = $"{contact1.OC_ContactName}@abc.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			var contactSubscriptionToUnsubscribe1 = contact1.Subscriptions.AddNew();
			contactSubscriptionToUnsubscribe1.GCS_MediaCategory = campaign.G0_Category;
			contactSubscriptionToUnsubscribe1.GCS_MediaType = campaign.G0_Type;
			contactSubscriptionToUnsubscribe1.GCS_IsSubscribed = true;

			var contactSubacriptionToKeep = contact1.Subscriptions.AddNew();
			contactSubacriptionToKeep.GCS_MediaCategory = "CAT";
			contactSubacriptionToKeep.GCS_IsSubscribed = true;

			Factory.Save();

			var unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, UnsubscribeType.MediaCategory);
			var subProp = new SubscriptionProperties();
			subProp.IsOrgLevel = false;
			subProp.IsSubscribed = false;
			subProp.MediaCategoryWithAll = "CT";
			subProp.MediaTypeWithAll = "TP";
			unsubscribe.Process(subProp, false);

			GlbCompanyCampaignSubscriptionForOrganisationContactCollection contactSubs = new GlbCompanyCampaignSubscriptionForOrganisationContactCollection(contact1);
			AssertUnsubscribedOrgContact(contactSubs[1], campaign.PK, campaign.G0_Category, campaign.G0_Type);
			AssertSubscribedOrgContact(contactSubacriptionToKeep, ZGuid.Empty, "CAT");
		}

		static void AssertSubscribedOrgContact(IGlbCompanyCampaignSubscription subscription, ZGuid campaignPk, ZString category, string type = "")
		{
			AssertEquals(campaignPk, subscription.GCS_G0);
			AssertEquals(true, subscription.GCS_IsSubscribed);
			AssertEquals(category, subscription.GCS_MediaCategory);
			AssertEquals(type, subscription.GCS_MediaType);
		}

		public void TestCompanySpecificDescriptionText()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GC = company1.PK;

			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.AlreadyUnsubscribed, company1.PK.ToGuid(), company2.PK.ToGuid(), "Already Unsubscribed");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.ResubscribeLabel, company1.PK.ToGuid(), company2.PK.ToGuid(), "Re-subscribe Label");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.ResubscribedSuccessfully, company1.PK.ToGuid(), company2.PK.ToGuid(), "Re-subscribed Successfully");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.UnsubscribedSuccessfully, company1.PK.ToGuid(), company2.PK.ToGuid(), "Unsubscribed Successfully");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferenceSuccessfullyUser, company1.PK.ToGuid(), company2.PK.ToGuid(), "Subscripition Preference Successfully User");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferencePageTitle, company1.PK.ToGuid(), company2.PK.ToGuid(), "Subscripition Preference Page Title");
			SetSubscriptionRegistryValue(OrganisationsDataRegistry.Instance.SubscripitionPreferencePageMessage, company1.PK.ToGuid(), company2.PK.ToGuid(), "Subscripition Preference Page Message");

			Factory.Save();

			var unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, "", "");

			AssertEquals("Company 1 Already Unsubscribed", unsubscribe.AlreadyUnsubscribedMessage);
			AssertEquals("Company 1 Re-subscribe Label", unsubscribe.ResubscribeLabelMessage);
			AssertEquals("Company 1 Re-subscribed Successfully", unsubscribe.ResubscribeSuccessMessage);
			AssertEquals("Company 1 Unsubscribed Successfully", unsubscribe.SuccessMessage);
			AssertEquals("Company 1 Subscripition Preference Successfully User", unsubscribe.SubscriptionPreferenceMessageUser);
			AssertEquals("Company 1 Subscripition Preference Page Title", unsubscribe.SubscripitionPreferencePageTitle);
			AssertEquals("Company 1 Subscripition Preference Page Message", unsubscribe.SubscripitionPreferencePageMessage);

			campaign.G0_GC = company2.PK;
			Factory.Save();

			unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, campaignItem.PK, "", "");
			AssertEquals("Company 2 Already Unsubscribed", unsubscribe.AlreadyUnsubscribedMessage);
			AssertEquals("Company 2 Re-subscribe Label", unsubscribe.ResubscribeLabelMessage);
			AssertEquals("Company 2 Re-subscribed Successfully", unsubscribe.ResubscribeSuccessMessage);
			AssertEquals("Company 2 Unsubscribed Successfully", unsubscribe.SuccessMessage);
			AssertEquals("Company 2 Subscripition Preference Successfully User", unsubscribe.SubscriptionPreferenceMessageUser);
			AssertEquals("Company 2 Subscripition Preference Page Title", unsubscribe.SubscripitionPreferencePageTitle);
			AssertEquals("Company 2 Subscripition Preference Page Message", unsubscribe.SubscripitionPreferencePageMessage);

			unsubscribe = new GlbCompanyCampaignUnsubscribe(Factory, Guid.Empty, "", "");
			AssertEquals("System Already Unsubscribed", unsubscribe.AlreadyUnsubscribedMessage);
			AssertEquals("System Re-subscribe Label", unsubscribe.ResubscribeLabelMessage);
			AssertEquals("System Re-subscribed Successfully", unsubscribe.ResubscribeSuccessMessage);
			AssertEquals("System Unsubscribed Successfully", unsubscribe.SuccessMessage);
			AssertEquals("System Subscripition Preference Successfully User", unsubscribe.SubscriptionPreferenceMessageUser);
			AssertEquals("System Subscripition Preference Page Title", unsubscribe.SubscripitionPreferencePageTitle);
			AssertEquals("System Subscripition Preference Page Message", unsubscribe.SubscripitionPreferencePageMessage);
		}

		void SetSubscriptionRegistryValue(MultilingualStringRegistryItem registryItem, Guid company1Pk, Guid company2Pk, string registryValue)
		{
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"System {registryValue}");
			registryItem.SetValue(company1Pk, Guid.Empty, Guid.Empty, $"Company 1 {registryValue}");
			registryItem.SetValue(company2Pk, Guid.Empty, Guid.Empty, $"Company 2 {registryValue}");
		}

		protected override BusinessObject GetNewBusinessObject() => new GlbCompanyCampaignUnsubscribe(Factory, ZGuid.BrettsGuid, "asdf", null);

		protected override void SetUp()
		{
			base.SetUp();
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmehserver.webvoting.com.au/");
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection { { "CT", (NoResString)"Category" } });
			OrganisationsDataRegistry.Instance.CampaignCategory2List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionBoolCollection { { "TP", (NoResString)"Category" } });
		}
	}
}

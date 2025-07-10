using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TransitionProgressViewModelDataAdapterTest : TestCaseWithFactory
	{
		public void TestGetTouchStatsData_LastStep()
		{
			var master = PrepareDataForStats();

			var stats = TransitionProgressViewModelDataAdapter.GetTouchStatsData(master).ToArray();
			AssertEquals(3, stats.Length);
			var stats1 = stats[0];
			var stats2 = stats[1];
			var stats3 = stats[2];

			AssertEquals(0, stats1.FailedCategoryCount);
			AssertEquals(0, stats1.FailedOtherCount);
			AssertEquals(0, stats1.NonDeliveredCount);
			AssertEquals(0, stats1.QueuedCategoryCount);
			AssertEquals(0, stats1.QueuedCount);
			AssertEquals(0, stats1.ScheduledCount);
			AssertEquals(51, stats1.SentCategoryCount);
			AssertEquals("Master", stats1.TouchName);
			AssertEquals(158, stats1.TransitionedToNextTouchCount);
			AssertEquals(0, stats1.FailedUnsubscribedCount);
			AssertEquals(0, stats1.UnverifiedCount);
			AssertEquals(0, stats1.VerifiedCount);
			AssertEquals(0, stats1.OpportunityQueuedCount);
			AssertEquals(0, stats1.OpportunityCreatedCount);

			AssertEquals(94, stats2.FailedCategoryCount);
			AssertEquals(64, stats2.FailedOtherCount);
			AssertEquals(30, stats2.NonDeliveredCount);
			AssertEquals(60, stats2.QueuedCategoryCount);
			AssertEquals(29, stats2.QueuedCount);
			AssertEquals(31, stats2.ScheduledCount);
			AssertEquals(98, stats2.SentCategoryCount);
			AssertEquals("Touch 1", stats2.TouchName);
			AssertEquals(4, stats2.TransitionedToNextTouchCount);
			AssertEquals(3, stats2.FailedUnsubscribedCount);
			AssertEquals(33, stats2.UnverifiedCount);
			AssertEquals(35, stats2.VerifiedCount);
			AssertEquals(0, stats2.OpportunityQueuedCount);
			AssertEquals(0, stats2.OpportunityCreatedCount);

			AssertEquals(21, stats3.FailedCategoryCount);
			AssertEquals(0, stats3.FailedOtherCount);
			AssertEquals(21, stats3.NonDeliveredCount);
			AssertEquals(45, stats3.QueuedCategoryCount);
			AssertEquals(22, stats3.QueuedCount);
			AssertEquals(23, stats3.ScheduledCount);
			AssertEquals(70, stats3.SentCategoryCount);
			AssertEquals("Touch 2", stats3.TouchName);
			AssertEquals(0, stats3.TransitionedToNextTouchCount);
			AssertEquals(0, stats3.FailedUnsubscribedCount);
			AssertEquals(24, stats3.UnverifiedCount);
			AssertEquals(25, stats3.VerifiedCount);
			AssertEquals(0, stats3.OpportunityQueuedCount);
			AssertEquals(0, stats3.OpportunityCreatedCount);
		}

		public void TestGetTouchStatsData()
		{
			var master = PrepareDataForStats();

			var touch3a = master.AllTouches.AddNew();
			touch3a.G0_CampaignName = "Touch 3a";
			touch3a.G0_HorizontalId = 3;
			touch3a.G0_VerticalId = "a";

			Factory.Save();

			var stats = TransitionProgressViewModelDataAdapter.GetTouchStatsData(master).ToArray();
			AssertEquals(4, stats.Length);
			var stats1 = stats[0];
			var stats2 = stats[1];
			var stats3 = stats[2];

			AssertEquals(0, stats1.FailedCategoryCount);
			AssertEquals(0, stats1.FailedOtherCount);
			AssertEquals(0, stats1.NonDeliveredCount);
			AssertEquals(0, stats1.QueuedCategoryCount);
			AssertEquals(0, stats1.QueuedCount);
			AssertEquals(0, stats1.ScheduledCount);
			AssertEquals(51, stats1.SentCategoryCount);
			AssertEquals("Master", stats1.TouchName);
			AssertEquals(158, stats1.TransitionedToNextTouchCount);
			AssertEquals(0, stats1.FailedUnsubscribedCount);
			AssertEquals(0, stats1.UnverifiedCount);
			AssertEquals(0, stats1.VerifiedCount);
			AssertEquals(0, stats1.OpportunityQueuedCount);
			AssertEquals(0, stats1.OpportunityCreatedCount);

			AssertEquals(94, stats2.FailedCategoryCount);
			AssertEquals(64, stats2.FailedOtherCount);
			AssertEquals(30, stats2.NonDeliveredCount);
			AssertEquals(60, stats2.QueuedCategoryCount);
			AssertEquals(29, stats2.QueuedCount);
			AssertEquals(31, stats2.ScheduledCount);
			AssertEquals(98, stats2.SentCategoryCount);
			AssertEquals("Touch 1", stats2.TouchName);
			AssertEquals(4, stats2.TransitionedToNextTouchCount);
			AssertEquals(3, stats2.FailedUnsubscribedCount);
			AssertEquals(33, stats2.UnverifiedCount);
			AssertEquals(35, stats2.VerifiedCount);
			AssertEquals(0, stats2.OpportunityQueuedCount);
			AssertEquals(0, stats2.OpportunityCreatedCount);

			AssertEquals(70, stats3.FailedCategoryCount);
			AssertEquals(49, stats3.FailedOtherCount);
			AssertEquals(21, stats3.NonDeliveredCount);
			AssertEquals(45, stats3.QueuedCategoryCount);
			AssertEquals(22, stats3.QueuedCount);
			AssertEquals(23, stats3.ScheduledCount);
			AssertEquals(70, stats3.SentCategoryCount);
			AssertEquals("Touch 2", stats3.TouchName);
			AssertEquals(0, stats3.TransitionedToNextTouchCount);
			AssertEquals(0, stats3.FailedUnsubscribedCount);
			AssertEquals(24, stats3.UnverifiedCount);
			AssertEquals(25, stats3.VerifiedCount);
			AssertEquals(0, stats3.OpportunityQueuedCount);
			AssertEquals(0, stats3.OpportunityCreatedCount);
		}

		GlbCompanyCampaign PrepareDataForStats()
		{
			var masterCampaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AddNewItems(masterCampaign1, "VER", 51);

			//touch 1a
			var touch1a = masterCampaign1.AllTouches.AddNew();
			touch1a.G0_CampaignName = "Touch 1a";
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "a";
			AddNewItems(touch1a, "NDR", 11);
			AddNewItems(touch1a, "QUE", 12);

			//SCH
			var items = AddNewItems(touch1a, "QUE", 13);
			Array.ForEach(items, (x) => x.G8_ScheduleTimeUtc = ZDateTime.UtcToday);

			AddNewItems(touch1a, "UNV", 14);
			var recipients = AddNewItems(touch1a, "VER", 15).Select(x => x.G8_RecipientID).ToArray();

			//UNS
			var itemNdr1 = AddNewItem(touch1a);
			itemNdr1.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			itemNdr1.RecipientAsOrgContact.OC_Email = "itemNdr1@a.com";
			UnsubscribCampaign(touch1a, itemNdr1.Recipient.Email);

			var itemNdr2 = AddNewItem(touch1a);
			itemNdr2.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			itemNdr2.RecipientAsOrgContact.OC_Email = "itemNdr2@a.com";
			UnsubscribCampaign(touch1a, itemNdr2.Recipient.Email);

			//touch 1b
			var touch1b = masterCampaign1.AllTouches.AddNew();
			touch1b.G0_CampaignName = "Touch 1a";
			touch1b.G0_HorizontalId = 1;
			touch1b.G0_VerticalId = "b";
			AddNewItems(touch1b, "NDR", 16);
			AddNewItems(touch1b, "QUE", 17);

			//SCH
			items = AddNewItems(touch1b, "QUE", 18);
			Array.ForEach(items, (x) => x.G8_ScheduleTimeUtc = ZDateTime.UtcToday);

			AddNewItems(touch1b, "UNV", 19);
			AddNewItems(touch1b, "VER", 20);

			//UNS
			var itemNdr3 = AddNewItem(touch1b);
			itemNdr3.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			itemNdr3.RecipientAsOrgContact.OC_Email = "itemNdr3@a.com";
			UnsubscribCampaign(touch1b, itemNdr3.Recipient.Email);

			//touch 2a
			var touch2a = masterCampaign1.AllTouches.AddNew();
			touch2a.G0_CampaignName = "Touch 1a";
			touch2a.G0_HorizontalId = 2;
			touch2a.G0_VerticalId = "a";
			AddNewItems(touch2a, "NDR", 21);
			AddNewItems(touch2a, "QUE", 22);

			//SCH
			items = AddNewItems(touch2a, "QUE", 23);
			Array.ForEach(items, (x) => x.G8_ScheduleTimeUtc = ZDateTime.UtcToday);

			AddNewItems(touch2a, "UNV", 24);
			var verified = AddNewItems(touch2a, "VER", 25);

			//TransitionedToNextTouchCount
			verified[0].G8_RecipientID = recipients[0];
			verified[1].G8_RecipientID = recipients[1];
			verified[2].G8_RecipientID = recipients[2];
			verified[3].G8_RecipientID = recipients[3];

			Factory.Save();
			return masterCampaign1;
		}

		public void TestGetTouchStatsData_OpportunityCreation()
		{
			var master = PrepareDataForStats_OpportunityCreation();
			var stats = TransitionProgressViewModelDataAdapter.GetTouchStatsData(master).ToArray();
			AssertEquals(2, stats.Length);

			var statsMaster = stats[0];
			var statsTouch1 = stats[1];

			AssertEquals(0, statsMaster.FailedCategoryCount);
			AssertEquals(0, statsMaster.FailedOtherCount);
			AssertEquals(0, statsMaster.NonDeliveredCount);
			AssertEquals(0, statsMaster.QueuedCategoryCount);
			AssertEquals(0, statsMaster.QueuedCount);
			AssertEquals(0, statsMaster.ScheduledCount);
			AssertEquals(0, statsMaster.SentCategoryCount);
			AssertEquals("Master", statsMaster.TouchName);
			AssertEquals(23, statsMaster.TransitionedToNextTouchCount);
			AssertEquals(0, statsMaster.FailedUnsubscribedCount);
			AssertEquals(0, statsMaster.UnverifiedCount);
			AssertEquals(0, statsMaster.VerifiedCount);
			AssertEquals(0, statsMaster.OpportunityQueuedCount);
			AssertEquals(0, statsMaster.OpportunityCreatedCount);

			AssertEquals(0, statsTouch1.FailedCategoryCount);
			AssertEquals(0, statsTouch1.FailedOtherCount);
			AssertEquals(0, statsTouch1.NonDeliveredCount);
			AssertEquals(0, statsTouch1.QueuedCategoryCount);
			AssertEquals(0, statsTouch1.QueuedCount);
			AssertEquals(0, statsTouch1.ScheduledCount);
			AssertEquals(0, statsTouch1.SentCategoryCount);
			AssertEquals("Touch 1", statsTouch1.TouchName);
			AssertEquals(0, statsTouch1.TransitionedToNextTouchCount);
			AssertEquals(0, statsTouch1.FailedUnsubscribedCount);
			AssertEquals(0, statsTouch1.UnverifiedCount);
			AssertEquals(0, statsTouch1.VerifiedCount);
			AssertEquals(15, statsTouch1.OpportunityQueuedCount);
			AssertEquals(8, statsTouch1.OpportunityCreatedCount);
		}

		GlbCompanyCampaign PrepareDataForStats_OpportunityCreation()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			//touch 1A
			var touch1A = masterCampaign.AllTouches.AddNew();
			touch1A.G0_CampaignName = "Touch 1a";
			touch1A.G0_HorizontalId = 1;
			touch1A.G0_VerticalId = "a";
			AddNewItems(touch1A, "OPQ", 15);
			AddNewItems(touch1A, "OPC", 8);

			Factory.Save();
			return masterCampaign;
		}

		GlbCompanyCampaignItem AddNewItem(GlbCompanyCampaign campaign)
		{
			return AddNewItem(campaign, ZGuid.Empty);
		}

		GlbCompanyCampaignItem AddNewItem(GlbCompanyCampaign campaign, ZGuid contactPK)
		{
			var result = campaign.CampaignsItemsSent.AddNew();
			result.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			result.G8_RecipientID = contactPK.IsEmpty ? Factory.NewWithValidTestData<OrgContact>().PK : contactPK;
			return result;
		}

		GlbCompanyCampaignItem[] AddNewItems(GlbCompanyCampaign campaign, ZString statusCode, int count)
		{
			var result = new List<GlbCompanyCampaignItem>();
			for (var index = 0; index < count; index++)
			{
				var item = AddNewItem(campaign);
				item.G8_TrackingStatus = statusCode;
				result.Add(item);
			}

			return result.ToArray();
		}

		void UnsubscribCampaign(GlbCompanyCampaign campaign, ZString email)
		{
			var campaignSubscription = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			campaignSubscription.GCS_G0 = campaign.PK;
			campaignSubscription.GCS_IsSubscribed = false;
			campaignSubscription.GCS_Email = email;
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignSummaryStatsTest : TestCaseWithFactory
	{
		public void TestSingleUnsubscribePerRecipient()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			var campaignSubscription1 = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			campaignSubscription1.GCS_G0 = helper.Touch1A.PK;
			campaignSubscription1.GCS_IsSubscribed = false;
			campaignSubscription1.GCS_Email = helper.Contact1.OC_Email;
			campaignSubscription1.GCS_MediaCategory = "";
			campaignSubscription1.GCS_MediaType = "***";

			var campaignSubscription2 = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			campaignSubscription2.GCS_G0 = helper.Touch1A.PK;
			campaignSubscription2.GCS_IsSubscribed = false;
			campaignSubscription2.GCS_Email = helper.Contact1.OC_Email;
			campaignSubscription2.GCS_MediaCategory = "***";
			campaignSubscription2.GCS_MediaType = "";

			Factory.Save();

			AssertEquals(1, helper.Master.SummaryStats.UnsubscribedCount);
		}

		public void TestHasHorizontalRecipients()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			AssertEquals(true, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact1.PK, 1));
			AssertEquals(true, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact2.PK, 1));
			AssertEquals(true, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact3.PK, 1));

			AssertEquals(false, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact1.PK, 2));
			AssertEquals(true, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact2.PK, 2));
			AssertEquals(true, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact3.PK, 2));

			AssertEquals(false, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact1.PK, 3));
			AssertEquals(false, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact2.PK, 3));
			AssertEquals(false, helper.Master.SummaryStats.HasHorizontalsRecipients(helper.Contact3.PK, 3));
		}

		public void TestHasTouchRecipients()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			AssertEquals(true, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact1.PK, 1, helper.Touch1A.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact1.PK, 1, helper.Touch1B.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact1.PK, 2, helper.Touch2A.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact1.PK, 2, helper.Touch2B.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact1.PK, 3, helper.Touch3A.PK));

			AssertEquals(true, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact2.PK, 1, helper.Touch1A.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact2.PK, 1, helper.Touch1B.PK));
			AssertEquals(true, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact2.PK, 2, helper.Touch2A.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact2.PK, 2, helper.Touch2B.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact2.PK, 3, helper.Touch3A.PK));

			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact3.PK, 1, helper.Touch1A.PK));
			AssertEquals(true, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact3.PK, 1, helper.Touch1B.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact3.PK, 2, helper.Touch2A.PK));
			AssertEquals(true, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact3.PK, 2, helper.Touch2B.PK));
			AssertEquals(false, helper.Master.SummaryStats.HasTouchRecipients(helper.Contact3.PK, 3, helper.Touch3A.PK));
		}

		public void TestLoadTransitionsAndLoadPreviousTransitions()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			GlbCompanyCampaign master1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master1.G0_CampaignName = "master 1";

			GlbCompanyCampaign master2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master2.G0_CampaignName = "master 2";

			var master1Touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master1Touch1.G0_CampaignName = "master1Touch1";
			master1Touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			master1Touch1.G0_HorizontalId = 1;
			master1Touch1.G0_VerticalId = "A";
			master1Touch1.G0_G0_Master = master1.PK;
			master1.AllTouches.Add(master1Touch1);

			var master1Touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master1Touch2a.G0_CampaignName = "master1Touch2a";
			master1Touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			master1Touch2a.G0_HorizontalId = 2;
			master1Touch2a.G0_VerticalId = "B";
			master1Touch2a.G0_G0_Master = master1.PK;
			master1.AllTouches.Add(master1Touch2a);

			var master2Touch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master2Touch1.G0_CampaignName = "master2Touch1";
			master2Touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			master2Touch1.G0_HorizontalId = 1;
			master2Touch1.G0_VerticalId = "A";
			master2Touch1.G0_G0_Master = master2.PK;
			master2.AllTouches.Add(master2Touch1);

			var master2Touch2a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master2Touch2a.G0_CampaignName = "master2Touch2a";
			master2Touch2a.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			master2Touch2a.G0_HorizontalId = 2;
			master2Touch2a.G0_VerticalId = "B";
			master2Touch2a.G0_G0_Master = master2.PK;
			master2.AllTouches.Add(master2Touch2a);

			var master1Item1 = master1Touch1.CampaignsItemsSent.AddNew();
			master1Item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			master1Item1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			master1Item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			master1Item1.G8_RecipientID = contact1.PK;

			var master1Item2 = master1Touch2a.CampaignsItemsSent.AddNew();
			master1Item2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			master1Item2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			master1Item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			master1Item2.G8_RecipientID = contact1.PK;

			var master2Item1 = master2Touch1.CampaignsItemsSent.AddNew();
			master2Item1.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			master2Item1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			master2Item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			master2Item1.G8_RecipientID = contact1.PK;

			var master2Item2 = master2Touch2a.CampaignsItemsSent.AddNew();
			master2Item2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			master2Item2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			master2Item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			master2Item2.G8_RecipientID = contact1.PK;

			Factory.Save();

			var prevTransitions = CampaignSummaryStats.LoadPreviousTransitions(new[] { master1Item1, master1Item2, master2Item1, master2Item2 });
			var master1Item1Transitions = CampaignSummaryStats.LoadTransitions(master1Item1);
			var master2Item2Transitions = CampaignSummaryStats.LoadTransitions(master2Item2);

			AssertEquals("prev item for master1Item2 is master1Item1", master1Item1.PK, prevTransitions[master1Item2.PK].CampaignItemId);
			AssertEquals("prev item for master2Item2 is master2Item1", master2Item1.PK, prevTransitions[master2Item2.PK].CampaignItemId);
			AssertEquals("prev item for master1Item1 is null", false, prevTransitions.ContainsKey(master1Item1.PK));
			AssertEquals("prev item for master2Item1 is null", false, prevTransitions.ContainsKey(master2Item1.PK));

			AssertEquals(2, master1Item1Transitions.Count);
			AssertEquals(2, master2Item2Transitions.Count);

			AssertNotNull(master1Item1Transitions.FirstOrDefault(x => x.CampaignItemId == master1Item1.PK));
			AssertNotNull(master1Item1Transitions.FirstOrDefault(x => x.CampaignItemId == master1Item2.PK));
			AssertNotNull(master2Item2Transitions.FirstOrDefault(x => x.CampaignItemId == master2Item1.PK));
			AssertNotNull(master2Item2Transitions.FirstOrDefault(x => x.CampaignItemId == master2Item2.PK));
		}

		public void TestFailed()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1 = masterCampaign.AllTouches.AddNew();
			touch1.G0_CampaignName = "Touch 11";
			touch1.G0_HorizontalId = 1;

			var itemNdr1 = AddNewItem(touch1);
			itemNdr1.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			var itemNdr2 = AddNewItem(touch1);
			itemNdr2.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			itemNdr2.RecipientAsOrgContact.OC_Email = "itemNdr2@a.com";
			UnsubscribCampaign(touch1, itemNdr2.Recipient.Email);

			var itemNdr3 = AddNewItem(touch1);
			itemNdr3.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			var item1 = AddNewItem(touch1);
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			var item2 = AddNewItem(touch1);
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var item3 = AddNewItem(touch1);
			item3.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item3.RecipientAsOrgContact.OC_Email = "item3@a.com";
			UnsubscribCampaign(touch1, item3.Recipient.Email);

			var touch2 = masterCampaign.AllTouches.AddNew();
			touch2.G0_CampaignName = "Touch 12";
			touch2.G0_HorizontalId = 2;

			var touch3 = masterCampaign.AllTouches.AddNew();
			touch3.G0_CampaignName = "Touch 13";
			touch3.G0_HorizontalId = 3;

			var itemNdr4 = AddNewItem(touch2);
			itemNdr4.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			AddNewItem(touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch2, item1.G8_RecipientID).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

			var item4 = AddNewItem(touch2, item2.G8_RecipientID);
			item4.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;

			var item5 = AddNewItem(touch2);
			item5.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			Factory.Save();

			var summaryStats1 = touch1.SummaryStats;
			var summaryStats2 = touch2.SummaryStats;

			AssertEquals("FailedCount", 4, summaryStats1.FailedToTransitionCount);
			AssertEquals("Failed", 4, summaryStats1.FailedToTransition.Count());
			AssertContainsExactElementsInAnyOrder(new[] { itemNdr1.PK, itemNdr2.PK, itemNdr3.PK, item3.PK }, summaryStats1.FailedToTransition.Select(f => f.CampaignItemId));
			var failedToTransition = summaryStats1.FailedToTransition;
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr1.PK).IsUnsubscribed);
			AssertEquals(true, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr2.PK).IsUnsubscribed);
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr3.PK).IsUnsubscribed);
			AssertEquals(true, failedToTransition.FirstOrDefault(x => x.CampaignItemId == item3.PK).IsUnsubscribed);

			AssertEquals("FailedCount", 3, summaryStats2.FailedToTransitionCount);
			AssertEquals("Failed", 3, summaryStats2.FailedToTransition.Count());
			AssertContainsExactElementsInAnyOrder(new[] { itemNdr4.PK, item4.PK, item5.PK }, summaryStats2.FailedToTransition.Select(f => f.CampaignItemId));
			failedToTransition = summaryStats2.FailedToTransition;
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr4.PK).IsUnsubscribed);
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == item4.PK).IsUnsubscribed);
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == item5.PK).IsUnsubscribed);
		}

		public void TestFailed_LastHorizontal()
		{
			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var touch1 = masterCampaign.AllTouches.AddNew();
			touch1.G0_CampaignName = "Touch 11";
			touch1.G0_HorizontalId = 1;

			var itemNdr1 = AddNewItem(touch1);
			itemNdr1.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			var itemNdr2 = AddNewItem(touch1);
			itemNdr2.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			itemNdr2.RecipientAsOrgContact.OC_Email = "itemNdr2@a.com";
			UnsubscribCampaign(touch1, itemNdr2.Recipient.Email);

			var itemNdr3 = AddNewItem(touch1);
			itemNdr3.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			var item1 = AddNewItem(touch1);
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			var item2 = AddNewItem(touch1);
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var item3 = AddNewItem(touch1);
			item3.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			item3.RecipientAsOrgContact.OC_Email = "item3@a.com";
			UnsubscribCampaign(touch1, item3.Recipient.Email);

			var touch2 = masterCampaign.AllTouches.AddNew();
			touch2.G0_CampaignName = "Touch 12";
			touch2.G0_HorizontalId = 2;

			var itemNdr4 = AddNewItem(touch2);
			itemNdr4.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;

			AddNewItem(touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(touch2, item1.G8_RecipientID).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

			var item4 = AddNewItem(touch2, item2.G8_RecipientID);
			item4.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;

			var item5 = AddNewItem(touch2);
			item5.G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			Factory.Save();

			var summaryStats1 = touch1.SummaryStats;
			var summaryStats2 = touch2.SummaryStats;

			AssertEquals("FailedCount", 4, summaryStats1.FailedToTransitionCount);
			AssertEquals("Failed", 4, summaryStats1.FailedToTransition.Count());
			AssertContainsExactElementsInAnyOrder(new[] { itemNdr1.PK, itemNdr2.PK, itemNdr3.PK, item3.PK }, summaryStats1.FailedToTransition.Select(f => f.CampaignItemId));
			var failedToTransition = summaryStats1.FailedToTransition;
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr1.PK).IsUnsubscribed);
			AssertEquals(true, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr2.PK).IsUnsubscribed);
			AssertEquals(false, failedToTransition.FirstOrDefault(x => x.CampaignItemId == itemNdr3.PK).IsUnsubscribed);
			AssertEquals(true, failedToTransition.FirstOrDefault(x => x.CampaignItemId == item3.PK).IsUnsubscribed);

			AssertEquals("FailedCount", 0, summaryStats2.FailedToTransitionCount);
			AssertEquals("Failed", 1, summaryStats2.FailedToTransition.Count());
		}

		public void TestGeneralUsage()
		{
			var masterCampaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign1Touch1 = masterCampaign1.AllTouches.AddNew();
			campaign1Touch1.G0_CampaignName = "Touch 11";
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			var item1 = AddNewItem(campaign1Touch1);
			item1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			item1.G8_ScheduleTimeUtc = ZDateTime.UtcToday;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.VER;
			AddNewItem(campaign1Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var campaignSubscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			campaignSubscription1.GCS_G0 = campaign1Touch1.PK;
			campaignSubscription1.GCS_IsSubscribed = false;
			campaignSubscription1.GCS_Email = "email1@test.com";

			var campaignSubscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			campaignSubscription2.GCS_G0 = campaign1Touch1.PK;
			campaignSubscription2.GCS_IsSubscribed = false;
			campaignSubscription2.GCS_Email = "email2@test.com";

			var campaign1Touch2 = masterCampaign1.AllTouches.AddNew();
			campaign1Touch2.G0_CampaignName = "Touch 12";
			AddNewItem(campaign1Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			AddNewItem(campaign1Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(campaign1Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			AddNewItem(campaign1Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;
			AddNewItem(campaign1Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			var masterCampaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var campaign2Touch1 = masterCampaign2.AllTouches.AddNew();
			campaign2Touch1.G0_CampaignName = "Touch 21";
			AddNewItem(campaign2Touch1).G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;

			var campaign2Touch2 = masterCampaign2.AllTouches.AddNew();
			campaign2Touch2.G0_CampaignName = "Touch 22";
			AddNewItem(campaign2Touch2).G8_TrackingStatus = TrackingStatusCodes.Codes.VER;

			Factory.Save();

			var summaryStats = CampaignSummaryStats.Load(masterCampaign1.PK, masterCampaign1.Horizontals.Max(h => h.Id));
			CombineAssertions("Summary stats for masterCampaign1 should be sum of all touches", () =>
			{
				AssertEquals("VerifiedCount", 2 + 1, summaryStats.VerifiedCount);
				AssertEquals("UnverifiedCount", 1 + 1, summaryStats.UnverifiedCount);
				AssertEquals("NonDeliveredCount", 3 + 1, summaryStats.NonDeliveredCount);
				AssertEquals("QueuedCount", 3 + 2, summaryStats.QueuedCount);
				AssertEquals("ScheduledCount", 1, summaryStats.ScheduledCount);
				AssertEquals("UnScheduledCount", 4, summaryStats.UnScheduledCount);
				AssertEquals("UnsubscribedCount", 2, summaryStats.UnsubscribedCount);
				AssertEquals("BlockedQueuedCount", 3 + 2, summaryStats.BlockedQueuedCount);
				AssertEquals("SentCount", 6 + 3, summaryStats.SentCount);
				AssertEquals("TotalCount", 3 + 2 + 6 + 3, summaryStats.TotalCount);
			});

			var subSummary1 = summaryStats.GetSubSummary(campaign1Touch1);
			CombineAssertions("Summary stats for campaign1 touch1", () =>
			{
				AssertEquals("VerifiedCount", 2, subSummary1.VerifiedCount);
				AssertEquals("UnverifiedCount", 1, subSummary1.UnverifiedCount);
				AssertEquals("NonDeliveredCount", 3, subSummary1.NonDeliveredCount);
				AssertEquals("QueuedCount", 3, subSummary1.QueuedCount);
				AssertEquals("ScheduledCount", 1, subSummary1.ScheduledCount);
				AssertEquals("UnScheduledCount", 2, subSummary1.UnScheduledCount);
				AssertEquals("UnsubscribedCount", 2, subSummary1.UnsubscribedCount);
				AssertEquals("BlockedQueuedCount", 3, subSummary1.BlockedQueuedCount);
				AssertEquals("SentCount", 6, subSummary1.SentCount);
				AssertEquals("TotalCount", 6 + 3, subSummary1.TotalCount);
			});

			var subSummary2 = summaryStats.GetSubSummary(campaign1Touch2);
			CombineAssertions("Summary stats for campaign1 touch2", () =>
			{
				AssertEquals("VerifiedCount", 1, subSummary2.VerifiedCount);
				AssertEquals("UnverifiedCount", 1, subSummary2.UnverifiedCount);
				AssertEquals("NonDeliveredCount", 1, subSummary2.NonDeliveredCount);
				AssertEquals("QueuedCount", 2, subSummary2.QueuedCount);
				AssertEquals("ScheduledCount", 0, subSummary2.ScheduledCount);
				AssertEquals("UnScheduledCount", 2, subSummary2.UnScheduledCount);
				AssertEquals("UnsubscribedCount", 0, subSummary2.UnsubscribedCount);
				AssertEquals("BlockedQueuedCount", 2, subSummary2.BlockedQueuedCount);
				AssertEquals("SentCount", 3, subSummary2.SentCount);
				AssertEquals("TotalCount", 3 + 2, subSummary2.TotalCount);
			});
		}

		public void TestOpportunityCreation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "AA@gmail.com";
			contact1.OC_ContactName = "AA";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "BB@gmail.com";
			contact2.OC_ContactName = "BB";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_Email = "CC@gmail.com";
			contact3.OC_ContactName = "CC";

			var masterCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			masterCampaign.G0_CampaignName = "master 1";

			var masterTouch1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterTouch1.G0_CampaignName = "masterTouch1";
			masterTouch1.HtmlDocumentBlob = ZBlob.FromAscii("Test");
			masterTouch1.G0_HorizontalId = 1;
			masterTouch1.G0_VerticalId = "A";
			masterTouch1.G0_G0_Master = masterCampaign.PK;
			masterCampaign.AllTouches.Add(masterTouch1);

			var masterTouch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			masterTouch2.G0_CampaignName = "masterTouch2a";
			masterTouch2.HtmlDocumentBlob = ZBlob.FromAscii("Test");
			masterTouch2.G0_HorizontalId = 2;
			masterTouch2.G0_VerticalId = "B";
			masterTouch2.G0_G0_Master = masterCampaign.PK;
			masterCampaign.AllTouches.Add(masterTouch2);

			var masterItem1 = masterTouch1.CampaignsItemsSent.AddNew();
			masterItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;
			masterItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			masterItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			masterItem1.G8_RecipientID = contact1.PK;

			var masterItem2 = masterTouch2.CampaignsItemsSent.AddNew();
			masterItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;
			masterItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			masterItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			masterItem2.G8_RecipientID = contact1.PK;

			var masterItem3 = masterTouch2.CampaignsItemsSent.AddNew();
			masterItem3.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;
			masterItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			masterItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			masterItem3.G8_RecipientID = contact2.PK;

			var masterItem4 = masterTouch2.CampaignsItemsSent.AddNew();
			masterItem4.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;
			masterItem4.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			masterItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			masterItem4.G8_RecipientID = contact3.PK;

			Factory.Save();

			var summaryStats = CampaignSummaryStats.Load(masterCampaign.PK, masterCampaign.Horizontals.Max(h => h.Id));
			CombineAssertions("Summary stats for masterCampaign should be sum of all touches", () =>
			{
				AssertEquals("TotalCount", 4, summaryStats.TotalCount);
				AssertEquals("VerifiedCount", 0, summaryStats.VerifiedCount);
				AssertEquals("UnverifiedCount", 0, summaryStats.UnverifiedCount);
				AssertEquals("OpportunityQueuedCount", 1, summaryStats.OpportunityQueuedCount);
				AssertEquals("OpportunityCreatedCount", 3, summaryStats.OpportunityCreatedCount);
				AssertEquals("FailedToTransitionCount", 0, summaryStats.FailedToTransitionCount);
			});
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

		void UnsubscribCampaign(GlbCompanyCampaign campaign, ZString email)
		{
			var campaignSubscription = Factory.NewWithValidTestData<GlbCompanyCampaignSubscription>();
			campaignSubscription.GCS_G0 = campaign.PK;
			campaignSubscription.GCS_IsSubscribed = false;
			campaignSubscription.GCS_Email = email;
		}
	}
}

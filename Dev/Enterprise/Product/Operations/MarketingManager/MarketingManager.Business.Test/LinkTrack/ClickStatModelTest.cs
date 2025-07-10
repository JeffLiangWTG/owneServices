using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ClickStatModel))]
	sealed class ClickStatModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadClicks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";

			var clickTime = ZDateTime.UtcNow;

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link1.PK;
			click12.GCC_ClickTimeUtc = clickTime;

			Factory.Save();

			ClickStatModel statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;
			statModel.LoadClicks();

			AssertEquals(2, statModel.LinkClicks.Count);

			AssertEquals("AAA", statModel.LinkClicks[0].Context);
			AssertEquals(2, statModel.LinkClicks[0].Clicks);
			AssertEquals(true, statModel.LinkClicks[0].ViewInChart);

			AssertEquals("BBB", statModel.LinkClicks[1].Context);
			AssertEquals(0, statModel.LinkClicks[1].Clicks);
			AssertEquals(false, statModel.LinkClicks[1].ViewInChart);
		}

		public void TestLoadClicks_TimeInterval()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ClickStatModel statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;

			ZDateTime from = new ZDateTime(2014, 10, 3, 0, 0, 0);

			AssertTimeInterval(statModel, from, from.AddMinutes(10), 10, new TimeSpan(0, 1, 0));
			AssertTimeInterval(statModel, from, from.AddMinutes(30), 15, new TimeSpan(0, 2, 0));
			AssertTimeInterval(statModel, from, from.AddHours(1), 12, new TimeSpan(0, 5, 0));
			AssertTimeInterval(statModel, from, from.AddHours(3), 18, new TimeSpan(0, 10, 0));
			AssertTimeInterval(statModel, from, from.AddHours(5), 20, new TimeSpan(0, 15, 0));
			AssertTimeInterval(statModel, from, from.AddHours(10), 20, new TimeSpan(0, 30, 0));
			AssertTimeInterval(statModel, from, from.AddDays(1), 24, new TimeSpan(1, 0, 0));
			AssertTimeInterval(statModel, from, from.AddDays(2), 24, new TimeSpan(2, 0, 0));
			AssertTimeInterval(statModel, from, from.AddDays(3), 18, new TimeSpan(4, 0, 0));
			AssertTimeInterval(statModel, from, from.AddDays(5), 20, new TimeSpan(6, 0, 0));
			AssertTimeInterval(statModel, from, from.AddDays(7), 28, new TimeSpan(6, 0, 0));
			AssertTimeInterval(statModel, from, from.AddDays(30), 30, new TimeSpan(24, 0, 0));
		}

		public void TestLoadClicks_UniqueClicks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webmail.org";
			link.GCL_Context = "A Mail";

			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://webmail.org";
			link2.GCL_Context = "AAA";

			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_URL = "http://webmail.org";
			link3.GCL_Context = "BBB";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();

			var clickTime = ZDateTime.UtcNow;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_ClickTimeUtc = clickTime;

			var campaignClick2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick2.GCC_GCL = link.PK;
			campaignClick2.GCC_G8_Recipient = campaignItem.PK;
			campaignClick2.GCC_ClickTimeUtc = clickTime;

			var campaignClick3 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick3.GCC_GCL = link2.PK;
			campaignClick3.GCC_G8_Recipient = campaignItem.PK;
			campaignClick3.GCC_ClickTimeUtc = clickTime;

			var clickTime2 = ZDateTime.UtcNow.AddDays(14);

			var campaignClick4 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick4.GCC_GCL = link3.PK;
			campaignClick4.GCC_G8_Recipient = campaignItem.PK;
			campaignClick4.GCC_ClickTimeUtc = clickTime2;

			Factory.Save();

			ClickStatModel statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;
			statModel.LoadClicks();

			AssertEquals(3, statModel.LinkClicks.Count);

			AssertEquals("A Mail", statModel.LinkClicks[0].Context);
			AssertEquals(2, statModel.LinkClicks[0].Clicks);
			AssertEquals(1, statModel.LinkClicks[0].UniqueClicks);

			statModel.ReportBy = ReportByList.Codes.Url;

			AssertEquals(1, statModel.LinkClicks[0].UniqueClicks);
		}

		public void TestLinkClicks_NoErrorOnReload()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webmail.org";
			link.GCL_Context = "A Mail";

			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://webmail.org";
			link2.GCL_Context = "AAA";

			Factory.Save();

			var statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;
			AssertEquals("Pre-condition", 2, statModel.LinkClicks.Count);

			statModel.ReportBy = ReportByList.Codes.Url;
			AssertNoExceptionThrown(() => statModel.ReloadLinksAndClicks());

			AssertEquals("LinkClicks should only have 1 value", 1, statModel.LinkClicks.Count);
		}

		public void TestLinkClicks_DoesNotIncludeRegistryTrackingImageLink()
		{
			var testHelper = new TrackingImageLinkTestHelper(Factory);

			AssertEquals(1, testHelper.StatModel.LinkClicks.Count);

			AssertEquals("A Mail", testHelper.StatModel.LinkClicks[0].Context);
			AssertEquals(2, testHelper.StatModel.LinkClicks[0].Clicks);

			testHelper.StatModel.ReportBy = ReportByList.Codes.Url;

			AssertEquals(1, testHelper.StatModel.LinkClicks.Count);
		}

		public void TestLoadClicks_ActualDateTimeRange()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ClickStatModel statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;

			AssertDateTimeRange(statModel,
				new ZDateTime(2014, 10, 3, 10, 28, 0), new ZDateTime(2014, 10, 3, 12, 48, 0),
				new ZDateTime(2014, 10, 3, 10, 20, 0), new ZDateTime(2014, 10, 3, 12, 50, 0),
				15, new TimeSpan(0, 10, 0));

			AssertDateTimeRange(statModel,
				new ZDateTime(2014, 10, 3, 10, 28, 0), new ZDateTime(2014, 10, 4, 12, 48, 0),
				new ZDateTime(2014, 10, 3, 10, 0, 0), new ZDateTime(2014, 10, 4, 14, 0, 0),
				14, new TimeSpan(2, 0, 0));

			AssertDateTimeRange(statModel,
				new ZDateTime(2014, 10, 3, 10, 28, 0), new ZDateTime(2014, 11, 3, 12, 48, 0),
				new ZDateTime(2014, 10, 2, 0, 0, 0), new ZDateTime(2014, 11, 4, 0, 0, 0),
				16, new TimeSpan(48, 0, 0));
		}

		[TestDate(2021, 02, 15)]
		public void TestReportTimeRange_UpdateDates()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var unsentCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_SystemCreateTimeUtc = new ZDateTime(2020, 05, 20);
			Factory.Save();

			var statModel = new ClickStatModel(campaign);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), false, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.LastSevenDays, new ZDateTime(2021, 02, 09), new ZDateTime(2021, 02, 16), false, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.LastMonth, new ZDateTime(2021, 01, 16), new ZDateTime(2021, 02, 16), false, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.Specific, new ZDateTime(2020, 05, 20), ZDateTime.Empty, false, false);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.LifeTime, ZDateTime.Empty, ZDateTime.Empty, false, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.SixHours, new ZDateTime(2020, 05, 20), new ZDateTime(2020, 05, 20, 6, 0, 0), true, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.TwentyFourHours, new ZDateTime(2020, 05, 20), new ZDateTime(2020, 05, 21), true, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.SevenDays, new ZDateTime(2020, 05, 20), new ZDateTime(2020, 05, 27), true, true);
			AssertFieldsOnReportTimeRangeChange(statModel, ReportTimeRangeList.Codes.OneMonth, new ZDateTime(2020, 05, 20), new ZDateTime(2020, 06, 20), true, true);

			var unsentStatModel = new ClickStatModel(unsentCampaign);
			AssertFieldsOnReportTimeRangeChange(unsentStatModel, ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), false, true);
			AssertFieldsOnReportTimeRangeChange(unsentStatModel, ReportTimeRangeList.Codes.SixHours, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), true, true);
			AssertFieldsOnReportTimeRangeChange(unsentStatModel, ReportTimeRangeList.Codes.TwentyFourHours, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), true, true);
			AssertFieldsOnReportTimeRangeChange(unsentStatModel, ReportTimeRangeList.Codes.SevenDays, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), true, true);
			AssertFieldsOnReportTimeRangeChange(unsentStatModel, ReportTimeRangeList.Codes.OneMonth, new ZDateTime(2021, 02, 15), new ZDateTime(2021, 02, 16), true, true);
		}

		[TestDate(2021, 02, 15)]
		public void TestDefaultReportTimeRange()
		{
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.SixHours, new ZDateTime(2021, 02, 14, 18, 0, 0), CampaignTypeList.Codes.Broadcast);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.TwentyFourHours, new ZDateTime(2021, 02, 14), CampaignTypeList.Codes.Broadcast);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.SevenDays, new ZDateTime(2021, 02, 08), CampaignTypeList.Codes.Broadcast);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.OneMonth, new ZDateTime(2021, 01, 15), CampaignTypeList.Codes.Broadcast);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.LifeTime, new ZDateTime(2021, 01, 01), CampaignTypeList.Codes.Broadcast);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 02, 14, 18, 0, 0), CampaignTypeList.Codes.LinkTracking);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 02, 14), CampaignTypeList.Codes.LinkTracking);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 02, 08), CampaignTypeList.Codes.LinkTracking);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 01, 15), CampaignTypeList.Codes.LinkTracking);
			AssertDefaultReportTimeRange(ReportTimeRangeList.Codes.Today, new ZDateTime(2021, 01, 01), CampaignTypeList.Codes.LinkTracking);
		}

		void AssertTimeInterval(ClickStatModel statModel, ZDateTime from, ZDateTime to, int intervalCount, TimeSpan intervalTimeSpan)
		{
			statModel.FromDateTime = from;
			statModel.ToDateTime = to;
			statModel.LoadClicks();
			CombineAssertions(() =>
			{
				AssertEquals("Interval count should be", intervalCount, statModel.ClicksTimeIntervalCount);
				AssertEquals("Interval time span should be", intervalTimeSpan, statModel.TimeSpanPerInterval);
			});
		}

		void AssertDateTimeRange(ClickStatModel statModel, ZDateTime from, ZDateTime to, ZDateTime actualStart, ZDateTime actualEnd, int intervalCount, TimeSpan intervalTimeSpan)
		{
			statModel.FromDateTime = from;
			statModel.ToDateTime = to;
			statModel.LoadClicks();

			CombineAssertions(() =>
			{
				AssertEquals("Actual start time should be", actualStart, statModel.ActualStartTime);
				AssertEquals("Actual end time should be", actualEnd, statModel.ActualEndTime);
				AssertEquals("Interval count should be", intervalCount, statModel.ClicksTimeIntervalCount);
				AssertEquals("Interval time span should be", intervalTimeSpan, statModel.TimeSpanPerInterval);
			});
		}

		void AssertFieldsOnReportTimeRangeChange(ClickStatModel statModel, ZString timeRange, ZDateTime expectedFrom, ZDateTime expectedTo, bool rangeFromCampaignSent, bool isFromDateReadOnly)
		{
			statModel.ReportTimeRange = timeRange;
			CombineAssertions(() =>
			{
				AssertEquals("From date time should be", expectedFrom, statModel.FromDateTime);
				AssertEquals("To date time should be", expectedTo, statModel.ToDateTime);
				AssertEquals("IsReportTimeRangeStartsFromCampaignSent should be", rangeFromCampaignSent, statModel.IsReportTimeRangeStartsFromCampaignSent);
				AssertEquals("FromDateTime_ReadOnly should be", isFromDateReadOnly, statModel.FromDateTime_ReadOnly);
			});
		}

		void AssertDefaultReportTimeRange(ZString expectedTimeRange, ZDateTime sentTime, ZString campaignType)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = campaignType;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			campaignItem.G8_SystemCreateTimeUtc = sentTime;
			Factory.Save();

			var statModel = new ClickStatModel(campaign);
			AssertEquals("Report time range should be", expectedTimeRange, statModel.ReportTimeRange);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			return new ClickStatModel(campaign);
		}
	}
}

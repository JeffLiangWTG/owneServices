using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignItemClickStatisticsTest : TestCaseWithFactory
	{
		[TestDate(2014, 7, 23, 18, 0, 0)]
		public void TestLoadClickCounts()
		{
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = ZGuid.NewZGuid();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem2 = campaign2.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = ZGuid.NewZGuid();

			var link1 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			var link2 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			var link3 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link1.GCL_G0_Campaign = campaign1.PK;
			link2.GCL_G0_Campaign = campaign1.PK;
			link3.GCL_G0_Campaign = campaign2.PK;
			link1.GCL_URL = "http://a.com";
			link2.GCL_URL = "http://b.com.au";
			link3.GCL_URL = "http://a.com";

			var time1 = ZDateTime.Now;
			var time = time1;
			int totalCampaign1Clicks = 0;
			for (int i = 0; i < 10; ++i)
			{
				if ((i % 2) == 0) // 0, 2, 4, 7, 8 (skip 1, 3, 5, 7, 9)
				{
					CreateManyClicks(link1, time, campaignItem1, i + 3);
					totalCampaign1Clicks += i + 3;
				}
				if ((i % 3) != 0) // 1, 2, 4, 5, 7, 8 (skip 0, 3, 6, 9)
				{
					CreateManyClicks(link2, time, campaignItem1, i + 1);
					totalCampaign1Clicks += i + 1;
				}
				if (i < 5) // 0..4
				{
					CreateManyClicks(link3, time, campaignItem1, i + 2);
				}
				time = time.AddHours(1);
			}
			CreateClick(link1, time, campaignItem1);
			++totalCampaign1Clicks;

			Factory.Save();

			var clickRanges = new List<CampaignItemClicks>();
			CampaignItemClickStatistics.LoadClickCounts(clickRanges, campaignItem1.PK, ReportByList.Codes.Context);
			AssertEquals(2, clickRanges.Count);
			CampaignItemClickStatistics.LoadClickCounts(clickRanges, campaignItem1.PK, ReportByList.Codes.Url);
			AssertEquals(2, clickRanges.Count);

			CampaignItemClickStatistics.LoadClickCounts(clickRanges, campaignItem2.PK, ReportByList.Codes.Context);
			AssertEquals(0, clickRanges.Count);
			CampaignItemClickStatistics.LoadClickCounts(clickRanges, campaignItem2.PK, ReportByList.Codes.Url);
			AssertEquals(0, clickRanges.Count);
		}

		void CreateManyClicks(GlbCompanyCampaignLink link, ZDateTime clickTimeUtc, GlbCompanyCampaignItem campaignItem, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				CreateClick(link, clickTimeUtc, campaignItem);
				clickTimeUtc = clickTimeUtc.AddDays(1);
			}
		}

		GlbCompanyCampaignClick CreateClick(GlbCompanyCampaignLink link, ZDateTime clickTimeUtc, GlbCompanyCampaignItem campaignItem)
		{
			var click = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click.GCC_GCL = link.PK;
			click.GCC_ClickTimeUtc = clickTimeUtc;
			click.GCC_G8_Recipient = campaignItem.PK;
			return click;
		}
	}
}

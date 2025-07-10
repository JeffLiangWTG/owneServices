using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TrackingStatisticsModelTest : TestCaseWithFactory
	{
		[TestDate(2014, 7, 24)]
		public void TestLoadClickCounts()
		{
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var link1 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			var link2 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			var link3 = Factory.NewWithValidTestData<GlbCompanyCampaignLink>();
			link1.GCL_G0_Campaign = campaign1.PK;
			link2.GCL_G0_Campaign = campaign1.PK;
			link3.GCL_G0_Campaign = campaign2.PK;

			var time1 = new ZDateTime(2014, 7, 23, 18, 0, 0);
			var time = time1;
			int totalCampaign1Clicks = 0;
			for (int i = 0; i < 10; ++i)
			{
				if ((i % 2) == 0) // 0, 2, 4, 7, 8 (skip 1, 3, 5, 7, 9)
				{
					CreateManyClicks(link1, time, i + 3);
					totalCampaign1Clicks += i + 3;
				}
				if ((i % 3) != 0) // 1, 2, 4, 5, 7, 8 (skip 0, 3, 6, 9)
				{
					CreateManyClicks(link2, time, i + 1);
					totalCampaign1Clicks += i + 1;
				}
				if (i < 5) // 0..4
				{
					CreateManyClicks(link3, time, i + 2);
				}
				time = time.AddHours(1);
			}
			CreateClick(link1, time);
			++totalCampaign1Clicks;

			Factory.Save();

			var clickRanges = new List<CampaignClicksPerInterval>();
			TrackingStatisticsModel.LoadClickCounts(clickRanges, campaign1.PK, time1, time1.AddHours(10), 10, new List<CampaignClicksByUrl>());

			AssertEquals("First opened + Click", 12, clickRanges.Count);
			time = time1;
			for (int i = 0; i < 10; ++i)
			{
				if ((i % 2) == 0) // 0, 2, 4, 7, 8 (skip 1, 3, 5, 7, 9)
				{
					var clickInfo = clickRanges.FirstOrDefault(x => x.LinkPk == link1.PK && x.IntervalIndex == i);
					AssertEquals("link1 interval " + i, i + 3, clickInfo.ClickCount);
					AssertEquals(time, clickInfo.StartDateInclusive);
					AssertEquals(time.AddHours(1), clickInfo.EndDateExclusive);
				}
				if ((i % 3) != 0) // 1, 2, 4, 5, 7, 8 (skip 0, 3, 6, 9)
				{
					var clickInfo = clickRanges.FirstOrDefault(x => x.LinkPk == link2.PK && x.IntervalIndex == i);
					AssertEquals("link2 interval " + i, i + 1, clickInfo.ClickCount);
					AssertEquals(time, clickInfo.StartDateInclusive);
					AssertEquals(time.AddHours(1), clickInfo.EndDateExclusive);
				}
				time = time.AddHours(1);
			}

			// start time empty
			TrackingStatisticsModel.LoadClickCounts(clickRanges, campaign1.PK, ZDateTime.Empty, time1.AddHours(10), 10, new List<CampaignClicksByUrl>());
			AssertEquals(12, clickRanges.Count);
			AssertEquals(time1, clickRanges[0].StartDateInclusive);

			// end time empty
			TrackingStatisticsModel.LoadClickCounts(clickRanges, campaign1.PK, time1, ZDateTime.Empty, 10, new List<CampaignClicksByUrl>());
			AssertEquals(time1.AddHours(10).AddMinutes(1), clickRanges[clickRanges.Count - 1].EndDateExclusive);
			AssertEquals(totalCampaign1Clicks, clickRanges.Where(x => !x.LinkPk.IsEmpty).Sum(x => x.ClickCount));

			// start and end time empty
			TrackingStatisticsModel.LoadClickCounts(clickRanges, campaign1.PK, ZDateTime.Empty, ZDateTime.Empty, 10, new List<CampaignClicksByUrl>());
			AssertEquals(time1, clickRanges[0].StartDateInclusive);
			AssertEquals(time1.AddHours(10).AddMinutes(1), clickRanges[clickRanges.Count - 1].EndDateExclusive);
			AssertEquals(totalCampaign1Clicks, clickRanges.Where(x => !x.LinkPk.IsEmpty).Sum(x => x.ClickCount));

			// different interval count
			TrackingStatisticsModel.LoadClickCounts(clickRanges, campaign1.PK, ZDateTime.Empty, ZDateTime.Empty, 1, new List<CampaignClicksByUrl>());
			AssertEquals(3, clickRanges.Count);
			AssertEquals(totalCampaign1Clicks, clickRanges.Where(x => !x.LinkPk.IsEmpty).Sum(x => x.ClickCount));
		}

		void CreateManyClicks(GlbCompanyCampaignLink link, ZDateTime clickTimeUtc, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				CreateClick(link, clickTimeUtc);
				clickTimeUtc = clickTimeUtc.AddMinutes(1);
			}
		}

		GlbCompanyCampaignClick CreateClick(GlbCompanyCampaignLink link, ZDateTime clickTimeUtc)
		{
			var click = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click.GCC_GCL = link.PK;
			click.GCC_ClickTimeUtc = clickTimeUtc;
			return click;
		}
	}
}

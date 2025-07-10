using System.Collections.Generic;
using System.Linq;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	public static class TransitionProgressViewModelDataAdapter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static IEnumerable<TouchStats> GetTouchStatsData(GlbCompanyCampaign companyCampaign)
		{
			if (companyCampaign == null)
			{
				return Enumerable.Empty<TouchStats>();
			}

			var touchStatsList = new List<TouchStats>();
			var masterList = new TouchStats();
			var touch1Total = companyCampaign.Horizontals.FirstOrDefault()?.Campaigns.Sum(x => x.SummaryStats.TotalCount);

			masterList.TouchName = Res.GetString("TransitionProgressViewModel|MasterTouchName", "Master");
			masterList.TransitionedToNextTouchCount = touch1Total ?? 0;
			masterList.SentCategoryCount = companyCampaign.CampaignsItemsSent.Count;
			touchStatsList.Add(masterList);

			foreach (var campaignHorizontal in companyCampaign.Horizontals)
			{
				var summaries = campaignHorizontal.Campaigns.Select(x => x.SummaryStats).ToArray();
				var touchStats = new TouchStats();

				touchStats.TouchName = campaignHorizontal.Name;
				touchStats.TransitionedToNextTouchCount = summaries.Sum(x => x.TransitionedToNextTouchCount);

				//sent group
				touchStats.UnverifiedCount = summaries.Sum(x => x.UnverifiedCount);
				touchStats.VerifiedCount = summaries.Sum(x => x.VerifiedCount);
				touchStats.NonDeliveredCount = summaries.Sum(x => x.NonDeliveredCount);
				touchStats.SentCategoryCount = touchStats.UnverifiedCount + touchStats.VerifiedCount + touchStats.NonDeliveredCount;

				//queued group
				touchStats.ScheduledCount = summaries.Sum(x => x.ScheduledCount);
				touchStats.QueuedCount = summaries.Sum(x => x.QueuedCount) - touchStats.ScheduledCount;
				touchStats.QueuedCategoryCount = touchStats.QueuedCount + touchStats.ScheduledCount;

				//failed group
				touchStats.FailedUnsubscribedCount = summaries.Sum(x => x.FailedToTransition.Count(f => f.IsUnsubscribed));
				touchStats.FailedNDRCount = summaries.Sum(x => x.FailedToTransition.Count(f => !f.IsUnsubscribed && f.TrackingStatus == TrackingStatusCodes.Codes.NDR));
				touchStats.FailedOtherCount = summaries.Sum(x => x.FailedToTransition.Count()) - touchStats.FailedUnsubscribedCount - touchStats.FailedNDRCount;
				touchStats.FailedCategoryCount = touchStats.FailedUnsubscribedCount + touchStats.FailedNDRCount + touchStats.FailedOtherCount;

				//opportunity group
				touchStats.OpportunityQueuedCount = summaries.Sum(x => x.OpportunityQueuedCount);
				touchStats.OpportunityCreatedCount = summaries.Sum(x => x.OpportunityCreatedCount);
				touchStatsList.Add(touchStats);
			}

			return touchStatsList;
		}

		public class TouchStats
		{
			//basic info
			public string TouchName { get; set; }
			public int TransitionedToNextTouchCount { get; set; }

			//sent group
			public int UnverifiedCount { get; set; }
			public int VerifiedCount { get; set; }
			public int NonDeliveredCount { get; set; }
			public int SentCategoryCount { get; set; }

			//queued group
			public int QueuedCount { get; set; }
			public int ScheduledCount { get; set; }
			public int QueuedCategoryCount { get; set; }

			//failed group
			public int FailedUnsubscribedCount { get; set; }
			public int FailedNDRCount { get; set; }
			public int FailedOtherCount { get; set; }
			public int FailedCategoryCount { get; set; }

			//opportunity group
			public int OpportunityQueuedCount { get; set; }
			public int OpportunityCreatedCount { get; set; }
		}
	}
}

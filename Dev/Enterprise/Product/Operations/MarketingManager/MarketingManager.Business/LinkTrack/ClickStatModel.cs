using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class ClickStatModel : NonPersistentBusinessObject
	{
		public ClickStatModel(GlbCompanyCampaign campaign)
			: base(campaign.Factory)
		{
			this.Campaign = campaign;
			SetDefaultReportTimeRange();
		}
		readonly GlbCompanyCampaign Campaign;

		public static class Schema
		{
			public const string ReportBy = "ReportBy";
			public const string ReportTimeRange = "ReportTimeRange";
			public const string FromDateTime = "FromDateTime";
			public const string ToDateTime = "ToDateTime";
		}

		#region Properties

		#region Tracked Links

		public GlbCompanyCampaignLinkCollection CampaignTrackedLinks
		{
			get { return Campaign.TrackedLinks; }
		}

		#endregion

		#region ReportBy

		[List("LinkTrackReportByList")]
		public ZString ReportBy
		{
			get { return reportBy; }
			set
			{
				if (value != reportBy)
				{
					reportBy = value;
					ReloadLinksAndClicks();
					ReportByInfo.RefreshBinding();
				}
			}
		}
		ZString reportBy;

		public ZPropertyInfo ReportByInfo
		{
			get { return GetZPropertyInfo(Schema.ReportBy); }
		}

		public CodeDescriptionPairList LinkTrackReportByList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("ClickStatModel.LinkTrackReportByList",
					() => { return new ReportByList(); });
			}
		}

		#endregion

		#region ReportTimeRange

		[List("LinkTrackReportTimeRangeList")]
		public ZString ReportTimeRange
		{
			get { return reportTimeRange; }
			set
			{
				reportTimeRange = value;
				UpdateDateTimeRange();
				ReloadLinksAndClicks();
				ReportTimeRangeInfo.RefreshBinding();
				FromDateTimeInfo.RefreshBinding();
				ToDateTimeInfo.RefreshBinding();
			}
		}

		ZString reportTimeRange;

		public ZPropertyInfo ReportTimeRangeInfo
		{
			get { return GetZPropertyInfo(Schema.ReportBy); }
		}

		public CodeDescriptionPairList LinkTrackReportTimeRangeList
		{
			get
			{
				return Factory.GetCachedValue("ClickStatModel.ReportTimeRangeList:" + Campaign.IsLinkTrackCampaign.ToString(),
					() =>
					{
						var result = new CodeDescriptionPairList();

						if (!Campaign.IsLinkTrackCampaign)
						{
							result.AddPair(ReportTimeRangeList.Codes.SixHours, ReportTimeRangeList.Descriptions.SixHours);
							result.AddPair(ReportTimeRangeList.Codes.TwentyFourHours, ReportTimeRangeList.Descriptions.TwentyFourHours);
							result.AddPair(ReportTimeRangeList.Codes.SevenDays, ReportTimeRangeList.Descriptions.SevenDays);
							result.AddPair(ReportTimeRangeList.Codes.OneMonth, ReportTimeRangeList.Descriptions.OneMonth);
							result.AddPair(ReportTimeRangeList.Codes.LifeTime, ReportTimeRangeList.Descriptions.LifeTime);
						}
						result.AddPair(ReportTimeRangeList.Codes.Today, ReportTimeRangeList.Descriptions.Today);
						result.AddPair(ReportTimeRangeList.Codes.LastSevenDays, ReportTimeRangeList.Descriptions.LastSevenDays);
						result.AddPair(ReportTimeRangeList.Codes.LastMonth, ReportTimeRangeList.Descriptions.LastMonth);
						result.AddPair(ReportTimeRangeList.Codes.Specific, ReportTimeRangeList.Descriptions.Specific);

						return result;
					});
			}
		}

		void UpdateDateTimeRange()
		{
			var firstSentTime = FirstCampaignItemSentTimeLocal;

			if (ReportTimeRange == ReportTimeRangeList.Codes.SixHours && firstSentTime.IsValid)
			{
				fromDateTime = GetRoundedTime(firstSentTime, 15, false);
				toDateTime = fromDateTime.AddHours(6);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.TwentyFourHours && firstSentTime.IsValid)
			{
				fromDateTime = new ZDateTime(firstSentTime.Year, firstSentTime.Month, firstSentTime.Day, firstSentTime.Hour, 0, 0);
				toDateTime = fromDateTime.AddDays(1);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.SevenDays && firstSentTime.IsValid)
			{
				fromDateTime = GetRoundedTime(firstSentTime, 360, false);
				toDateTime = fromDateTime.AddDays(7);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.OneMonth && firstSentTime.IsValid)
			{
				fromDateTime = firstSentTime.Date;
				toDateTime = fromDateTime.AddMonths(1);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.Today)
			{
				fromDateTime = ZDateTime.Today;
				toDateTime = fromDateTime.AddDays(1);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.LastSevenDays)
			{
				toDateTime = ZDateTime.Today.AddDays(1);
				fromDateTime = toDateTime.AddDays(-7);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.LastMonth)
			{
				toDateTime = ZDateTime.Today.AddDays(1);
				fromDateTime = toDateTime.AddMonths(-1);
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.Specific)
			{
				fromDateTime = firstSentTime;
				toDateTime = ZDateTime.Empty;
			}
			else if (ReportTimeRange == ReportTimeRangeList.Codes.LifeTime)
			{
				fromDateTime = ZDateTime.Empty;
				toDateTime = ZDateTime.Empty;
			}
		}

		public bool IsReportTimeRangeStartsFromCampaignSent
		{
			get
			{
				return ReportTimeRange == ReportTimeRangeList.Codes.SixHours
					|| ReportTimeRange == ReportTimeRangeList.Codes.TwentyFourHours
					|| ReportTimeRange == ReportTimeRangeList.Codes.SevenDays
					|| ReportTimeRange == ReportTimeRangeList.Codes.OneMonth;
			}
		}

		public void SetDefaultReportTimeRange()
		{
			if (Campaign.IsLinkTrackCampaign)
			{
				reportTimeRange = ReportTimeRangeList.Codes.Today;
			}
			else
			{
				reportTimeRange = ReportTimeRangeList.Codes.LifeTime;

				var firstSentTime = FirstCampaignItemSentTimeLocal;
				if (firstSentTime.IsValid)
				{
					var timeSpan = ZDateTime.Now - firstSentTime;
					if (timeSpan.TotalHours <= 6)
					{
						reportTimeRange = ReportTimeRangeList.Codes.SixHours;
					}
					else if (timeSpan.TotalHours <= 24)
					{
						reportTimeRange = ReportTimeRangeList.Codes.TwentyFourHours;
					}
					else if (timeSpan.TotalDays <= 7)
					{
						reportTimeRange = ReportTimeRangeList.Codes.SevenDays;
					}
					else if (timeSpan.TotalDays <= 31)
					{
						reportTimeRange = ReportTimeRangeList.Codes.OneMonth;
					}
				}
			}

			UpdateDateTimeRange();
		}

		#endregion

		#region FromDateTime

		public ZDateTime FromDateTime
		{
			get { return fromDateTime; }
			set
			{
				if (value != fromDateTime)
				{
					fromDateTime = value;
					LoadClicks();
					FromDateTimeInfo.RefreshBinding();
				}
			}
		}
		ZDateTime fromDateTime;

		public ZPropertyInfo FromDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.FromDateTime); }
		}

		public bool FromDateTime_ReadOnly
		{
			get { return ReportTimeRange != ReportTimeRangeList.Codes.Specific; }
		}

		public ZDateTime ActualStartTime { get; private set; }

		#endregion

		#region ToDateTime

		public ZDateTime ToDateTime
		{
			get { return toDateTime; }
			set
			{
				if (value != toDateTime)
				{
					toDateTime = value;
					LoadClicks();
					ToDateTimeInfo.RefreshBinding();
				}
			}
		}
		ZDateTime toDateTime;

		public ZPropertyInfo ToDateTimeInfo
		{
			get { return GetZPropertyInfo(Schema.ToDateTime); }
		}

		public bool ToDateTime_ReadOnly
		{
			get { return ReportTimeRange != ReportTimeRangeList.Codes.Specific; }
		}

		public ZDateTime ActualEndTime { get; private set; }

		#endregion

		#region ClicksPerIntervalData

		public List<CampaignClicksPerInterval> ClicksPerIntervalData
		{
			get { return clicksPerIntervalData ?? (clicksPerIntervalData = new List<CampaignClicksPerInterval>()); }
		}
		List<CampaignClicksPerInterval> clicksPerIntervalData;

		public List<CampaignClicksByUrl> ClicksByUrlData
		{
			get { return clicksByUrlData ?? (clicksByUrlData = new List<CampaignClicksByUrl>()); }
		}
		List<CampaignClicksByUrl> clicksByUrlData;

		public ClickStatDataCollection LinkClicks
		{
			get
			{
				if (linkClicks == null)
				{
					linkClicks = new ClickStatDataCollection(Campaign);
				}
				return linkClicks;
			}
		}
		ClickStatDataCollection linkClicks;

		public void RefreshLinkTrackedUrl()
		{
			if (linkClicks != null)
			{
				foreach (ClickStatData clickData in linkClicks)
				{
					clickData.TrackedUrlInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region FirstCampaignItemSentTimeLocal

		public ZDateTime FirstCampaignItemSentTimeLocal
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				query.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, Campaign.PK);
				query.OrderBy = GlbCompanyCampaignItemSchema.Constants.G8_SystemCreateTimeUtc;
				var firstCampaignItemSent = Campaign.Factory.LoadTop1<GlbCompanyCampaignItem>(query);
				return firstCampaignItemSent != null ? Env.Time.GetLocalTimeFromUtc(firstCampaignItemSent.G8_SystemCreateTimeUtc.ToDateTime()) : ZDateTime.Empty;
			}
		}

		#endregion

		#region IsLinkTrackOnlyCampaign

		public bool IsCampaignLinkTrackOnly
		{
			get { return !Campaign.IsDeleted && Campaign.IsLinkTrackCampaign; }
		}

		#endregion

		#endregion

		#region Load

		public void ReloadLinksAndClicks()
		{
			LinkClicks.RemoveAll();

			List<string> urls = new List<string>();
			foreach (GlbCompanyCampaignLink link in CampaignTrackedLinks)
			{
				if (!link.GCL_IsImage && (ReportBy == ReportByList.Codes.Context || (ReportBy == ReportByList.Codes.Url && !urls.Contains(link.GCL_URL))))
				{
					linkClicks.Add(new ClickStatData(link));
					urls.Add(link.GCL_URL);
				}
			}

			LoadClicks();
		}

		public void LoadClicks()
		{
			if (!ReportBy.IsEmpty)
			{
				CalculateActualTimeRange();
				if (ActualEndTime.IsValid && ActualStartTime.IsValid)
				{
					LoadRawStatData();
				}

				var reportByProperty = (ReportBy == ReportByList.Codes.Context ? ClickStatData.Schema.Context : ClickStatData.Schema.URL);

				foreach (ClickStatData click in LinkClicks)
				{
					click.Clicks = 0;
					click.UniqueClicks = 0;
					click.SetViewInChart(false);
					click.ReportByProperty = reportByProperty;
				}

				if (ReportBy == ReportByList.Codes.Context)
				{
					CalculateLinkClicksByContext();
				}
				else if (reportByProperty.Equals(ClickStatData.Schema.URL))
				{
					CalculateLinkClicksByUrl();
				}

				foreach (ClickStatData click in LinkClicks)
				{
					int recipients = click.CampaignRecipients;
					click.ClicksRate = recipients > 0 ? (click.UniqueClicks / (double)recipients) * 100 : ZDecimal.Zero;
					if (click.Clicks > 0)
					{
						click.SetViewInChart(true);
					}
				}

				LinkClicks.Sort(reportByProperty);
			}
		}

		void CalculateLinkClicksByContext()
		{
			var clicksMap = LinkClicks.Cast<ClickStatData>().ToDictionary(click => click.Link.PK);
			foreach (CampaignClicksPerInterval clicksPerInterval in ClicksPerIntervalData)
			{
				if (clicksMap.ContainsKey(clicksPerInterval.LinkPk))
				{
					var click = clicksMap[clicksPerInterval.LinkPk];
					click.Clicks += clicksPerInterval.ClickCount;
					click.UniqueClicks = clicksPerInterval.UniqueClickCount;
				}
			}
		}

		void CalculateLinkClicksByUrl()
		{
			var clicksMap = LinkClicks.Cast<ClickStatData>().ToDictionary(click => click.Link.GCL_URL);
			foreach (CampaignClicksPerInterval clicksPerInterval in ClicksPerIntervalData)
			{
				GlbCompanyCampaignLink link = CampaignTrackedLinks.FindByPK(clicksPerInterval.LinkPk) as GlbCompanyCampaignLink;
				if (link != null && clicksMap.ContainsKey(link.GCL_URL))
				{
					var click = clicksMap[link.GCL_URL];
					click.Clicks += clicksPerInterval.ClickCount;
				}
			}

			foreach (ClickStatData link in LinkClicks)
			{
				var urlData = ClicksByUrlData.FirstOrDefault(c => c.LinkURL == link.URL);
				link.UniqueClicks = urlData != null ? urlData.UniqueClickCount : ZInt.Zero;
			}
		}

		protected virtual void LoadRawStatData()
		{
			TrackingStatisticsModel.LoadClickCounts(
							ClicksPerIntervalData,
							Campaign.PK,
							ActualStartTime.IsValid ? Env.Time.GetUtcFromLocalTime(ActualStartTime.ToDateTime()) : ZDateTime.Empty,
							ActualEndTime.IsValid ? Env.Time.GetUtcFromLocalTime(ActualEndTime.ToDateTime()) : ZDateTime.Empty,
							ClicksTimeIntervalCount,
							ClicksByUrlData);
		}

		public int ClicksTimeIntervalCount { get; private set; }
		public TimeSpan TimeSpanPerInterval { get; private set; }

		void CalculateActualTimeRange()
		{
			ActualStartTime = FromDateTime;
			ActualEndTime = ToDateTime;

			if (!ActualStartTime.IsValid || !ActualEndTime.IsValid)
			{
				var factory = new BusinessObjectFactory();
				var query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignClick));
				var linkSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignLink), GlbCompanyCampaignClickSchema.GCC_GCL);
				linkSubQuery.AddToFilter(GlbCompanyCampaignLinkSchema.GCL_G0_Campaign, Campaign.PK);
				query.AddSubQuery(linkSubQuery, JoinCondition.And);

				if (!ActualStartTime.IsValid)
				{
					query.OrderBy = GlbCompanyCampaignClickSchema.Constants.GCC_ClickTimeUtc;
					var firstClick = factory.LoadTop1<GlbCompanyCampaignClick>(query);
					if (firstClick != null)
					{
						ActualStartTime = Env.Time.GetLocalTimeFromUtc(firstClick.GCC_ClickTimeUtc.ToSmallDateTimeFloor().ToDateTime());
					}
				}

				if (!ActualEndTime.IsValid)
				{
					query.OrderBy = GlbCompanyCampaignClickSchema.Constants.GCC_ClickTimeUtc + OrderByClause.Descending;
					var lastClick = factory.LoadTop1<GlbCompanyCampaignClick>(query);
					if (lastClick != null)
					{
						ActualEndTime = Env.Time.GetLocalTimeFromUtc(lastClick.GCC_ClickTimeUtc.ToSmallDateTimeFloor().AddMinutes(1).ToDateTime());
					}
				}
			}

			if (ActualEndTime.IsValid && ActualStartTime.IsValid && ActualEndTime > ActualStartTime)
			{
				int timeIntervalMinutes = CalculateTimeIntervalMinutes();

				ActualStartTime = GetRoundedTime(ActualStartTime, timeIntervalMinutes, false);
				ActualEndTime = GetRoundedTime(ActualEndTime, timeIntervalMinutes, true);
				TimeSpanPerInterval = TimeSpan.FromMinutes(timeIntervalMinutes);
				ClicksTimeIntervalCount = (int)((ActualEndTime - ActualStartTime).TotalMinutes / timeIntervalMinutes);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int CalculateTimeIntervalMinutes()
		{
			var timeRange = ActualEndTime - ActualStartTime;

			int timeIntervalMinutes = 0;

			if (timeRange.TotalMinutes <= 20)
			{
				timeIntervalMinutes = 1;
			}
			else if (timeRange.TotalMinutes <= 40)
			{
				timeIntervalMinutes = 2;
			}
			else if (timeRange.TotalHours <= 2)
			{
				timeIntervalMinutes = 5;
			}
			else if (timeRange.TotalHours <= 3)
			{
				timeIntervalMinutes = 10;
			}
			else if (timeRange.TotalHours <= 6)
			{
				timeIntervalMinutes = 15;
			}
			else if (timeRange.TotalHours <= 12)
			{
				timeIntervalMinutes = 30;
			}
			else if (timeRange.TotalHours <= 24)
			{
				timeIntervalMinutes = 60;
			}
			else if (timeRange.TotalDays <= 2)
			{
				timeIntervalMinutes = 120;
			}
			else if (timeRange.TotalDays <= 3)
			{
				timeIntervalMinutes = 240;
			}
			else if (timeRange.TotalDays <= 7)
			{
				timeIntervalMinutes = 360;
			}
			else if (timeRange.TotalDays <= 31)
			{
				timeIntervalMinutes = 1440;
			}
			else if (timeRange.TotalDays <= 62)
			{
				timeIntervalMinutes = 2880;
			}
			else
			{
				var timeIntervalDays = (int)(timeRange.TotalDays / 24);
				timeIntervalMinutes = timeIntervalDays * 1440;
			}
			return timeIntervalMinutes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		ZDateTime GetRoundedTime(ZDateTime dateTime, int timeIntervalMinutes, bool isRoundingUp)
		{
			ZDateTime result = dateTime;

			if (timeIntervalMinutes <= 60)
			{
				var minuteDiff = dateTime.Minute % timeIntervalMinutes;
				if (minuteDiff != 0 || dateTime.Second != 0)
				{
					result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0).AddMinutes(-minuteDiff);
					if (isRoundingUp)
					{
						result = result.AddMinutes(timeIntervalMinutes);
					}
				}
			}
			else if (timeIntervalMinutes <= 1440)
			{
				var timeIntervalHours = timeIntervalMinutes / 60;
				var hourDiff = dateTime.Hour % timeIntervalHours;
				if (hourDiff != 0 || dateTime.Second != 0 || dateTime.Minute != 0)
				{
					result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0).AddHours(-hourDiff);
					if (isRoundingUp)
					{
						result = result.AddHours(timeIntervalHours);
					}
				}
			}
			else
			{
				var timeIntervalDays = timeIntervalMinutes / 1440;
				var dayDiff = dateTime.Day % timeIntervalDays;
				if (dayDiff != 0 || dateTime.Second != 0 || dateTime.Minute != 0 || dateTime.Hour != 0)
				{
					result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0).AddDays(-dayDiff);
					if (isRoundingUp)
					{
						result = result.AddDays(timeIntervalDays);
					}
				}
			}

			return result;
		}

		#endregion
	}
}

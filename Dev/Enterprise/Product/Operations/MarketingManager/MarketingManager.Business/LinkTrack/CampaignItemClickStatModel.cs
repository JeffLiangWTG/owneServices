using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignItemClickStatModel : NonPersistentBusinessObject
	{
		public CampaignItemClickStatModel(GlbCompanyCampaignItem campaignItem)
			: base(campaignItem.Factory)
		{
			this.CampaignItem = campaignItem;
		}

		readonly GlbCompanyCampaignItem CampaignItem;

		static string DaysAgoText
		{
			get { return Res.GetString("7e1658ae-bf18-4228-afa1-e0297e78624d", "Day(s) ago"); }
		}

		public static class Schema
		{
			public const string ReportBy = "ReportBy";
			public const string FromDateTime = "FromDateTime";
			public const string ToDateTime = "ToDateTime";
		}

		#region Properties

		public GlbCompanyCampaignItem BusinessEntity
		{
			get { return CampaignItem; }
		}

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
					LinkClicks.RemoveAll();
					LoadClicks();
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

		#region First Activity Date

		public ZDateTime FirstActivityDate
		{
			get
			{
				firstActivityDate = ZDateTime.Empty;
				var clickData = CampaignItemsClickData.Cast<CampaignItemClicks>().OrderBy(c => c.FirstClickDateTime).FirstOrDefault();
				if (clickData != null)
				{
					firstActivityDate = Env.Time.GetLocalTimeFromUtc(clickData.FirstClickDateTime.ToDateTime());
				}

				return firstActivityDate;
			}
		}
		ZDateTime firstActivityDate;

		#endregion

		#region Last Activity Date

		public ZString LastActivityDays
		{
			get
			{
				lastActivityDays = "";

				if (!FirstActivityDate.IsEmpty)
				{
					lastActivityDays = (ZDateTime.Now - FirstActivityDate).TotalDays.ToString("0") + " " + DaysAgoText;
				}

				return lastActivityDays;
			}
		}
		ZString lastActivityDays;

		#endregion

		#region Total Unique Opens

		public ZString TotalUniqueOpensText
		{
			get
			{
				return CampaignItem != null && !FirstActivityDate.IsEmpty ? Res.GetString("37aa55c3-8cb5-4c0b-9966-40ecb32c9dc3", "{0} unique day(s)", TotalUniqueOpens.ToString()) : "";
			}
		}

		public ZString ContactNameAssociatedWithUniqueOpensText
		{
			get
			{
				ZString result = "";
				if (CampaignItem != null && !CampaignItem.ContactName.IsEmpty && !FirstActivityDate.IsEmpty)
				{
					var names = CampaignItem.ContactName.Split(new char[] { ' ' });
					ZString firstName = names[0];
					result = Res.GetString("aa40d35f-1a45-4272-b312-90aa378c20de", "{0} has viewed this campaign on ", firstName.ToTitleCase());
				}
				return result;
			}
		}

		ZInt TotalUniqueOpens
		{
			get
			{
				return Factory.GetCachedValue("CampaignItemClickModel.TotalUniqueOpens:" + CampaignItem.PK, delegate
				{
					return CalculateUniqueOpens();
				});
			}
		}

		ZInt CalculateUniqueOpens()
		{
			DateTime[] dateTimes = CampaignClickCollection.Cast<GlbCompanyCampaignClick>().Select(click => GetDateTimeFromTimeZone(click.GCC_ClickTimeUtc).ToDateTime()).ToArray();
			var groupedDates = from date in dateTimes
							   group date by date.Date
								   into g
							   select new { DateValue = new ZDateTime(g.Key) };

			return groupedDates.Count();
		}

#if DEBUG
		internal
#endif
		ZDateTime GetDateTimeFromTimeZone(ZDateTime utcDateTime)
		{
			RefUNLOCO unloco = null;

			var orgContact = BusinessEntity.RecipientAsOrgContact;
			if (orgContact != null)
			{
				unloco = orgContact.Header.UNLOCO;
			}
			else
			{
				var inquiry = BusinessEntity.RecipientAsSalesEnquiry;
				if (inquiry != null && !inquiry.O1_PortOrCountry.IsEmpty)
				{
					unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, inquiry.O1_PortOrCountry));
				}
			}

			if (unloco != null)
			{
				return Env.Time.GetUnlocoTimeFromUtc(unloco.Code, utcDateTime.ToDateTime());
			}
			return utcDateTime.ToLocalBranchTime(Factory);
		}

		#endregion

		public CampaignItemClickStatDataCollection LinkClicks
		{
			get
			{
				if (linkClicks == null)
				{
					linkClicks = new CampaignItemClickStatDataCollection(CampaignItem);
				}
				return linkClicks;
			}
		}
		CampaignItemClickStatDataCollection linkClicks;

		public GlbCompanyCampaignClickCollection CampaignClickCollection
		{
			get
			{
				if (campaignClickCollection == null)
				{
					campaignClickCollection = new GlbCompanyCampaignClickCollection(CampaignItem);
				}
				return campaignClickCollection;
			}
		}
		GlbCompanyCampaignClickCollection campaignClickCollection;

		public ZString DeliveryStatusDescription
		{
			get { return CampaignItem.TrackingStatusDescription; }
		}

		#region CampaignItemClicksData

		public List<CampaignItemClicks> CampaignItemsClickData
		{
			get { return campaignItemsClickData ?? (campaignItemsClickData = new List<CampaignItemClicks>()); }
		}
		List<CampaignItemClicks> campaignItemsClickData;

		#endregion

		#endregion

		#region Load

		public void LoadClicks()
		{
			if (!ReportBy.IsEmpty && !CampaignItem.G8_RecipientID.IsEmpty)
			{
				LoadRawStatData();

				var reportByProperty = (ReportBy == ReportByList.Codes.Context ? CampaignItemClickStatData.Schema.Context : CampaignItemClickStatData.Schema.URL);

				foreach (CampaignItemClickStatData click in LinkClicks)
				{
					click.Clicks = 0;
					click.FirstClick = ZDateTime.Empty;
					click.LastClick = ZString.Empty;
					click.ReportByProperty = reportByProperty;
				}

				if (ReportBy == ReportByList.Codes.Context)
				{
					CalculateLinkClicksByContext();
				}
				else if (reportByProperty.Equals(CampaignItemClickStatData.Schema.URL))
				{
					CalculateLinkClicksByUrl();
				}
			}
		}

		static string Today
		{
			get { return Res.GetString("1ae9f79c-f3e9-4909-b053-45a34643076d", "Today"); }
		}
		static string Yesterday
		{
			get { return Res.GetString("efbe231d-fc7d-4261-8be1-20e0ca3844bb", "Yesterday"); }
		}

		void CalculateLinkClicksByContext()
		{
			var clicksMap = LinkClicks.Cast<CampaignItemClickStatData>().ToDictionary(click => click.Context);
			foreach (CampaignItemClicks campaignItemClickData in CampaignItemsClickData)
			{
				if (clicksMap.ContainsKey(campaignItemClickData.LinkContext))
				{
					var click = clicksMap[campaignItemClickData.LinkContext];
					click.Clicks = campaignItemClickData.ClickCount;
					click.FirstClick = Env.Time.GetLocalTimeFromUtc(campaignItemClickData.FirstClickDateTime.ToDateTime());
					int dayDiff = (int)(Env.Time.CurrentLocalDateTime - click.FirstClick).TotalDays;
					if (dayDiff == 0)
					{
						click.LastClick = Today;
					}
					else if (dayDiff == 1)
					{
						click.LastClick = Yesterday;
					}
					else
					{
						click.LastClick = (Env.Time.CurrentLocalDateTime - click.FirstClick).TotalDays.ToString("0") + " " + DaysAgoText;
					}
				}
			}
		}

		void CalculateLinkClicksByUrl()
		{
			var clicksMap = LinkClicks.Cast<CampaignItemClickStatData>().ToDictionary(click => click.URL);
			foreach (CampaignItemClicks campaignItemClickData in CampaignItemsClickData)
			{
				if (clicksMap.ContainsKey(campaignItemClickData.LinkUrl))
				{
					var click = clicksMap[campaignItemClickData.LinkUrl];
					click.Clicks = campaignItemClickData.ClickCount;
					click.FirstClick = Env.Time.GetLocalTimeFromUtc(campaignItemClickData.FirstClickDateTime.ToDateTime());
					int dayDiff = (int)(Env.Time.CurrentLocalDateTime - click.FirstClick).TotalDays;
					if (dayDiff == 0)
					{
						click.LastClick = Today;
					}
					else if (dayDiff == 1)
					{
						click.LastClick = Yesterday;
					}
					else
					{
						click.LastClick = (Env.Time.CurrentLocalDateTime - click.FirstClick).TotalDays.ToString("0") + " " + DaysAgoText;
					}
				}
			}
		}

		protected virtual void LoadRawStatData()
		{
			CampaignItemClickStatistics.LoadClickCounts(CampaignItemsClickData, BusinessEntity.PK, ReportBy);
			foreach (var clickData in CampaignItemsClickData)
			{
				if (!clickData.IsImage)
				{
					LinkClicks.Add(new CampaignItemClickStatData(this.Factory, clickData.LinkContext, clickData.LinkUrl));
				}
			}
		}

		#endregion
	}
}

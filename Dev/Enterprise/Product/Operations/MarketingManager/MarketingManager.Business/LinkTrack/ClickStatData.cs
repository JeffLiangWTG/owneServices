using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class ClickStatData : NonPersistentBusinessObject
	{
		public ClickStatData(GlbCompanyCampaignLink link)
			: base(link.Factory)
		{
			this.link = link;
			this.campaign = link.Campaign;
		}
		GlbCompanyCampaignLink link;

		public ClickStatData(GlbCompanyCampaign campaign)
			: base(campaign.Factory)
		{
			this.campaign = campaign;
		}
		readonly GlbCompanyCampaign campaign;

		public static class Schema
		{
			public const string ViewInChart = "ViewInChart";
			public const string Clicks = "Clicks";
			public const string UniqueClicks = "UniqueClicks";
			public const string UniqueClickCountByUrl = "UniqueClickCountByUrl";
			public const string ClicksRate = "ClicksRate";
			public const string ClicksRatePercentage = "ClicksRatePercentage";
			public const string Context = "Context";
			public const string URL = "URL";
			public const string TrackedUrl = "TrackedUrl";
		}

		#region Properties

		public GlbCompanyCampaignLink Link
		{
			get { return link; }
		}

		#region View In Chart

		public ZBool ViewInChart
		{
			get { return viewInChart; }
			set
			{
				if (value != viewInChart)
				{
					SetNonPersistentPropertyValue(ViewInChartInfo, ref viewInChart, value);
				}
			}
		}
		ZBool viewInChart;

		public void SetViewInChart(bool viewInChart)
		{
			this.viewInChart = viewInChart;
		}

		public ZPropertyInfo ViewInChartInfo
		{
			get { return GetZPropertyInfo(Schema.ViewInChart); }
		}

		public bool ViewInChart_ReadOnly
		{
			get { return Clicks == 0; }
		}

		#endregion

		#region Clicks

		public ZInt Clicks
		{
			get { return clicks; }
			set
			{
				SetNonPersistentPropertyValue(ClicksInfo, ref clicks, value);
			}
		}
		ZInt clicks;

		public ZPropertyInfo ClicksInfo
		{
			get { return GetZPropertyInfo(Schema.Clicks); }
		}

		public bool Clicks_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Unique Clicks

		public ZInt UniqueClicks
		{
			get { return uniqueClicks; }
			set { SetNonPersistentPropertyValue(UniqueClicksInfo, ref uniqueClicks, value); }
		}
		ZInt uniqueClicks;

		public ZPropertyInfo UniqueClicksInfo
		{
			get { return GetZPropertyInfo(Schema.UniqueClicks); }
		}

		public bool UniqueClicks_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Link Impressions

		public ZInt CampaignRecipients
		{
			get { return campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().Count(x => x.G8_TrackingStatus != TrackingStatusCodes.Codes.NDR); }
		}

		#endregion

		#region Clicks Rate

		[DecimalPlaces(2)]
		public ZDecimal ClicksRate
		{
			get { return clicksRate; }
			set
			{
				SetNonPersistentPropertyValue(ClicksRateInfo, ref clicksRate, value);
			}
		}
		ZDecimal clicksRate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Display constants for CTR Information")]
		public ZString ClicksRatePercentage
		{
			get { return ClicksRate < 0.01 ? "< 0.01%" : System.Math.Round(ClicksRate, 2).ToString() + "%"; }
		}

		public ZPropertyInfo ClicksRateInfo
		{
			get { return GetZPropertyInfo(Schema.ClicksRate); }
		}

		public bool ClicksRate_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Context

		[MaxLength(GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength)]
		public ZString Context
		{
			get { return Link != null ? Link.GCL_Context : ZString.Empty; }
			set
			{
				if (Link == null && !value.IsEmpty)
				{
					link = campaign.TrackedLinks.AddNew();
				}

				if (Link != null)
				{
					Link.GCL_Context = value;
					ContextInfo.RefreshBinding();
				}
			}
		}

		public bool Context_ReadOnly
		{
			get { return !campaign.IsLinkTrackCampaign; }
		}

		public ZPropertyInfo ContextInfo
		{
			get
			{
				if (Link == null)
				{
					return GetZPropertyInfo(Schema.Context);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.Context, x => Link.GCL_ContextInfo);
				}
			}
		}

		#endregion

		#region URL

		[MaxLength(GlbCompanyCampaignLink.Schema.GCL_URLMaxLength)]
		public ZString URL
		{
			get { return Link != null ? Link.GCL_URL : ZString.Empty; }
			set
			{
				if (Link == null && !value.IsEmpty)
				{
					link = campaign.TrackedLinks.AddNew();
				}
				Link.GCL_URL = value;
				URLInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo URLInfo
		{
			get
			{
				if (Link == null)
				{
					return GetZPropertyInfo(Schema.URL);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.URL, x => Link.GCL_URLInfo);
				}
			}
		}

		public bool URL_ReadOnly
		{
			get { return !campaign.IsLinkTrackCampaign || HasClickData; }
		}

		#endregion

		#region TrackedUrl

		public ZString TrackedUrl
		{
			get { return Link != null && Link.IsInDatabase && !Link.GCL_URLInfo.HasErrors() ? Link.TrackedUrl : ZString.Empty; }
		}

		public bool TrackedUrl_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo TrackedUrlInfo
		{
			get { return GetZPropertyInfo(Schema.TrackedUrl); }
		}

		#endregion

		#region Report By

		public ZString ReportByProperty { get; set; }

		public ZString ReportBy
		{
			get
			{
				if (ReportByProperty == Schema.Context)
				{
					return Context;
				}
				else if (ReportByProperty == Schema.URL)
				{
					return URL;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region Has Click Data

		bool HasClickData
		{
			get
			{
				if (Link != null)
				{
					return Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(GlbCompanyCampaignClick)), new ZQuery(GlbCompanyCampaignClickSchema.GCC_GCL, Link.PK));
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			if (link != null && !link.IsDeleted)
			{
				link.Delete();
			}
		}

		public override bool CanDelete
		{
			get
			{
				var result = base.CanDelete;
				if (result)
				{
					result = !HasClickData;
				}
				return result;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ResString.GetMultilingualString("e56ac73d-0baa-4c08-92e1-ee087ae0ecbc", "Cannot delete - link has been tracked and click data exist.");
			}
		}

		#endregion
	}
}

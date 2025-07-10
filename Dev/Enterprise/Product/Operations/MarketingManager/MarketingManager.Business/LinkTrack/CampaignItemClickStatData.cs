using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public class CampaignItemClickStatData : NonPersistentBusinessObject
	{
		public CampaignItemClickStatData(BusinessObjectFactory factory, ZString context, ZString url)
			: base(factory)
		{
			this.context = context;
			this.url = url;
		}

		readonly ZString context;
		readonly ZString url;

		public static class Schema
		{
			public const string Clicks = "Clicks";
			public const string Context = "Context";
			public const string FirstClick = "FirstClick";
			public const string LastClick = "LastClick";
			public const string URL = "URL";
		}

		#region Properties

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

		#region First Click

		public ZDateTime FirstClick
		{
			get { return firstClick; }
			set { SetNonPersistentPropertyValue(FirstClickInfo, ref firstClick, value); }
		}
		ZDateTime firstClick;

		public ZPropertyInfo FirstClickInfo
		{
			get { return GetZPropertyInfo(Schema.FirstClick); }
		}

		public bool FirstClick_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Last Click

		public ZString LastClick
		{
			get { return lastClick; }
			set { SetNonPersistentPropertyValue(LastClickInfo, ref lastClick, value); }
		}
		ZString lastClick;

		public ZPropertyInfo LastClickInfo
		{
			get { return GetZPropertyInfo(Schema.LastClick); }
		}

		public bool LastClick_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Context

		[MaxLength(GlbCompanyCampaignLink.Schema.GCL_ContextMaxLength)]
		public ZString Context
		{
			get { return context; }
		}

		public ZPropertyInfo ContextInfo
		{
			get { return GetZPropertyInfo(Schema.Context); }
		}

		#endregion

		#region URL

		[MaxLength(GlbCompanyCampaignLink.Schema.GCL_URLMaxLength)]
		public ZString URL
		{
			get { return url; }
		}

		public ZPropertyInfo URLInfo
		{
			get { return GetZPropertyInfo(Schema.URL); }
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

		#endregion
	}
}

using System;
using System.Data;
using System.Net;
using System.Web;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignLink : AutoGlbCompanyCampaignLink
	{
		public GlbCompanyCampaignLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoGlbCompanyCampaignLink.Schema
		{
			public const string HasTrackingMacro = "HasTrackingMacro";
			public const string Order = "Order";
			public const string UnicodeUrl = "UnicodeUrl";
		}

		#endregion

		#region Campaign

		[RelatedBusinessObject("Campaign")]
		public override ZGuid GCL_G0_Campaign
		{
			get { return base.GCL_G0_Campaign; }
			set { base.GCL_G0_Campaign = value; }
		}

		public virtual GlbCompanyCampaign Campaign
		{
			get { return Factory.Load<GlbCompanyCampaign>(GCL_G0_Campaign); }
		}

		#endregion

		#region Url

		protected bool GCL_URL_ReadOnly
		{
			get { return GCL_IsTracked; }
		}

		/// <summary>
		/// URL with unicode characters in the path as unicode rather than in encoded form. More readable for users.
		/// The query string is not affected.
		/// </summary>
		[MaxLength(GlbCompanyCampaignLink.Schema.GCL_URLMaxLength)]
		public ZString UnicodeUrl
		{
			get
			{
				ZString result;
				int i = GCL_URL.IndexOf('?');
				if (i >= 0)
				{
					result = WebUtility.UrlDecode(GCL_URL.Substring(0, i)) + GCL_URL.Substring(i);
				}
				else
				{
					result = WebUtility.UrlDecode(GCL_URL);
				}

				return result;
			}
			set
			{
				GCL_URL = HttpUtility.UrlPathEncode(value);
				UnicodeUrlInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnicodeUrlInfo
		{
			// Not use GetWrappedZPropertyInfo because of an UI issue
			// Method IBuisinessObjectInternals.Validate which is called by ZGrid.OnCurrentCellChanged, calls RefreshBinding() if the PropetyInfo passed in is ZWrappedPropertyInfo
			// RefreshBinding() then triggers OnValueChange event, in this case it forces URLs be parsed again and breaks link highlighting function
			get { return GetZPropertyInfo(Schema.UnicodeUrl); }
		}

		protected bool UnicodeUrl_ReadOnly
		{
			get { return GCL_URL_ReadOnly; }
		}

		#endregion UnicodeUrl

		#region GCL_Context

		protected bool GCL_Context_ReadOnly
		{
			get { return !HasTrackingMacro; }
		}

		#endregion

		#region HasTrackingMacro

		public ZBool HasTrackingMacro
		{
			get { return hasTrackingMacro; }
			set { SetNonPersistentPropertyValue(HasTrackingMacroInfo, ref hasTrackingMacro, value); }
		}

		ZBool hasTrackingMacro;

		public ZPropertyInfo HasTrackingMacroInfo
		{
			get { return GetZPropertyInfo(Schema.HasTrackingMacro); }
		}

		protected bool HasTrackingMacro_ReadOnly
		{
			get { return GCL_IsTracked; }
		}

		#endregion

		#region Order

		public ZInt Order
		{
			get { return order; }
			set { SetNonPersistentPropertyValue(OrderInfo, ref order, value); }
		}

		ZInt order;

		public ZPropertyInfo OrderInfo
		{
			get { return GetZPropertyInfo(Schema.Order); }
		}

		#endregion

		#region Tracked URL

		public ZString TrackedUrl
		{
			get { return !GCL_URL.IsEmpty ? GenerateTrackedUrl(ZGuid.Empty) : string.Empty; }
		}

		public string GenerateTrackedUrl(GlbCompanyCampaignItem recipient)
		{
			return GenerateTrackedUrl(recipient.PK);
		}

		public string GenerateTrackedUrl(ZGuid recipientPk)
		{
			return GenerateTrackedUrl(recipientPk, GCL_URL);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "URL query string keys are culture invariant")]
		string GenerateTrackedUrl(ZGuid recipientPk, ZString destinationUrl)
		{
			var indexOfHashmark = destinationUrl.LastIndexOf('#');
			return OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value +
				"?" + (indexOfHashmark >= 0 ? destinationUrl.SubstringSafe(0, indexOfHashmark) : destinationUrl) +
				"&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() +
				"&x=" + PK.ToGuid().ToString("N") +
				(!recipientPk.IsEmpty ? "&u=" + recipientPk.ToGuid().ToString("N") : string.Empty) +
				(indexOfHashmark >= 0 ? "&f=" + WebUtility.UrlEncode(destinationUrl.SubstringSafe(indexOfHashmark + 1)) : string.Empty);
		}

		/// <summary>
		/// Convert a LinkTracking macro to a tracked URL.
		/// </summary>
		public static string GenerateTrackedUrl(GlbCompanyCampaignItem item, ZString contextDisplayName, ZString destinationUrl)
		{
			var campaign = item.CompanyCampaign;
			if (campaign != null)
			{
				foreach (var link in campaign.TrackedLinks)
				{
					if (link.GCL_Context.EqualsIgnoringCase(contextDisplayName))
					{
						if (destinationUrl.IsEmpty || link.GCL_URL.EqualsIgnoringCase(destinationUrl))
						{
							return link.GenerateTrackedUrl(item);
						}
						else if (link.GCL_URL.Contains(Constants.DocumentEngine.EmailParsing.StartTag, StringComparison.OrdinalIgnoreCase)
							&& link.GCL_URL.Contains(Constants.DocumentEngine.EmailParsing.EndTag, StringComparison.OrdinalIgnoreCase))
						{
							return link.GenerateTrackedUrl(item.PK, destinationUrl);
						}
					}
				}
			}
			return destinationUrl;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GCL_URL = "http://localhost";
		}
#endif
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class UnsubscribeUrlHelper
	{
		public static ResourceString GetUriFormatErrorMessage(ICompany campaignCompany)
		{
			return ResString.GetMultilingualString("be93a4bb-3d14-42a0-8728-552e24022e0a",
				"Can't create Web Campaign link. Please verify the value of the registry item '{0}' for the Company {1}.",
				WebDataRegistry.Instance.WebCampaignUrl.GetLocation(),
				campaignCompany?.Name);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "fixed Url name")]
		public static Uri GetUnsubscribeUrl(this GlbCompanyCampaignItem campaignItem, string unsubscribeType, bool isResubscribe = false)
		{
			var campaignCompany = campaignItem?.CompanyCampaign?.Company ?? Env.CurrentCompany;
			var campaignUrl = WebDataRegistry.Instance.WebCampaignUrl.GetValueWithoutFallback(campaignCompany.PK, Guid.Empty, Guid.Empty);
			if (campaignUrl.IsNullOrEmpty())
			{
				throw new UriFormatException(GetUriFormatErrorMessage(campaignCompany));
			}

			var campaignItemPK = campaignItem?.PK ?? Guid.Empty;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.PK.Name] = campaignItemPK.ToString(),
				[UnsubscribeTypeKey] = unsubscribeType,
				[ResubscribeKey] = isResubscribe.ToString()
			};

			try
			{
				var uriBuilder = new UriBuilder(new Uri(campaignUrl));
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/unsubscribe.aspx", uriBuilder.Path.TrimEnd('/'));
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(GetUriFormatErrorMessage(campaignCompany), e);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "fixed Url name")]
		public static Uri GetSubscriptionPreferenceUrl(this GlbCompanyCampaignItem campaignItem)
		{
			if (campaignItem == null)
			{
				throw new ArgumentNullException(nameof(campaignItem));
			}

			var campaignUrl = WebDataRegistry.Instance.WebCampaignUrl.GetValueWithoutFallback(campaignItem.CompanyCampaign.G0_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (campaignUrl.IsNullOrEmpty())
			{
				throw new UriFormatException(GetUriFormatErrorMessage(campaignItem.CompanyCampaign.Company));
			}

			var campaignItemPk = campaignItem.PK;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.PK.Name] = campaignItemPk.ToString()
			};

			try
			{
				var uriBuilder = new UriBuilder(new Uri(campaignUrl));
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/subscribepreference.aspx", uriBuilder.Path.TrimEnd('/'));
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(GetUriFormatErrorMessage(campaignItem.CompanyCampaign.Company), e);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "fixed Url name")]
		public static Uri GetSubscriptionPreferenceUrl(ZGuid contactPK, string publishedListCode, double expireMinutes)
		{
			var campaignUrl = WebDataRegistry.Instance.WebCampaignUrl.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
			if (campaignUrl.IsNullOrEmpty())
			{
				throw new UriFormatException(GetUriFormatErrorMessage(Env.CurrentCompany));
			}

			var queryString = new SecureQueryString
			{
				[OrgContactSchema.PK.Name] = contactPK.ToString(),
				[PublishedListCodeKey] = publishedListCode
			};
			if (expireMinutes > 0)
			{
				queryString.ExpireTime = TimeSpan.FromMinutes(expireMinutes);
			}
			try
			{
				var uriBuilder = new UriBuilder(new Uri(campaignUrl));
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/subscribepreference.aspx", uriBuilder.Path.TrimEnd('/'));
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(GetUriFormatErrorMessage(Env.CurrentCompany), e);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Method to convert Uri to string")]
		public static string GetUnsubscribeUrlString(this GlbCompanyCampaignItem campaignItem, UnsubscribeType unsubscribeType, bool isResubscribe = false)
		{
			return GetUnsubscribeUrl(campaignItem, unsubscribeType.ToString("G"), isResubscribe).AbsoluteUri;
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Method to convert Uri to string")]
		public static string GetSubscriptionPreferenceUrlString(this GlbCompanyCampaignItem campaignItem)
		{
			return GetSubscriptionPreferenceUrl(campaignItem).AbsoluteUri;
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "Method to convert Uri to string")]
		public static string GetSubscriptionPreferenceUrlString(ZGuid contactPK, string publishedListCode, double expireMinutes = 0)
		{
			return GetSubscriptionPreferenceUrl(contactPK, publishedListCode, expireMinutes).AbsoluteUri;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Fixed identifier")]
		public const string VoteExamSurveyQueryStringKey = "data";
		public const string UnsubscribeTypeKey = "unsubscribeType";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Key")]
		public const string ResubscribeKey = "resubscribe";
		public const string PublishedListCodeKey = "PublishedListCode";
	}
}

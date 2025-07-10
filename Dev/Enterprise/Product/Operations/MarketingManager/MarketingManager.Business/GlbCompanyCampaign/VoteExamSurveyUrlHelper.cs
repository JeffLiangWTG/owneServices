using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public static class VoteExamSurveyUrlHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string VoteExamSurveyQueryStringKey = "data";

		public const string RecipientIDQueryStringKey = "recipientID";
		public const string RecipientTableCodeQueryStringKey = "recipientPrefix";
		public const string TestExpiryDateStringKey = "testExpiry";
		public const string JobSkillCodeStringKey = "jobSkillCode";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The key should be constant as the others are above")]
		public const string LanguageStringKey = "language";
		public const string ExamSettingsStringKey = "examSettingsCode";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "The key should be constant as the others are above")]
		public const string GroupedExamCampaignsStringKey = "campaigns";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetCampaignPreviewUrl(GlbCompanyCampaign campaign)
		{
			return GetCampaignUrl(campaign, "", null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetCampaignUrl(GlbCompanyCampaignItem campaignItem, string examSettingCode = "")
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (!campaignItem.CurrentQuestionsCountryCode.IsEmpty)
			{
				list.Add(new KeyValuePair<string, string>(RefCountrySchema.Constants.Prefix, campaignItem.CurrentQuestionsCountryCode));
			}

			if (!string.IsNullOrEmpty(examSettingCode))
			{
				list.Add(new KeyValuePair<string, string>(ExamSettingsStringKey, examSettingCode));
			}

			return GetURL(GlbCompanyCampaignItemSchema.PK, campaignItem.PK, campaignItem.CompanyCampaign.Company, list.ToArray());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetCampaignUrl(GlbCompanyCampaign campaign, string countryCode, GlbCompanyCampaignItem.RecipientInfo recipientInfo, string jobSkillCode = "", string language = "", string examSettingsCode = "")
		{
			return GetCampaignUrl(campaign, countryCode, recipientInfo, ZDate.Empty, jobSkillCode, language, examSettingsCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public static string GetCampaignUrl(GlbCompanyCampaign campaign, string countryCode, GlbCompanyCampaignItem.RecipientInfo recipientInfo, ZDate testExpiry, string jobSkillCode = "", string language = "", string examSettingsCode = "")
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (!string.IsNullOrEmpty(countryCode))
			{
				list.Add(new KeyValuePair<string, string>(RefCountrySchema.Constants.Prefix, countryCode));
			}

			if (recipientInfo != null)
			{
				list.Add(new KeyValuePair<string, string>(RecipientIDQueryStringKey, recipientInfo.recipientPK.ToString()));
				list.Add(new KeyValuePair<string, string>(RecipientTableCodeQueryStringKey, recipientInfo.recipientTableCode));
			}

			if (!testExpiry.IsEmpty)
			{
				list.Add(new KeyValuePair<string, string>(TestExpiryDateStringKey, testExpiry.ToJulianDateString()));
			}

			if (!string.IsNullOrEmpty(jobSkillCode))
			{
				list.Add(new KeyValuePair<string, string>(JobSkillCodeStringKey, jobSkillCode));
			}

			if (!string.IsNullOrEmpty(language))
			{
				list.Add(new KeyValuePair<string, string>(LanguageStringKey, language));
			}

			if (!string.IsNullOrEmpty(examSettingsCode))
			{
				list.Add(new KeyValuePair<string, string>(ExamSettingsStringKey, examSettingsCode));
			}

			return GetURL(GlbCompanyCampaignSchema.PK, campaign.PK, campaign.Company, list.ToArray());
		}

		static string GetURL(SchemaPKColumn pkColumn, ZGuid pk, ICompany company, params KeyValuePair<string, string>[] additionalQueryStrings)
		{
			SecureQueryString queryString = new SecureQueryString();
			queryString.Add(pkColumn.Name, pk.ToString());
			foreach (KeyValuePair<string, string> pair in additionalQueryStrings)
			{
				queryString.Add(pair.Key, pair.Value);
			}

			try
			{
				var uriBuilder = new UriBuilder(WebDataRegistry.Instance.WebCampaignUrl.GetValueWithoutFallback(company.PK, Guid.Empty, Guid.Empty));
				uriBuilder.Path = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", uriBuilder.Path.TrimEnd('/'), "login.aspx");
				uriBuilder.Query = string.Format(CultureInfo.InvariantCulture, "{0}={1}", VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
				return uriBuilder.Uri.AbsoluteUri;
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException(UnsubscribeUrlHelper.GetUriFormatErrorMessage(company), e);
			}
		}
	}
}

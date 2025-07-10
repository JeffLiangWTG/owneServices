using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.WebVoting
{
	internal static class SecureQueryStringHelper
	{
		public static string GetQuestionsCountryCode(NameValueCollection queryString)
		{
			return GetValue(queryString, RefCountrySchema.Constants.Prefix);
		}

		public static GlbCompanyCampaignItem.RecipientInfo GetRecipientInfo(NameValueCollection queryString)
		{
			GlbCompanyCampaignItem.RecipientInfo result = null;

			ZGuid recipientPK = GetZGuid(queryString, VoteExamSurveyUrlHelper.RecipientIDQueryStringKey);
			if (!recipientPK.IsEmpty)
			{
				string tableCode = GetValue(queryString, VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey);
				result = new GlbCompanyCampaignItem.RecipientInfo(recipientPK, tableCode);
			}

			return result;
		}

		public static ZGuid GetZGuid(NameValueCollection queryString, string keyName)
		{
			ZGuid result;
			ZGuid.TryParse(GetValue(queryString, keyName), out result);
			return result;
		}

		public static bool GetBoolean(NameValueCollection queryString, string keyName)
		{
			bool result;
			bool.TryParse(GetValue(queryString, keyName), out result);
			return result;
		}

		public static ZDate GetZDate(NameValueCollection queryString, string keyName)
		{
			ZDate result;
			ZDate.TryParseJulianDate(GetValue(queryString, keyName), out result);
			return result;
		}

		public static bool IsQueryNeverExpire(NameValueCollection queryString)
		{
			try
			{
				string encryptedText = queryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
				if (!string.IsNullOrEmpty(encryptedText))
				{
					var secureQueryString = new SecureQueryString(encryptedText);
					return secureQueryString.AbsoluteExpireTime >= new DateTime(2079, 6, 6);
				}
				else
				{
					return true;
				}
			}
			catch (ExpiredQueryStringException)
			{
				return false;
			}
		}

		public static string GetValue(NameValueCollection queryString, string keyName)
		{
			string result = "";

			try
			{
				string encryptedText = queryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
				if (!string.IsNullOrEmpty(encryptedText))
				{
					SecureQueryString secureQueryString = new SecureQueryString(encryptedText);
					result = secureQueryString[keyName];
				}

#if DEBUG
				if (string.IsNullOrEmpty(result) && !Globals.IsTest)
				{
					result = queryString[keyName];
				}
#endif
			}
			catch (InvalidQueryStringException)
			{
			}

			return result;
		}
	}
}

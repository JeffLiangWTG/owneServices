using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	internal class SecureQueryStringHelperTest : TestCase
	{
		public void TestIsQueryNeverExpire()
		{
			AddValueForTest("A", "ABC");
			nameValueCollection = new NameValueCollection();
			nameValueCollection.Add(VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, SecureQueryString.ToString());
			AssertEquals(true, SecureQueryStringHelper.IsQueryNeverExpire(NameValueCollection));

			nameValueCollection = null;
			SecureQueryString.ExpireTime = TimeSpan.FromDays(10);
			AssertEquals(false, SecureQueryStringHelper.IsQueryNeverExpire(NameValueCollection));
		}

		public void TestGetQuestionsCountryCode()
		{
			AssertNull(SecureQueryStringHelper.GetQuestionsCountryCode(NameValueCollection));

			AddValueForTest(RefCountrySchema.Constants.Prefix, "AU");
			AssertEquals("AU", SecureQueryStringHelper.GetQuestionsCountryCode(NameValueCollection));
		}

		public void TestGetRecipientInfo()
		{
			AssertNull(SecureQueryStringHelper.GetRecipientInfo(NameValueCollection));

			ZGuid guid = ZGuid.NewZGuid();
			AddValueForTest(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, guid);
			AddValueForTest(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, "OC");

			GlbCompanyCampaignItem.RecipientInfo recipientInfo = SecureQueryStringHelper.GetRecipientInfo(NameValueCollection);
			AssertEquals(guid, recipientInfo.recipientPK);
			AssertEquals("OC", recipientInfo.recipientTableCode);
		}

		public void TestGetZGuid()
		{
			AssertEquals(ZGuid.Empty, SecureQueryStringHelper.GetZGuid(NameValueCollection, "key"));

			ZGuid guid = ZGuid.NewZGuid();
			AddValueForTest("key", guid);
			AssertEquals(guid, SecureQueryStringHelper.GetZGuid(NameValueCollection, "key"));
		}

		public void TestGetBoolean()
		{
			Assert(!SecureQueryStringHelper.GetBoolean(NameValueCollection, "key"));

			AddValueForTest("key", true);
			Assert(SecureQueryStringHelper.GetBoolean(NameValueCollection, "key"));

			AddValueForTest("key", false);
			Assert(!SecureQueryStringHelper.GetBoolean(NameValueCollection, "key"));
		}

		public void TestGetZDate()
		{
			AssertEquals(ZDate.Empty, SecureQueryStringHelper.GetZDate(NameValueCollection, "key"));

			ZDate date = new ZDate(2009, 10, 30);

			AddValueForTest("key", date.ToJulianDateString());
			AssertEquals(date, SecureQueryStringHelper.GetZDate(NameValueCollection, "key"));
		}

		public void TestGetValue()
		{
			AssertNull(SecureQueryStringHelper.GetValue(NameValueCollection, "key"));

			AddValueForTest("key", "MEH");
			AssertEquals("MEH", SecureQueryStringHelper.GetValue(NameValueCollection, "key"));
		}

		void AddValueForTest(string key, object value)
		{
			SecureQueryString.Add(key, value.ToString());
			nameValueCollection = null;
		}

		NameValueCollection NameValueCollection
		{
			get
			{
				if (nameValueCollection == null)
				{
					nameValueCollection = new NameValueCollection();
					nameValueCollection.Add(VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, SecureQueryString.ToString());
				}
				return nameValueCollection;
			}
		}

		SecureQueryString SecureQueryString
		{
			get { return secureQueryString ?? (secureQueryString = new SecureQueryString()); }
		}

		SecureQueryString secureQueryString;
		NameValueCollection nameValueCollection;
	}
}
